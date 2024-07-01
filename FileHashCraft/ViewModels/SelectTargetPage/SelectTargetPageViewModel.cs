/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface ISelectTargetPageViewModel
    {
        /// <summary>
        /// PageSelectTargetViewModelのメインViewModel
        /// </summary>
        IShowTargetInfoUserControlViewModel ViewModelMain { get; }
        /// <summary>
        /// PageSelectTargetViewModelの拡張子ViewModel
        /// </summary>
        ISetExtentionControlViewModel ViewModelExtention { get; }
        /// <summary>
        /// PageSelectTargetViewModelのワイルドカードViewModel
        /// </summary>
        ISetWildcardControlViewModel ViewModelWildcard { get; }

        /// <summary>
        ///  PageSelectTargetViewModelの正規表現ViewModel
        /// </summary>
        ISetRegexControlViewModel ViewModelRegEx { get; }

        /// <summary>
        /// PageSelectTargetViewModelExpert 上級者向け設定のViewModel
        /// </summary>
        ISetExpertControlViewModel ViewModelExpert { get; }
        /// <summary>
        /// 他ページから移動してきた時の初期化処理をします。
        /// </summary>
        void Initialize();
        /// <summary>
        /// リストボックスの幅
        /// </summary>
        double ListWidth { get; set; }
        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        int CountFilteredGetHash { get; }
    }
    #endregion インターフェース

    public partial class SelectTargetPageViewModel : BaseViewModel, ISelectTargetPageViewModel
    {
        #region バインディング
        /// <summary>
        /// PageSelectTargetViewModelのメインViewModel
        /// </summary>
        public IShowTargetInfoUserControlViewModel ViewModelMain { get; }

        /// <summary>
        /// PageSelectTargetViewModelの拡張子ViewModel
        /// </summary>
        public ISetExtentionControlViewModel ViewModelExtention { get; }

        /// <summary>
        ///  PageSelectTargetViewModelのワイルドカードViewModel
        /// </summary>
        public ISetWildcardControlViewModel ViewModelWildcard { get; }

        /// <summary>
        ///  PageSelectTargetViewModelの正規表現ViewModel
        /// </summary>
        public ISetRegexControlViewModel ViewModelRegEx { get; }

        /// <summary>
        /// PageSelectTargetViewModelExpert 上級者向け設定のViewModel
        /// </summary>
        public ISetExpertControlViewModel ViewModelExpert { get; }

        /// <summary>
        /// フィルタするファイル
        /// </summary>
        [ObservableProperty]
        private string _FilterTextBox = string.Empty;

        /// <summary>
        /// ツリービュー横幅の設定
        /// </summary>
        private double _TreeWidth;
        public double TreeWidth
        {
            get => _TreeWidth;
            set
            {
                if (value == _TreeWidth) { return; }
                SetProperty(ref _TreeWidth, value);
                _settingsService.SendTreeWidth(value);
            }
        }
        /// <summary>
        /// リストボックスの幅を設定します
        /// </summary>
        private double _ListWidth;
        public double ListWidth
        {
            get => _ListWidth;
            set
            {
                if (value == _ListWidth) { return; }
                SetProperty(ref _ListWidth, value);
                _settingsService.SendListWidth(value);
            }
        }
        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        public int CountFilteredGetHash { get => ViewModelMain.CountFilteredGetHash; }
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
        /// エクスプローラー画面に戻ります。
        /// </summary>
        public RelayCommand ToExplorerPage { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IScanHashFiles _scanHashFiles;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IControDirectoryTreeViewlModel _controDirectoryTreeViewlViewModel;
        private bool IsExecuting = false;

        public SelectTargetPageViewModel(
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain,
            ISetExtentionControlViewModel pageSelectTargetViewModelExtention,
            ISetWildcardControlViewModel pageSelectTargetViewModelWildcard,
            ISetRegexControlViewModel pageSelectTargetViewModelRegEx,
            ISetExpertControlViewModel pageSelectTargetViewModelExpert,
            IFileSystemServices fileSystemServices,
            IMessenger messenger,
            ISettingsService settingsService,
            IDirectoriesManager directoriesManager,
            IScanHashFiles scanHashFiles,
            IScannedFilesManager scannedFilesManager,
            IHelpWindowViewModel helpWindowViewModel,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel
        ) : base(messenger, settingsService)
        {
            ViewModelMain = pageSelectTargetViewModelMain;
            ViewModelExtention = pageSelectTargetViewModelExtention;
            ViewModelWildcard = pageSelectTargetViewModelWildcard;
            ViewModelRegEx = pageSelectTargetViewModelRegEx;
            ViewModelExpert = pageSelectTargetViewModelExpert;
            _fileSystemServices = fileSystemServices;
            _directoriesManager = directoriesManager;
            _scanHashFiles = scanHashFiles;
            _scannedFilesManager = scannedFilesManager;
            _helpWindowViewModel = helpWindowViewModel;
            _controDirectoryTreeViewlViewModel = controDirectoryTreeViewlViewModel;

            // カレントハッシュ計算アルゴリズムを保存
            ViewModelMain.SelectedHashAlgorithm = _settingsService.HashAlgorithm;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
            {
                IsExecuting = true;
                _fileSystemServices.NavigateToSettingsPage(ReturnPageEnum.SelecTargettPage);
            });
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
            // エクスプローラー風画面に移動するコマンド
            ToExplorerPage = new RelayCommand(() =>
            {
                CTS?.Cancel();
                _fileSystemServices.NavigateToExplorerPage();
            });

            // スキャンした全ディレクトリ数に加算するメッセージ
            _messenger.Register<AddScannedDirectoriesCountMessage>(this, (_, m)
                => ViewModelMain.AddScannedDirectoriesCount(m.DirectoriesCount));

            // 全管理対象ファイルを追加するメッセージ
            _messenger.Register<AddFileToAllFilesMessage>(this, (_, m)
                => ViewModelExtention.AddFileToAllFiles(m.FileFullPath));

            // ファイルスキャンが完了したディレクトリ数に加算するメッセージ
            _messenger.Register<AddFilesScannedDirectoriesCountMessage>(this, (_, _)
                => ViewModelMain.AddFilesScannedDirectoriesCount());

            // ハッシュ取得対象となる総対象ファイル数にファイル数を設定するメッセージ
            _messenger.Register<SetAllTargetfilesCountMessge>(this, (_, _)
                => ViewModelMain.SetAllTargetfilesCount());

            // 拡張子をリストボックスに追加するメッセージ
            _messenger.Register<AddExtentionMessage>(this, (_, m)
                => ViewModelExtention.AddExtention(m.Extention));

            // ファイルの拡張子グループをリストボックスに追加するメッセージ
            _messenger.Register<AddFileTypesMessage>(this, (_, _)
                => ViewModelExtention.AddFileTypes());

            // ツリービュー幅変更メッセージ受信
            _messenger.Register<TreeWidthChangedMessage>(this, (_, m)
                => TreeWidth = m.TreeWidth);

            // リストボックス幅変更メッセージ受信
            _messenger.Register<ListWidthChangedMessage>(this, (_, m)
                => ListWidth = m.ListWidth);

            _TreeWidth = _settingsService.TreeWidth;
            _ListWidth = _settingsService.ListWidth;
            ViewModelMain.SelectedHashAlgorithm= _settingsService.HashAlgorithm;
        }
        #endregion コンストラクタ

        #region 初期処理
        public CancellationTokenSource? CTS;
        public CancellationToken cancellationToken;

        private readonly List<string> NestedDirectories = [];
        private readonly List<string> NonNestedDirectories = [];

        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize()
        {
            // 言語変更に伴う対策
            ViewModelMain.LanguageChangedMeasures();

            // 言語が変わった場合に備えて、拡張子グループを再設定
            ViewModelExtention.RefreshExtentionLanguage();

            // 設定画面から戻ってきた場合、処理を終了する
            if (IsExecuting)
            {
                IsExecuting = false;
                return;
            }

            // ツリービューのアイテムを初期化する
            InitializeTreeView();
            try
            {
                ViewModelMain.ChangeCurrentPath(_controDirectoryTreeViewlViewModel.TreeRoot[0].FullPath);
                _controDirectoryTreeViewlViewModel.TreeRoot[0].IsSelected = true;
            }
            catch { }
            // 既にファイル検索がされていて、ディレクトリ選択設定が変わっていなければ終了
            if (ViewModelMain.Status == FileScanStatus.Finished
             && _directoriesManager.NestedDirectories.OrderBy(x => x).SequenceEqual(NestedDirectories.OrderBy(x => x))
             && _directoriesManager.NonNestedDirectories.OrderBy(x => x).SequenceEqual(NonNestedDirectories.OrderBy(x => x)))
            {
                return;
            }

            // 現在のディレクトリ選択設定を保存する
            SaveCurrentDirectorySelectionSettings();

            // 上級者向け設定の反映をする
            ReflectionExpertSettings();

            // 状況が変わっているので、必要な値の初期化をする
            ResetInitializedValues();

            // ファイルスキャンの開始
            StartFileScan();
        }

        /// <summary>
        /// ツリービューの初期化をします。
        /// </summary>
        private void InitializeTreeView()
        {
            _controDirectoryTreeViewlViewModel.ClearRoot();
            _controDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            // 該当以下のディレクトリを含むディレクトリのパスをツリービューに追加する。
            foreach (var root in _directoriesManager.NestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            // 該当ディレクトリのみを単独でツリービューに追加する。
            foreach (var root in _directoriesManager.NonNestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }

        /// <summary>
        /// 現在のディレクトリ選択設定を保存します。
        /// </summary>
        private void SaveCurrentDirectorySelectionSettings()
        {
            NestedDirectories.Clear();
            NestedDirectories.AddRange(_directoriesManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_directoriesManager.NonNestedDirectories);
        }

        /// <summary>
        /// 上級者向けの設定を反映する
        /// </summary>
        private void ReflectionExpertSettings()
        {
            ViewModelExpert.IsReadOnlyFileInclude = _settingsService.IsReadOnlyFileInclude;
            ViewModelExpert.IsHiddenFileInclude = _settingsService.IsHiddenFileInclude;
            ViewModelExpert.IsZeroSizeFileDelete = _settingsService.IsZeroSizeFileDelete;
            ViewModelExpert.IsEmptyDirectoryDelete = _settingsService.IsEmptyDirectoryDelete;
        }

        /// <summary>
        /// 必要な値の初期化をします。
        /// </summary>
        private void ResetInitializedValues()
        {
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                ViewModelMain.CountScannedDirectories = 0;
                ViewModelMain.CountHashFilesDirectories = 0;
                ViewModelMain.CountAllTargetFilesGetHash = 0;
                ViewModelMain.CountFilteredGetHash = 0;
                ViewModelExtention.ExtentionCollection.Clear();
                ViewModelExtention.ExtentionsGroupCollection.Clear();
            });
        }

        /// <summary>
        /// チェックされたディレクトリのファイルスキャンをします。
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void StartFileScan()
        {
            CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;

            // 移動ボタンの利用状況を設定
            ViewModelMain.Status = FileScanStatus.None;
            ViewModelMain.ToHashCalcingPage.NotifyCanExecuteChanged();

            // スキャンするディレクトリの追加
            Task.Run(() => ScanFiles(cancellationToken));
        }
        #endregion 初期処理

        #region メイン処理
        /// <summary>
        /// スキャンするファイルを検出します。
        /// </summary>
        public async Task ScanFiles(CancellationToken cancellation)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _scannedFilesManager.AllFiles.Clear();
            _scanHashFiles.DirectoriesHashSet.Clear();
            ViewModelExtention.ClearExtentions();

            try
            {
                // ディレクトリのスキャン
                ViewModelMain.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await _scanHashFiles.DirectoriesScan(cancellation);

                // ファイルのスキャン
                ViewModelMain.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => _scanHashFiles.DirectoryFilesScan(cancellation), cancellation);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            _scanHashFiles.ScanExtention(cancellation);

            // スキャン終了の表示に切り替える
            ViewModelMain.ChangeHashScanStatus(FileScanStatus.Finished);

            //--------------------- 開発用自動化処理
            App.Current?.Dispatcher.InvokeAsync(() =>
            {
                ViewModelWildcard.SearchCriteriaText = "*";
                ViewModelWildcard.AddCriteria();
                ViewModelMain.ToHashCalcingPage.Execute(this);
            });
        }
        #endregion メイン処理

    }
}
