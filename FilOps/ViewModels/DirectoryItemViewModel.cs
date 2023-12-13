using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models.StorageOperation;
using FilOps.Models.WindowsAPI;

namespace FilOps.ViewModels
{
    public class DirectoryItemViewModel : ObservableObject
    {
        private readonly MainViewModel? _mainViewModel;

        public DirectoryItemViewModel()
        {
            throw new InvalidOperationException("DirectoryItemViewModel");
        }

        public DirectoryItemViewModel(MainViewModel mv)
        {
            _mainViewModel = mv;
            if (_mainViewModel != null)
            {
                _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            }
        }
        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.FontSize))
            {
                // MainViewModel の FontSize が変更された場合、DirectoryItemViewModel のプロパティも更新
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #region データバインディング用
        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        private ObservableCollection<DirectoryItemViewModel> _Children = [];
        public ObservableCollection<DirectoryItemViewModel> Children
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
                Name = WindowsGetFolderDisplayName.GetDisplayName(FullPath);
                Icon = WindowsFileSystem.GetIcon(value);
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
        private DirectoryItemViewModel? _Parent = null;
        public DirectoryItemViewModel? Parent
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
                    if (_mainViewModel != null)
                    {
                        _mainViewModel.CurrentItem = this;
                    }
                    SetProperty(ref _IsSelected, value);

                    if (_IsSelected && _mainViewModel != null)
                    {
                        _mainViewModel.CurrentDir = this.FullPath;
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
                if (SetProperty(ref _HasChildren, value))
                {
                    if (value && Children.Count == 0 && _mainViewModel != null)
                    {
                        Children.Add(new DirectoryItemViewModel(_mainViewModel) { Name = "【dummy】" });
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
                if (SetProperty(ref _IsExpanded, value))
                {
                    if (IsReady)
                    {
                        Children.Clear();
                        if (_mainViewModel == null) return;
                        var dirs = new Dirs();
                        foreach (var child in dirs.GetDirInformation(FullPath))
                        {
                            var item = new DirectoryItemViewModel(_mainViewModel)
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
            get
            {
                if (_mainViewModel != null)
                {
                    return _mainViewModel.FontSize;
                }
                return SystemFonts.MessageFontSize;
            }
            set
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.FontSize = value;
                }
            }
        }
        #endregion データバインディング用
    }
}
