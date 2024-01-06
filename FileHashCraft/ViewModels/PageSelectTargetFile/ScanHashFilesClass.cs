using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public void ScanHashFiles();
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        public ScanHashFilesClass() { throw new NotImplementedException(); }
        public ScanHashFilesClass(
            ICheckedDirectoryManager checkedDirectoryManager)
        {
            _checkedDirectoryManager = checkedDirectoryManager;
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async void ScanHashFiles()
        {
            DebugManager.InfoWrite("PageSelectTargetFileViewModel ScanHashFiles");

            var sw = new Stopwatch();
            sw.Start();
            // XML からファイルを読み込む
            FileHashInfoManager.Instance.LoadHashXML();

            // ディレクトリスキャンの表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.DirectoryScanning));

            // ディレクトリのスキャン
            await Task.Run(() => DirectoriesScan()).ConfigureAwait(true);

            // ファイルスキャンの表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.XMLWriting));

            // XML にファイルを書き込む
            await Task.Run(() => FileHashInfoManager.Instance.SaveHashXML()).ConfigureAwait(true);

            // スキャン終了の表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.Finished));
            sw.Stop();
#if DEBUG
            DebugManager.InfoWrite($"Debug ScanHashFiles : {sw.Elapsed.TotalSeconds} ms.");
#else
            DebugManager.InfoWrite($"Release ScanHashFiles : {sw.Elapsed.TotalSeconds} ms.");
#endif
        }

        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        private void DirectoriesScan()
        {
            // 各ドライブに対してタスクを回す
            var sw = new Stopwatch();
            sw.Start();
            RecursivelyDirectorySearch(_checkedDirectoryManager.NestedDirectories);
            foreach (var directory in _checkedDirectoryManager.NonNestedDirectories)
            {
                ScanDirectory(directory);
            }
            sw.Stop();
            DebugManager.InfoWrite($"Scan Time : {sw.ElapsedMilliseconds} ms");
        }

        #region 再帰的にディレクトリを検索する
        private readonly List<Task> scanTasks = [];
        private readonly SemaphoreSlim _semaphone = new(50);

        /// <summary>
        /// 並列処理でディレクトリを検索して、スキャン処理に渡します。
        /// </summary>
        private void RecursivelyDirectorySearch(List<string> rootDrives)
        {
            Parallel.ForEach(rootDrives, RecursivelyRetrieveDirectories);
            Task.WhenAll(scanTasks);
        }

        /// <summary>
        /// ディレクトリをスキャンしてXMLに反映させて、子のディレクトリに再帰処理をします。
        /// </summary>
        /// <param name="fullPath">スキャンするディレクトリ</param>
        private void RecursivelyRetrieveDirectories(string fullPath)
        {
            ScanDirectory(fullPath);

            var fileInfoManager = new ScanFileItems();
            foreach (var directory in fileInfoManager.EnumerateDirectories(fullPath))
            {
                RecursivelyRetrieveDirectories(directory);
            }
        }

        /// <summary>
        /// ディレクトリ情報をXMLファイルに反映させます。
        /// </summary>
        /// <param name="fullPath">反映させるディレクトリのフルパス</param>
        private void ScanDirectory(string fullPath)
        {
            scanTasks.Add(new Task(() =>
            {
                try
                {
                    _semaphone.Wait();
                    FileHashInfoManager.Instance.ScanDirectory(fullPath);
                }
                finally { _semaphone.Release(); }
            }
            ));

            //FileHashInfoManager.Instance.ScanDirectory(fullPath);
            WeakReferenceMessenger.Default.Send(new HashScanDirectoriesAdded(1));
        }
        #endregion 再帰的にディレクトリを検索する

    }
}
