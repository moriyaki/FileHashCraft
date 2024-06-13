using System.IO;
using System.Text.RegularExpressions;

namespace FileHashCraft.Models.FileScan
{
    public interface IScannedFilesManager
    {
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        void AddFile(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        HashSet<HashFile> AllFiles { get; }
        /// <summary>
        /// 属性条件に合致する全てのファイル数を取得します。
        /// </summary>
        int GetAllFilesCount(bool includeHidden, bool includeReadOnly);
        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        int GetAllCriteriaFilesCount(bool includeHidden, bool includeReadOnly);
        /// <summary>
        /// 検索条件に合致するファイルを取得する
        /// </summary>
        HashSet<HashFile> GetAllCriteriaFileName(bool includeHidden, bool includeReadOnly);
        /// <summary>
        /// ファイルが検索条件に合致しているかを取得します。
        /// </summary>
        bool IsCriteriaFile(string fileFullPath, bool includeHidden, bool includeReadOnly);
    }

    public class ScannedFilesManager : IScannedFilesManager
    {
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public HashSet<HashFile> AllFiles { get; } = [];

        /// <summary>
        /// ファイルを追加します
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        /// <param name="hashSHA256">SHA256のハッシュ</param>
        /// <param name="hashSHA384">SHA384のハッシュ</param>
        /// <param name="hashSHA512">SHA512のハッシュ</param>
        public void AddFile(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "")
        {
            var fileInfo = new FileInfo(fileFullPath);
            var existingFIle = AllFiles.FirstOrDefault(f => f.FileFullPath == fileFullPath);
            if (existingFIle != null)
            {
                AllFiles.Remove(existingFIle);
            }

            var hashFile = new HashFile(fileFullPath, hashSHA256, hashSHA384, hashSHA512);
            AllFiles.Add(hashFile);
        }

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object lockObject = new();

        /// <summary>
        /// ファイルが属性対象かどうかを取得します。
        /// </summary>
        /// <param name="file">HashFile型のファイル情報</param>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>属性対象かどうか</returns>
        private static bool MatchFileAttributesCriteria(HashFile file, bool includeHidden, bool includeReadOnly)
        {
            var isReadOnly = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            var matchesReadOnlyCriteria = includeReadOnly || !isReadOnly;

            var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            var matchesHiddenCriteria = includeHidden || !isHidden;

            return matchesReadOnlyCriteria && matchesHiddenCriteria;
        }

        /// <summary>
        /// ワイルドカードを含むファイル名をRegex型に変換します。
        /// </summary>
        /// <param name="WildcardPattern">ワイルドカードを含むファイル名</param>
        /// <returns>Regex型</returns>
        public static Regex WildcardToRegexPattern(string WildcardPattern)
        {
            if (WildcardPattern.Contains('.'))
            {
                // ワイルドカードに拡張子がある時の処理
                var fileName = Path.GetFileNameWithoutExtension(WildcardPattern);
                var fileNamePattern = Regex.Escape(fileName).Replace("\\*", ".*").Replace("\\?", ".");

                var fileExtention = Path.GetExtension(WildcardPattern);
                if (fileExtention == ".*")
                {
                    // 拡張子全ての時、例えば"file"と"file.txt"の両方をヒットさせる
                    return new Regex("^" + fileNamePattern + "(?:\\..+)?$", RegexOptions.IgnoreCase);
                }
                else
                {
                    var extensionPattern = Regex.Escape(fileExtention).Replace("\\*", ".*").Replace("\\?", ".");
                    return new Regex("^" + fileNamePattern + extensionPattern + "$", RegexOptions.IgnoreCase);
                }
            }
            else
            {
                // ワイルドカードに拡張子がない時の処理
                var fileNamePattern = Regex.Escape(WildcardPattern).Replace("\\*", ".*").Replace("\\?", ".");
                return new Regex("^" + fileNamePattern + "$", RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// 属性条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>属性条件に合致するファイル</returns>
        public int GetAllFilesCount(bool includeHidden, bool includeReadOnly)
        {
            return AllFiles.Count(c => MatchFileAttributesCriteria(c, includeHidden, includeReadOnly));
        }

        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>検索条件合致ファイル数</returns>
        public int GetAllCriteriaFilesCount(bool includeHidden, bool includeReadOnly)
        {
            lock (lockObject)
            {
                return GetAllCriteriaFileName(includeHidden, includeReadOnly).Count;
            }
        }

        /// <summary>
        /// 検索条件に合致するファイルを取得します。
        /// </summary>
        /// <returns>検索条件合致ファイル</returns>
        public HashSet<HashFile> GetAllCriteriaFileName(bool includeHidden, bool includeReadOnly)
        {
            var files = new HashSet<HashFile>();
            foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
            {
                switch (criteria.SearchOption)
                {
                    case FileSearchOption.Extention:
                        var extentionFiles = AllFiles.Where(f =>
                            String.Equals(criteria.SearchPattern,
                            Path.GetExtension(f.FileFullPath),
                            StringComparison.CurrentCultureIgnoreCase)
                        && MatchFileAttributesCriteria(f, includeHidden, includeReadOnly))
                        .Select(f => f);
                        files.UnionWith(extentionFiles);
                        break;
                    case FileSearchOption.Wildcard:
                        var wildcardRegex = WildcardToRegexPattern(criteria.SearchPattern);
                        var wildcardFiles = AllFiles.Where(f =>
                            wildcardRegex.IsMatch(f.FileFullPath)
                        && MatchFileAttributesCriteria(f, includeHidden, includeReadOnly))
                        .Select(f => f);
                        files.UnionWith(wildcardFiles);
                        break;
                    case FileSearchOption.Regex:
                        break;
                    default:
                        throw new NotFiniteNumberException("GetAllCriteriaFilesCount");
                }
            }
            return files;
        }

        /// <summary>
        /// ファイルが検索条件に合致しているかを取得します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        /// <returns>検索条件に合致してるかどうか</returns>
        public bool IsCriteriaFile(string fileFullPath, bool includeHidden, bool includeReadOnly)
        {
            int count = 0;
            var file = AllFiles.FirstOrDefault(c => c.FileFullPath == fileFullPath);
            if (file == null) { return false; }
            foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
            {
                switch (criteria.SearchOption)
                {
                    case FileSearchOption.Extention:
                        count = AllFiles.Count(c =>
                        {
                            return String.Equals(
                                criteria.SearchPattern,
                                Path.GetExtension(c.FileFullPath),
                                StringComparison.CurrentCultureIgnoreCase)
                            && MatchFileAttributesCriteria(c, includeHidden, includeReadOnly);
                        });
                        if (count > 0) { return true; } else { count = 0; }
                        break;
                    case FileSearchOption.Wildcard:
                        var wildcardRegex = WildcardToRegexPattern(criteria.SearchPattern);
                        if (wildcardRegex.IsMatch(file.FileFullPath) && MatchFileAttributesCriteria(file, includeHidden, includeReadOnly))
                        { return true; }
                        break;
                    case FileSearchOption.Regex:
                        break;
                    default:
                        throw new NotFiniteNumberException("IsCriteriaFile");
                }
            }
            return false;
        }
    }
}
