using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;

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

    public interface IFileSearchCriteriaManager
    {
        /// <summary>
        /// 全検索条件
        /// </summary>
        List<FileSearchCriteria> AllCriteria { get; }
        /// <summary>
        /// FileSearchCriteria型で検索条件を追加します。
        /// </summary>
        void AddCriteria(FileSearchCriteria criteria);
        /// <summary>
        /// 検索条件を追加します。
        /// </summary>
        void AddCriteria(string pattern, FileSearchOption criteriaType);
        /// <summary>
        /// 検索条件を削除します。
        /// </summary>
        void RemoveCriteria(string pattern, FileSearchOption criteriaType);
    }

    public class FileSearchCriteriaManager : IFileSearchCriteriaManager
    {
        private readonly IMessenger _messenger;

        public FileSearchCriteriaManager() { throw new NotImplementedException(nameof(IFileSearchCriteriaManager)); }
        public FileSearchCriteriaManager(IMessenger messenger)
        {
            _messenger = messenger;
        }

        /// <summary>
        /// 全検索条件
        /// </summary>
        public List<FileSearchCriteria> AllCriteria { get; } = [];

        /// <summary>
        /// FileSearchCriteria型で検索条件を追加します。
        /// </summary>
        /// <param name="criteria">追加する検索条件</param>
        public void AddCriteria(FileSearchCriteria criteria)
        {
            AllCriteria.Add(criteria);
        }

        /// <summary>
        /// 検索条件を追加します。
        /// </summary>
        /// <param name="pattern">検索条件</param>
        public void AddCriteria(string pattern, FileSearchOption criteriaType)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == pattern && e.SearchOption == criteriaType);

            if (foundCriteria != null) { return; }

            AllCriteria.Add(new FileSearchCriteria(criteriaType, pattern));
            //_messenger.Send(new ChangeSelectedCountMessage());
        }

        /// <summary>
        /// 検索条件を削除します。
        /// </summary>
        /// <param name="pattern">検索条件</param>
        public void RemoveCriteria(string pattern, FileSearchOption criteriaType)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == pattern && e.SearchOption == criteriaType);

            if (foundCriteria == null) { return; }

            AllCriteria.Remove(foundCriteria);
            //_messenger.Send(new ChangeSelectedCountMessage());
        }
    }
}
