using System.Diagnostics;
using System.IO;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.ViewModels.FileSystemWatch
{
    #region イベント引数
    /// <summary>
    /// カレントディレクトリへの追加削除イベント引数
    /// </summary>
    public class CurrentDirectoryFileChangedEventArgs : EventArgs
    {
        public string FullPath { get; }

        public CurrentDirectoryFileChangedEventArgs()
        {
            throw new NotImplementedException();
        }

        public CurrentDirectoryFileChangedEventArgs(string fullPath)
        {
            FullPath = fullPath;
        }
    }

    /// <summary>
    /// カレントディレクトリへの追加削除イベント引数
    /// </summary>
    public class CurrentDirectoryFileRenamedEventArgs : EventArgs
    {
        public string OldFullPath { get; }
        public string FullPath { get; }

        public CurrentDirectoryFileRenamedEventArgs()
        {
            throw new NotImplementedException();
        }

        public CurrentDirectoryFileRenamedEventArgs(string fullPath)
        {
            throw new NotImplementedException();
        }

        public CurrentDirectoryFileRenamedEventArgs(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            FullPath = newFullPath;
        }
    }
    #endregion イベント引数

    #region インターフェース
    public interface ICurrentDirectoryFIleSystemWatcherService
    {
        public void SetCurrentDirectoryWatcher(string currentDirectory);
        public event EventHandler<CurrentDirectoryFileChangedEventArgs>? Created;
        public event EventHandler<CurrentDirectoryFileChangedEventArgs>? Deleted;
        public event EventHandler<CurrentDirectoryFileRenamedEventArgs>? Renamed;
    }
    #endregion インターフェース
    public class CurrentDirectoryFIleSystemWatcherService : ICurrentDirectoryFIleSystemWatcherService
    {
        #region FileSystemWatcherの宣言
        // イベントのデリゲート定義
        public delegate void FileChangedEventHandler(object sender, CurrentDirectoryFileChangedEventArgs filePath);
        public event EventHandler<CurrentDirectoryFileChangedEventArgs>? Created;
        public event EventHandler<CurrentDirectoryFileChangedEventArgs>? Deleted;
        public delegate void FileRenamedEventHandler(object sender, CurrentDirectoryFileRenamedEventArgs filePath);
        public event EventHandler<CurrentDirectoryFileRenamedEventArgs>? Renamed;

        private readonly FileSystemWatcher CurrentWatcher = new();

        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をします。
        /// </summary>
        /// <param name="rootTreeItem"></param>
        public void SetCurrentDirectoryWatcher(string currentDirectory)
        {
            // System.IO.FileNotFoundException: 'Error reading the C:\Windows\CSC directory.'
            if (!Path.Exists(currentDirectory)) return;
            try
            {
                CurrentWatcher.Created -= OnCreated;
                CurrentWatcher.Deleted -= OnDeleted;
                CurrentWatcher.Renamed -= OnRenamed;
                CurrentWatcher.Error -= OnError;

                CurrentWatcher.Path = currentDirectory;
                CurrentWatcher.NotifyFilter
                    = NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size;
                CurrentWatcher.EnableRaisingEvents = true;
                CurrentWatcher.IncludeSubdirectories = false;

                CurrentWatcher.Created += OnCreated;
                CurrentWatcher.Deleted += OnDeleted;
                CurrentWatcher.Renamed += OnRenamed;

                CurrentWatcher.Error += OnError;
            }
            catch (FileNotFoundException) { }
        }
        /// <summary>
        /// FIleSystemWatcherエラーイベントの処理をします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"FileSystemWatcher エラー: {e.GetException()}");
        }
        #endregion FileSystemWatcherの宣言

        #region ファイル変更通知
        /// <summary>
        /// ファイルが作成された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            Created?.Invoke(this, new CurrentDirectoryFileChangedEventArgs(e.FullPath));
        }

        /// <summary>
        /// ファイルが削除された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Deleted) return;
            Deleted?.Invoke(this, new CurrentDirectoryFileChangedEventArgs(e.FullPath));
        }

        /// <summary>
        /// ファイルが名前変更された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;
            Renamed?.Invoke(this, new CurrentDirectoryFileRenamedEventArgs(e.OldFullPath, e.FullPath));
        }
        #endregion ファイル変更通知
    }
}
