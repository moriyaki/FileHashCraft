using System.Diagnostics;
using System.IO;
using FileHashCraft.Views;

namespace FileHashCraft.Models
{
    #region インターフェース
    public interface ISearchManager
    {
        /// <summary>
        /// 全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; }
        /// <summary>
        /// 検索条件を保管するリスト
        /// </summary>
        public List<Condition> ConditionsList { get; }
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> ConditionFiles { get; }
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        public void AddFile(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをファイルディクショナリから削除します。
        /// </summary>
        /// <param name="directoryFullPath"></param>
        public void RemoveDirectory(string directoryFullPath);
        /// <summary>
        /// 検索条件コレクションに追加します。
        /// </summary>
        public Task AddCondition(SearchConditionType type, string contidionString);
        /// <summary>
        /// 検索条件コレクションから削除します。
        /// </summary>
        public Task RemoveCondition(SearchConditionType type, string contidionString);
    }
    #endregion インターフェース

    /// <summary>
    /// 検索条件の管理クラス
    /// </summary>
    public class SearchManager : ISearchManager
    {
        #region 内部データ
        /// <summary>
        /// 検索対象全ファイルを保持するリスト
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; } = [];
        /// <summary>
        /// 検索条件を保管するリスト
        /// </summary>
        public List<Condition> ConditionsList { get; } = [];
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> ConditionFiles { get; } = [];
        #endregion 内部データ

        #region ファイルの追加とディレクトリの削除
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
            if (AllFiles.TryGetValue(fileFullPath, out HashFile? value))
            {
                // 同一日付とサイズなら追加しない
                if (fileInfo.LastWriteTime == value.LastWriteTime && fileInfo.Length == value.Length) { return; }

                // 既にハッシュを持っているなら設定する
                if (!string.IsNullOrEmpty(value.SHA256) && string.IsNullOrEmpty(hashSHA256)) { hashSHA256 = value.SHA256; }
                if (!string.IsNullOrEmpty(value.SHA384) && string.IsNullOrEmpty(hashSHA384)) { hashSHA384 = value.SHA384; }
                if (!string.IsNullOrEmpty(value.SHA512) && string.IsNullOrEmpty(hashSHA512)) { hashSHA512 = value.SHA512; }

                // データが異なるか、ハッシュ更新されていれば昔のデータを削除する
                AllFiles.Remove(fileFullPath);
            }
            // 新しいデータなら追加する(日付と更新日はコンストラクタで設定される)
            var hashFile = new HashFile(fileFullPath, hashSHA256, hashSHA384, hashSHA512);
            AllFiles.Add(fileFullPath, hashFile);
        }

        public void RemoveDirectory(string directoryFullPath)
        {
            foreach (var fileToRemove in AllFiles.Keys.Where(d => Path.GetDirectoryName(d) == directoryFullPath).ToList())
            {
                AllFiles.Remove(fileToRemove);
            }
        }
        #endregion ファイルの追加

        private readonly object _lock = new();

        #region 検索条件の操作
        /// <summary>
        /// 正規表現なら正しければ、それ以外は type=None でなければ無条件に検索条件リストに追加します。
        /// </summary>
        /// <param name="type">検索条件タイプ</param>
        /// <param name="contidionString">検索条件</param>
        /// <returns>成功の可否</returns>
        public async Task AddCondition(SearchConditionType type, string contidionString)
        {
            await Task.Run(() =>
            {
                var condition = new Condition();
                if (!condition.SetCondition(type, contidionString)) { return; }
                lock (_lock)
                {
                    ConditionsList.Add(condition);
                    foreach (var file in condition.ConditionFiles)
                    {
                        ConditionFiles.Add(file);
                    }
                }
            });
        }

        /// <summary>
        /// 検索条件を削除します。
        /// </summary>
        /// <param name="type">検索条件のタイプ</param>
        /// <param name="contidionString">検索条件</param>
        public async Task RemoveCondition(SearchConditionType type, string contidionString)
        {
            await Task.Run(() =>
            {
                var condition = ConditionsList.Find(c => c.Type == type && c.ConditionString == contidionString);
                if (condition == null) return;
                lock (_lock)
                {
                    foreach (var file in condition.ConditionFiles)
                    {
                        ConditionFiles.Remove(file);
                    }
                    condition.ConditionFiles.Clear();
                    ConditionsList.Remove(condition);
                }
            });
        }
        #endregion 検索条件の操作
    }
}
