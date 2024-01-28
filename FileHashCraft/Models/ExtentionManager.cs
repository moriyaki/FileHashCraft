/*  ExtentionManager.cs

    拡張子によるファイル管理の管理クラスです。
    拡張子を持つファイル数を管理します。
*/
using System.IO;
using FileHashCraft.Models.Helpers;

namespace FileHashCraft.Models
{
    #region インターフェース
    public interface IExtentionManager
    {
        /// <summary>
        /// ファイルを拡張子の辞書に登録します。
        /// </summary>
        /// <param name="fileFullPath"></param>
        public void AddFile(string fileFullPath);
        /// <summary>
        /// 拡張子のコレクションを取得します。
        /// </summary>
        public IEnumerable<string> GetExtentions();
        /// <summary>
        /// 拡張子グループを対象に、拡張子を取得します。
        /// </summary>
        public IEnumerable<string> GetGroupExtentions(FileGroupType fileGroupType);
        /// <summary>
        /// 拡張子を持つファイル数を取得します。
        /// </summary>
        public int GetExtentionsCount(string extention);
        /// <summary>
        /// 拡張子グループのファイル数を取得します。
        /// </summary>
        public int GetExtentionGroupCount(FileGroupType fileGroupType);
    }
    #endregion インターフェース

    /// <summary>
    /// ファイル拡張子を扱うクラス
    /// </summary>
    public class ExtentionManager : IExtentionManager
    {
        #region 拡張子の管理
        /// <summary>
        /// 拡張子を持つファイル数の辞書
        /// </summary>
        private readonly Dictionary<string, int> _extentionCountDictionary = [];

        /// <summary>
        /// フルパスから拡張子辞書に登録します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        public void AddFile(string fileFullPath)
        {
            var extention = Path.GetExtension(fileFullPath).ToLower();
            if (extention == null) { return; }

            if (_extentionCountDictionary.TryGetValue(extention, out int value))
            {
                _extentionCountDictionary[extention] = ++value;
            }
            else
            {
                _extentionCountDictionary[extention] = 1;
            }
        }

        /// <summary>
        /// 拡張子のコレクションを取得します。
        /// </summary>
        /// <returns>拡張子コレクション</returns>
        public IEnumerable<string> GetExtentions()
        {
            return _extentionCountDictionary.Keys.OrderBy(key => key);
        }

        /// <summary>
        /// 拡張子グループを対象に、拡張子を取得します。
        /// </summary>
        /// <param name="fileGroupType">拡張子の種類</param>
        /// <returns>拡張子コレクション</returns>
        public IEnumerable<string> GetGroupExtentions(FileGroupType fileGroupType)
        {
            if (fileGroupType == FileGroupType.Others)
            {
                var excludedGroups = new[]
                {
                    FileGroupType.Movies,
                    FileGroupType.Pictures,
                    FileGroupType.Musics,
                    FileGroupType.Documents,
                    FileGroupType.Applications,
                    FileGroupType.Archives,
                    FileGroupType.SourceCodes,
                    FileGroupType.Registrations
                };

                foreach (var extension in _extentionCountDictionary.Keys.OrderBy(key => key))
                {
                    if (excludedGroups.Any(group => ExtentionTypeHelper.GetFileGroupExtention(group).Contains(extension))) { continue; }
                    yield return extension;
                }
            }
            else
            {
                foreach (var extention in ExtentionTypeHelper.GetFileGroupExtention(fileGroupType))
                {
                    yield return extention;
                }
            }
        }

        /// <summary>
        /// 拡張子を持つファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <returns>ファイル数</returns>
        public int GetExtentionsCount(string extention)
        {
            extention = extention.ToLower();
            return _extentionCountDictionary.TryGetValue(extention, out int value) ? value : 0;
        }

        /// <summary>
        /// 拡張子グループのファイル数を取得します。
        /// </summary>
        /// <param name="fileGroupType">拡張子グループ</param>
        /// <returns>ファイル数</returns>
        public int GetExtentionGroupCount(FileGroupType fileGroupType)
        {
            var groupCount = 0;
            foreach (var extention in GetGroupExtentions(fileGroupType))
            {
                groupCount += GetExtentionsCount(extention);
            }
            return groupCount;
        }
        #endregion 拡張子の管理
    }
}
