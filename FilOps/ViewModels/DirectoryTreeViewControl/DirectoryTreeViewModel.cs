using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels.DirectoryTreeViewControl
{
    public partial class DirectoryTreeViewModel : ObservableObject, IComparable<DirectoryTreeViewModel>
    {
        #region コンストラクタ

        /// <summary>
        /// TreeVIew コントロールのViewModelです。
        /// </summary>
        private readonly DirectoryTreeViewControlViewModel ControlVM;

        /// <summary>
        /// 引数を持たないコンストラクタは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">許容されないコンストラクタ呼び出し</exception>
        public DirectoryTreeViewModel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// コンストラクタで、DirectoryTreeViewControlViewModelの設定をします
        /// </summary>
        /// <param name="vm">DirectoryTreeViewControlViewModelの設定をします</param>
        public DirectoryTreeViewModel(DirectoryTreeViewControlViewModel vm)
        {
            ControlVM = vm;
            ControlVM.PropertyChanged += ControlVM_PropertyChanged;
        }

        /// <summary>
        /// コンストラクタで、DirectoryTreeViewControlViewModelとファイル情報の設定をします
        /// </summary>
        /// <param name="vm">DirectoryTreeViewControlViewModelの設定をします</param>
        /// <param name="f">ファイル情報</param>
        public DirectoryTreeViewModel(DirectoryTreeViewControlViewModel vm, FileItemInformation f) : this(vm)
        {
            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;
        }

        /// <summary>
        /// コンストラクタで、DirectoryTreeViewControlViewModelとファイル情報、親ディレクトリの設定をします
        /// </summary>
        /// <param name="vm">DirectoryTreeViewControlViewModelの設定をします</param>
        /// <param name="f">ファイル情報</param>
        /// <param name="parent">親ディレクトリ</param>
        public DirectoryTreeViewModel(DirectoryTreeViewControlViewModel vm, FileItemInformation f, DirectoryTreeViewModel parent) : this(vm, f)
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

        /// <summary>
        /// コントロールのフォントサイズ変更を受け取ります
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">PropertyChangedEventArgs</param>
        private void ControlVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DirectoryTreeViewControlViewModel.FontSize))
            {
                OnPropertyChanged(nameof(ControlVM.FontSize));
            }
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
                Name = WindowsAPI.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = WindowsAPI.GetIcon(FullPath);
                    FileType = WindowsAPI.GetType(FullPath);
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
                    Children.Add(new DirectoryTreeViewModel(ControlVM) { Name = "【dummy】" });
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
        /// MainViewから伝搬したユーザーコントロールのフォントサイズを利用する
        /// </summary>
        public double FontSize
        {
            get => ControlVM.FontSize;
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
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            set => SetProperty(ref _IsCheckBoxVisible, value);
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
                if (FullPath == string.Empty) { return; }
                if (value != _IsChecked)
                {
                    CheckCheckBoxStatusChanged(this, value);
                }
                SetProperty(ref _IsChecked, value, nameof(IsChecked));

                ParentCheckBoxChange(this);

                Debug.WriteLine($"Sync Called : {this.FullPath}");
                SyncSpecialDirectory(this, value);
            }
        }

        public bool? IsCheckedForSync
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value, nameof(IsChecked));
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
                        ControlVM.CurrentFullPath = this.FullPath;
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
                    if (value && !IsKicked) { KickChild(); }
                }
                if (value)
                {
                    foreach (var child in Children)
                    {
                        ControlVM.AddDirectoryToExpandedDirectoryManager(child);
                        if (IsChecked == true) { child.IsChecked = true; }

                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        ControlVM.RemoveDirectoryToExpandedDirectoryManager(child);
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool IsKicked = false;

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChild(bool force = false)
        {
            if (!IsKicked || force)
            {
                Debug.WriteLine($"{FullPath} is kicked.");
                Children.Clear();
                foreach (var child in FileSystemInformationManager.ScanFileItems(FullPath, false))
                {
                    var item = new DirectoryTreeViewModel(ControlVM, child, this);
                    Children.Add(item);
                }
            }
            IsKicked = true;
        }
        #endregion データバインディング
    }
}
    