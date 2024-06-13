/*  ScanHashFiles.cs

    ファイルを全スキャンする処理を実装するクラスです。
    ScanFiles だけを利用します。

 */
using System.Diagnostics;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
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
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IExtentionManager _extentionManager;
        private readonly IPageSelectTargetViewModel _pageSelectTargetViewModel;
        private readonly ITreeManager _treeManager;
        public ScanHashFiles() { throw new NotImplementedException(); }
        public ScanHashFiles(
            IScannedFilesManager scannedFilesManager,
            IExtentionManager extentionManager,
            IPageSelectTargetViewModel pageSelectTargetViewModel,
            ITreeManager treeManager)
        {
            _scannedFilesManager = scannedFilesManager;
            _extentionManager = extentionManager;
            _pageSelectTargetViewModel = pageSelectTargetViewModel;
            _treeManager = treeManager;
        }

        #endregion コンストラクタと初期化

        #region メイン処理
        /// <summary>
        /// スキャンするファイルを検出します。
        /// </summary>
        public async Task ScanFiles(CancellationToken cancellation)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _scannedFilesManager.AllFiles.Clear();
            _directoriesHashSet.Clear();
            _pageSelectTargetViewModel.ViewModelExtention.ClearExtentions();

            try
            {
                // ディレクトリのスキャン
                _pageSelectTargetViewModel.ViewModelMain.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await DirectoriesScan(cancellation);

                // ファイルのスキャン
                _pageSelectTargetViewModel.ViewModelMain.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => DirectoryFilesScan(cancellation), cancellation);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            ScanExtention(cancellation);

            // スキャン終了の表示に切り替える
            _pageSelectTargetViewModel.ViewModelMain.ChangeHashScanStatus(FileScanStatus.Finished);
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
                    foreach (var fileFullPath in FileManager.EnumerateFiles(directoryFullPath))
                    {
                        _pageSelectTargetViewModel.ViewModelExtention.AddFileToAllFiles(fileFullPath);
                    }

                    _pageSelectTargetViewModel.ViewModelMain.AddFilesScannedDirectoriesCount();
                    _pageSelectTargetViewModel.ViewModelMain.SetAllTargetfilesCount();

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
                _pageSelectTargetViewModel.ViewModelExtention.AddExtentions(extention);
                if (cancellation.IsCancellationRequested) { return; }
            }
            // 拡張子を集める処理
            _pageSelectTargetViewModel.ViewModelExtention.AddFileTypes();
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
            await DirectorySearch(_treeManager.NestedDirectories, cancellation);
            _pageSelectTargetViewModel.ViewModelMain.AddScannedDirectoriesCount(_treeManager.NonNestedDirectories.Count);
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

            foreach (var rootDirectory in rootDirectories)
            {
                await Task.Run(() =>
                {
                    _directoriesHashSet.UnionWith(GetDirectories(rootDirectory, cancellation));
                    if (cancellation.IsCancellationRequested) { return; }
                }, cancellation);
            }
        }

        /// <summary>
        /// ディレクトリ内部をを検索して取得します。
        /// </summary>
        /// <param name="rootDirectory">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        private HashSet<string> GetDirectories(string rootDirectory, CancellationToken cancellation)
        {
            HashSet<string> result = [];
            Stack<string> paths = [];

            paths.Push(rootDirectory);
            while (paths.Count > 0)
            {
                string currentDirectory = paths.Pop();
                result.Add(currentDirectory);
                _pageSelectTargetViewModel.ViewModelMain.AddScannedDirectoriesCount();

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
