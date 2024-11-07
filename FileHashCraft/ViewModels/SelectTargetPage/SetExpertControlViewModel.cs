using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public interface ISetExpertControlViewModel
    {
        bool IsReadOnlyFileInclude { get; set; }
        bool IsHiddenFileInclude { get; set; }
        bool IsZeroSizeFileDelete { get; set; }
        bool IsEmptyDirectoryDelete { get; set; }
    }

    public class SetExpertControlViewModel : BaseViewModel, ISetExpertControlViewModel
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
                if (_IsReadOnlyFileInclude == value) return;
                SetProperty(ref _IsReadOnlyFileInclude, value);
                _SettingsService.IsReadOnlyFileInclude = value;
                _Messanger.Send(new ChangeSelectedCountMessage());
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
                if (_IsHiddenFileInclude == value) return;
                SetProperty(ref _IsHiddenFileInclude, value);
                _SettingsService.IsReadOnlyFileInclude = value;
                _Messanger.Send(new ChangeSelectedCountMessage());
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
                if (_IsZeroSizeFileDelete == value) return;
                SetProperty(ref _IsZeroSizeFileDelete, value);
                _SettingsService.IsReadOnlyFileInclude = value;
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
                if (_IsEmptyDirectoryDelete == value) return;
                SetProperty(ref _IsEmptyDirectoryDelete, value);
                _SettingsService.IsReadOnlyFileInclude = value;
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

        #region コンストラクタ

        public SetExpertControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        ) : base(messenger, settingsService)
        {
            // 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsReadOnlyFileIncludeClicked = new RelayCommand(()
                => IsReadOnlyFileInclude = !IsReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsHiddenFileIncludeClicked = new RelayCommand(()
                => IsHiddenFileInclude = !IsHiddenFileInclude);

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new RelayCommand(()
                => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new RelayCommand(()
                => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);
        }

        #endregion コンストラクタ
    }
}