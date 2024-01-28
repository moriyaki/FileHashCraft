using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.Models
{
    public interface ISearchFileManager
    {
        /// <summary>
        /// 全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; }
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; }
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        public void AddFile(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをファイルディクショナリから削除します。
        /// </summary>
        /// <param name="directoryFullPath"></param>
        public void RemoveDirectory(string directoryFullPath);
    }
    public class SearchFileManager : ISearchFileManager
    {
        /// <summary>
        /// 全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; } = [];
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; } = [];

        public readonly IExtentionManager _ExtentionManager;

        public SearchFileManager() { throw new NotImplementedException(); }
        public SearchFileManager(
            IExtentionManager extentionManager)
        {
            _ExtentionManager = extentionManager;

            WeakReferenceMessenger.Default.Register<AddConditionFile>(this, (_, message) =>
                AllConditionFiles.Add(message.ConditionFiles));
        }

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

            // 拡張子ヘルパーに拡張子を登録する(カウントもする)
            _ExtentionManager.AddFile(fileFullPath);
        }

        public void RemoveDirectory(string directoryFullPath)
        {
            foreach (var fileToRemove in AllFiles.Keys.Where(d => Path.GetDirectoryName(d) == directoryFullPath).ToList())
            {
                AllFiles.Remove(fileToRemove);
            }
        }
        #endregion ファイルの追加とディレクトリの削除
    }
}
