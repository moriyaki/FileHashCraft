using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public class ExplorerTreeNodeViewModel : ObservableObject
    {
        private readonly ExplorerPageViewModel? _explorerPageViewModel;

        public ExplorerTreeNodeViewModel()
        {
            throw new InvalidOperationException("ExplorerTreeNodeViewModel");
        }

        public ExplorerTreeNodeViewModel(ExplorerPageViewModel mv)
        {
            _explorerPageViewModel = mv;
            if (_explorerPageViewModel is not null)
            {
                _explorerPageViewModel.PropertyChanged += ExplorerPageViewModel_PropertyChanged;
            }
        }
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
                SetProperty(ref _FullPath, value);
                Name = WindowsAPI.GetDisplayName(FullPath);
                Icon = WindowsAPI.GetIcon(value);
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
            set => SetProperty(ref _IsReady, value);
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
                if (value != _IsSelected)
                {
                    if (_explorerPageViewModel is not null)
                    {
                        _explorerPageViewModel.CurrentItem = this ?? null;
                    }

                    if (SetProperty(ref _IsSelected, value) && _explorerPageViewModel is not null)
                    {
                        _explorerPageViewModel.CurrentDir = this.FullPath;
                    }
                }

            }
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
                if (SetProperty(ref _HasChildren, value) &&
                    Children.Count == 0 && _explorerPageViewModel is not null)
                {
                    Children.Add(new ExplorerTreeNodeViewModel(_explorerPageViewModel) { Name = "【dummy】" });
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
                    Children.Clear();
                    if (_explorerPageViewModel == null) return;
                    foreach (var child in FileSystemManager.Instance.GetFilesInformation(FullPath, true))
                    {
                        var item = new ExplorerTreeNodeViewModel(_explorerPageViewModel)
                        {
                            FullPath = child.FullPath,
                            HasChildren = child.HasChildren,
                            IsReady = this.IsReady,
                            Parent = this,
                        };
                        Children.Add(item);
                    }
                }
            }
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
            get => _explorerPageViewModel?.FontSize ?? SystemFonts.MessageFontSize;
            set
            {
                if (_explorerPageViewModel is not null)
                {
                    _explorerPageViewModel.FontSize = value;
                }
            }
        }
        #endregion データバインディング用
    }
}
