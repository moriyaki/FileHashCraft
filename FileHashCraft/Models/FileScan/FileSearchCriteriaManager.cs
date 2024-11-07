using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.Models.FileScan
{
    #region インターフェース
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
    #endregion インターフェース

    public class FileSearchCriteriaManager : IFileSearchCriteriaManager
    {
        private readonly IMessenger _Messanger;

        public FileSearchCriteriaManager()
        { throw new NotImplementedException(nameof(IFileSearchCriteriaManager)); }

        public FileSearchCriteriaManager(IMessenger messenger)
        {
            _Messanger = messenger;
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
        }
    }
}
