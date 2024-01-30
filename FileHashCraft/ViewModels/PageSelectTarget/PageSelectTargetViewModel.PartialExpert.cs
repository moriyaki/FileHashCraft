/*  PageSelectTargetViewModel.PartialExpert.cs

    エキスパート向けの検索条件を提供するタブの ViewModel を提供します。
 */

using CommunityToolkit.Mvvm.Input;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public partial class PageSelectTargetViewModel
    {
        #region バインディング
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        private bool _IsReadOnlyFileInclude;
        public bool IsReadOnlyFileInclude
        {
            get => _IsReadOnlyFileInclude;
            set
            {
                if (value == _IsReadOnlyFileInclude) { return; }
                SetProperty(ref _IsReadOnlyFileInclude, value);
                _messageServices.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _IsHiddenFileInclude;
        public bool IsHiddenFileInclude
        {
            get => _IsHiddenFileInclude;
            set
            {
                if (value == _IsHiddenFileInclude) { return; }
                SetProperty(ref _IsHiddenFileInclude, value);
                _messageServices.SendEmptyDirectoryDelete(value);
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _IsZeroSizeFileDelete;
        public bool IsZeroSizeFileDelete
        {
            get => _IsZeroSizeFileDelete;
            set
            {
                if (value == _IsZeroSizeFileDelete) { return; }
                SetProperty(ref _IsZeroSizeFileDelete, value);
                _messageServices.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _IsEmptyDirectoryDelete;
        public bool IsEmptyDirectoryDelete
        {
            get => _IsEmptyDirectoryDelete;
            set
            {
                if (value == _IsEmptyDirectoryDelete) { return; }
                SetProperty(ref _IsEmptyDirectoryDelete, value);
                _messageServices.SendEmptyDirectoryDelete(value);
            }
        }
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsReadOnlyFileIncludeClicked { get; set; }
        /// <summary>
        /// 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsHiddenFileIncludeClicked { get; set; }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsEmptyDirectoryDeleteClicked { get; set; }
        #endregion コマンド
    }
}
