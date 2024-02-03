/*  ScanHashFiles.cs

    ファイルを全スキャンする処理を実装するクラスです。
    ScanFiles だけを利用します。

 */
using FileHashCraft.Models;
using FileHashCraft.ViewModels.ControlDirectoryTree;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IScanHashFiles
    {
        public Task ScanFiles(CancellationToken cancellation);
    }

    public class ScanHashFiles : IScanHashFiles
    {
        #region コンストラクタと初期化
        private readonly IExtentionManager _extentionManager;
        private readonly IPageSelectTargetViewModel _pageSelectTargetViewModel;
        private readonly ITreeManager _directoryTreeManager;
        public ScanHashFiles() { throw new NotImplementedException(); }
        public ScanHashFiles(
            IExtentionManager extentionManager,
            IPageSelectTargetViewModel pageSelectTargetViewModel,
            ITreeManager directoryTreeManager)
        {
            _extentionManager = extentionManager;
            _pageSelectTargetViewModel = pageSelectTargetViewModel;
            _directoryTreeManager = directoryTreeManager;
        }

        #endregion コンストラクタと初期化

        #region メイン処理
        /// <summary>
        /// スキャンするファイルを検出します。
        /// </summary>
        public async Task ScanFiles(CancellationToken cancellation)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _pageSelectTargetViewModel.AllFiles.Clear();
            _directoriesHashSet.Clear();
            _pageSelectTargetViewModel.ClearExtentions();

            try
            {
                /*
                var sw = new Stopwatch();
                sw.Start();
                */
                // ディレクトリのスキャン
                _pageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await DirectoriesScan(cancellation);

                // ファイルのスキャン
                _pageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => DirectoryFilesScan(cancellation), cancellation);
                /*
                sw.Stop();
                DebugManager.InfoWrite($"GetFileSecurity Version : {sw.ElapsedMilliseconds}ms.", true);
                */
            }
            catch (OperationCanceledException)
            {
                return;
            }

            ScanExtention(cancellation);

            // スキャン終了の表示に切り替える
            _pageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }
        #endregion メイン処理

        #region ハッシュを取得するファイルのスキャン処理
        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private async Task DirectoryFilesScan(CancellationToken cancellation)
        {
            var semaphore = new SemaphoreSlim(5);

            foreach (var directoryFullPath in _directoriesHashSet)
            {
                try
                {
                    await semaphore.WaitAsync(cancellation);
                    // ファイルを保持する
                    var fileCount = 0;
                    foreach (var fileFullPath in FileManager.EnumerateFiles(directoryFullPath))
                    {
                        _pageSelectTargetViewModel.AddFileToAllFiles(fileFullPath);
                        fileCount++;
                    }

                    _pageSelectTargetViewModel.AddFilesScannedDirectoriesCount();
                    _pageSelectTargetViewModel.AddAllTargetFiles(fileCount);

                    if (cancellation.IsCancellationRequested) { return; }
                }
                finally { semaphore.Release(); }
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private void ScanExtention(CancellationToken cancellation)
        {
            foreach (var extention in _extentionManager.GetExtentions())
            {
                _pageSelectTargetViewModel.AddExtentions(extention);
                if (cancellation.IsCancellationRequested) { return; }
            }
            // 拡張子を集める処理
            _pageSelectTargetViewModel.AddFileTypes();
        }
        #endregion ハッシュを取得するファイルのスキャン処理

        #region ディレクトリを検索する
        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private async Task DirectoriesScan(CancellationToken cancellation)
        {
            // 各ドライブに対してタスクを回す
            await DirectorySearch(_directoryTreeManager.NestedDirectories, cancellation);
            _pageSelectTargetViewModel.AddScannedDirectoriesCount(_directoryTreeManager.NonNestedDirectories.Count);
        }

        /// <summary>
        /// 全ディレクトリのリスト
        /// </summary>
        private readonly HashSet<string> _directoriesHashSet = [];

        /// <summary>
        /// ディレクトリを全て検索します
        /// </summary>
        /// <param name="rootDirectories">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        private async Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellation)
        {
            var directoriesList = new List<string>();

            await Task.Run(() =>
            {
                foreach (var rootDirectory in rootDirectories)
                {
                    foreach (var dir in GetDirectories(rootDirectory, cancellation))
                    {
                        _directoriesHashSet.Add(dir);
                        if (cancellation.IsCancellationRequested) { return; }
                    }
                }
            }, cancellation);
        }

        /// <summary>
        /// ディレクトリ内部をを検索して取得します。
        /// </summary>
        /// <param name="rootDirectory">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        private List<string> GetDirectories(string rootDirectory, CancellationToken cancellation)
        {
            List<string> result = [];
            Stack<string> paths = [];

            paths.Push(rootDirectory);
            while (paths.Count > 0)
            {
                string currentDirectory = paths.Pop();
                result.Add(currentDirectory);
                _pageSelectTargetViewModel.AddScannedDirectoriesCount();

                foreach (var subDir in FileManager.EnumerateDirectories(currentDirectory))
                {
                    paths.Push(subDir);
                    if (cancellation.IsCancellationRequested) { return []; }
                }
            }
            return result;
        }
        #endregion ディレクトリを検索する
    }
}
