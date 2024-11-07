using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.HashCalc;
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

        private readonly IMessenger _Messanger;
        private readonly ISettingsService _SettingsService;
        private readonly IDuplicateFilesManager _DupFilesManager;

        public DupTreeItem()
        {
            _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            _SettingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));
            _DupFilesManager = Ioc.Default.GetService<IDuplicateFilesManager>() ?? throw new NullReferenceException(nameof(IDuplicateFilesManager));
            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m)
                => FontSize = m.FontSize);

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;
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
        private bool _IsSelected = false;

        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                SetProperty(ref _IsSelected, value);
                _DupFilesManager.GetDuplicateLinkFiles(Hash);
            }
        }

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
                _SettingsService.CurrentFont = value;
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
                _SettingsService.FontSize = value;
            }
        }

        #endregion バインディング
    }
}