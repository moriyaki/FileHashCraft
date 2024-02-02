/*  SearchConditionsManager.cs

    検索条件群とその対象ファイルを管理するクラスです。
 */
using System.IO;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.Models
{
    #region インターフェース
    public interface ISearchConditionsManager
    {
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; }

        /// <summary>
        /// 検索条件コレクションに追加します。
        /// </summary>
        public void AddCondition(SearchConditionType type, string contidionString);
        /// <summary>
        /// 検索条件コレクションから削除します。
        /// </summary>
        public void RemoveCondition(SearchConditionType type, string contidionString);
    }
    #endregion インターフェース

    /// <summary>
    /// 検索条件の管理クラス
    /// </summary>
    public class SearchConditionsManager : ISearchConditionsManager
    {
        #region 内部データ
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; } = [];
        #endregion 内部データ

        #region コンストラクタ
        //private readonly IExtentionManager _ExtentionHelper;
        private readonly ISearchFileManager _SearchFileManager;

        public SearchConditionsManager() { throw new NotImplementedException(); }
        public SearchConditionsManager(
            ISearchFileManager searchFileManager)
        {
            _SearchFileManager = searchFileManager;
        }
        #endregion コンストラクタ

        #region 検索条件の操作
        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 正規表現なら正しければ、それ以外は type=None でなければ無条件に検索条件リストに追加します。
        /// </summary>
        /// <param name="type">検索条件タイプ</param>
        /// <param name="contidionString">検索条件</param>
        /// <returns>成功の可否</returns>
        public void AddCondition(SearchConditionType type, string contidionString)
        {
            var condition = SearchCondition.AddCondition(type, contidionString);
            if (condition == null) { return; }
            lock (_lock)
            {
                switch (type)
                {
                    case SearchConditionType.Extention:
                        foreach (var extentionFile in _SearchFileManager.AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), contidionString, StringComparison.OrdinalIgnoreCase)))
                        {
                            extentionFile.ConditionCount++;

                            // 条件辞書にファイルを登録する
                            if (!ConditionFiles.TryGetValue(condition, out HashSet<HashFile>? value))
                            {
                                value = ([]);
                                ConditionFiles.Add(condition, value);
                            }
                            value.Add(extentionFile);
                            _SearchFileManager.AllConditionFiles.Add(extentionFile);
                        }
                        break;
                    case SearchConditionType.WildCard:
                        break;
                    case SearchConditionType.RegularExprettion:
                        break;
                }
            }
        }

        /// <summary>
        /// 検索条件を削除します。
        /// </summary>
        /// <param name="type">検索条件のタイプ</param>
        /// <param name="contidionString">検索条件</param>
        public void RemoveCondition(SearchConditionType type, string contidionString)
        {
            var condition = ConditionFiles.Keys.FirstOrDefault(c => c.Type == type && c.ConditionString == contidionString);
            if (condition == null) { return; }

            lock (_lock)
            {
                foreach (var file in ConditionFiles[condition])
                {
                    file.ConditionCount--;
                    if (file.ConditionCount == 0)
                    {
                        _SearchFileManager.AllConditionFiles.Remove(file);
                    }
                    ConditionFiles.Remove(condition);
                }
            }
        }
        #endregion 検索条件の操作
    }
}
