using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using FilOps.ViewModels.ExplorerPage;
using Microsoft.Extensions.DependencyInjection;


namespace FilOps.Models
{
    #region イベント引数
    /// <summary>
    /// ドライブルートの追加削除イベント引数
    /// </summary>
    public class DirectoryChangedEventArgs : EventArgs
    {
        public string FullPath { get; }

        public DirectoryChangedEventArgs()
        {
            throw new NotImplementedException();
        }

        public DirectoryChangedEventArgs(string fullPath)
        {
            FullPath = fullPath;
        }
    }

    /// <summary>
    /// カレントディレクトリへの追加削除イベント引数
    /// </summary>
    public class DirectoryRenamedEventArgs : EventArgs
    {
        public string OldFullPath { get; }
        public string FullPath { get; }

        public DirectoryRenamedEventArgs()
        {
            throw new NotImplementedException();
        }

        public DirectoryRenamedEventArgs(string fullPath)
        {
            throw new NotImplementedException();
        }

        public DirectoryRenamedEventArgs(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            FullPath = newFullPath;
        }
    }
    #endregion イベント引数

    #region インターフェース
    public interface IDrivesFileSystemWatcherService
    {
        public void SetRootDirectoryWatcher(FileItemInformation rootDrive);
        public event EventHandler<DirectoryChangedEventArgs>? Changed;
        public event EventHandler<DirectoryChangedEventArgs>? Created;
        public event EventHandler<DirectoryRenamedEventArgs>? Renamed;

        // リムーバブルドライブの追加と削除
        public event EventHandler<DirectoryChangedEventArgs>? OpticalDriveMediaInserted;
        public event EventHandler<DirectoryChangedEventArgs>? OpticalDriveMediaEjected;
        public void InsertOpticalDriveMedia(char driveLetter);
        public void EjectOpticalDriveMedia(char driveLetter);
    }
    #endregion インターフェース

    public class DrivesFileSystemWatcherService : IDrivesFileSystemWatcherService
    {
        #region イベントのデリゲート定義とコンストラクタ
        // イベントのデリゲート定義
        public delegate void FileChangedEventHandler(object sender, CurrentDirectoryFileChangedEventArgs filePath);
        public event EventHandler<DirectoryChangedEventArgs>? Changed;
        public event EventHandler<DirectoryChangedEventArgs>? Created;
        public delegate void FileRenamedEventHandler(object sender, CurrentDirectoryFileRenamedEventArgs filePath);
        public event EventHandler<DirectoryRenamedEventArgs>? Renamed;

        // リムーバブルメディアのデリゲート定義
        public event EventHandler<DirectoryChangedEventArgs>? OpticalDriveMediaInserted;
        public event EventHandler<DirectoryChangedEventArgs>? OpticalDriveMediaEjected;

        private readonly IExpandedDirectoryManager ExpandDirManager;

        /// <summary>
        /// ドライブ内のディレクトリ変更を監視するインスタンス
        /// </summary>
        private readonly List<FileSystemWatcher> DrivesWatcher = [];

        /// <summary>
        /// IExpandedDirectoryManager を注入するコンストラクタ
        /// </summary>
        /// <param name="expandedDirManager">IExpandedDirectoryManager</param>
        public DrivesFileSystemWatcherService(IExpandedDirectoryManager expandedDirManager)
        {
            ExpandDirManager = expandedDirManager;
        }

        public DrivesFileSystemWatcherService()
        {
            throw new NotImplementedException();
        }
        #endregion イベントのデリゲート定義とコンストラクタ

        #region ディレクトリ変更通知処理
        /// <summary>
        /// ドライブに対して、ファイルアイテム変更監視の設定をします。
        /// </summary>
        /// <param name="rootDrive"></param>
        public void SetRootDirectoryWatcher(FileItemInformation rootDrive)
        {
            var watcher = new FileSystemWatcher();

            if (rootDrive.IsReady)
            {
                try
                {
                    watcher.Path = rootDrive.FullPath;
                    watcher.NotifyFilter
                        = NotifyFilters.DirectoryName
                        | NotifyFilters.LastWrite;
                    watcher.EnableRaisingEvents = true;
                    watcher.IncludeSubdirectories = true;
                    watcher.InternalBufferSize = 65536;

                    watcher.Created += OnCreated;
                    watcher.Changed += OnChanged;
                    watcher.Renamed += OnRenamed;

                    watcher.Error += OnError;
                    DrivesWatcher.Add(watcher);
                }
                catch (FileNotFoundException) { }
            }
        }

        /// <summary>
        /// ディレクトリ監視の変更通知を無視していい物を選別します。
        /// </summary>
        /// <param name="fullPath">変更されたディレクトリのフルパス</param>
        /// <returns>変更通知を無視していいかどうか</returns>
        private bool IsEventNotCatch(string fullPath)
        {
            if (fullPath.Length == 3) { return true; }
            if (fullPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.Windows))) return true;
            if (fullPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))) return true;
            if (fullPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))) return true;
            if (fullPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))) return true;
            if (fullPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))) return true;
            if (fullPath.Contains(Path.GetTempPath())) return true;
            if (!(ExpandDirManager.IsExpandedDirectory(fullPath) || ExpandDirManager.IsExpandedDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty))) return true;

            try
            {
                var hasDirectory = Directory.EnumerateFileSystemEntries(fullPath).Any();
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException ||
                    ex is IOException)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// FIleSystemWatcherエラーイベントを処理します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"FileSystemWatcher エラー: {e.GetException()}");
        }

        /// <summary>
        /// ファイルアイテムが変更された時のイベントをを処理します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnChanged(object? sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            if (IsEventNotCatch(e.FullPath)) return;

            Changed?.Invoke(this, new DirectoryChangedEventArgs(e.FullPath));
        }

        /// <summary>
        /// ファイルアイテムが作成された時のイベントをを処理します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">FileSystemEventArgs</param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (IsEventNotCatch(e.FullPath)) return;
            Created?.Invoke(this, new DirectoryChangedEventArgs(e.FullPath));
        }

        /// <summary>
        /// ファイルアイテムが名前変更された時のイベントをを処理します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">RenamedEventArgs</param>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (IsEventNotCatch(e.FullPath)) return;
            Renamed?.Invoke(this, new DirectoryRenamedEventArgs(e.OldFullPath, e.FullPath));
        }
        #endregion ディレクトリ変更通知処理

        #region リムーバブルディスクの着脱処理
        /// <summary>
        /// リムーバブルドライブの追加または挿入処理をします。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター</param>
        public void InsertOpticalDriveMedia(char driveLetter)
        {
            var path = driveLetter + @":\";
            OpticalDriveMediaInserted?.Invoke(this, new DirectoryChangedEventArgs(path));
        }

        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター></param>
        public void EjectOpticalDriveMedia(char driveLetter)
        {
            var path = driveLetter + @":\";
            OpticalDriveMediaEjected?.Invoke(this, new DirectoryChangedEventArgs(path));
        }
        #endregion リムーバブルディスクの着脱処理

    }
}
