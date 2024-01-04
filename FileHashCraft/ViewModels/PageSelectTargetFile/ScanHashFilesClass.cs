using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public void ScanHashFiles();
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly IControDirectoryTreeViewlViewModel _controDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        private readonly ISpecialFolderAndRootDrives _specialFolderAndRootDrives;
        private readonly IMainWindowViewModel _mainWindowViewModel;

        public ScanHashFilesClass() { throw new NotImplementedException(); }

        public ScanHashFilesClass(
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            ICheckedDirectoryManager checkedDirectoryManager,
            ISpecialFolderAndRootDrives specialFolderAndRootDrives,
            IMainWindowViewModel mainWindowViewModel)
        {
            _controDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _checkedDirectoryManager = checkedDirectoryManager;
            _specialFolderAndRootDrives = specialFolderAndRootDrives;
            _mainWindowViewModel = mainWindowViewModel;
        }

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async void ScanHashFiles()
        {
            // ディレクトリスキャンの表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.DirectoryScanning));

            // ドライブ毎にパスを振り分ける
            var driveDirectory = MakeDirectoryDictionary();

            // ディレクトリのスキャン
            await DirectoriesScan(driveDirectory);

            // 各ドライブにファイルパスを振り分ける
            var driveFiles = MakeDirectoryFiles();

            // ファイルスキャンの表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.FileScanning));

            // ファイルのスキャン
            await FilesScan(driveFiles);

            // スキャン終了の表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.Finished));
        }
        #endregion コンストラクタと初期化

        #region ハッシュ計算するファイルを取得
        /// <summary>
        /// ドライブ毎にパスを振り分ける
        /// </summary>
        /// <returns>ドライブレターをキーとしたディレクトリリストの値</returns>
        private Dictionary<string, List<string>> MakeDirectoryDictionary()
        {
            // 
            var driveDirectory = new Dictionary<string, List<string>>();

            // ディレクトリのスキャン準備：ドライブ毎に振り分ける
            foreach (var directory in _checkedDirectoryManager.NestedDirectories)
            {
                var fileInfoManager = new ScanFileItems();
                var item = _specialFolderAndRootDrives.GetFileInformationFromDirectorPath(directory);
                var node = _controDirectoryTreeViewlViewModel.AddRoot(item, false);
                node.Name = node.FullPath;

                // ドライブルートを取得する
                var drive = Path.GetPathRoot(directory);
                if (drive == null) continue;

                // ドライブルートが Dictionary に登録されてなければ、リストを登録する
                if (!driveDirectory.TryGetValue(drive, out List<string>? value))
                {
                    value = ([]);
                    driveDirectory[drive] = ([]);
                }
                // リストにパスを登録する
                driveDirectory[drive].Add(directory);
            }
            return driveDirectory;
        }

        /// <summary>
        /// ドライブ毎にファイルパスを振り分ける
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> MakeDirectoryFiles()
        {
            var driveFiles = new Dictionary<string, List<string>>();
            foreach (var directory in ListDirectories)
            {
                var drive = Path.GetPathRoot(directory);
                if (drive == null) continue;

                if (!driveFiles.TryGetValue(drive, out List<string>? value))
                {
                    value = ([]);
                    driveFiles[drive] = ([]);
                }
                // リストにファイルパスを登録する
                driveFiles[drive].Add(directory);
            }
            return driveFiles;
        }

        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        /// <param name="driveDirectory">ドライブ毎にリスト化されたディレクトリ辞書</param>
        private async Task DirectoriesScan(Dictionary<string, List<string>> driveDirectory)
        {
            var tasks = new List<Task>();
            foreach (var key in driveDirectory.Keys)
            {
                var value = driveDirectory[key];
                if (value == null) continue;
                // 各ドライブに対してタスクを回す
                tasks.Add(Task.Run(() =>
                {
                    foreach (var directory in value)
                    {
                        RecursivelyDirectorySearch(directory);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// ハッシュを取得するファイルをスキャンする
        /// </summary>
        /// <param name="driveFiles">ドライブ毎にリスト化されたファイル名辞書</param>
        private static async Task FilesScan(Dictionary<string, List<string>> driveFiles)
        {
           var tasks = new List<Task>();
            foreach (var key in driveFiles.Keys)
            {
                var value = driveFiles[key];
                if (value == null) continue;
                // 各ドライブに対してタスクを回す
                var fileCount = 0;
                var directoryCount = 0;
                tasks.Add(Task.Run(() =>
                {
                    var fileInforManager = new ScanFileItems();
                    foreach (var directory in value)
                    {
                        foreach (var file in fileInforManager.EnumerateFiles(directory))
                        {
                            fileCount++;
                        }
                        directoryCount++;

                        if (directoryCount >= UpCount)
                        {
                            WeakReferenceMessenger.Default.Send(new HashAllFilesAdded(fileCount));
                            WeakReferenceMessenger.Default.Send(new HashADirectoryScannedAdded(directoryCount));

                            fileCount = 0;
                            directoryCount = 0;
                        }
                    }
                    WeakReferenceMessenger.Default.Send(new HashAllFilesAdded(fileCount));
                    WeakReferenceMessenger.Default.Send(new HashADirectoryScannedAdded(directoryCount));
                }
                ));
            }
            await Task.WhenAll(tasks);
        }

        #endregion ハッシュ計算するファイルを取得

        #region 再帰的にディレクトリを検索する
        /// <summary>
        /// UI に反映させる為の閾値数
        /// </summary>
        private const int UpCount = 100;
        private readonly List<string> ListDirectories = [];
        private int ScannedCount = 0;

        private void RecursivelyDirectorySearch(string fullPath)
        {
            RecursivelyRetrieveDirectories(fullPath);
            WeakReferenceMessenger.Default.Send(new HashScanDirectoriesAdded(ScannedCount));
            App.Current?.Dispatcher?.Invoke(() => ScannedCount = 0);
        }

        private void RecursivelyRetrieveDirectories(string fullPath)
        {
            ListDirectories.Add(fullPath);
            ScannedCount++;
            if (ScannedCount == UpCount)
            {
                WeakReferenceMessenger.Default.Send(new HashScanDirectoriesAdded(UpCount));
                ScannedCount = 0;
            }
            var fileInfoManager = new ScanFileItems();
            var infoCollection = fileInfoManager.EnumerateDirectories(fullPath);
            foreach (var directory in infoCollection)
            {
                RecursivelyRetrieveDirectories(directory);
            }
        }
        #endregion 再帰的にディレクトリを検索する

    }
}
