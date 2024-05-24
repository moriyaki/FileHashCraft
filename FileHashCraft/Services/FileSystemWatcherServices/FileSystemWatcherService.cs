namespace FileHashCraft.Services.FileSystemWatcherServices
{
    public interface IFileSystemWatcherService
    {
        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をします。
        /// </summary>
        public void SetCurrentDirectoryWatcher(string currentDirectory);
        /// <summary>
        /// ドライブに対して、ファイルアイテム変更監視の設定をします。
        /// </summary>
        public void SetRootDirectoryWatcher(FileItemInformation rootDrive);
        /// <summary>
        /// リムーバブルドライブの追加または挿入処理をします。
        /// </summary>
        public void InsertOpticalDriveMedia(char driveLetter);
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います。
        /// </summary>
        public void EjectOpticalDriveMedia(char driveLetter);
    }

    public class FileSystemWatcherService : IFileSystemWatcherService
    {
        private readonly ICurrentDirectoryFIleSystemWatcherService _currentDirectoryFIleSystemWatcherService;
        private readonly IFileWatcherService _drivesFileSystemWatcherService;

        public FileSystemWatcherService() { throw new NotImplementedException(); }
        public FileSystemWatcherService(
            ICurrentDirectoryFIleSystemWatcherService currentDirectoryFIleSystemWatcherService,
            IFileWatcherService drivesFileSystemWatcherService)
        {
            _currentDirectoryFIleSystemWatcherService = currentDirectoryFIleSystemWatcherService;
            _drivesFileSystemWatcherService = drivesFileSystemWatcherService;
        }

        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をします。
        /// </summary>
        /// <param name="currentDirectory">カレントディレクトリのフルパス</param>
        public void SetCurrentDirectoryWatcher(string currentDirectory) =>
            _currentDirectoryFIleSystemWatcherService.SetCurrentDirectoryWatcher(currentDirectory);

        /// <summary>
        /// ドライブに対して、ファイルアイテム変更監視の設定をします。
        /// </summary>
        /// <param name="rootDrive">ドライブルートのファイルアイテム</param>
        public void SetRootDirectoryWatcher(FileItemInformation rootDrive) =>
            _drivesFileSystemWatcherService.SetRootDirectoryWatcher(rootDrive);

        /// <summary>
        /// リムーバブルドライブの追加または挿入処理をします。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター</param>
        public void InsertOpticalDriveMedia(char driveLetter) =>
            _drivesFileSystemWatcherService.InsertOpticalDriveMedia(driveLetter);

        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター></param>
        public void EjectOpticalDriveMedia(char driveLetter) =>
            _drivesFileSystemWatcherService.EjectOpticalDriveMedia(driveLetter);
    }
}
