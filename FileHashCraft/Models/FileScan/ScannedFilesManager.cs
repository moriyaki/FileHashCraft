using System.IO;

namespace FileHashCraft.Models.FileScan
{
    public interface IScannedFilesManager
    {
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        public void AddFileToAllFiles(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; }
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; }
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; }
        /// <summary>
        /// 検索条件コレクションに追加します。
        /// </summary>
        public void AddCondition(SearchConditionType type, string contidionString);
        /// <summary>
        /// 検索条件コレクションから削除します。
        /// </summary>
        public void RemoveCondition(SearchConditionType type, string contidionString);
    }

    public class ScannedFilesManager : IScannedFilesManager
    {
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; } = [];
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; } = [];
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; } = [];

        #region ファイルの追加
        /// <summary>
        /// ファイルを追加します
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        /// <param name="hashSHA256">SHA256のハッシュ</param>
        /// <param name="hashSHA384">SHA384のハッシュ</param>
        /// <param name="hashSHA512">SHA512のハッシュ</param>
        public void AddFileToAllFiles(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "")
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

        #endregion ファイルの追加

        #region 検索条件の操作
        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _conditionLock = new();

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
            lock (_conditionLock)
            {
                switch (type)
                {
                    case SearchConditionType.Extention:
                        foreach (var extentionFile in AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), contidionString, StringComparison.OrdinalIgnoreCase)))
                        {
                            // 条件辞書にファイルを登録する
                            if (!ConditionFiles.TryGetValue(condition, out HashSet<HashFile>? value))
                            {
                                value = ([]);
                                ConditionFiles.Add(condition, value);
                            }
                            value.Add(extentionFile);
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

            lock (_conditionLock)
            {
                foreach (var file in ConditionFiles[condition])
                {
                    ConditionFiles.Remove(condition);
                }
            }
        }
        #endregion 検索条件の操作
    }
}
