using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
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
    public class HashCalcingPageViewModel : BaseViewModel, IHashCalcingPageViewModel
    {
        #region バインディング

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
                        //WeakReferenceMessenger.Default.Send(new FileHashCalcFinished());
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
        private Brush _StatusColor = Brushes.LightGreen;
        public Brush StatusColor
        {
            get => _StatusColor;
            set => SetProperty(ref _StatusColor, value);
        }

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        private string _StatusMessage = string.Empty;
        public string StatusMessage
        {
            get => _StatusMessage;
            set => SetProperty(ref _StatusMessage, value);
        }

        /// <summary>
        /// ハッシュを取得する全てのファイル数
        /// </summary>
        private int _AllHashGetFilesCount = 0;
        public int AllHashGetFilesCount
        {
            get => _AllHashGetFilesCount;
            set => SetProperty(ref _AllHashGetFilesCount, value);
        }

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
            get => (double)HashGotFileCount / AllHashGetFilesCount * 100;
        }

        /// <summary>
        /// ファイルが同一だった件数
        /// </summary>
        private double _MatchHashCount = 0;
        public double MatchHashCount
        {
            get => _MatchHashCount;
            set
            {
                SetProperty(ref _MatchHashCount, value);
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
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IScannedFilesManager _scannedFilesManager;

        public HashCalcingPageViewModel(
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IFileSystemServices fileSystemServices,
            IScannedFilesManager scannedFilesManager
        ) : base(settingsService)
        {
            _fileSystemServices = fileSystemServices;
            _helpWindowViewModel = helpWindowViewModel;
            _scannedFilesManager = scannedFilesManager;

            AllHashGetFilesCount = _scannedFilesManager.GetAllCriteriaFilesCount(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude);

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
                _fileSystemServices.SendToSettingsPage(ReturnPageEnum.HashCalcingPage));

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
                _fileSystemServices.SendToSelectTargetPage());
            // 同一ファイル選択ページに移動するコマンド
            ToSameFileSelectPage = new RelayCommand(
                () => MessageBox.Show("ToSameFileSelectPage未実装"),
                () => false
            );

            Initialize();
        }

        public void Initialize()
        {
            Task.Run(async () =>
            {
                Status = FileHashCalcStatus.FileCalcing;
                for (var i = 0; i < AllHashGetFilesCount; i++)
                {
                    await Task.Delay(10);
                    App.Current?.Dispatcher?.Invoke(() => HashGotFileCount++);
                }
                Status = FileHashCalcStatus.FileMatching;
                for (var i = 0; i < AllHashGetFilesCount; i++)
                {
                    await Task.Delay(10);
                    App.Current?.Dispatcher?.Invoke(() => MatchHashCount++);
                }

                Status = FileHashCalcStatus.Finished;
            });
        }
        #endregion コンストラクタ
    }
}
