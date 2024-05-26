using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Properties;
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
        public ObservableCollection<HashListFileItems> HashFileListItems { get; }
        /// <summary>
        /// ファイルスキャン状況
        /// </summary>
        public FileScanStatus Status { get; set; }
        /// <summary>
        /// ファイルスキャン状況に合わせた背景色
        /// </summary>
        public Brush StatusColor { get; set; }
        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        public string StatusMessage { get; set; }
        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        public int CountScannedDirectories { get; set; }
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        public int CountHashFilesDirectories { get; set; }
        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        public int CountAllTargetFilesGetHash { get; set; }
        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        public int CountFilteredGetHash { get; set; }
        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        public RelayCommand ToPageHashCalcing { get; set; }
        /// <summary>
        /// 検索ステータスを変更します。
        /// </summary>
        public void ChangeHashScanStatus(FileScanStatus status);
        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        public void AddScannedDirectoriesCount(int count = 1);
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        public void AddFilesScannedDirectoriesCount(int count = 1);
        /// <summary>
        /// 総対象ファイル数に加算します。
        /// </summary>
        public void AddAllTargetFiles(int allTargetFiles);
        /// <summary>
        /// ツリービューの選択ディレクトリが変更された時の処理です。
        /// </summary>
        public void ChangeCurrentPath(string currentFullPath);
        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        public void ChangeExtensionToListBox(string extention, bool IsTarget);
    }
    #endregion インターフェース

    public class PageSelectTargetViewModelMain : ObservableObject, IPageSelectTargetViewModelMain
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
        private readonly IMessageServices _messageServices;
        public PageSelectTargetViewModelMain(
            IScannedFilesManager scannedFilesManager,
            IMessageServices messageServices
        )
        {
            _messageServices = messageServices;
            _scannedFilesManager = scannedFilesManager;

            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new RelayCommand(
                () => _messageServices.SendToHashCalcingPage(),
                () => CountFilteredGetHash > 0
            );

            // カレントディレクトリが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChanged>(this, (_, m)
                => ChangeCurrentPath(m.CurrentFullPath));
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
        /// ハッシュ取得対象となる全てのファイル数を設定します。
        /// </summary>
        /// <param name="targetFilesCount">ハッシュ取得対象となるファイル数</param>
        public void AddAllTargetFiles(int targetFilesCount)
        {
            App.Current?.Dispatcher?.Invoke(() => CountAllTargetFilesGetHash += targetFilesCount);
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
                    IsHashTarget = _scannedFilesManager.GetAllCriteriaFileName().Any(f => f.FileFullPath == file)
                };
                App.Current?.Dispatcher?.Invoke(() => HashFileListItems.Add(item));
            }
        }
        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <param name="IsTarget">対象ファイルかどうか</param>
        public void ChangeExtensionToListBox(string extention, bool IsTarget)
        {
            foreach (var item in HashFileListItems)
            {
                var fileExtention = Path.GetExtension(item.FileFullPath);
                if (string.Equals(fileExtention, extention, StringComparison.OrdinalIgnoreCase))
                {
                    App.Current.Dispatcher.Invoke(() => item.IsHashTarget = IsTarget);
                }
            }
        }

        #endregion ツリービューのカレントディレクトリ、リストビューの色変え処理
    }
}
