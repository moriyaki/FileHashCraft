using System.IO;
using System.Windows;

namespace FileHashCraft.Models.FileScan
{
    public interface IScannedFilesManager
    {
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        public void AddFile(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public HashSet<HashFile> AllFiles { get; }
        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        public int GetAllCriteriaFileCount();
        /// <summary>
        /// 検索条件に合致するファイルを取得する
        /// </summary>
        public HashSet<HashFile> GetAllCriteriaFileName();
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
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <returns>検索条件合致ファイル数</returns>
        public int GetAllCriteriaFileCount()
        {
            lock (lockObject)
            {
                var count = 0;
                foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
                {
                    if (criteria.SearchOption == FileSearchOption.Extention)
                    {
                        count += AllFiles.Count(
                            c => String.Equals(criteria.SearchPattern, Path.GetExtension(c.FileFullPath), StringComparison.CurrentCultureIgnoreCase));
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
            foreach (var criteria in FileSearchCriteriaManager.AllCriteria)
            {
                if (criteria.SearchOption == FileSearchOption.Extention)
                {
                    return AllFiles.Where(
                        f => String.Equals(criteria.SearchPattern, Path.GetExtension(f.FileFullPath), StringComparison.CurrentCultureIgnoreCase)).ToHashSet();
                }
            }
            return [];
        }
    }
}
