using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    public partial class ExplorerTreeNodeViewModel : ExplorerItemViewModelBase
    {
        public ExplorerTreeNodeViewModel(IExplorerPageViewModel explorerVM) : base(explorerVM) { }

        public ExplorerTreeNodeViewModel(IExplorerPageViewModel explorerVM, FileItemInformation f) : base(explorerVM, f) { }
        public ExplorerTreeNodeViewModel(IExplorerPageViewModel explorerVM, FileItemInformation f, ExplorerTreeNodeViewModel parent) : base(explorerVM, f)
        {
            Parent = parent;
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

        //private bool IsUpdating = false;
        /// <summary>
        /// ディレクトリのチェックボックスがチェックされているかどうか
        /// </summary>
        private bool? _IsChecked = false;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                /*
                if (IsUpdating)
                {
                    SetProperty(ref _IsChecked, value);
                }
                else
                {
                    IsUpdating = true;
                */
                if (FullPath == string.Empty) { return;  }
                if (value != _IsChecked)
                {
                    CheckStatusChanged(this, value);
                }        
                SetProperty(ref _IsChecked, value);
                

                /*
                    IsUpdating = false;
                }
                */
            }
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
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        public override bool HasChildren
        {
            get => _HasChildren;
            set
            {
                if (SetProperty(ref _HasChildren, value) && Children.Count == 0)
                {
                    Children.Add(new ExplorerTreeNodeViewModel(ExplorerVM) { Name = "【dummy】" });

                }
            }
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        public override bool IsReady
        {
            get => _IsReady;
            set
            {
                SetProperty(ref _IsReady, value);
                if (!value) { Children.Clear(); }
            }
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
                        ExplorerVM.CurrentDirectoryItem = this;
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
                    if (value && !IsKicked) { KickChildGet(); }
                }
                if (value)
                {
                    foreach (var child in Children)
                    {
                        ExplorerVM.AddDirectoryToExpandedDirectoryManager(child);
                        if (IsChecked == true) { child.IsChecked = true; }
                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        ExplorerVM.RemoveDirectoryToExpandedDirectoryManager(child);
                    }
                }
            }
        }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChildGet()
        {
            Children.Clear();
            foreach (var child in FileSystemInformationManager.FileItemScan(FullPath, false))
            {
                var item = new ExplorerTreeNodeViewModel(ExplorerVM, child, this);
                Children.Add(item);

            }
            IsKicked = true;
        }
        #endregion データバインディング用
    }
}
