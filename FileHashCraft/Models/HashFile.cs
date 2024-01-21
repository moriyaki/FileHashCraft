using System.IO;

namespace FileHashCraft.Models
{
    /// <summary>
    /// ファイル情報を保持するクラス
    /// </summary>
    public class HashFile
    {
        #region メンバ
        public string FileFullPath { get; }
        public DateTime LastWriteTime { get; }
        public long Length { get; }
        public string SHA256 { get; set; } = string.Empty;
        public string SHA384 { get; set; } = string.Empty;
        public string SHA512 { get; set; } = string.Empty;
        public int ConditionCount { get; set; } = 0;
        #endregion メンバ

        #region 設定処理
        /// <summary>
        /// コンストラクタに引数無しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無し</exception>
        public HashFile() { throw new NotImplementedException(); }

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
        }
        #endregion 設定処理

        #region ハッシュコードの算出
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
        #endregion ハッシュコードの算出
    }
}
