/*  PageSelectTargetViewModel.PartialNormal.cs

    通常向けの拡張子検索画面を提供するタブの ViewModel を提供します。
 */

using System.Collections.ObjectModel;
using System.Windows.Media;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;

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

    public partial class PageSelectTargetViewModel
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
                ToPageHashCalcing.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// ファイルの種類による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; } = [];

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtensionOrTypeCheckBoxBase> ExtentionCollection { get; set; } = [];

        #endregion バインディング

        #region ファイル数の管理処理
        /// <summary>
        /// 検索のステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public void ChangeHashScanStatus(FileScanStatus status)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => Status = status);
        }
        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountScannedDirectories += directoriesCount);
        }
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddFilesScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountHashFilesDirectories += directoriesCount);
        }
        /// <summary>
        /// ハッシュ取得対象となる全てのファイル数を設定します。
        /// </summary>
        public void AddAllTargetFiles(int targetFilesCount)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountAllTargetFilesGetHash += targetFilesCount);
        }
        #endregion ファイル数の管理処理

        #region ファイル絞り込みの処理
        /// <summary>
        /// ファイルの種類をリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(FileGroupType.Sounds);
            var documents = new ExtentionGroupCheckBoxViewModel(FileGroupType.Documents);
            var applications = new ExtentionGroupCheckBoxViewModel(FileGroupType.Applications);
            var archives = new ExtentionGroupCheckBoxViewModel(FileGroupType.Archives);
            var sources = new ExtentionGroupCheckBoxViewModel(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel();

            App.Current?.Dispatcher.Invoke(() =>
            {
                if (movies.ExtentionCount > 0) { ExtentionsGroupCollection.Add(movies); }
                if (pictures.ExtentionCount > 0) { ExtentionsGroupCollection.Add(pictures); }
                if (musics.ExtentionCount > 0) { ExtentionsGroupCollection.Add(musics); }
                if (documents.ExtentionCount > 0) { ExtentionsGroupCollection.Add(documents); }
                if (applications.ExtentionCount > 0) { ExtentionsGroupCollection.Add(applications); }
                if (archives.ExtentionCount > 0) { ExtentionsGroupCollection.Add(archives); }
                if (sources.ExtentionCount > 0) { ExtentionsGroupCollection.Add(sources); }
                if (registrations.ExtentionCount > 0) { ExtentionsGroupCollection.Add(registrations); }
                if (others.ExtentionCount > 0) { ExtentionsGroupCollection.Add(others); }
            });
        }

        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void AddExtentions(string extention)
        {
            if (_ExtentionManager.GetExtentionsCount(extention) > 0)
            {
                var item = new ExtensionCheckBox(extention);
                App.Current?.Dispatcher.Invoke(() => ExtentionCollection.Add(item));
            }
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => ExtentionCollection.Clear());
        }
        // TODO : Modelから取得するようにする
        /// <summary>
        /// 拡張子チェックボックスにより、スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public void ExtentionCountChanged()
        {
            App.Current?.Dispatcher?.Invoke(() =>
                CountFilteredGetHash = _SearchManager.AllConditionFiles.Count);
        }
        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        /// <param name="changedCheck">チェックされたか外されたか</param>
        /// <param name="extentionCollention">拡張子のリストコレクション</param>
        public void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention)
        {
            var changedCollection = ExtentionCollection.Where(e => extentionCollention.Contains(e.ExtentionOrGroup));

            App.Current?.Dispatcher?.Invoke(() =>
            {
                foreach (var extension in changedCollection)
                {
                    extension.IsChecked = changedCheck;
                }
            });
        }
        #endregion ファイル絞り込みの処理
    }
}
