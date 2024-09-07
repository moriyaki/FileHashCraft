/*  HashFile.cs

    ハッシュを保持するファイル情報のクラスです。
 */
using System.IO;
using System.Security.Cryptography;
using FileHashCraft.Models.Helpers;

namespace FileHashCraft.Models
{
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
        public long FileSize { get; }
        /// <summary>
        /// ファイルの属性
        /// </summary>
        public FileAttributes Attributes { get; }
        /// <summary>
        /// ファイルハッシュアルゴリズム
        /// </summary>
        public FileHashAlgorithm HashAlgorithm { get; set; }
        /// <summary>
        /// ファイルハッシュ
        /// </summary>
        public string FileHash { get; set; } = string.Empty;
        #endregion メンバ

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

        #region 設定処理
        /// <summary>
        /// ファイル名とハッシュを保存します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        /// <param name="hashAlgorithm">ハッシュアルゴリズム</param>
        /// <param name="fileHash">ファイルハッシュ</param>
        public HashFile(string fileFullPath, FileHashAlgorithm hashAlgorithm, string fileHash)
        {
            FileFullPath = fileFullPath;
            HashAlgorithm = hashAlgorithm;
            FileHash = fileHash;
            var fileInfo = new FileInfo(fileFullPath);
            LastWriteTime = fileInfo.LastWriteTime;
            FileSize = fileInfo.Length;
            Attributes = fileInfo.Attributes;
        }
        #endregion 設定処理
    }
}
