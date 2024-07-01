using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Models.Helpers;
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
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// ハッシュを取得する全てのファイル数
        /// </summary>
        private int _allHashGetFilesCount = 0;
        public int AllHashGetFilesCount
        {
            get => _allHashGetFilesCount;
            set => SetProperty(ref _allHashGetFilesCount, value);
        }

        /// <summary>
        /// ハッシュ計算のアルゴリズム
        /// </summary>
        private string _hashAlgorithm = string.Empty;
        public string HashAlgorithm
        {
            get => _hashAlgorithmHelper.GetAlgorithmCaption(_settingsService.HashAlgorithm);
            set => SetProperty(ref _hashAlgorithm, value);
        }

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
            get => (double)HashGotFileCount / AllHashGetFilesCount * 100;
        }

        /// <summary>
        /// ファイルが同一だった件数
        /// </summary>
        private double _matchHashCount = 0;
        public double MatchHashCount
        {
            get => _matchHashCount;
            set
            {
                SetProperty(ref _matchHashCount, value);
                OnPropertyChanged(nameof(MatchHashPercent));
            }
        }

        /// <summary>
        /// 同一ファイルチェックのパーセンテージ
        /// </summary>
        public double MatchHashPercent
        {
            get => (double)MatchHashCount / AllHashGetFilesCount * 100;
        }

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
        /// 同一ファイル選択ページに移動します。
        /// </summary>
        public RelayCommand ToSameFileSelectPage { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileHashCalc _fileHashCalc;
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IHashAlgorithmHelper _hashAlgorithmHelper;

        public HashCalcingPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileHashCalc fileHashCalc,
            IHelpWindowViewModel helpWindowViewModel,
            IFileSystemServices fileSystemServices,
            IScannedFilesManager scannedFilesManager,
            IHashAlgorithmHelper hashAlgorithmHelper
        ) : base(messenger, settingsService)
        {
            _fileHashCalc = fileHashCalc;
            _fileSystemServices = fileSystemServices;
            _helpWindowViewModel = helpWindowViewModel;
            _scannedFilesManager = scannedFilesManager;
            _hashAlgorithmHelper = hashAlgorithmHelper;

            HashAlgorithm = _settingsService.HashAlgorithm;
            AllHashGetFilesCount = _scannedFilesManager.GetAllCriteriaFilesCount(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude);

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

            // 同一ファイル選択ページに移動するコマンド
            ToSameFileSelectPage = new RelayCommand(
                () => MessageBox.Show("ToSameFileSelectPage未実装"),
                () => false
            );

            // ハッシュ計算を開始したメッセージ
            _messenger.Register<StartCalcingFile>(this, (_, m) =>
            {
                if (string.IsNullOrEmpty(m.BeforeFile))
                {
                    App.Current?.Dispatcher.Invoke(() => CalcingFiles.Add(m.CalcingFile));
                }
                else
                {
                    var drive = m.BeforeFile[..2];

                    // インデックスを手動で検索
                    for (int i = 0; i < CalcingFiles.Count; i++)
                    {
                        if (CalcingFiles[i].StartsWith(drive))
                        {
                            App.Current?.Dispatcher.Invoke(() => CalcingFiles[i] = m.CalcingFile);
                            break;
                        }
                    }
                }
                App.Current?.Dispatcher.Invoke(() => HashGotFileCount++);
            });
            // ハッシュ計算を終了したメッセージ
            _messenger.Register<EndCalcingFile>(this, (_, m)
                => App.Current?.Dispatcher.Invoke(() => CalcingFiles.Remove(m.CalcingFile)));

            Initialize();
        }

        public void Initialize()
        {
            Task.Run(async () =>
            {
                Status = FileHashCalcStatus.FileCalcing;
                await _fileHashCalc.ProcessGetHashFilesAsync();
                Status = FileHashCalcStatus.FileMatching;

                /*
                for (var i = 0; i < AllHashGetFilesCount; i++)
                {
                    await Task.Delay(1);
                    App.Current?.Dispatcher?.Invoke(() => MatchHashCount++);
                }
                */
                App.Current?.Dispatcher?.Invoke(() => MatchHashCount = AllHashGetFilesCount);
                Status = FileHashCalcStatus.Finished;
            });
        }
        #endregion コンストラクタ
    }
}
