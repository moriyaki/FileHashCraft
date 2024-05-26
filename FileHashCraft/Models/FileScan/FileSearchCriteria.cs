using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashCraft.Models.FileScan
{
    /// <summary>
    /// ファイル検索の種類
    /// </summary>
    public enum FileSearchOption
    {
        Extention,
        Wildcar,
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
        /// 拡張子の条件追加
        /// </summary>
        /// <param name="extention"></param>
        public static void AddCriteriaExtention(string extention)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == extention && e.SearchOption == FileSearchOption.Extention);

            if (foundCriteria != null) { return; }

            AllCriteria.Add(new FileSearchCriteria(FileSearchOption.Extention, extention));
        }

        /// <summary>
        /// 拡張子の条件削除
        /// </summary>
        /// <param name="extention"></param>
        public static void RemoveCriteriaExtention(string extention)
        {
            var foundCriteria = AllCriteria.Find(
                e => e.SearchPattern == extention && e.SearchOption == FileSearchOption.Extention);

            if (foundCriteria == null) { return; }

            AllCriteria.Remove(foundCriteria);
        }
    }
}
