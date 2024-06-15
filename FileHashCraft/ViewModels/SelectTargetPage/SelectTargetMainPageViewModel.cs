using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
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
    public interface IPageSelectTargetViewModelMain
    {
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
        RelayCommand ToPageHashCalcing { get; set; }
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

    public class SelectTargetMainPageViewModel : BaseViewModel, IPageSelectTargetViewModelMain
    {
        #region バインディング
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
                        WeakReferenceMessenger.Default.Send(new FileScanFinished());
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
                ToPageHashCalcing.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムのリストボックスコレクションです。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; set; } = [];

        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        public RelayCommand ToPageHashCalcing { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IFileSystemServices _fileSystemService;
        public SelectTargetMainPageViewModel(
            ISettingsService settingsService,
            IScannedFilesManager scannedFilesManager,
            IFileSystemServices fileSystemServices
        ) : base(settingsService)
        {
            _scannedFilesManager = scannedFilesManager;
            _fileSystemService = fileSystemServices;

            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new RelayCommand(
                () => _fileSystemService.SendToHashCalcingPage(),
                () => CountFilteredGetHash > 0
            );

            // カレントディレクトリが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChanged>(this, (_, m)
                => ChangeCurrentPath(m.CurrentFullPath));

            // 拡張子チェックボックスのチェック状態が変更されたら、カレントディレクトリリストボックス変更
            WeakReferenceMessenger.Default.Register<ExtentionCheckChangedToListBox>(this, (_, _)
                => ChangeSelectedToListBox());
        }
        #endregion コンストラクタ

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
        public void AddScannedDirectoriesCount(int directoriesCount = 1)
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
            CountAllTargetFilesGetHash = _scannedFilesManager.GetAllFilesCount(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude));
        }

        /// <summary>
        /// スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public void SetTargetCountChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            CountFilteredGetHash = _scannedFilesManager.GetAllCriteriaFilesCount(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude));
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

            foreach (var file in FileManager.EnumerateFiles(currentFullPath))
            {
                var item = new HashListFileItems
                {
                    FileFullPath = file,
                    //IsHashTarget = _scannedFilesManager.GetAllCriteriaFileName().Any(f => f.FileFullPath == file)
                    IsHashTarget = _scannedFilesManager.IsCriteriaFile(file, _settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude),
                };
                App.Current?.Dispatcher?.Invoke(() => HashFileListItems.Add(item));
            }
        }
        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        //public void ChangeExtensionToListBox(string extention, bool IsTarget)
        public void ChangeSelectedToListBox()
        {
            foreach (var item in HashFileListItems)
            {
                App.Current?.Dispatcher?.Invoke(() =>
                    item.IsHashTarget = _scannedFilesManager.IsCriteriaFile(item.FileFullPath, _settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude));
            }
        }
        #endregion ツリービューのカレントディレクトリ、リストビューの色変え処理
    }
}
