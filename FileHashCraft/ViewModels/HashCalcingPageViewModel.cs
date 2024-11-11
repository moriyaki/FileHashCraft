using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Resources;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.HashCalcingPage
{
    #region ハッシュ計算の進行状況

    public enum FileHashCalcStatus
    {
        None,
        FileCalcing,
        FileMatching,
        Finished,
    }

    #endregion ハッシュ計算の進行状況

    #region インターフェース

    public interface IHashCalcingPageViewModel
    {
        /// <summary>
        /// 初期化処理
        /// </summary>
        Task InitializeAsync();
    }

    #endregion インターフェース

    public partial class HashCalcingPageViewModel : BaseViewModel, IHashCalcingPageViewModel
    {
        #region バインディング

        /// <summary>
        /// メニュー「設定」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string MenuSettings { get => ResourceService.GetString("MenuSettings"); }

        /// <summary>
        /// メニュー「ヘルプ」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string MenuHelp { get => ResourceService.GetString("MenuHelp"); }

        /// <summary>
        /// ラベル「Hash Algorithm」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelShowTargetInfo_HashAlgorithm { get => ResourceService.GetString("LabelShowTargetInfo_HashAlgorithm"); }

        /// <summary>
        /// ラベル「All HashGet Files Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_AllTargetFilesCount { get => ResourceService.GetString("LabelShowTargetInfo_HashAlgorithm"); }

        /// <summary>
        /// ラベル「All Hash Need ToGet Files Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_AllHashNeedToGetFilesCount { get => ResourceService.GetString("LabelHashCalcing_AllHashNeedToGetFilesCount"); }

        /// <summary>
        /// ラベル「Hash Got Files Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_HashGotFileCount { get => ResourceService.GetString("LabelHashCalcing_HashGotFileCount"); }

        /// <summary>
        /// ラベル「Match Hash Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_HashGotPercent { get => ResourceService.GetString("LabelHashCalcing_HashGotPercent"); }

        /// <summary>
        /// ラベル「Duplicate Hash Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_MatchHashCount { get => ResourceService.GetString("LabelHashCalcing_MatchHashCount"); }

        /// <summary>
        /// ラベル「Match Hash Count」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string LabelHashCalcing_ProcessingFileName { get => ResourceService.GetString("LabelHashCalcing_ProcessingFileName"); }

        /// <summary>
        /// ラベル「キャンセル」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string ButtonCancel { get => ResourceService.GetString("ButtonCancel"); }

        /// <summary>
        /// ラベル「Delete duplicate files」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string ButtonDupDelete { get => ResourceService.GetString("ButtonDupDelete"); }

        /// <summary>
        /// ファイルハッシュ計算の進行状況
        /// </summary>
        private FileHashCalcStatus _Status = FileHashCalcStatus.None;

        public FileHashCalcStatus Status
        {
            get => _Status;
            set
            {
                _Status = value;
                switch (value)
                {
                    case FileHashCalcStatus.None:
                        StatusColor = Brushes.Pink;
                        break;

                    case FileHashCalcStatus.FileCalcing:
                        StatusColor = Brushes.Pink;
                        StatusMessage = ResourceService.GetString("LabelFileCalcing");
                        break;

                    case FileHashCalcStatus.FileMatching:
                        StatusColor = Brushes.Yellow;
                        StatusMessage = ResourceService.GetString("LabelFileMatching");
                        break;

                    case FileHashCalcStatus.Finished:
                        StatusColor = Brushes.LightGreen;
                        StatusMessage = ResourceService.GetString("LabelFinished");
                        break;

                    default:
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
        /// 総対象ファイル数
        /// </summary>
        [ObservableProperty]
        private int _AllTargetFilesCount = 0;

        /// <summary>
        /// ハッシュを取得する全てのファイル数
        /// </summary>
        [ObservableProperty]
        private int _AllHashNeedToGetFilesCount = 0;

        /// <summary>
        /// ハッシュ計算のアルゴリズム
        /// </summary>
        [ObservableProperty]
        private string _HashAlgorithm = string.Empty;

        /// <summary>
        /// ハッシュ取得済みのファイル数
        /// </summary>
        private int _HashGotFileCount = 0;

        public int HashGotFileCount
        {
            get => _HashGotFileCount;
            set
            {
                SetProperty(ref _HashGotFileCount, value);
                OnPropertyChanged(nameof(HashGotPercent));
            }
        }

        /// <summary>
        /// ハッシュ取得状況のパーセンテージ
        /// </summary>
        public double HashGotPercent
        {
            get
            {
                if (AllHashNeedToGetFilesCount == 0)
                {
                    return 0d;
                }
                else
                {
                    return (double)HashGotFileCount / AllHashNeedToGetFilesCount * 100;
                }
            }
        }

        /// <summary>
        /// ファイルが同一だった件数
        /// </summary>
        [ObservableProperty]
        private double _MatchHashCount = 0;

        /// <summary>
        /// 計算中のハッシュファイル
        /// </summary>
        public ObservableCollection<string> CalcingFiles { get; set; } = [];

        /// <summary>
        /// 設定画面を開く
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
        /// 重複ファイル削除画面に移動します。
        /// </summary>
        public RelayCommand ToDupDeletePage { get; set; }

        #endregion バインディング

        #region コンストラクタと初期化

        private readonly IFileHashCalc _FileHashCalc;
        private readonly IScannedFilesManager _ScannedFilesManager;
        private readonly IHelpWindowViewModel _HelpWindowViewModel;
        private readonly IFileSystemServices _FileSystemServices;
        private readonly IDuplicateFilesManager _DuplicateFilesManager;

        public HashCalcingPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileHashCalc fileHashCalc,
            IHelpWindowViewModel helpWindowViewModel,
            IScannedFilesManager scannedFilesManager,
            IFileSystemServices fileSystemServices,
            IDuplicateFilesManager duplicateFilesManager
        ) : base(messenger, settingsService)
        {
            _FileHashCalc = fileHashCalc;
            _ScannedFilesManager = scannedFilesManager;
            _FileSystemServices = fileSystemServices;
            _HelpWindowViewModel = helpWindowViewModel;
            _DuplicateFilesManager = duplicateFilesManager;

            HashAlgorithm = _SettingsService.HashAlgorithm;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
                _FileSystemServices.NavigateToSettingsPage(ReturnPageEnum.HashCalcingPage));

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
            // ファイル選択画面に戻るコマンド
            ToSelectTargetPage = new RelayCommand(() =>
                _FileSystemServices.NavigateToSelectTargetPage());

            ToDupDeletePage = new RelayCommand(
                () => _FileSystemServices.NavigateToDuplicateSelectPage(),
                () => MatchHashCount > 0
            );

            // ドライブの追加
            _Messanger.Register<CalcingDriveMessage>(this, (_, m) =>
            {
                foreach (var drive in m.Drives)
                {
                    App.Current?.Dispatcher?.Invoke(() => CalcingFiles.Add(drive + " All Finished."));
                }
            });

            // ハッシュ計算を開始したメッセージ
            _Messanger.Register<StartCalcingFileMessage>(this, (_, m) =>
            {
                var drive = Path.GetPathRoot(m.CalcingFile) ?? "";
                for (int i = 0; i < CalcingFiles.Count; i++)
                {
                    if (CalcingFiles[i].StartsWith(drive))
                    {
                        App.Current?.Dispatcher.Invoke(() => CalcingFiles[i] = m.CalcingFile);
                        break;
                    }
                }
                HashGotFileCount++;
            });

            // ハッシュ計算を終了したメッセージ
            _Messanger.Register<EndCalcingFileMessage>(this, (_, m) =>
            {
                if (string.IsNullOrEmpty(m.CalcingFile)) { return; }

                var drive = Path.GetPathRoot(m.CalcingFile) ?? "";
                for (int i = 0; i < CalcingFiles.Count; i++)
                {
                    if (CalcingFiles[i].StartsWith(drive))
                    {
                        App.Current?.Dispatcher.Invoke(() => CalcingFiles[i] = drive + " All Finished.");
                        break;
                    }
                }
            });

            // ハッシュ計算全終了メッセージ
            _Messanger.Register<FinishedCalcingFileMessage>(this, (_, _)
                => HashGotFileCount = AllHashNeedToGetFilesCount);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public async Task InitializeAsync()
        {
            // 計算結果のクリア
            App.Current?.Dispatcher?.Invoke(() => CalcingFiles.Clear());

            // ファイルハッシュの計算
            AllTargetFilesCount = _ScannedFilesManager.GetAllCriteriaFileName(_SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude).Count;
            Status = FileHashCalcStatus.FileCalcing;
            var dupFileCandidate = _FileHashCalc.GetHashDriveFiles();
            foreach (var fileInDrive in dupFileCandidate)
            {
                AllHashNeedToGetFilesCount += fileInDrive.Value.Count;
            }
            await _FileHashCalc.ProcessGetHashFilesAsync(dupFileCandidate);
            App.Current?.Dispatcher?.Invoke(() => Status = FileHashCalcStatus.FileMatching);

            // 同一ファイルの抽出と格納
            var duplicateFiles = dupFileCandidate.Values
                .SelectMany(hashSet => hashSet)
                .Where(file => !string.IsNullOrEmpty(file.FileHash))
                .GroupBy(file => new { file.FileSize, file.FileHash })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            _DuplicateFilesManager.AddDuplicateFiles(duplicateFiles);

            // 
            App.Current?.Dispatcher?.Invoke(() =>
            {
                Status = FileHashCalcStatus.Finished;
                MatchHashCount = duplicateFiles.Count;
                ToDupDeletePage.NotifyCanExecuteChanged();
                /*
                // 自動化処理--------------------------------------------------
                _FileSystemServices.NavigateToDuplicateSelectPage();
                //-------------------------------------------------------------
                */
            });
        }

        #endregion コンストラクタと初期化
    }
}