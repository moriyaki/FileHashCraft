/*  DirectoryTreeViewModel.cs

    ディレクトリツリービューのアイテム ViewModel を提供します。
 */
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

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IDirectoryTreeItem
    {
        /// <summary>
        /// ファイル表示名を設定もしくは取得します。
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// ファイルのアイコンを設定もしくは取得します。
        /// </summary>
        BitmapSource? Icon { get; set; }
        /// <summary>
        /// ファイルのフルパスを取得します。
        /// </summary>
        string FullPath { get; }
        /// <summary>
        /// 子ディレクトリを持っているかどうかを取得します。
        /// </summary>
        bool HasChildren { get; }
        /// <summary>
        /// 子ディレクトリのコレクションを取得します。
        /// </summary>
        ObservableCollection<DirectoryTreeItem> Children { get; }
        /// <summary>
        /// 子ディレクトリを取得します。
        /// </summary>
        void KickChild(bool force = false);
        /// <summary>        /// <summary>
        /// ノードがチェックされているかどうかを設定もしくは取得します。
        /// </summary>
        bool? IsChecked { get; set; }
        /// <summary>
        /// ノードが選択されているかどうかを取得します。
        /// </summary>
        bool IsSelected { get; }
        /// <summary>
        /// ノードが展開されているかどうかを取得します。
        /// </summary>
        bool IsExpanded { get; }
    }
    #endregion インターフェース
    public partial class DirectoryTreeItem : ObservableObject, IComparable<DirectoryTreeItem>, IDirectoryTreeItem
    {
        #region コンストラクタ
        private readonly IMessenger _messenger;
        private readonly ISettingsService _settingsService;
        private readonly IFileManager _fileManager;
        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入です。
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        public DirectoryTreeItem()
        {
            _messenger = Ioc.Default.GetService<IMessenger>() ?? throw new InvalidOperationException($"{nameof(IMessenger)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            _fileManager = Ioc.Default.GetService<IFileManager>() ?? throw new InvalidOperationException($"{nameof(IFileManager)} dependency not resolved.");
            // フォント変更メッセージ受信
            _messenger.Register<CurrentFontFamilyChangedMessage>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _messenger.Register<FontSizeChangedMessage>(this, (_, m)
                => FontSize = m.FontSize);

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
        }

        /// <summary>
        /// コンストラクタで、ファイル情報の設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        public DirectoryTreeItem(FileItemInformation f) : this()
        {
            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;
        }
        /// <summary>
        /// コンストラクタで、ファイル情報、親ディレクトリの設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        /// <param name="parent">親ディレクトリ</param>
        public DirectoryTreeItem(FileItemInformation f, DirectoryTreeItem parent) : this(f)
        {
            Parent = parent;
        }
        #endregion コンストラクタ

        #region メソッド
        /// <summary>
        /// ソートのための比較関数です。
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        public int CompareTo(DirectoryTreeItem? other)
        {
            return FullPath.CompareTo(other?.FullPath);
        }

        #endregion メソッド

        #region バインディング
        /// <summary>
        /// ファイルの表示名
        /// </summary>
        [ObservableProperty]
        private string _Name = string.Empty;

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _Icon;

        /// <summary>
        /// ファイル実体名
        /// </summary>
        public string FileName
        {
            get => Path.GetFileName(FullPath);
        }

        /// <summary>
        /// ファイルまたはフォルダのフルパス
        /// </summary>
        private string _FullPath = string.Empty;
        public string FullPath
        {
            get => _FullPath;
            set
            {
                SetProperty(ref _FullPath, value);
                UpdatePropertiesFromFullPath();
            }
        }

        /// <summary>
        /// フルパスが変更されてアイコンやファイル種類を変更する
        /// </summary>
        private void UpdatePropertiesFromFullPath()
        {
            Name = WindowsAPI.GetDisplayName(FullPath);

            App.Current?.Dispatcher.Invoke(() =>
            {
                Icon = WindowsAPI.GetIcon(FullPath);
                FileType = WindowsAPI.GetType(FullPath);
            });
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        [ObservableProperty]
        private string _FileType = string.Empty;

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        [ObservableProperty]
        protected bool _IsReady = false;

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        [ObservableProperty]
        private bool _IsRemovable = false;

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        [ObservableProperty]
        private bool _IsDirectory;

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        private bool _HasChildren = false;
        public bool HasChildren
        {
            get => _HasChildren;
            set
            {
                if (!value)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _HasChildren, value) && Children.Count == 0)
                {
                    Children.Add(new DirectoryTreeItem() { Name = "【dummy】" });
                }
            }
        }

        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeItem> Children { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        public Visibility IsCheckBoxVisible
        {
            get => _messenger.Send(new TreeViewIsCheckBoxVisible());
        }

        /// <summary>
        /// ディレクトリのチェックボックスがチェックされているかどうか
        /// </summary>
        private bool? _IsChecked = false;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (FullPath?.Length == 0) { return; }
                if (value == _IsChecked) return;

                // サブディレクトリのチェック状態を変更する
                CheckCheckBoxStatusChanged(this, value);

                // 値をセットする
                SetProperty(ref _IsChecked, value, nameof(IsChecked));

                // 親ディレクトリのチェック状態を変更する
                ParentCheckBoxChange(this);

                // 特殊フォルダのチェック状態を変更する
                SyncSpecialDirectory(this, value);
            }
        }

        /// <summary>
        /// 他に影響を与えない形でディレクトリのチェックボックス状態を変更します。
        /// </summary>
        public bool? IsCheckedForSync
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value, nameof(IsChecked));
        }

        /// <summary>
        /// ディレクトリの親ディレクトリ
        /// </summary>
        [ObservableProperty]
        private DirectoryTreeItem? _parent;

        /// <summary>
        /// ディレクトリが選択されているかどうか
        /// </summary>
        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected != value)
                {
                    SetProperty(ref _IsSelected, value);
                    if (value)
                    {
                        _messenger.Send(new CurrentDirectoryChangedMessage(this.FullPath));
                        if (HasChildren)
                        {
                            KickChild();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ディレクトリが展開されているかどうか
        /// </summary>
        private bool _IsExpanded;
        public bool IsExpanded
        {
            get => _IsExpanded;
            set
            {
                if (!HasChildren)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _IsExpanded, value) && IsReady)
                {
                    if (value && !_IsKicked) { KickChild(); }
                }
                if (value)
                {
                    foreach (var child in Children)
                    {
                        // ディレクトリノードを展開マネージャに追加
                        _messenger.Send(new AddToExpandDirectoryManagerMessage(child));
                        if (IsChecked == true) { child.IsChecked = true; }
                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        // ディレクトリノードを展開マネージャから削除
                        _messenger.Send(new RemoveFromExpandDirectoryManagerMessage(child));
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool _IsKicked;
        public bool IsKicked { get => _IsKicked; }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChild(bool force = false)
        {
            if (!_IsKicked || force)
            {
                Children.Clear();
                foreach (var childPath in _fileManager.EnumerateDirectories(FullPath))
                {
                    var child = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(childPath);
                    var item = new DirectoryTreeItem(child, this);
                    Children.Add(item);
                }
            }
            _IsKicked = true;
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
