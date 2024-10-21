using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Properties;
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
    public interface IHashCalcingPageViewModel;
    #endregion インターフェース

    public partial class HashCalcingPageViewModel : BaseViewModel, IHashCalcingPageViewModel
    {
        #region バインディング
        /// <summary>
        /// ファイルハッシュ計算の進行状況
        /// </summary>
        private FileHashCalcStatus _status = FileHashCalcStatus.None;
        public FileHashCalcStatus Status
        {
            get => _status;
            set
            {
                _status = value;
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
                        //_messenger.Send(new FileHashCalcFinished());
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
        private Brush _statusColor = Brushes.LightGreen;

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = string.Empty;

        /// <summary>
        /// 総対象ファイル数
        /// </summary>
        [ObservableProperty]
        private int _allTargetFilesCount = 0;

        /// <summary>
        /// ハッシュを取得する全てのファイル数
        /// </summary>
        [ObservableProperty]
        private int _allHashNeedToGetFilesCount = 0;

        /// <summary>
        /// ハッシュ計算のアルゴリズム
        /// </summary>
        [ObservableProperty]
        private string _hashAlgorithm = string.Empty;

        /// <summary>
        /// ハッシュ取得済みのファイル数
        /// </summary>
        private int _hashGotFileCount = 0;
        public int HashGotFileCount
        {
            get => _hashGotFileCount;
            set
            {
                SetProperty(ref _hashGotFileCount, value);
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
        private double _matchHashCount = 0;

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
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileHashCalc _fileHashCalc;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IFileSystemServices _fileSystemServices;

        public HashCalcingPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileHashCalc fileHashCalc,
            IHelpWindowViewModel helpWindowViewModel,
            IScannedFilesManager scannedFilesManager,
            IFileSystemServices fileSystemServices
        ) : base(messenger, settingsService)
        {
            _fileHashCalc = fileHashCalc;
            _scannedFilesManager = scannedFilesManager;
            _fileSystemServices = fileSystemServices;
            _helpWindowViewModel = helpWindowViewModel;

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

            // ドライブの追加
            _messenger.Register<CalcingDriveMessage>(this, (_, m) =>
            {
                foreach (var drive in m.Drives)
                {
                    App.Current?.Dispatcher?.Invoke(() => CalcingFiles.Add(drive));
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
                    }
                }
                HashGotFileCount++;
            });

            // ハッシュ計算を終了したメッセージ
            _messenger.Register<EndCalcingFileMessage>(this, (_, m)
                => App.Current?.Dispatcher.Invoke(() => CalcingFiles.Remove(m.CalcingFile)));

            Initialize();
        }

        public void Initialize()
        {
            Task.Run(async () =>
            {
                AllTargetFilesCount = _scannedFilesManager.GetAllCriteriaFileName(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude).Count;
                Status = FileHashCalcStatus.FileCalcing;
                var sameFileCandidate = _fileHashCalc.GetHashDriveFiles();
                foreach (var fileInDrive in sameFileCandidate)
                {
                    AllHashNeedToGetFilesCount += fileInDrive.Value.Count;
                }
                await _fileHashCalc.ProcessGetHashFilesAsync(sameFileCandidate);
                App.Current?.Dispatcher?.Invoke(() => Status = FileHashCalcStatus.FileMatching);

                var sameFiles = sameFileCandidate.Values
                    .SelectMany(hashSet => hashSet)
                    .GroupBy(file => new { file.FileSize, file.FileHash })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
                    .ToHashSet();

                App.Current?.Dispatcher?.Invoke(() =>
                {
                    MatchHashCount = sameFiles.Count;
                    Status = FileHashCalcStatus.Finished;
                });
            });
        }
        #endregion コンストラクタ
    }
}
