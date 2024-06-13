namespace FileHashCraft.Models.FileScan
{
    /// <summary>
    /// ファイル検索の種類
    /// </summary>
    public enum FileSearchOption
    {
        Extention,
        Wildcard,
        Regex
    }

    public class FileSearchCriteria
    {
        /// <summary>
        /// 検索種類(変更不可)
        /// </summary>
        public FileSearchOption SearchOption { get; }
        /// <summary>
        /// 検索文字列
        /// </summary>
        private string searchPattern = string.Empty;
        public string SearchPattern
        {
            get => searchPattern;
            set => searchPattern = value ?? string.Empty;
        }

        public FileSearchCriteria(FileSearchOption searchOption, string searchPattern)
        {
            SearchOption = searchOption;
            SearchPattern = searchPattern ?? string.Empty;
        }
    }

    public static class FileSearchCriteriaManager
    {
        /// <summary>
        /// 全検索条件
        /// </summary>
        public static List<FileSearchCriteria> AllCriteria { get; } = [];

        /// <summary>
        /// 検索条件を追加します。
        /// </summary>
        /// <param name="criteria">追加する検索条件</param>
        public static void AddCriteria(FileSearchCriteria criteria)
        {
            AllCriteria.Add(criteria);
        }

        /// <summary>
        /// 検索条件の追加
        /// </summary>
        /// <param name="pattern">検索条件</param>
        public static void AddCriteria(string pattern, FileSearchOption criteriaType)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == pattern && e.SearchOption == criteriaType);

            if (foundCriteria != null) { return; }

            AllCriteria.Add(new FileSearchCriteria(criteriaType, pattern));
        }

        /// <summary>
        /// 検索条件の削除
        /// </summary>
        /// <param name="pattern">検索条件</param>
        public static void RemoveCriteria(string pattern, FileSearchOption criteriaType)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == pattern && e.SearchOption == criteriaType);

            if (foundCriteria == null) { return; }

            AllCriteria.Remove(foundCriteria);
        }
    }
}
