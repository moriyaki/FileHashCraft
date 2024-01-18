using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IScanHashFiles
    {
        public Task ScanFiles(CancellationToken cancellationToken);
        public void ScanExtention();
    }

    public class ScanHashFiles : IScanHashFiles
    {
        #region コンストラクタと初期化
        private readonly IFileManager _FileManager;
        private readonly ISearchManager _SearchManager;
        private readonly IExtentionHelper _ExtentionManager;
        private readonly IPageSelectTargetViewModel _PageSelectTargetViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        public ScanHashFiles() { throw new NotImplementedException(); }
        public ScanHashFiles(
            IFileManager fileManager,
            ISearchManager searchManager,
            IExtentionHelper extentionManager,
            IPageSelectTargetViewModel pageSelectTargetViewModel,
            ICheckedDirectoryManager checkedDirectoryManager)
        {
            _SearchManager = searchManager;
            _FileManager = fileManager;
            _ExtentionManager = extentionManager;
            _PageSelectTargetViewModel = pageSelectTargetViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async Task ScanFiles(CancellationToken cancellationToken)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _directoriesList.Clear();
            try
            {
                // ディレクトリのスキャン
                _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await DirectoriesScan(cancellationToken);

                // ファイルのスキャン
                _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => DirectoryFilesScan(cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _PageSelectTargetViewModel.ClearExtentions();
            ScanExtention();

            // スキャン終了の表示に切り替える
            _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }

        #region ハッシュを取得するファイルのスキャン処理
        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="cancellationToken">キャンセリングトークン</param>
        private void DirectoryFilesScan(CancellationToken cancellationToken)
        {
            IExtentionHelper _ExtentionManager = Ioc.Default.GetService<IExtentionHelper>() ?? throw new InvalidOperationException($"{nameof(IExtentionHelper)} dependency not resolved.");

            try
            {
                int fileCount = 0;
                foreach (var directoryFullPath in _directoriesList)
                {
                    // ファイルを保持する
                    fileCount = 0;
                    foreach (var fileFullPath in _FileManager.EnumerateFiles(directoryFullPath))
                    {
                        _ExtentionManager.AddFile(fileFullPath);
                        _SearchManager.AddFile(fileFullPath);
                        fileCount++;
                    }

                    _PageSelectTargetViewModel.AddFilesScannedDirectoriesCount();
                    // ここ、ファイル数
                    _PageSelectTargetViewModel.AddAllTargetFiles(fileCount);

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
            foreach (var extention in _ExtentionManager.GetExtensions())
            {
                _PageSelectTargetViewModel.AddExtentions(extention);
            }
            // 拡張子を集める処理
            _PageSelectTargetViewModel.AddFileTypes();
        }
        #endregion ハッシュを取得するファイルのスキャン処理

        #region ディレクトリを検索する
        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        private async Task DirectoriesScan(CancellationToken cancellationToken)
        {
            // 各ドライブに対してタスクを回す
            await DirectorySearch(_CheckedDirectoryManager.NestedDirectories, cancellationToken);
            _PageSelectTargetViewModel.AddScannedDirectoriesCount(_CheckedDirectoryManager.NonNestedDirectories.Count);
        }

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
                    _PageSelectTargetViewModel.AddScannedDirectoriesCount();

                    foreach (var subDir in _FileManager.EnumerateDirectories(currentDirectory))
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
