using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
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
    public interface IShowTargetInfoUserControlViewModel
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

    public partial class ShowTargetInfoUserControlViewModel : BaseViewModel, IShowTargetInfoUserControlViewModel
    {
        #region バインディング
        /// <summary>
        /// ファイルスキャン状況
        /// </summary>
        private FileScanStatus _status = FileScanStatus.None;
        public FileScanStatus Status
        {
            get => _status;
            set
            {
                _status = value;
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
                        _messenger.Send(new FileScanFinished());
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
        private Brush _statusColor = Brushes.LightGreen;

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = string.Empty;

        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } = [];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        private string _selectedHashAlgorithm;
        public string SelectedHashAlgorithm
        {
            get => _selectedHashAlgorithm;
            set
            {
                if (value == _selectedHashAlgorithm) return;
                SetProperty(ref _selectedHashAlgorithm, value);
                _settingsService.SendHashAlogrithm(value);
            }
        }

        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        private int _countScannedDirectories = 0;
        public int CountScannedDirectories
        {
            get => _countScannedDirectories;
            set
            {
                SetProperty(ref _countScannedDirectories, value);
                Status = FileScanStatus.DirectoriesScanning;
            }
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        private int _countHashFilesDirectories = 0;
        public int CountHashFilesDirectories
        {
            get => _countHashFilesDirectories;
            set
            {
                SetProperty(ref _countHashFilesDirectories, value);
                Status = FileScanStatus.FilesScanning;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        private int _countAllTargetFilesGetHash = 0;
        public int CountAllTargetFilesGetHash
        {
            get => _countAllTargetFilesGetHash;
            set
            {
                SetProperty(ref _countAllTargetFilesGetHash, value);
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        private int _countFilteredGetHash = 0;
        public int CountFilteredGetHash
        {
            get => _countFilteredGetHash;
            set
            {
                SetProperty(ref _countFilteredGetHash, value);
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
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileManager _fileManager;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IHashAlgorithmHelper _hashAlgorithmHelper;
        public ShowTargetInfoUserControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileManager fileManager,
            IScannedFilesManager scannedFilesManager,
            IFileSystemServices fileSystemServices,
            IHashAlgorithmHelper hashAlgorithmHelper
        ) : base(messenger, settingsService)
        {
            _fileManager = fileManager;
            _scannedFilesManager = scannedFilesManager;
            _fileSystemServices = fileSystemServices;
            _selectedHashAlgorithm = _settingsService.HashAlgorithm;
            _hashAlgorithmHelper = hashAlgorithmHelper;

            HashAlgorithms =
            [
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

            // ハッシュ計算画面に移動するコマンド
            ToHashCalcingPage = new RelayCommand(
                () => _fileSystemServices.NavigateToHashCalcingPage(),
                () => CountFilteredGetHash > 0
            );

            // カレントディレクトリが変更されたメッセージ受信
            _messenger.Register<CurrentDirectoryChangedMessage>(this, (_, m)
                => ChangeCurrentPath(m.CurrentFullPath));

            // 拡張子チェックボックスのチェック状態が変更されたら、カレントディレクトリリストボックス変更
            _messenger.Register<ExtentionCheckChangedToListBoxMessage>(this, (_, _)
                => ChangeSelectedToListBox());
        }
        #endregion コンストラクタ

        /// <summary>
        /// 表示言語の変更に伴う対策をします。
        /// </summary>
        public void LanguageChangedMeasures()
        {
            var currentAlgorithm = _settingsService.HashAlgorithm;

            HashAlgorithms.Clear();
            HashAlgorithms =
            [
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
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

            foreach (var file in _fileManager.EnumerateFiles(currentFullPath))
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
