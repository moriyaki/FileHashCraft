using System.IO;
using FilOps.Models.WindowsAPI;

namespace FilOps.Models.StorageOperation
{
    #region ディレクトリ情報
    /// <summary>
    /// ディレクトリ情報
    /// </summary>
    public class DirInformation
    {
        /// <summary>
        /// ディレクトリのフルパス
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// ドライブが準備できているかどうか
        /// </summary>
        public bool DriveIsReady { get; set; } = false;

        /// <summary>
        /// 子ディレクトリが存在するかどうか
        /// </summary>
        public bool HasChildren { get; set; } = false;
    }
    #endregion ディレクトリ情報

    #region ディレクトリの管理
    /// <summary>
    /// ディレクトリの管理
    /// </summary>
    public class Dirs
    {
        /// <summary>
        /// 指定されたディレクトリに子ディレクトリが存在するかどうかを判定します。
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>子ディレクトリが存在する場合は true、それ以外は false</returns>
       private static bool HasChildrenDirectories(string full_path)
        {
            try
            {
                var HasDirectory = Directory.EnumerateDirectories(full_path).Any();
                if (HasDirectory)
                {
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// 特殊フォルダをスキャンして情報を取得します。
        /// </summary>
        /// <returns>特殊フォルダの情報のコレクション</returns>
        public static IEnumerable<DirInformation> SpecialFolderScan()
        {
            IEnumerable<string> special_folder_path =
            [
                WindowsSpecialFolder.GetPath(KnownFolder.Objects3D),
                WindowsSpecialFolder.GetPath(KnownFolder.Downloads),
                WindowsSpecialFolder.GetPath(KnownFolder.Desktop),
                WindowsSpecialFolder.GetPath(KnownFolder.Documents),
                WindowsSpecialFolder.GetPath(KnownFolder.Pictures),
                WindowsSpecialFolder.GetPath(KnownFolder.Videos),
                WindowsSpecialFolder.GetPath(KnownFolder.Music),
                WindowsSpecialFolder.GetPath(KnownFolder.User),
            ];

            foreach (var special_folder in special_folder_path)
            {
                var item = new DirInformation
                {
                    FullPath = special_folder,
                    DriveIsReady = true,
                };
                item.HasChildren = HasChildrenDirectories(item.FullPath);

                yield return item;
            }
        }

        /// <summary>
        /// ルートドライブをスキャンして情報を取得します。
        /// </summary>
        /// <returns>ルートドライブのフォルダ情報のコレクション</returns>
        public static IEnumerable<DirInformation> DriveScan()
        {
            foreach (var dir in DriveInfo.GetDrives())
            {
                var item = new DirInformation
                {
                    FullPath = dir.Name,
                    DriveIsReady = dir.IsReady,
                };
                item.HasChildren = item.DriveIsReady && HasChildrenDirectories(item.FullPath);

                yield return item;
            }
        }
        
        /// <summary>
        /// 指定したディレクトリをスキャンして情報を取得します。
        /// </summary>
        /// <param name="path">スキャンするディレクトリのパス</param>
        /// <returns>フォルダ情報のコレクション</returns>
        private static IEnumerable<DirInformation> DirectoryScan(string path)
        {
            IEnumerable<string> folders;
            try
            {
                folders = Directory.EnumerateDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }

            foreach (var folder in folders)
            {
                FileAttributes attributes = File.GetAttributes(folder);
                if ((attributes & FileAttributes.System) != FileAttributes.System)
                {
                    var item = new DirInformation
                    {
                        FullPath = Path.Combine(path, folder),
                    };
                    item.HasChildren = HasChildrenDirectories(item.FullPath);
                    yield return item;
                }
            }
        }

        /// <summary>
        /// ディレクトリ情報キャッシュのコレクション
        /// </summary>
        private readonly Dictionary<string, IEnumerable<DirInformation>> DirectoryCache = [];

        /// <summary>
        /// ディレクトリ情報のキャッシュからディレクトリ情報のコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを得るディレクトリのパス</param>
        /// <returns>フォルダ情報のコレクション</returns>
        public IEnumerable<DirInformation> GetDirInformation(string path)
        {
            if (DirectoryCache.TryGetValue(path, out var result))
            {
                foreach (var item in result)
                {
                    yield return item;
                }
            }
            else
            {
                List<DirInformation> newFolder = DirectoryScan(path).ToList();
                DirectoryCache[path] = newFolder;
                foreach (var folder in newFolder)
                {
                    yield return folder;
                }
            }
        }
    }
    #endregion ディレクトリの管理

    #region ファイル情報
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// ファイルのフルパス
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// ファイルの更新日時
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        public long FileSize { get; set; } = 0;

        /// <summary>
        /// ファイルがディレクトリかどうか
        /// </summary>
        public bool IsDirectory { get; set; } = false;
    }
    #endregion ファイル情報

    #region ファイルの管理
    public class Files
    {
        /// <summary>
        /// 指定したディレクトリ内のフォルダとファイルをスキャンします
        /// </summary>
        /// <param name="path">スキャンするディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public static IEnumerable<FileInformation> FolderFileScan(string path)
        {
            IEnumerable<string> folders;
            try
            {
                folders = Directory.EnumerateDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }
            catch (IOException)
            {
                yield break;
            }
            foreach (var folder in folders)
            {
                FileAttributes attributes = File.GetAttributes(folder);
                if ((attributes & FileAttributes.System) != FileAttributes.System)
                {
                    var fileInfo = new FileInfo(folder);
                    var item = new FileInformation
                    {
                        FullPath = fileInfo.FullName,
                        LastModifiedDate = fileInfo.LastWriteTime,
                        IsDirectory = true
                    };
                    yield return item;
                }
            }
            foreach (var file in Directory.EnumerateFiles(path))
            {
                try
                {
                    using FileStream fs = File.OpenRead(file);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (IOException) { }

                var fileInfo = new FileInfo(file);
                var item = new FileInformation
                {
                    FullPath = fileInfo.FullName,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    IsDirectory = false
                };
                yield return item;

            }
        }

        /// <summary>
        /// ファイル情報キャッシュのコレクション
        /// </summary>
        private readonly Dictionary<string, IEnumerable<FileInformation>> FilesCache = [];

        /// <summary>
        /// ファイル情報のキャッシュからディレクトリ情報のコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを得るディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public IEnumerable<FileInformation> GetFilesInformation(string path)
        {
            if (FilesCache.TryGetValue(path, out var result))
            {
                foreach (var item in result)
                {
                    yield return item;
                }
            }
            else
            {
                List<FileInformation> newFiles = FolderFileScan(path).ToList();
                FilesCache[path] = newFiles;
                foreach (var fileInfo in newFiles)
                {
                    yield return fileInfo;
                }
            }
        }
    }
    #endregion ファイルの管理
}
