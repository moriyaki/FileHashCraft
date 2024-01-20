using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FileHashCraft.Models
{
    #region 検索条件のタイプ
    /// <summary>
    /// 検索条件のタイプ
    /// </summary>
    public enum SearchConditionType
    {
        None,
        Extention,
        WildCard,
        RegularExprettion,
    }
    #endregion 検索条件のタイプ

    /// <summary>
    /// 検索条件を保持するクラス
    /// </summary>
    public class Condition
    {
        #region プロパティ
        private SearchConditionType _Type = SearchConditionType.None;
        public SearchConditionType Type { get => _Type; }
        private string _ConditionString = string.Empty;
        public string ConditionString { get => _ConditionString; }
        public HashSet<HashFile> ConditionFiles { get; set; } = [];
        #endregion プロパティ

        private readonly ISearchManager _SearchManager;
        public Condition()
        {
            _SearchManager = Ioc.Default.GetService<ISearchManager>() ?? throw new NullReferenceException(nameof(ISearchManager)) ;
        }
        #region 検索条件の追加と置換
        /// <summary>
        /// 正規表現検索の時は、正しい正規表現しか許容しません。
        /// </summary>
        /// <param name="searchConditionString"></param>
        /// <returns>成功の可否</returns>
        private bool IsRegExTrue(string searchConditionString)
        {
            // 正規表現のチェック
            if (Type == SearchConditionType.RegularExprettion)
            {
                try
                {
                    var regex = new Regex(searchConditionString);
                }
                catch (RegexParseException)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 検索条件を設定します。
        /// </summary>
        /// <param name="type">検索条件のタイプ</param>
        /// <param name="conditionString">検索条件</param>
        /// <returns>成功の可否</returns>
        public bool SetCondition(SearchConditionType type, string conditionString)
        {
            if (type == SearchConditionType.None) { return false; }
            _Type = type;
            _ConditionString = conditionString;

            switch (type)
            {
                case SearchConditionType.Extention:
                    ConditionFiles.UnionWith(_SearchManager.AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), conditionString, StringComparison.OrdinalIgnoreCase)));
                    break;
                case SearchConditionType.WildCard:
                    break;
                case SearchConditionType.RegularExprettion:
                    if (!IsRegExTrue(conditionString)) { return false; }
                    break;
                default:
                    throw new InvalidDataException("AddContidion Type is None");
            }
            return true;
        }

        /// <summary>
        /// 検索条件文字列を置き換えます。
        /// </summary>
        /// <param name="searchConditionString">置き換える検索条件</param>
        /// <returns>成功の可否</returns>
        public bool ReplaceSearchCondition(string searchConditionString)
        {
            if (!IsRegExTrue(searchConditionString)) { return false; }

            _ConditionString = searchConditionString;
            return true;
        }

        #endregion 検索条件の追加と置換
    }
}
