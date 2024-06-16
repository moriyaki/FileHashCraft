/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.ControlDirectoryTree;
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

    public class SelectTargetPageViewModel : BaseViewModel, ISelectTargetPageViewModel
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
        private string _FilterTextBox = string.Empty;
        public string FilterTextBox
        {
            get => _FilterTextBox;
            set => SetProperty(ref _FilterTextBox, value);
        }
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
        public RelayCommand ToPageExplorer { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileSystemServices _fileSystemService;
        private readonly ITreeManager _directoryTreeManager;
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
            ISettingsService settingsService,
            ITreeManager directoryTreeManager,
            IHelpWindowViewModel helpWindowViewModel,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel
        ) : base(settingsService)
        {
            ViewModelMain = pageSelectTargetViewModelMain;
            ViewModelExtention = pageSelectTargetViewModelExtention;
            ViewModelWildcard = pageSelectTargetViewModelWildcard;
            ViewModelRegEx = pageSelectTargetViewModelRegEx;
            ViewModelExpert = pageSelectTargetViewModelExpert;
            _fileSystemService = fileSystemServices;
            _directoryTreeManager = directoryTreeManager;
            _helpWindowViewModel = helpWindowViewModel;
            _controDirectoryTreeViewlViewModel = controDirectoryTreeViewlViewModel;

            // カレントハッシュ計算アルゴリズムを保存
            ViewModelMain.SelectedHashAlgorithm = _settingsService.HashAlgorithm;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
            {
                IsExecuting = true;
                _fileSystemService.SendToSettingsPage(ReturnPageEnum.PageTargetSelect);
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
            ToPageExplorer = new RelayCommand(() =>
            {
                CTS?.Cancel();
                _fileSystemService.SendToExplorerPage();
            });

            // ツリービュー幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<TreeWidthChanged>(this, (_, m)
                => TreeWidth = m.TreeWidth);

            // リストボックス幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<ListWidthChanged>(this, (_, m)
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
             && _directoryTreeManager.NestedDirectories.OrderBy(x => x).SequenceEqual(NestedDirectories.OrderBy(x => x))
             && _directoryTreeManager.NonNestedDirectories.OrderBy(x => x).SequenceEqual(NonNestedDirectories.OrderBy(x => x)))
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
            foreach (var root in _directoryTreeManager.NestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            // 該当ディレクトリのみを単独でツリービューに追加する。
            foreach (var root in _directoryTreeManager.NonNestedDirectories)
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
            NestedDirectories.AddRange(_directoryTreeManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_directoryTreeManager.NonNestedDirectories);
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
            var scanHashFilesClass = Ioc.Default.GetService<IScanHashFiles>() ?? throw new InvalidOperationException($"{nameof(IScanHashFiles)} dependency not resolved.");
            CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;

            // 移動ボタンの利用状況を設定
            ViewModelMain.Status = FileScanStatus.None;
            ViewModelMain.ToPageHashCalcing.NotifyCanExecuteChanged();

            // スキャンするディレクトリの追加
            scanHashFilesClass.ScanFiles(cancellationToken);
        }
        #endregion 初期処理

    }
}
