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
    public interface IDirectoryTreeViewItemModel
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
        ObservableCollection<DirectoryTreeViewItemModel> Children { get; }
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
    public partial class DirectoryTreeViewItemModel : ObservableObject, IComparable<DirectoryTreeViewItemModel>, IDirectoryTreeViewItemModel
    {
        #region コンストラクタ
        private readonly ISettingsService _settingsService;
        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入です。
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        public DirectoryTreeViewItemModel()
        {
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);

            var settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            _currentFontFamily = settingsService.CurrentFont;
            _fontSize = settingsService.FontSize;
        }

        /// <summary>
        /// コンストラクタで、ファイル情報の設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        public DirectoryTreeViewItemModel(FileItemInformation f) : this()
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
        public DirectoryTreeViewItemModel(FileItemInformation f, DirectoryTreeViewItemModel parent) : this(f)
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
        public int CompareTo(DirectoryTreeViewItemModel? other)
        {
            return FullPath.CompareTo(other?.FullPath);
        }

        #endregion メソッド

        #region バインディング
        /// <summary>
        /// ファイルの表示名
        /// </summary>
        [ObservableProperty]
        private string _name = string.Empty;

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _icon;

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
        private string _fullPath = string.Empty;
        public string FullPath
        {
            get => _fullPath;
            set
            {
                SetProperty(ref _fullPath, value);
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
        private string _fileType = string.Empty;

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        [ObservableProperty]
        protected bool _isReady = false;

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        [ObservableProperty]
        private bool _isRemovable = false;

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        [ObservableProperty]
        private bool _isDirectory;

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        private bool _hasChildren = false;
        public bool HasChildren
        {
            get => _hasChildren;
            set
            {
                if (!value)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _hasChildren, value) && Children.Count == 0)
                {
                    Children.Add(new DirectoryTreeViewItemModel() { Name = "【dummy】" });
                }
            }
        }

        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewItemModel> Children { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        public static Visibility IsCheckBoxVisible
        {
            get => WeakReferenceMessenger.Default.Send(new TreeViewIsCheckBoxVisible());
        }

        /// <summary>
        /// ディレクトリのチェックボックスがチェックされているかどうか
        /// </summary>
        private bool? _isChecked = false;
        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (FullPath?.Length == 0) { return; }
                if (value == _isChecked) return;

                // サブディレクトリのチェック状態を変更する
                CheckCheckBoxStatusChanged(this, value);

                // 値をセットする
                SetProperty(ref _isChecked, value, nameof(IsChecked));

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
            get => _isChecked;
            set => SetProperty(ref _isChecked, value, nameof(IsChecked));
        }

        /// <summary>
        /// ディレクトリの親ディレクトリ
        /// </summary>
        [ObservableProperty]
        private DirectoryTreeViewItemModel? _parent;

        /// <summary>
        /// ディレクトリが選択されているかどうか
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    SetProperty(ref _isSelected, value);
                    if (value)
                    {
                        WeakReferenceMessenger.Default.Send(new CurrentDirectoryChangedMessage(this.FullPath));
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
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (!HasChildren)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _isExpanded, value) && IsReady)
                {
                    if (value && !_isKicked) { KickChild(); }
                }
                if (value)
                {
                    foreach (var child in Children)
                    {
                        // ディレクトリノードを展開マネージャに追加
                        WeakReferenceMessenger.Default.Send(new AddToExpandDirectoryManagerMessage(child));
                        if (IsChecked == true) { child.IsChecked = true; }
                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        // ディレクトリノードを展開マネージャから削除
                        WeakReferenceMessenger.Default.Send(new RemoveFromExpandDirectoryManagerMessage(child));
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool _isKicked;
        public bool IsKicked { get => _isKicked; }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChild(bool force = false)
        {
            if (!_isKicked || force)
            {
                Children.Clear();
                foreach (var childPath in FileManager.EnumerateDirectories(FullPath))
                {
                    var child = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(childPath);
                    var item = new DirectoryTreeViewItemModel(child, this);
                    Children.Add(item);
                }
            }
            _isKicked = true;
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _currentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _currentFontFamily;
            set
            {
                if (_currentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _currentFontFamily, value);
                _settingsService.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _fontSize;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) { return; }
                SetProperty(ref _fontSize, value);
                _settingsService.SendFontSize(value);
            }
        }
        #endregion バインディング
    }
}
