using System.IO;
using System.Windows.Controls;

namespace FilOps.Models
{

    #region ディレクトリとファイル情報
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// ディレクトリのフルパス
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// ドライブが準備できているかどうか
        /// </summary>
        public bool IsReady { get; set; } = false;

        /// <summary>
        /// ファイルがディレクトリかどうか
        /// </summary>
        public bool IsDirectory { get; set; } = false;

        /// <summary>
        /// 子ディレクトリが存在するかどうか
        /// </summary>
        public bool HasChildren { get; set; } = false;

        /// <summary>
        /// ファイルの更新日時
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        public long FileSize { get; set; } = 0;

    }
    #endregion ディレクトリとファイル情報

    #region ディレクトリとファイルの管理
    public class Files
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
        public static IEnumerable<FileInformation> SpecialFolderScan()
        {
            IEnumerable<string> special_folder_path =
            [
                WindowsAPI.GetPath(KnownFolder.Objects3D),
                WindowsAPI.GetPath(KnownFolder.Downloads),
                WindowsAPI.GetPath(KnownFolder.Desktop),
                WindowsAPI.GetPath(KnownFolder.Documents),
                WindowsAPI.GetPath(KnownFolder.Pictures),
                WindowsAPI.GetPath(KnownFolder.Videos),
                WindowsAPI.GetPath(KnownFolder.Music),
                WindowsAPI.GetPath(KnownFolder.User),
            ];

            foreach (var folder in special_folder_path)
            {
                var fileInfo = new FileInfo(folder);
                var item = new FileInformation
                {
                    FullPath = folder,
                    IsReady = true,
                    IsDirectory = true,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = 0,
                };
                item.HasChildren = HasChildrenDirectories(item.FullPath);

                yield return item;
            }
        }

        /// <summary>
        /// ルートドライブをスキャンして情報を取得します。
        /// </summary>
        /// <returns>ルートドライブのフォルダ情報のコレクション</returns>
        public static IEnumerable<FileInformation> DriveScan()
        {
            foreach (var dir in DriveInfo.GetDrives())
            {
                var fileInfo = new FileInfo(dir.Name);
                var item = new FileInformation
                {
                    FullPath = dir.Name,
                    IsReady = dir.IsReady,
                    IsDirectory = true,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = 0,
                };
                item.HasChildren = item.IsReady && HasChildrenDirectories(item.FullPath);

                yield return item;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のフォルダとファイルをスキャンします
        /// </summary>
        /// <param name="path">スキャンするディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public static IEnumerable<FileInformation> FileItemScan(string path)
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
                        IsDirectory = true,
                        IsReady = true,
                        LastModifiedDate = fileInfo.LastWriteTime,
                        FileSize = 0,
                    };
                    item.HasChildren = HasChildrenDirectories(item.FullPath);
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
                    IsDirectory = false,
                    IsReady = true,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    HasChildren = false,
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
                List<FileInformation> newFiles = FileItemScan(path).ToList();
                FilesCache[path] = newFiles;
                foreach (var fileInfo in newFiles)
                {
                    yield return fileInfo;
                }
            }
        }
    }
    #endregion ディレクトリとファイルの管理
}
