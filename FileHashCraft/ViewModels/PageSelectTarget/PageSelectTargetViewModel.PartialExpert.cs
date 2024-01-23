/*  PageSelectTargetViewModel.PartialExpert.cs

    エキスパート向けの検索条件を提供するタブの ViewModel を提供します。
 */

using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public partial class PageSelectTargetViewModel
    {
        #region バインディング
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        public bool IsReadOnlyFileInclude
        {
            get => _MainWindowViewModel.IsReadOnlyFileInclude;
            set
            {
                if (_MainWindowViewModel.IsReadOnlyFileInclude == value) return;
                _MainWindowViewModel.IsReadOnlyFileInclude = value;
                OnPropertyChanged(nameof(IsReadOnlyFileInclude));
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        public bool IsHiddenFileInclude
        {
            get => _MainWindowViewModel.IsHiddenFileInclude;
            set
            {
                if (_MainWindowViewModel.IsHiddenFileInclude == value) return;
                _MainWindowViewModel.IsHiddenFileInclude = value;
                OnPropertyChanged(nameof(IsHiddenFileInclude));
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        public bool IsZeroSizeFileDelete
        {
            get => _MainWindowViewModel.IsZeroSizeFileDelete;
            set
            {
                if (_MainWindowViewModel.IsZeroSizeFileDelete == value) return;
                _MainWindowViewModel.IsZeroSizeFileDelete = value;
                OnPropertyChanged(nameof(IsZeroSizeFileDelete));
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        public bool IsEmptyDirectoryDelete
        {
            get => _MainWindowViewModel.IsEmptyDirectoryDelete;
            set
            {
                if (_MainWindowViewModel.IsEmptyDirectoryDelete == value) return;
                _MainWindowViewModel.IsEmptyDirectoryDelete = value;
                OnPropertyChanged(nameof(IsEmptyDirectoryDelete));
            }
        }
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public DelegateCommand IsReadOnlyFileIncludeClicked { get; set; }
        /// <summary>
        /// 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public DelegateCommand IsHiddenFileIncludeClicked { get; set; }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsEmptyDirectoryDeleteClicked { get; set; }
        #endregion コマンド
    }
}
