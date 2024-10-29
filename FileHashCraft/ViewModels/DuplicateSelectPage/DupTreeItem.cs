using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    #region インターフェース
    public interface IDupTreeItem
    {
        /// <summary>
        /// ツリービューの子
        /// </summary>
        ObservableCollection<IDupTreeItem> Children { get; set; }
        /// <summary>
        /// 親ディレクトリ
        /// </summary>
        string Parent { get; }
        /// <summary>
        /// 選択されているかどうか
        /// </summary>
        bool IsSelected { get; set; }
    }
    #endregion インターフェース
    public partial class DupTreeItem : ObservableObject, IDupTreeItem
    {
        #region コンストラクタ
        private readonly IMessenger _messenger;
        private readonly ISettingsService _settingsService;

        public DupTreeItem()
        {
            _messenger = Ioc.Default.GetService<IMessenger>() ?? throw new InvalidOperationException($"{nameof(IMessenger)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            // フォント変更メッセージ受信
            _messenger.Register<CurrentFontFamilyChangedMessage>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _messenger.Register<FontSizeChangedMessage>(this, (_, m)
                => FontSize = m.FontSize);

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
        }

        public DupTreeItem(string parent) : this()
        {
            Parent = parent;
            _Name = Path.GetFileName(parent);
            _Icon = WindowsAPI.GetIcon(parent);
            IsSelected = true;
            IsExpanded = true;
        }

        public DupTreeItem(string parent, string name) : this()
        {
            Parent = parent;
            _Name = Path.GetFileName(name);
            _Icon = WindowsAPI.GetIcon(name);
            _IsCheckBoxVisible = Visibility.Hidden;
        }

        public DupTreeItem(string parent, HashFile hashFile) : this()
        {
            Parent = parent;
            _Name = Path.GetFileName(hashFile.FileFullPath);
            _Icon = WindowsAPI.GetIcon(hashFile.FileFullPath);
            _Hash = hashFile.FileHash;
            _FileSize = hashFile.FileSize;
        }
        #endregion コンストラクタ

        #region バインディング
        /// <summary>
        /// ツリービューの子
        /// </summary>
        public ObservableCollection<IDupTreeItem> Children { get; set; } = [];

        public string Parent { get; } = string.Empty;

        /// <summary>
        /// ファイル名
        /// </summary>
        [ObservableProperty]
        private string _Name = string.Empty;

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _Icon;

        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        [ObservableProperty]
        private long? _FileSize;

        /// <summary>
        /// ファイルのハッシュ
        /// </summary>
        [ObservableProperty]
        private string _Hash = string.Empty;

        /// <summary>
        /// 展開されているかどうか
        /// </summary>
        [ObservableProperty]
        private bool _IsExpanded = false;

        /// <summary>
        /// 選択されているかどうか
        /// </summary>
        [ObservableProperty]
        private bool _IsSelected = false;

        /// <summary>
        /// チェックされているかどうか
        /// </summary>
        [ObservableProperty]
        private bool? _IsChecked = false;

        /// <summary>
        /// チェックボックスを表示するか
        /// </summary>
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            set
            {
                if (value == Visibility.Hidden)
                {
                    IsChecked = null;
                }
                SetProperty(ref _IsCheckBoxVisible, value);
            }
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _settingsService.CurrentFont = value;
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _FontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) { return; }
                SetProperty(ref _FontSize, value);
                _settingsService.FontSize = value;
            }
        }
        #endregion バインディング
    }
}
