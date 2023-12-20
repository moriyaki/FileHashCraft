using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public class ExplorerListItemViewModel : ObservableObject, IComparable<ExplorerListItemViewModel>
    {
        private readonly ExplorerPageViewModel? _explorerPageViewModel;

        public ExplorerListItemViewModel()
        {
            throw new InvalidOperationException("ExplorerListItemViewModel");
        }
        public ExplorerListItemViewModel(ExplorerPageViewModel mv, FileInformation f)
        {
            _explorerPageViewModel = mv;
            if (_explorerPageViewModel is not null)
            {
                _explorerPageViewModel.PropertyChanged += ExplorerPageViewModel_PropertyChanged;
                FullPath = f.FullPath;
                LastModifiedDate = f.LastModifiedDate;
                FileSize = f.FileSize;
                IsDirectory = f.IsDirectory;
            }
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
                // ExplorerPageViewModel の FontSize が変更された場合、ExplorerListItemViewModel のプロパティも更新
                OnPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// ソートのための比較関数
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(ExplorerListItemViewModel? other)
        {
            return Name.CompareTo(other?.Name);
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
                Name = WindowsAPI.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = WindowsAPI.GetIcon(FullPath);
                    FileType = WindowsAPI.GetType(FullPath);
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
            get => _explorerPageViewModel?.FontSize ?? SystemFonts.MessageFontSize;
            set
            {
                if (_explorerPageViewModel is not null)
                {
                    _explorerPageViewModel.FontSize = value;
                }
            }
        }
        #endregion データバインディング
    }
}
