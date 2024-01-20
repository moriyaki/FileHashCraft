using System.IO;
using System.Runtime.InteropServices;

namespace FileHashCraft.Models
{
    #region インターフェース
    public interface IFileManager
    {
        /// <summary>
        /// 除外するファイルかどうかを判定します。(true = 除外)
        /// </summary>
        public bool IsIgnoreFileEntries(string fullPath);
        /// <summary>
        /// 指定したディレクトリ内のディレクトリをスキャンします。
        /// </summary>
        public IEnumerable<string> EnumerateDirectories(string fullPath);
        /// <summary>
        /// 指定したディレクトリ内のファイルをスキャンします。
        /// </summary>
        public IEnumerable<string> EnumerateFiles(string fullPath);
        /// <summary>
        /// 指定したディレクトリの内ファイルアイテムをスキャンします。
        /// </summary>
        public IEnumerable<string> EnumerateFileSystemEntries(string fullPath);
    }
    #endregion インターフェース

    // TODO : Hidden と ReadOnly もスキャンできるようにする
    public class FileManager : IFileManager
    {
        #region ディレクトリとファイルのスキャン関連
        /// <summary>
        /// 除外するファイルエントリかどうかを判断する。
        /// </summary>
        /// <param name="fullPath">ファイルエントリのフルパス</param>
        /// <returns>除外するかどうか</returns>
        public bool IsIgnoreFileEntries(string fullPath)
        {
            List<string> ignoreDirectory =
                [
                    // C:\Program Files
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    // C:\Program Files (x86)
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    // ユーザーのAppData
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    // C:\Windows
                    Environment.GetEnvironmentVariable("SystemRoot") ?? throw new InvalidOperationException("%SystemRoot% not found!"),
                    // C:\Progra Data
                    Environment.GetEnvironmentVariable("ProgramData") ?? throw new InvalidOperationException("%ProgramData% not found!"),
                    // テンポラリフォルダ
                    Path.GetTempPath(),
                ];

            FileAttributes attributes = File.GetAttributes(fullPath);

            // システム、隠しファイルファイルは除外する
            if ((attributes & FileAttributes.System) == FileAttributes.System) return true;
            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden) return true;

            // システム系ディレクトリは除外する
            if (ignoreDirectory.Any(ignore => fullPath.StartsWith(ignore, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 指定したディレクトリ内のディレクトリをスキャンします。
        /// </summary>
        /// <param name="directoryFullPath">スキャンするディクレトリのフルパス</param>
        /// <returns>ディレクトリ情報のコレクション</returns>
        public IEnumerable<string> EnumerateDirectories(string directoryFullPath)
        {
            IEnumerable<string> dirs;
            try
            {
                 dirs = Directory.EnumerateDirectories(directoryFullPath);
            }
            catch (UnauthorizedAccessException) { yield break; }

            // ディレクトリを取得します
            foreach (var dir in dirs)
            {
                // システムディレクトリの除外
                if (IsIgnoreFileEntries(dir)) continue;

                yield return dir;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のファイルをスキャンします。
        /// </summary>
        /// <param name="directoryFullPath">スキャンするディクレトリのフルパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public IEnumerable<string> EnumerateFiles(string directoryFullPath)
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directoryFullPath);
            }
            catch (UnauthorizedAccessException) { yield break; }

            foreach (var file in files)
            {
                // システムディレクトリの除外
                if (IsIgnoreFileEntries(file)) continue;

                yield return file;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のディレクトリとファイルをスキャンします。
        /// </summary>
        /// <param name="itemFullPath">スキャンするディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public IEnumerable<string> EnumerateFileSystemEntries(string itemFullPath)
        {
            // ディレクトリをスキャンします
            foreach (var dir in EnumerateDirectories(itemFullPath))
            {
                yield return dir;
            }

            // ファイルもスキャンします
            foreach (var file in EnumerateFiles(itemFullPath))
            {
                yield return file;
            }
        }
        #endregion ディレクトリとファイルのスキャン関連
    }
}
