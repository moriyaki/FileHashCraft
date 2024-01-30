/*  FileManager.cs

    ファイルの管理とスキャンをします。
 */
using System.IO;

namespace FileHashCraft.Models
{
    // TODO : Hidden と ReadOnly もスキャンできるようにする
    public static class FileManager
    {
        #region ディレクトリとファイルのスキャン関連
        /// <summary>
        /// 除外するファイルエントリかどうかを判断する。
        /// </summary>
        /// <param name="fullPath">ファイルエントリのフルパス</param>
        /// <returns>除外するかどうか</returns>
        public static bool IsIgnoreFileEntries(string fullPath)
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

            // ごみ箱は除外する
            if (fullPath.Contains("$RECYCLE.BIN")) return true;

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
        public static IEnumerable<string> EnumerateDirectories(string directoryFullPath)
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
        public static IEnumerable<string> EnumerateFiles(string directoryFullPath)
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
        public static IEnumerable<string> EnumerateFileSystemEntries(string itemFullPath)
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
