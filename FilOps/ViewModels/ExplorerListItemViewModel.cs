using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models.WindowsAPI;

namespace FilOps.ViewModels
{
    public class ExplorerListItemViewModel : ObservableObject
    {
        private readonly MainViewModel? _mainViewModel;

        public ExplorerListItemViewModel()
        {
            throw new InvalidOperationException("ExplorerListItemViewModel");
        }

        public ExplorerListItemViewModel(MainViewModel mv)
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
                // MainViewModel の FontSize が変更された場合、ExplorerListItemViewModel のプロパティも更新
                OnPropertyChanged(nameof(FontSize));
            }
        }

        #region データバインディング
        /// <summary>
        /// ファイル表示名
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
                Name = WindowsGetFolderDisplayName.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = WindowsFileSystem.GetIcon(FullPath);
                    FileType = WindowsFileSystem.GetType(FullPath);
                }));
            }
        }

        /// <summary>
        /// チェックボックス
        /// </summary>
        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }

        /// <summary>
        /// アイコン
        /// </summary>
        private BitmapSource? _Icon = null;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        /// <summary>
        /// 表示用の更新日時文字列
        /// </summary>
        public string LastFileUpdate
        {
            get => LastModifiedDate?.ToString("yy/MM/dd HH:mm") ?? string.Empty;
        }

        /// <summary>
        /// 更新日時
        /// </summary>
        private DateTime? _LastModifiedDate = null;
        public DateTime? LastModifiedDate
        {
            get => _LastModifiedDate;
            set => SetProperty(ref _LastModifiedDate, value);
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
        /// ファイルのサイズ
        /// </summary>
        private long? _FileSize = null;
        public long? FileSize
        {
            get => _FileSize;
            set => SetProperty(ref _FileSize, value);
        }

        /// <summary>
        /// 書式化したファイルのサイズ文字列
        /// </summary>
        public string FormattedFileSize
        {
            get
            {
                if (FileSize == null || IsDirectory) return string.Empty;
                var kb_filesize = (long)FileSize / 1024;
                return kb_filesize.ToString("N") + "KB";
            }
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
        #endregion データバインディング
    }
}
