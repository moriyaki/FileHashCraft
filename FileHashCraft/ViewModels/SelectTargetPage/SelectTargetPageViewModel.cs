/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */

using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    #region ハッシュ計算するファイルの取得状況

    public enum FileScanStatus
    {
        None,
        DirectoriesScanning,
        FilesScanning,
        Finished,
    }

    #endregion ハッシュ計算するファイルの取得状況

    #region インターフェース

    public interface ISelectTargetPageViewModel
    {
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
        double FilesListBoxWidth { get; set; }

        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムのリストボックスコレクションです。
        /// </summary>
        ObservableCollection<HashListFileItems> HashFileListItems { get; }

        /// <summary>
        /// ファイルスキャン状況
        /// </summary>
        FileScanStatus Status { get; set; }

        /// <summary>
        /// ファイルスキャン状況に合わせた背景色
        /// </summary>
        Brush StatusColor { get; set; }

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        string StatusMessage { get; set; }

        /// <summary>
        /// ハッシュアルゴリズム
        /// </summary>
        string SelectedHashAlgorithm { get; set; }

        /// <summary>
        /// 表示言語の変更に伴う対策をします。
        /// </summary>
        void LanguageChangedMeasures();

        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        int CountScannedDirectories { get; set; }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        int CountHashFilesDirectories { get; set; }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        int CountAllTargetFilesGetHash { get; set; }

        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        int CountFilteredGetHash { get; set; }

        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        RelayCommand ToHashCalcingPage { get; set; }

        /// <summary>
        /// 検索ステータスを変更します。
        /// </summary>
        void ChangeHashScanStatus(FileScanStatus status);

        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        void AddScannedDirectoriesCount(int count = 1);

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        void AddFilesScannedDirectoriesCount(int count = 1);

        /// <summary>
        /// 総対象ファイル数を設定します。
        /// </summary>
        void SetAllTargetfilesCount();

        /// <summary>
        /// スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        void SetTargetCountChanged();

        /// <summary>
        /// ツリービューの選択ディレクトリが変更された時の処理です。
        /// </summary>
        void ChangeCurrentPath(string currentFullPath);

        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        void ChangeSelectedToListBox();
    }

    #endregion インターフェース

    public partial class SelectTargetPageViewModel : BaseViewModel, ISelectTargetPageViewModel
    {
        #region バインディング

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
        /// ファイルスキャン状況
        /// </summary>
        private FileScanStatus _Status = FileScanStatus.None;

        public FileScanStatus Status
        {
            get => _Status;
            set
            {
                _Status = value;
                switch (value)
                {
                    case FileScanStatus.None:
                        StatusColor = Brushes.Pink;
                        break;

                    case FileScanStatus.DirectoriesScanning:
                        StatusColor = Brushes.Pink;
                        StatusMessage = $"{Resources.LabelDirectoryScanning} {CountScannedDirectories}";
                        break;

                    case FileScanStatus.FilesScanning:
                        StatusColor = Brushes.Yellow;
                        StatusMessage = $"{Resources.LabelDirectoryCount} ({CountHashFilesDirectories} / {CountScannedDirectories})";
                        break;

                    case FileScanStatus.Finished:
                        StatusColor = Brushes.LightGreen;
                        StatusMessage = Resources.LabelFinished;
                        _Messanger.Send(new FileScanFinished());
                        break;

                    default: // 異常
                        StatusColor = Brushes.Red;
                        break;
                }
            }
        }

        /// <summary>
        /// ファイルスキャン状況に合わせた背景色
        /// </summary>
        [ObservableProperty]
        private Brush _StatusColor = Brushes.LightGreen;

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        [ObservableProperty]
        private string _StatusMessage = string.Empty;

        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } = [];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        private string _SelectedHashAlgorithm;

        public string SelectedHashAlgorithm
        {
            get => _SelectedHashAlgorithm;
            set
            {
                if (value == _SelectedHashAlgorithm) return;
                SetProperty(ref _SelectedHashAlgorithm, value);
                _SettingsService.HashAlgorithm = value;
            }
        }

        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountScannedDirectories = 0;

        public int CountScannedDirectories
        {
            get => _CountScannedDirectories;
            set
            {
                SetProperty(ref _CountScannedDirectories, value);
                Status = FileScanStatus.DirectoriesScanning;
            }
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountHashFilesDirectories = 0;

        public int CountHashFilesDirectories
        {
            get => _CountHashFilesDirectories;
            set
            {
                SetProperty(ref _CountHashFilesDirectories, value);
                Status = FileScanStatus.FilesScanning;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        private int _CountAllTargetFilesGetHash = 0;

        public int CountAllTargetFilesGetHash
        {
            get => _CountAllTargetFilesGetHash;
            set
            {
                SetProperty(ref _CountAllTargetFilesGetHash, value);
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        private int _CountFilteredGetHash = 0;

        public int CountFilteredGetHash
        {
            get => _CountFilteredGetHash;
            set
            {
                SetProperty(ref _CountFilteredGetHash, value);
                ToHashCalcingPage.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムのリストボックスコレクションです。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; set; } = [];

        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        public RelayCommand ToHashCalcingPage { get; set; }

        /// <summary>
        /// フィルタするファイル
        /// </summary>
        [ObservableProperty]
        private string _FilterTextBox = string.Empty;

        /// <summary>
        /// リストボックスの幅を設定します
        /// </summary>
        private double _FilesListBoxWidth;

        public double FilesListBoxWidth
        {
            get => _FilesListBoxWidth;
            set
            {
                if (value == _FilesListBoxWidth) { return; }
                SetProperty(ref _FilesListBoxWidth, value);
                _SettingsService.FilesListBoxWidth = value;
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
        /// エクスプローラー画面に戻ります。
        /// </summary>
        public RelayCommand ToExplorerPage { get; set; }

        #endregion バインディング

        #region コンストラクタ

        private readonly IFileManager _FileManager;
        private readonly IFileSystemServices _FileSystemServices;
        private readonly IDirectoriesCollection _DirectoriesManager;
        private readonly IScanHashFiles _ScanHashFiles;
        private readonly IScannedFilesManager _ScannedFilesManager;
        private readonly IHelpWindowViewModel _HelpWindowViewModel;
        private readonly IControDirectoryTreeViewlModel _ControDirectoryTreeViewlViewModel;
        private readonly IHashAlgorithmHelper _HashAlgorithmHelper;
        private bool IsExecuting = false;

        public SelectTargetPageViewModel(
            ISetExtentionControlViewModel pageSelectTargetViewModelExtention,
            ISetWildcardControlViewModel pageSelectTargetViewModelWildcard,
            ISetRegexControlViewModel pageSelectTargetViewModelRegEx,
            ISetExpertControlViewModel pageSelectTargetViewModelExpert,
            IFileSystemServices fileSystemServices,
            IMessenger messenger,
            ISettingsService settingsService,
            IFileManager fileManager,
            IDirectoriesCollection directoriesManager,
            IScanHashFiles scanHashFiles,
            IScannedFilesManager scannedFilesManager,
            IHelpWindowViewModel helpWindowViewModel,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel,
            IHashAlgorithmHelper hashAlgorithmHelper
        ) : base(messenger, settingsService)
        {
            ViewModelExtention = pageSelectTargetViewModelExtention;
            ViewModelWildcard = pageSelectTargetViewModelWildcard;
            ViewModelRegEx = pageSelectTargetViewModelRegEx;
            ViewModelExpert = pageSelectTargetViewModelExpert;
            _FileManager = fileManager;
            _FileSystemServices = fileSystemServices;
            _DirectoriesManager = directoriesManager;
            _ScanHashFiles = scanHashFiles;
            _ScannedFilesManager = scannedFilesManager;
            _HelpWindowViewModel = helpWindowViewModel;
            _ControDirectoryTreeViewlViewModel = controDirectoryTreeViewlViewModel;
            _HashAlgorithmHelper = hashAlgorithmHelper;

            // カレントハッシュ計算アルゴリズムを保存
            //ViewModelMain.SelectedHashAlgorithm = _SettingsService.HashAlgorithm;
            HashAlgorithms =
[
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

            // ハッシュ計算画面に移動するコマンド
            ToHashCalcingPage = new RelayCommand(
                () => _FileSystemServices.NavigateToHashCalcingPage(),
                () => CountFilteredGetHash > 0
            );

            // カレントディレクトリが変更されたメッセージ受信
            _Messanger.Register<CurrentDirectoryChangedMessage>(this, (_, m)
                => ChangeCurrentPath(m.CurrentFullPath));

            // 拡張子チェックボックスのチェック状態が変更されたら、カレントディレクトリリストボックス変更
            _Messanger.Register<ExtentionCheckChangedToListBoxMessage>(this, (_, _)
                => ChangeSelectedToListBox());

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
            {
                IsExecuting = true;
                _FileSystemServices.NavigateToSettingsPage(ReturnPageEnum.SelecTargettPage);
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
                _HelpWindowViewModel.Initialize(HelpPage.Index);
            });
            // エクスプローラー風画面に移動するコマンド
            ToExplorerPage = new RelayCommand(() =>
            {
                CTS?.Cancel();
                _FileSystemServices.NavigateToExplorerPage();
            });

            // スキャンした全ディレクトリ数に加算するメッセージ
            _Messanger.Register<AddScannedDirectoriesCountMessage>(this, (_, m)
                => AddScannedDirectoriesCount(m.DirectoriesCount));

            // ファイルスキャンが完了したディレクトリ数に加算するメッセージ
            _Messanger.Register<AddFilesScannedDirectoriesCountMessage>(this, (_, _)
                => AddFilesScannedDirectoriesCount());

            // ハッシュ取得対象となる総対象ファイル数にファイル数を設定するメッセージ
            _Messanger.Register<SetAllTargetfilesCountMessge>(this, (_, _)
                => SetAllTargetfilesCount());

            // 拡張子をリストボックスに追加するメッセージ
            _Messanger.Register<AddExtentionMessage>(this, (_, m)
                => ViewModelExtention.AddExtention(m.Extention));

            // ファイルの拡張子グループをリストボックスに追加するメッセージ
            _Messanger.Register<AddFileTypesMessage>(this, (_, _)
                => ViewModelExtention.AddFileTypes());

            // 拡張子のチェック状態がされたらグループも変更する
            _Messanger.Register<ExtentionCheckReflectToGroupMessage>(this, (_, _)
                => SetTargetCountChanged());
            _Messanger.Register<ExtentionUncheckReflectToGroupMessage>(this, (_, _)
                => SetTargetCountChanged());

            // ファイル対象数の変更通知
            _Messanger.Register<ChangeSelectedCountMessage>(this, (_, _) =>
            {
                SetTargetCountChanged();
                ChangeSelectedToListBox();
            });
            _FilesListBoxWidth = _SettingsService.FilesListBoxWidth;
            _SelectedHashAlgorithm = _SettingsService.HashAlgorithm;
            OnPropertyChanged(nameof(SelectedHashAlgorithm));
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
            LanguageChangedMeasures();

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
                ChangeCurrentPath(_ControDirectoryTreeViewlViewModel.TreeRoot[0].FullPath);
                _ControDirectoryTreeViewlViewModel.TreeRoot[0].IsSelected = true;
            }
            catch { }
            // 既にファイル検索がされていて、ディレクトリ選択設定が変わっていなければ終了
            if (Status == FileScanStatus.Finished
             && _DirectoriesManager.NestedDirectories.Order().SequenceEqual(NestedDirectories.Order())
             && _DirectoriesManager.NonNestedDirectories.Order().SequenceEqual(NonNestedDirectories.Order()))
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
            _ControDirectoryTreeViewlViewModel.ClearRoot();
            _ControDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            // 該当以下のディレクトリを含むディレクトリのパスをツリービューに追加する。
            foreach (var root in _DirectoriesManager.NestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            // 該当ディレクトリのみを単独でツリービューに追加する。
            foreach (var root in _DirectoriesManager.NonNestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }

        /// <summary>
        /// 現在のディレクトリ選択設定を保存します。
        /// </summary>
        private void SaveCurrentDirectorySelectionSettings()
        {
            NestedDirectories.Clear();
            NestedDirectories.AddRange(_DirectoriesManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_DirectoriesManager.NonNestedDirectories);
        }

        /// <summary>
        /// 上級者向けの設定を反映する
        /// </summary>
        private void ReflectionExpertSettings()
        {
            ViewModelExpert.IsReadOnlyFileInclude = _SettingsService.IsReadOnlyFileInclude;
            ViewModelExpert.IsHiddenFileInclude = _SettingsService.IsHiddenFileInclude;
            ViewModelExpert.IsZeroSizeFileDelete = _SettingsService.IsZeroSizeFileDelete;
            ViewModelExpert.IsEmptyDirectoryDelete = _SettingsService.IsEmptyDirectoryDelete;
        }

        /// <summary>
        /// 必要な値の初期化をします。
        /// </summary>
        private void ResetInitializedValues()
        {
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                CountScannedDirectories = 0;
                CountHashFilesDirectories = 0;
                CountAllTargetFilesGetHash = 0;
                CountFilteredGetHash = 0;
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
            Status = FileScanStatus.None;
            ToHashCalcingPage.NotifyCanExecuteChanged();

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
            _ScannedFilesManager.AllFiles.Clear();
            _ScanHashFiles.DirectoriesHashSet.Clear();
            ViewModelExtention.ClearExtentions();

            try
            {
                ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await _ScanHashFiles.DirectoriesScan(cancellation);

                ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => _ScanHashFiles.DirectoryFilesScan(cancellation), cancellation);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            _ScanHashFiles.ScanExtention(cancellation);

            // スキャン終了の表示に切り替える
            ChangeHashScanStatus(FileScanStatus.Finished);

            /*
            //--------------------- 開発用自動化処理
            App.Current?.Dispatcher.InvokeAsync(() => ToHashCalcingPage.Execute(this));
            */
        }

        #endregion メイン処理

        /// <summary>
        /// 表示言語の変更に伴う対策をします。
        /// </summary>
        public void LanguageChangedMeasures()
        {
            var currentAlgorithm = _SettingsService.HashAlgorithm;

            HashAlgorithms.Clear();
            HashAlgorithms =
            [
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(_HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

            // ハッシュ計算アルゴリズムを再設定
            SelectedHashAlgorithm = currentAlgorithm;
            OnPropertyChanged(nameof(HashAlgorithms));
        }

        #region ファイル数の管理処理

        /// <summary>
        /// 検索のステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public void ChangeHashScanStatus(FileScanStatus status)
        {
            App.Current?.Dispatcher?.Invoke(() => Status = status);
        }

        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddScannedDirectoriesCount(int directoriesCount)
        {
            App.Current?.Dispatcher?.Invoke(() => CountScannedDirectories += directoriesCount);
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddFilesScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.Invoke(() => CountHashFilesDirectories += directoriesCount);
        }

        /// <summary>
        /// ハッシュ取得対象となる総対象ファイル数にファイル数を設定します
        /// </summary>
        public void SetAllTargetfilesCount()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            CountAllTargetFilesGetHash = _ScannedFilesManager.GetAllFilesCount(_SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude));
        }

        /// <summary>
        /// スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public void SetTargetCountChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            CountFilteredGetHash = _ScannedFilesManager.GetAllCriteriaFilesCount(_SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude));
        }

        #endregion ファイル数の管理処理

        #region ツリービューのカレントディレクトリ、リストビューの色変え処理

        /// <summary>
        /// ツリービューの選択ディレクトリが変更された時の処理です。
        /// </summary>
        /// <param name="currentFullPath">カレントディレクトリ</param>
        /// <exception cref="NullReferenceException">IFileManagerが取得できなかった時の例外</exception>
        public void ChangeCurrentPath(string currentFullPath)
        {
            App.Current?.Dispatcher?.Invoke(() => HashFileListItems.Clear());

            foreach (var file in _FileManager.EnumerateFiles(currentFullPath))
            {
                var item = new HashListFileItems
                {
                    FileFullPath = file,
                    //IsHashTarget = _ScannedFilesManager.GetAllCriteriaFileName().Any(f => f.FileFullPath == file)
                    IsHashTarget = _ScannedFilesManager.IsCriteriaFile(file, _SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude),
                };
                App.Current?.Dispatcher?.Invoke(() => HashFileListItems.Add(item));
            }
        }

        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        public void ChangeSelectedToListBox()
        {
            foreach (var item in HashFileListItems)
            {
                App.Current?.Dispatcher?.Invoke(() =>
                    item.IsHashTarget = _ScannedFilesManager.IsCriteriaFile(item.FileFullPath, _SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude));
            }
        }

        #endregion ツリービューのカレントディレクトリ、リストビューの色変え処理
    }
}