using System.IO;

namespace FileHashCraft.Models
{
    public sealed class FileManager
    {
        public static FileManager Instance { get; } = new();
        private FileManager() { }

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
        /// <param name="fullPath">スキャンするディクレトリのフルパス</param>
        /// <returns>ディレクトリ情報のコレクション</returns>
        public IEnumerable<string> EnumerateDirectories(string fullPath)
        {
            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(fullPath);
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is IOException) { yield break; }
                throw;
            }

            // ディレクトリを取得します
            foreach (var dir in directories)
            {
                // システムディレクトリの除外
                if (IsIgnoreFileEntries(dir)) continue;

                yield return dir;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のファイルをスキャンします。
        /// </summary>
        /// <param name="fullPath">スキャンするディクレトリのフルパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public IEnumerable<string> EnumerateFiles(string fullPath)
        {
            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(fullPath);
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is IOException) { yield break; }
                throw;
            }

            foreach (var file in files)
            {
                try
                {
                    using FileStream fs = File.OpenRead(file);
                }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { }

                // システムディレクトリの除外
                if (IsIgnoreFileEntries(file)) continue;

                yield return file;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のディレクトリとファイルをスキャンします。
        /// </summary>
        /// <param name="fullPath">スキャンするディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public IEnumerable<string> ScanFiles(string fullPath)
        {
            // ディレクトリをスキャンします
            foreach (var dir in EnumerateDirectories(fullPath))
            {
                yield return dir;
            }

            // ファイルもスキャンします
            foreach (var file in EnumerateFiles(fullPath))
            {
                yield return file;
            }
        }
        #endregion ディレクトリとファイルのスキャン関連
    }
}
