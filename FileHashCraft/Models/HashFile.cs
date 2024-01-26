/*  HashFile.cs

    ハッシュを保持するファイル情報のクラスです。
 */
using System.IO;

namespace FileHashCraft.Models
{
    public enum HashFileAttribute
    {
        Normal,
        Readonly,
        Hidden,
        ReadonlyAndHidden,
    }

    /// <summary>
    /// ファイル情報を保持するクラス
    /// </summary>
    public class HashFile
    {
        #region メンバ
        /// <summary>
        /// ファイルのフルパス
        /// </summary>
        public string FileFullPath { get; }
        /// <summary>
        /// ファイルの最終更新日
        /// </summary>
        public DateTime LastWriteTime { get; }
        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        public long Length { get; }
        /// <summary>
        /// ファイルの属性
        /// </summary>
        public HashFileAttribute Attribute { get; }
        /// <summary>
        /// SHA256ハッシュ
        /// </summary>
        public string SHA256 { get; set; } = string.Empty;
        /// <summary>
        /// SHA384ハッシュ
        /// </summary>
        public string SHA384 { get; set; } = string.Empty;
        /// <summary>
        /// SHA512ハッシュ
        /// </summary>
        public string SHA512 { get; set; } = string.Empty;
        /// <summary>
        /// 条件に合致している件数
        /// </summary>
        public int ConditionCount { get; set; } = 0;
        #endregion メンバ

        #region 設定処理
        /// <summary>
        /// ファイル名とハッシュを保存します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        /// <param name="sha256">SHA256ハッシュ</param>
        /// <param name="sha384">SHA384ハッシュ</param>
        /// <param name="sha512">SHA512ハッシュ</param>
        public HashFile(string fileFullPath, string sha256 = "", string sha384 = "", string sha512 = "")
        {
            FileFullPath = fileFullPath;
            SHA256 = sha256;
            SHA384 = sha384;
            SHA512 = sha512;
            var fileInfo = new FileInfo(fileFullPath);
            LastWriteTime = fileInfo.LastWriteTime;
            Length = fileInfo.Length;
            if (((fileInfo.Attributes & FileAttributes.ReadOnly)== FileAttributes.ReadOnly)
             && ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
            {
                Attribute = HashFileAttribute.ReadonlyAndHidden;
            }
            else if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                Attribute = HashFileAttribute.Readonly;
            }
            else if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                Attribute = HashFileAttribute.Hidden;
            }
            else
            {
                Attribute = HashFileAttribute.Normal;
            }
        }
        #endregion 設定処理

        #region DictionaryやHashSet用ハッシュコードの算出
        public override bool Equals(object? obj)
        {
            if (obj is HashFile hashFile)
            {
                return string.Equals(this.FileFullPath, hashFile.FileFullPath, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.FileFullPath);
        }
        #endregion DictionaryやHashSet用ハッシュコードの算出
    }
}
