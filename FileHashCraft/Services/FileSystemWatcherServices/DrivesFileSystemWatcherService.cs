/*  DrivesFileSystemWatcherService.cs

    ドライブ以下全てのファイルを監視するクラスです。
 */
using System.IO;
using FileHashCraft.Models.Helpers;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Services.FileSystemWatcherServices
{
    #region インターフェース
    public interface IFileWatcherService
    {
        /// <summary>
        /// ドライブに対して、ファイルアイテム変更監視の設定をします。
        /// </summary>
        void SetRootDirectoryWatcher(FileItemInformation rootDrive);
        /// <summary>
        /// リムーバブルドライブの追加または挿入処理をします。
        /// </summary>
        void InsertOpticalDriveMedia(char driveLetter);
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います。
        /// </summary>
        void EjectOpticalDriveMedia(char driveLetter);
    }
    #endregion インターフェース

    public class DrivesFileSystemWatcherService : IFileWatcherService
    {
        #region コンストラクタ
        /// <summary>
        /// ドライブ内のディレクトリ変更を監視するインスタンス
        /// </summary>
        private readonly List<FileSystemWatcher> DrivesWatcher = [];

        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public DrivesFileSystemWatcherService()
        {
            throw new NotImplementedException();
        }

        private readonly IFileSystemServices _fileSystemService;
        private readonly ITreeManager _directoryTreeManager;

        /// <summary>
        /// コンストラクタインジェクションをします。
        /// </summary>
        /// <param name="directoryTreeManager">IDirectoryTreeManager</param>
        public DrivesFileSystemWatcherService(
            IFileSystemServices fileSystemServices,
            ITreeManager directoryTreeManager
        )
        {
            _fileSystemService = fileSystemServices;
            _directoryTreeManager = directoryTreeManager;
        }
        #endregion コンストラクタ

        #region ディレクトリ変更通知処理
        /// <summary>
        /// ドライブに対して、ファイルアイテム変更監視の設定をします。
        /// </summary>
        /// <param name="rootDrive">ドライブルートのファイルアイテム</param>
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

                    watcher.Created += OnDirectoryCreated;
                    watcher.Changed += OnDirectoryChanged;
                    watcher.Renamed += OnDirectoryRenamed;

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
            // ごみ箱は通知を必要とする
            if (fullPath.Contains("$RECYCLE.BIN"))
            {
                return false;
            }
            // 展開マネージャに登録されてないディレクトリ、またはその親が登録されてない場合は通知しない
            if (!(_directoryTreeManager.IsExpandedDirectory(fullPath) || _directoryTreeManager.IsExpandedDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty)))
            {
                return true;
            }

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
            DebugManager.ExceptionWrite($"FileSystemWatcher エラー: {e.GetException()}");
        }

        /// <summary>
        /// ファイルアイテムが変更された時のイベントをを処理し、削除を捕捉ます。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnDirectoryChanged(object? sender, FileSystemEventArgs e)
        {
            if (IsEventNotCatch(e.FullPath)) return;
            //WeakReferenceMessenger.Default.Send(new DirectoryItemDeleted(e.FullPath));
            _fileSystemService.SendDirectoryItemDeleted(e.FullPath);
        }

        /// <summary>
        /// ファイルアイテムが作成された時のイベントをを処理します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">FileSystemEventArgs</param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnDirectoryCreated(object sender, FileSystemEventArgs e)
        {
            if (IsEventNotCatch(e.FullPath)) return;
            //WeakReferenceMessenger.Default.Send(new DirectoryItemCreated(e.FullPath));
            _fileSystemService.SendDirectoryItemCreated(e.FullPath);
        }

        /// <summary>
        /// ファイルアイテムが名前変更された時のイベントをを処理します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">RenamedEventArgs</param>
        private void OnDirectoryRenamed(object sender, RenamedEventArgs e)
        {
            if (IsEventNotCatch(e.FullPath)) return;
            //WeakReferenceMessenger.Default.Send(new DirectoryItemRenamed(e.OldFullPath, e.FullPath));
            _fileSystemService.SendDirectoryItemRenamed(e.OldFullPath, e.FullPath);
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
            //WeakReferenceMessenger.Default.Send(new OpticalDriveMediaInserted(path));
            _fileSystemService.SendInsertOpticalMedia(path);
        }

        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター></param>
        public void EjectOpticalDriveMedia(char driveLetter)
        {
            var path = driveLetter + @":\";
            //WeakReferenceMessenger.Default.Send(new OpticalDriveMediaEjected(path));
            _fileSystemService.SendEjectOpticalMedia(path);
        }
        #endregion リムーバブルディスクの着脱処理
    }
}
