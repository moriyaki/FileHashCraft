using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public class ExplorerTreeNodeViewModel : ObservableObject, IComparable<ExplorerTreeNodeViewModel>
    {
        private readonly ExplorerPageViewModel ExplorerVM;

        private ExplorerTreeNodeViewModel()
        {
            throw new InvalidOperationException("ExplorerTreeNodeViewModel");
        }

        public ExplorerTreeNodeViewModel(ExplorerPageViewModel mv)
        {
            ExplorerVM = mv;
             ExplorerVM.PropertyChanged += ExplorerPageViewModel_PropertyChanged;
        }
        public ExplorerTreeNodeViewModel(ExplorerPageViewModel vm, FileInformation f)
        {
            ExplorerVM = vm;
            ExplorerVM.PropertyChanged += ExplorerPageViewModel_PropertyChanged;
            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            HasChildren = f.HasChildren;
        }

        /// <summary>
        /// ソートのための比較関数
        /// </summary>
        /// <param name="other">ExplorerTreeNodeViewModel?</param>
        /// <returns><bool/returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(ExplorerTreeNodeViewModel? other)
        {
            return Name.CompareTo(other?.Name);
        }


        /// <summary>
        /// PageのViewModelからフォントサイズの変更を受け取る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerPageViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)

        {
            if (e.PropertyName == nameof(ExplorerPageViewModel.FontSize))
            {
                // ExplorerPageViewModel の FontSize が変更された場合、ExplorerTreeNodeViewModel のプロパティも更新
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #region データバインディング用
        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        private ObservableCollection<ExplorerTreeNodeViewModel> _Children = [];
        public ObservableCollection<ExplorerTreeNodeViewModel> Children
        {
            get => _Children;
            set => SetProperty(ref _Children, value);
        }

        public void AddChildren(ExplorerTreeNodeViewModel item)
        {
            Children.Add(item);
        }

        /// <summary>
        /// ディレクトリの表示名
        /// </summary>
        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        /// <summary>
        /// ディレクトリのの実体名
        /// </summary>
        public string FileName
        {
            get => Path.GetFileName(FullPath);
        }

        /// <summary>
        /// ディレクトリのフルパス
        /// </summary>
        private string _FullPath = string.Empty;
        public string FullPath
        {
            get => _FullPath;
            set
            {
                Name = WindowsAPI.GetDisplayName(value);
                Icon = WindowsAPI.GetIcon(value);
                SetProperty(ref _FullPath, value);
            }
        }

        /// <summary>
        /// ディレクトリのアイコン
        /// </summary>
        private BitmapSource? _Icon = null;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        /// <summary>
        /// ディレクトリの親ディレクトリ
        /// </summary>
        private ExplorerTreeNodeViewModel? _Parent = null;
        public ExplorerTreeNodeViewModel? Parent
        {
            get => _Parent;
            private set => SetProperty(ref _Parent, value);
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        private bool _IsReady = false;
        public bool IsReady
        {
            get => _IsReady;
            set
            {
                SetProperty(ref _IsReady, value);
                if (!value) { Children.Clear(); }
            }
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
                    if (value && ExplorerVM != null)
                    {
                        ExplorerVM.CurrentItem = this;
                        if (!IsKicked) { KickChildGet(); }
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool IsKicked = false;

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        private bool _HasChildren = false;
        public bool HasChildren
        {
            get => _HasChildren;
            set
            {
                if (SetProperty(ref _HasChildren, value) &&
                    Children.Count == 0 && ExplorerVM is not null)
                {
                    Children.Add(new ExplorerTreeNodeViewModel(ExplorerVM) { Name = "【dummy】" });

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
                    if (value && !IsKicked)
                    {
                        KickChildGet();
                    }
                }
                if (value)
                {
                    ExplorerVM.TreeViewManager.AddDirectory(this.FullPath);
                }
                else
                {
                    ExplorerVM.TreeViewManager.RemoveDirectory(this.FullPath);
                }
            }
        }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChildGet()
        {
            if (ExplorerVM == null) { return; }
            Children.Clear();
            foreach (var child in FileSystemManager.FileItemScan(FullPath, false))
            {
                var item = new ExplorerTreeNodeViewModel(ExplorerVM)
                {
                    FullPath = child.FullPath,
                    IsReady = this.IsReady,
                    IsRemovable = this.IsRemovable,
                    HasChildren = child.HasChildren,
                    Parent = this,
                };
                Children.Add(item);

            }
            IsKicked = true;
        }

        /// <summary>
        /// ディレクトリのチェックボックスがチェックされているかどうか
        /// </summary>
        private bool? _IsChecked = false;
        public bool? IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public double FontSize
        {
            get => ExplorerVM?.FontSize ?? SystemFonts.MessageFontSize;
            set
            {
                if (ExplorerVM is not null)
                {
                    ExplorerVM.FontSize = value;
                }
            }
        }
        #endregion データバインディング用
    }
}
