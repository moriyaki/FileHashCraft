using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.Models;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    public partial class DirectoryTreeViewModel : ObservableObject, IComparable<DirectoryTreeViewModel>
    {
        #region コンストラクタ
        /// <summary>
        /// TreeVIew コントロールのViewModelです。
        /// </summary>
        private readonly IControDirectoryTreeViewlViewModel _ControDirectoryTreeViewlViewModel;
        private readonly IWindowsAPI _WindowsAPI;
        private readonly ISpecialFolderAndRootDrives _SpecialFolderAndRootDrives;
        private readonly IMainWindowViewModel _MainWindowViewModel;
        /// <summary>
        /// コンストラクタで、TreeViewModelを取得して、変更通知を受け取ります。
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public DirectoryTreeViewModel()
        {
            _WindowsAPI = Ioc.Default.GetService<IWindowsAPI>() ?? throw new InvalidOperationException($"{nameof(IWindowsAPI)} dependency not resolved.");
            _ControDirectoryTreeViewlViewModel = Ioc.Default.GetService<IControDirectoryTreeViewlViewModel>() ?? throw new InvalidOperationException($"{nameof(IControDirectoryTreeViewlViewModel)} dependency not resolved.");
            _SpecialFolderAndRootDrives = Ioc.Default.GetService<ISpecialFolderAndRootDrives>() ?? throw new InvalidOperationException($"{nameof(ISpecialFolderAndRootDrives)} dependency not resolved.");
            _MainWindowViewModel = Ioc.Default.GetService<IMainWindowViewModel>() ?? throw new InvalidOperationException($"{nameof(IMainWindowViewModel)} dependency not resolved.");

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) => UsingFont = message.UsingFont);
            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) => FontSize = message.FontSize);
        }

        /// <summary>
        /// コンストラクタで、ファイル情報の設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        public DirectoryTreeViewModel(FileItemInformation f) : this()
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
        public DirectoryTreeViewModel(FileItemInformation f, DirectoryTreeViewModel parent) : this(f)
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
        public int CompareTo(DirectoryTreeViewModel? other)
        {
            return FullPath.CompareTo(other?.FullPath);
        }

        #endregion メソッド

        #region データバインディング
        /// <summary>
        /// ファイルの表示名
        /// </summary>
        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

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
                Name = _WindowsAPI.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = _WindowsAPI.GetIcon(FullPath);
                    FileType = _WindowsAPI.GetType(FullPath);
                }));
            }
        }

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        private BitmapSource? _Icon = null;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _FileType = string.Empty;
        public string FileType
        {
            get => _FileType;
            set => SetProperty(ref _FileType, value);
        }

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        private bool _HasChildren = false;
        public bool HasChildren
        {
            get => _HasChildren;
            set
            {
                if (SetProperty(ref _HasChildren, value) && Children.Count == 0)
                {
                    Children.Add(new DirectoryTreeViewModel() { Name = "【dummy】" });
                }
            }
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        protected bool _IsReady = false;
        public virtual bool IsReady
        {
            get => _IsReady;
            set => SetProperty(ref _IsReady, value);
        }

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        private bool _IsRemovable = false;
        public bool IsRemovable
        {
            get => _IsRemovable;
            set => SetProperty(ref _IsRemovable, value);
        }

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        private bool _IsDirectory = false;
        public bool IsDirectory
        {
            get => _IsDirectory;
            set => SetProperty(ref _IsDirectory, value);
        }

        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        private ObservableCollection<DirectoryTreeViewModel> _Children = [];
        public ObservableCollection<DirectoryTreeViewModel> Children
        {
            get => _Children;
            set => SetProperty(ref _Children, value);
        }

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        public Visibility IsCheckBoxVisible
        {
            get => _ControDirectoryTreeViewlViewModel.IsCheckBoxVisible;
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

        public bool? IsCheckedForSync
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value, nameof(IsChecked));
            }
        }

        /// <summary>
        /// ディレクトリの親ディレクトリ
        /// </summary>
        private DirectoryTreeViewModel? _Parent = null;
        public DirectoryTreeViewModel? Parent
        {
            get => _Parent;
            protected set => SetProperty(ref _Parent, value);
        }

        /// <summary>
        /// ディレクトリが選択されているかどうか
        /// </summary>
        private bool _IsSelected = false;
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
                        _ControDirectoryTreeViewlViewModel.CurrentFullPath = this.FullPath;
                        KickChild();
                    }
                }
            }
        }

        /// <summary>
        /// ディレクトリが展開されているかどうか
        /// </summary>
        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get => _IsExpanded;
            set
            {
                if (SetProperty(ref _IsExpanded, value) && IsReady)
                {
                    if (value && !_IsKicked) { KickChild(); }
                }
                if (value)
                {
                    foreach (var child in Children)
                    {
                        _ControDirectoryTreeViewlViewModel.AddDirectoryToExpandedDirectoryManager(child);
                        if (IsChecked == true) { child.IsChecked = true; }
                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        _ControDirectoryTreeViewlViewModel.RemoveDirectoryToExpandedDirectoryManager(child);
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool _IsKicked = false;
        public bool IsKicked { get => _IsKicked; }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChild(bool force = false)
        {
            if (!_IsKicked || force)
            {
                Children.Clear();
                var fileInfoManager = new ScanFileItems();
                foreach (var childPath in fileInfoManager.EnumerateDirectories(FullPath))
                {
                    var child = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(childPath);
                    var item = new DirectoryTreeViewModel(child, this);
                    Children.Add(item);
                }
            }
            _IsKicked = true;
        }

        /// <summary>
        /// フォントの取得と設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _MainWindowViewModel.UsingFont;
            set
            {
                _MainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの取得と設定
        /// </summary>
        public double FontSize
        {
            get => _MainWindowViewModel.FontSize;
            set
            {
                _MainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
        #endregion データバインディング
    }
}
