using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    public interface IDuplicateSelectPageViewModel
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();
        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        double DupFilesDirsListBoxWidth { get; set; }
    }
    public class DuplicateSelectPageViewModel : BaseViewModel, IDuplicateSelectPageViewModel
    {
        #region バインディング

        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        private double _DupFilesDirsListBoxWidth;
        public double DupFilesDirsListBoxWidth
        {
            get => _DupFilesDirsListBoxWidth;
            set
            {
                if (_DupFilesDirsListBoxWidth == value) return;
                SetProperty(ref _DupFilesDirsListBoxWidth, value);
                _settingsService.DupDirsFilesTreeViewWidth = value;
            }
        }

        /// <summary>
        /// 設定画面を開きます。
        /// </summary>
        public RelayCommand SettingsOpen { get; set; }
        /// <summary>
        /// デバッグウィンドウを開きます。
        /// </summary>
        public RelayCommand DebugOpen { get; set; }
        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public RelayCommand HelpOpen { get; set; }
        /// <summary>
        /// ファイル選択画面に戻ります。
        /// </summary>
        public RelayCommand ToSelectTargetPage { get; set; }
        /// <summary>
        /// 削除を実行します。
        /// </summary>
        public RelayCommand DeleteCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IDupFilesDirsListBoxViewModel _dupDirsFilesTreeViewControlViewModel;
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IHelpWindowViewModel _helpWindowViewModel;

        public DuplicateSelectPageViewModel() { throw new NotImplementedException(nameof(DuplicateSelectPageViewModel)); }

        public DuplicateSelectPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IDupFilesDirsListBoxViewModel dupFilesDirsListBoxControlViewModel,
            IFileSystemServices fileSystemServices,
            IHelpWindowViewModel helpWindowViewModel
        ) : base(messenger, settingsService)
        {
            _dupDirsFilesTreeViewControlViewModel = dupFilesDirsListBoxControlViewModel;
            _fileSystemServices = fileSystemServices;
            _helpWindowViewModel = helpWindowViewModel;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(()
                => _fileSystemServices.NavigateToSettingsPage(ReturnPageEnum.SelecTargettPage));
            // デバッグウィンドウを開くコマンド
            DebugOpen = new RelayCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });
            // ヘルプウィンドウを開くコマンド
            HelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Index);
            });
            // 削除コマンド
            DeleteCommand = new RelayCommand(()
                => System.Windows.MessageBox.Show("まだだよ"));

            ToSelectTargetPage = new RelayCommand(() =>
                _fileSystemServices.NavigateToSelectTargetPage());

            DupFilesDirsListBoxWidth = settingsService.DupFilesDirsListBoxWidth;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            _dupDirsFilesTreeViewControlViewModel.Initialize();
        }
        #endregion コンストラクタ
    }
}
