using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.DupSelectAndDelete;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.Views;

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
        void Initialize();
    }
    #endregion インターフェース

    public partial class HashCalcingPageViewModel : BaseViewModel, IHashCalcingPageViewModel
    {
        #region バインディング
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
                        StatusMessage = $"{Resources.LabelFileCalcing}";
                        break;
                    case FileHashCalcStatus.FileMatching:
                        StatusColor = Brushes.Yellow;
                        StatusMessage = $"{Resources.LabelFileMatching}";
                        break;
                    case FileHashCalcStatus.Finished:
                        StatusColor = Brushes.LightGreen;
                        StatusMessage = Resources.LabelFinished;
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
        private readonly IFileHashCalc _fileHashCalc;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IDupFilesManager _duplicateFilesManager;

        public HashCalcingPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileHashCalc fileHashCalc,
            IHelpWindowViewModel helpWindowViewModel,
            IScannedFilesManager scannedFilesManager,
            IFileSystemServices fileSystemServices,
            IDupFilesManager duplicateFilesManager
        ) : base(messenger, settingsService)
        {
            _fileHashCalc = fileHashCalc;
            _scannedFilesManager = scannedFilesManager;
            _fileSystemServices = fileSystemServices;
            _helpWindowViewModel = helpWindowViewModel;
            _duplicateFilesManager = duplicateFilesManager;

            HashAlgorithm = _settingsService.HashAlgorithm;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
                _fileSystemServices.NavigateToSettingsPage(ReturnPageEnum.HashCalcingPage));

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
            // ファイル選択画面に戻るコマンド
            ToSelectTargetPage = new RelayCommand(() =>
                _fileSystemServices.NavigateToSelectTargetPage());

            ToDupDeletePage = new RelayCommand(
                () => _fileSystemServices.NavigateToDuplicateSelectPage(),
                () => MatchHashCount > 0
            );

            // ドライブの追加
            _messenger.Register<CalcingDriveMessage>(this, (_, m) =>
            {
                foreach (var drive in m.Drives)
                {
                    App.Current?.Dispatcher?.Invoke(() => CalcingFiles.Add(drive + " All Finished."));
                }
            });

            // ハッシュ計算を開始したメッセージ
            _messenger.Register<StartCalcingFileMessage>(this, (_, m) =>
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
            _messenger.Register<EndCalcingFileMessage>(this, (_, m) =>
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
            _messenger.Register<FinishedCalcingFileMessage>(this, (_, _)
                => HashGotFileCount = AllHashNeedToGetFilesCount);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            CalcingFiles.Clear();
            Task.Run(async () =>
            {
                AllTargetFilesCount = _scannedFilesManager.GetAllCriteriaFileName(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude).Count;
                Status = FileHashCalcStatus.FileCalcing;
                var dupFileCandidate = _fileHashCalc.GetHashDriveFiles();
                foreach (var fileInDrive in dupFileCandidate)
                {
                    AllHashNeedToGetFilesCount += fileInDrive.Value.Count;
                }
                await _fileHashCalc.ProcessGetHashFilesAsync(dupFileCandidate);
                App.Current?.Dispatcher?.Invoke(() => Status = FileHashCalcStatus.FileMatching);

                var sameFiles = dupFileCandidate.Values
                    .SelectMany(hashSet => hashSet)
                    .GroupBy(file => new { file.FileSize, file.FileHash })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
                    .ToHashSet();

                _duplicateFilesManager.AddDuplicateFiles(sameFiles);
                App.Current?.Dispatcher?.Invoke(() =>
                {
                    Status = FileHashCalcStatus.Finished;
                    MatchHashCount = sameFiles.Count;
                    ToDupDeletePage.NotifyCanExecuteChanged();
                });
            });
        }
        #endregion コンストラクタと初期化
    }
}
