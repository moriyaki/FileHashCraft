using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.FileScan
{
    public interface IScanHashFiles
    {
        /// <summary>
        /// 全ディレクトリのリスト
        /// </summary>
        HashSet<string> DirectoriesHashSet { get; set; }
        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンします。
        /// </summary>
        Task DirectoriesScan(CancellationToken cancellation);
        /// <summary>
        /// ディレクトリを全て検索します。
        /// </summary>
        Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellation);
        /// <summary>
        /// ディレクトリ内部をを検索して取得します。
        /// </summary>
        HashSet<string> GetDirectories(string rootDirectory, CancellationToken cancellation);
        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンします。
        /// </summary>
        Task DirectoryFilesScan(CancellationToken cancellation);
        /// <summary>
        /// 拡張子によるファイルフィルタの設定をします。
        /// </summary>
        public void ScanExtention(CancellationToken cancellation);
    }
    public class ScanHashFiles : IScanHashFiles
    {
        /// <summary>
        /// 全ディレクトリのリスト
        /// </summary>
        public HashSet<string> DirectoriesHashSet { get; set; } = [];

        public ScanHashFiles() { throw new NotImplementedException(nameof(ScanHashFiles)); }

        private readonly IMessenger _messenger;
        private readonly IExtentionManager _extentionManager;
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IFileManager _fileManager;
        public ScanHashFiles(
            IMessenger messenger,
            IExtentionManager extentionManager,
            IDirectoriesManager directoriesManager,
            IFileManager fileManager
        )
        {
            _messenger = messenger;
            _extentionManager = extentionManager;
            _directoriesManager = directoriesManager;
            _fileManager = fileManager;
        }

        #region ディレクトリを検索する
        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンします。
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        public async Task DirectoriesScan(CancellationToken cancellation)
        {
            // TODO: DirectoryListにNonNestedDirectorisの処理追加

            // 各ドライブに対してタスクを回す
            await DirectorySearch(_directoriesManager.NestedDirectories, cancellation);
            _messenger.Send(new AddScannedDirectoriesCountMessage(_directoriesManager.NonNestedDirectories.Count));
        }

        /// <summary>
        /// ディレクトリを全て検索します。
        /// </summary>
        /// <param name="rootDirectories">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        public async Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellation)
        {
            var directoriesList = new List<string>();

            foreach (var rootDirectory in rootDirectories)
            {
                // TODO: DirectoriesHashSet を DirectoriesList に変更！
                await Task.Run(() =>
                {
                    DirectoriesHashSet.UnionWith(GetDirectories(rootDirectory, cancellation));
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
        public HashSet<string> GetDirectories(string rootDirectory, CancellationToken cancellation)
        {
            HashSet<string> result = [];
            Stack<string> paths = [];

            paths.Push(rootDirectory);
            while (paths.Count > 0)
            {
                string currentDirectory = paths.Pop();
                result.Add(currentDirectory);
                _messenger.Send(new AddScannedDirectoriesCountMessage(1));
                foreach (var subDir in _fileManager.EnumerateDirectories(currentDirectory))
                {
                    paths.Push(subDir);
                    if (cancellation.IsCancellationRequested) { return []; }
                }
            }
            return result;
        }
        #endregion ディレクトリを検索する

        #region ハッシュを取得するファイルのスキャン処理
        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンします。
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        public async Task DirectoryFilesScan(CancellationToken cancellation)
        {
            // TODO : DirectoriesHashSet→DirectoriesListからドライブ取得してDirectory<string, List<string>>に
            // SemaphoneSlimの数はDirectoryのKey数に

            var semaphore = new SemaphoreSlim(5);

            foreach (var directoryFullPath in DirectoriesHashSet)
            {
                try
                {
                    await semaphore.WaitAsync(cancellation);
                    _messenger.Send(new AddFilesToAllFilesMessage(_fileManager.EnumerateFiles(directoryFullPath).ToList()));

                    _messenger.Send(new AddFilesScannedDirectoriesCountMessage());
                    _messenger.Send(new SetAllTargetfilesCountMessge());

                    if (cancellation.IsCancellationRequested) { return; }
                }
                finally { semaphore.Release(); }
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定をします。
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        public void ScanExtention(CancellationToken cancellation)
        {
            foreach (var extention in _extentionManager.GetExtentions())
            {
                _messenger.Send(new AddExtentionMessage(extention));
                if (cancellation.IsCancellationRequested) { return; }
            }
            _messenger.Send(new AddFileTypesMessage());
        }
        #endregion ハッシュを取得するファイルのスキャン処理
    }
}
