using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IPageSelectTargetViewModelExpert
    {
        bool IsReadOnlyFileInclude { get; set; }
        bool IsHiddenFileInclude { get; set; }
        bool IsZeroSizeFileDelete { get; set; }
        bool IsEmptyDirectoryDelete { get; set; }
    }

    public class PageSelectTargetViewModelExpert : BaseViewModel, IPageSelectTargetViewModelExpert
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
                _settingsService.SendReadOnlyFileInclude(value);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelExtention.ExtentionCountChanged();
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
                _settingsService.SendHiddenFileInclude(value);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelExtention.ExtentionCountChanged();
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
                _settingsService.SendZeroSizeFileDelete(value);
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
        private readonly IPageSelectTargetViewModelMain _pageSelectTargetViewModelMain;
        private readonly IPageSelectTargetViewModelExtention _pageSelectTargetViewModelExtention;
        public PageSelectTargetViewModelExpert(
            ISettingsService settingsService,
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain,
            IPageSelectTargetViewModelExtention pageSelectTargetViewModelExtention
        ) : base(settingsService)
        {
            //_settingsService = settingsService;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;
            _pageSelectTargetViewModelExtention = pageSelectTargetViewModelExtention;

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
            WeakReferenceMessenger.Default.Register<ReadOnlyFileIncludeChanged>(this, (_, m)
                => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<HiddenFileIncludeChanged>(this, (_, m)
                => IsHiddenFileInclude = m.HiddenFileInclude);

            // 0サイズファイルを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<ZeroSizeFileDeleteChanged>(this, (_, m)
                => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);

            //空ディレクトリを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<EmptyDirectoryDeleteChanged>(this, (_, m)
                => IsEmptyDirectoryDelete = m.EmptyDirectoryDelete);
        }
        #endregion コンストラクタ
    }
}
