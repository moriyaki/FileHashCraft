using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    public class ExplorerListItemViewModel : ObservableObject, IComparable<ExplorerListItemViewModel>
    {
        #region コンストラクタ
        /// <summary>
        /// コンストラクタで渡されるIExplorerPageViewModel
        /// </summary>
        protected readonly ExplorerPageViewModel ExplorerVM;

        public ExplorerListItemViewModel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// コンストラクタで、IExplorerPageViewModelの設定をします
        /// </summary>
        /// <param name="explorerVM">IExplorerPageViewModel</param>
        public ExplorerListItemViewModel(ExplorerPageViewModel explorerVM)
        {
            ExplorerVM = explorerVM;
            ExplorerVM.PropertyChanged += ExplorerPageViewModel_PropertyChanged;

        }

        /// <summary>
        /// コンストラクタで、IExplorerPageViewModelとファイル情報の設定をします
        /// </summary>
        /// <param name="explorerVM">IExplorerPageViewModel</param>
        /// <param name="f">FileItemInformation</param>
        public ExplorerListItemViewModel(ExplorerPageViewModel explorerVM, FileItemInformation f)
        {
            ExplorerVM = explorerVM;
            ExplorerVM.PropertyChanged += ExplorerPageViewModel_PropertyChanged;

            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;

            LastModifiedDate = f.LastModifiedDate;
            FileSize = f.FileSize;
        }
        #endregion コンストラクタ

        #region メソッド
        /// <summary>
        /// PageのViewModelからフォントサイズの変更を受け取ります。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerPageViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExplorerPageViewModel.FontSize))
            {
                // ExplorerPageViewModel の FontSize が変更された場合、ExplorerItemViewModelBase のプロパティも更新
                OnPropertyChanged(nameof(ExplorerVM.FontSize));

            }
        }

        /// <summary>
        /// ソートのための比較関数です。
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(ExplorerListItemViewModel? other)
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
        protected bool _HasChildren = false;
        public virtual bool HasChildren
        {
            get => _HasChildren;
            set => SetProperty(ref _HasChildren, value);
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
        /// フォントサイズ
        /// </summary>
        public double FontSize
        {
            get => ExplorerVM.FontSize;
        }

        /// <summary>
        /// 表示用の更新日時文字列
        /// </summary>
        public string LastFileUpdate
        {
            get => LastModifiedDate?.ToString("yy/MM/dd HH:mm") ?? string.Empty;
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
        /// 更新日時
        /// </summary>
        private DateTime? _LastModifiedDate = null;
        public DateTime? LastModifiedDate
        {
            get => _LastModifiedDate;
            set => SetProperty(ref _LastModifiedDate, value);
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
        #endregion データバインディング
    }
}
