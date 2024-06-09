using System.IO;

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
        int GetAllFilesCount(bool includeHidden = false, bool includeReadOnly = false);
        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        int GetAllCriteriaFilesCount(bool includeHidden = false, bool includeReadOnly = false);
        /// <summary>
        /// 検索条件に合致するファイルを取得する
        /// </summary>
        HashSet<HashFile> GetAllCriteriaFileName();
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

        private static bool MatchFileAttributesCriteria(HashFile file, bool includeHidden = false, bool includeReadOnly = false)
        {
            var isReadOnly = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            var matchesReadOnlyCriteria = includeReadOnly || !isReadOnly;

            var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            var matchesHiddenCriteria = includeHidden || !isHidden;

            return matchesReadOnlyCriteria && matchesHiddenCriteria;
        }

        /// <summary>
        /// 属性条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>属性条件に合致するファイル</returns>
        public int GetAllFilesCount(bool includeHidden = false, bool includeReadOnly = false)
        {
            return AllFiles.Count(c => MatchFileAttributesCriteria(c, includeHidden, includeReadOnly));
        }

        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>検索条件合致ファイル数</returns>
        public int GetAllCriteriaFilesCount(bool includeHidden = false, bool includeReadOnly = false)
        {
            lock (lockObject)
            {
                var count = 0;
                foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
                {
                    switch (criteria.SearchOption)
                    {
                        case FileSearchOption.Extention:
                            count += AllFiles.Count(c =>
                            {
                                return String.Equals(
                                    criteria.SearchPattern,
                                    Path.GetExtension(c.FileFullPath),
                                    StringComparison.CurrentCultureIgnoreCase)
                                && MatchFileAttributesCriteria(c, includeHidden, includeReadOnly);
                            });
                            break;
                        case FileSearchOption.Wildcar:
                            break;
                        case FileSearchOption.Regex:
                            break;
                        default:
                            throw new NotFiniteNumberException("GetAllCriteriaFilesCount");
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// 検索条件に合致するファイルを取得する
        /// </summary>
        /// <returns>検索条件合致ファイル</returns>
        public HashSet<HashFile> GetAllCriteriaFileName()
        {
            var files = new HashSet<HashFile>();
            foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
            {
                switch (criteria.SearchOption)
                {
                    case FileSearchOption.Extention:
                        files = AllFiles.Where(
                        f => String.Equals(criteria.SearchPattern, Path.GetExtension(f.FileFullPath), StringComparison.CurrentCultureIgnoreCase)).ToHashSet();
                        break;
                    case FileSearchOption.Wildcar:
                        break;
                    case FileSearchOption.Regex:
                        break;
                    default:
                        throw new NotFiniteNumberException("GetAllCriteriaFilesCount");
                }
            }
            return files;
        }
    }
}
