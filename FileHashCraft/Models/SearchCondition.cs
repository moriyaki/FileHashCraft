/*  SearchCondition.cs

    ファイルの検索条件を管理するクラスです。
 */
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
    public class SearchCondition
    {
        #region プロパティ
        /// <summary>
        /// 検索条件の種類
        /// </summary>
        public SearchConditionType Type { get; } = SearchConditionType.None;
        /// <summary>
        /// 検索条件文字列
        /// </summary>
        public string ConditionString { get; private set; } = string.Empty;
        #endregion プロパティ

        public SearchCondition()
        {
        }

        public SearchCondition(SearchConditionType type, string conditionString)
        {
            Type = type;
            ConditionString = conditionString;
        }

        #region 検索条件の追加と置換
        /// <summary>
        /// 正規表現検索の時は、正しい正規表現しか許容しません。
        /// </summary>
        /// <param name="searchConditionString"></param>
        /// <returns>成功の可否</returns>
        private static bool IsRegExTrue(string searchConditionString)
        {
            // 正規表現のチェック
            try
            {
                var regex = new Regex(searchConditionString);
            }
            catch (RegexParseException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 検索条件を設定します。
        /// </summary>
        /// <param name="type">検索条件のタイプ</param>
        /// <param name="conditionString">検索条件</param>
        /// <returns>成功の可否</returns>
        public static SearchCondition? Add(SearchConditionType type, string conditionString)
        {
            if (type == SearchConditionType.None) { return null; }
            var searchCondition = new SearchCondition(type, conditionString);

            switch (type)
            {
                case SearchConditionType.Extention:
                    return searchCondition;
                case SearchConditionType.WildCard:
                    return searchCondition;
                case SearchConditionType.RegularExprettion:
                    if (!IsRegExTrue(conditionString)) { return null; }
                    return searchCondition;
                default:
                    throw new InvalidDataException("AddContidion Type is None");
            }
        }

        /// <summary>
        /// 検索条件文字列を置き換えます。
        /// </summary>
        /// <param name="searchConditionString">置き換える検索条件</param>
        /// <returns>成功の可否</returns>
        public bool ReplaceSearchCondition(string searchConditionString)
        {
            if (!IsRegExTrue(searchConditionString)) { return false; }

            ConditionString = searchConditionString;
            return true;
        }

        #endregion 検索条件の追加と置換

        #region SearchConditionのハッシュと等価チェック
        public override int GetHashCode()
        {
            return Type.GetHashCode() + (13 * ConditionString.GetHashCode());
        }

        public override bool Equals(object? obj)
        {
            if (obj is  SearchCondition searchCondition)
            {
                return searchCondition.Type == Type && searchCondition.ConditionString == ConditionString;
            }
            return false;
        }
        #endregion SearchConditionのハッシュと等価チェック
    }
}
