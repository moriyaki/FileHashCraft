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
        private string _SHA256 = string.Empty;
        public string SHA256
        {
            get => _SHA256;
        }
        private string _SHA384 = string.Empty;
        public string SHA384
        {
            get => _SHA384;
        }
        private string _SHA512 = string.Empty;
        public string SHA512
        {
            get => _SHA512;
        }
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
            _SHA256 = sha256;
            _SHA384 = sha384;
            _SHA512 = sha512;
            var fileInfo = new FileInfo(fileFullPath);
            LastWriteTime = fileInfo.LastWriteTime;
            Length = fileInfo.Length;
        }
        /// <summary>
        /// SHA256ハッシュを設定する
        /// </summary>
        /// <param name="hashSHA256">SHA256ハッシュ</param>
        public void SetSHA256(string hashSHA256)
        {
            _SHA256 = hashSHA256;
        }
        /// <summary>
        /// SHA256ハッシュを設定する
        /// </summary>
        /// <param name="hashSHA384">SHA256ハッシュ</param>
        public void SetSHA384(string hashSHA384)
        {
            _SHA384 = hashSHA384;
        }
        /// <summary>
        /// SHA256ハッシュを設定する
        /// </summary>
        /// <param name="hashSHA512">SHA256ハッシュ</param>
        public void SetSHA512(string hashSHA512)
        {
            _SHA512 = hashSHA512;
        }
        #endregion 設定処理
    }
}
