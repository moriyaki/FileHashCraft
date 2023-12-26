using System.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    public class ExplorerListItemViewModel : ExplorerItemViewModelBase
    {
        public ExplorerListItemViewModel() : base() { }

        public ExplorerListItemViewModel(ExplorerPageViewModel explorerVM, FileItemInformation f) : base(explorerVM, f)
        {
            LastModifiedDate = f.LastModifiedDate;
            FileSize = f.FileSize;
        }

 

        #region データバインディング
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
