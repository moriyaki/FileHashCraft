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
}