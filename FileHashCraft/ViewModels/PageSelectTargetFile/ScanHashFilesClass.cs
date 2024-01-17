using System.Diagnostics;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType, CancellationToken cancellationToken);
        public void ScanExtention();
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly IPageSelectTargetFileViewModel _PageSelectTargetFileViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        public ScanHashFilesClass() { throw new NotImplementedException(); }
        public ScanHashFilesClass(
            IPageSelectTargetFileViewModel pageSelectTargetFileViewModel,
            ICheckedDirectoryManager checkedDirectoryManager)
        {
            _PageSelectTargetFileViewModel = pageSelectTargetFileViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType, CancellationToken cancellationToken)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _directoriesList.Clear();
            try
            {
                var sw = new Stopwatch();

                // ディレクトリのスキャン
                _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                sw.Start();
                await DirectoriesScan(cancellationToken);
                sw.Stop();
                DebugManager.InfoWrite($"Directory Scan : {sw.ElapsedMilliseconds}ms.", true);

                // ファイルのスキャン
                _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);

                sw.Restart();
                await Task.Run(() => DirectoryFilesScan(cancellationToken), cancellationToken);
                sw.Stop();
                DebugManager.InfoWrite($"File Scan : {sw.ElapsedMilliseconds}ms.", true);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _PageSelectTargetFileViewModel.ClearExtentions();
            ScanExtention();

            // スキャン終了の表示に切り替える
            _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }

        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        private async Task DirectoriesScan(CancellationToken cancellationToken)
        {
            // 各ドライブに対してタスクを回す
            await DirectorySearch(_CheckedDirectoryManager.NestedDirectories, cancellationToken);
            _PageSelectTargetFileViewModel.AddScannedDirectoriesCount(_CheckedDirectoryManager.NonNestedDirectories.Count);
        }

        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="cancellationToken">キャンセリングトークン</param>
        private void DirectoryFilesScan(CancellationToken cancellationToken)
        {
            try
            {
                int fileCount = 0;
                foreach (var directoryFullPath in _directoriesList)
                {
                    // ファイルを保持する
                    fileCount = 0;
                    foreach (var fileFullPath in FileManager.Instance.EnumerateFiles(directoryFullPath))
                    {
                        FileExtentionManager.Instance.AddFile(fileFullPath);
                        fileCount++;
                    }

                    _PageSelectTargetFileViewModel.AddFilesScannedDirectoriesCount();
                    // ここ、ファイル数
                    _PageSelectTargetFileViewModel.AddAllTargetFiles(fileCount);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        public void ScanExtention()
        {
            foreach (var extention in FileExtentionManager.Instance.GetExtensions())
            {
                _PageSelectTargetFileViewModel.AddExtentions(extention);
            }
            // 拡張子を集める処理
            _PageSelectTargetFileViewModel.AddFileTypes();
        }

        #region ディレクトリを検索する
        private readonly List<string> _directoriesList = [];

        private async Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellationToken)
        {
            var directoriesList = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    foreach (var rootDirectory in rootDirectories)
                    {
                        _directoriesList.AddRange(GetDirectories(rootDirectory, cancellationToken));
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException) { return; }
        }

        private List<string> GetDirectories(string rootDirectory, CancellationToken cancellationToken)
        {
            List<string> result = [];
            Stack<string> paths = [];
            try
            {
                paths.Push(rootDirectory);
                while (paths.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string currentDirectory = paths.Pop();
                    result.Add(currentDirectory);
                    _PageSelectTargetFileViewModel.AddScannedDirectoriesCount();

                    foreach (var subDir in FileManager.Instance.EnumerateDirectories(currentDirectory))
                    {
                        paths.Push(subDir);
                    }
                }
                return result;
            }
            catch (OperationCanceledException) { return []; }
        }

        #endregion ディレクトリを検索する
    }
}
