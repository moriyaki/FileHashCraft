using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
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
        private bool _isReadOnlyFileInclude;
        public bool IsReadOnlyFileInclude
        {
            get => _isReadOnlyFileInclude;
            set
            {
                if (_isReadOnlyFileInclude == value) return;
                SetProperty(ref _isReadOnlyFileInclude, value);
                _settingsService.SendReadOnlyFileInclude(value);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _isHiddenFileInclude;
        public bool IsHiddenFileInclude
        {
            get => _isHiddenFileInclude;
            set
            {
                if (_isHiddenFileInclude == value) return;
                SetProperty(ref _isHiddenFileInclude, value);
                _settingsService.SendHiddenFileInclude(value);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _isZeroSizeFileDelete;
        public bool IsZeroSizeFileDelete
        {
            get => _isZeroSizeFileDelete;
            set
            {
                if (_isZeroSizeFileDelete == value) return;
                SetProperty(ref _isZeroSizeFileDelete, value);
                _settingsService.SendZeroSizeFileDelete(value);
            }
        }
        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _isEmptyDirectoryDelete;
        public bool IsEmptyDirectoryDelete
        {
            get => _isEmptyDirectoryDelete;
            set
            {
                if (_isEmptyDirectoryDelete == value) return;
                SetProperty(ref _isEmptyDirectoryDelete, value);
                _settingsService.SendEmptyDirectoryDelete(value);
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
        //private readonly ISettingsService _settingsService;
        private readonly IShowTargetInfoUserControlViewModel _pageSelectTargetViewModelMain;
        public SetExpertControlViewModel(
            ISettingsService settingsService,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(settingsService)
        {
            //_settingsService = settingsService;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;

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

            // 読み取り専用ファイルを利用するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<ReadOnlyFileIncludeChangedMessage>(this, (_, m)
                => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<HiddenFileIncludeChangedMessage>(this, (_, m)
                => IsHiddenFileInclude = m.HiddenFileInclude);

            // 0サイズファイルを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<ZeroSizeFileDeleteChangedMessage>(this, (_, m)
                => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);

            //空ディレクトリを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<EmptyDirectoryDeleteChangedMessage>(this, (_, m)
                => IsEmptyDirectoryDelete = m.EmptyDirectoryDelete);
        }
        #endregion コンストラクタ
    }
}
