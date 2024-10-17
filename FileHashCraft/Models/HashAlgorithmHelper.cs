/*  HashAlgorithmHelper.cs

    ハッシュアルゴリズムのタイプと文字列を相互変換するヘルパークラスです。
 */
using FileHashCraft.Properties;

namespace FileHashCraft.Models
{
    /// <summary>
    /// 洗濯できるハッシュアルゴリズム
    /// </summary>
    public enum FileHashAlgorithm
    {
        SHA256,
        SHA384,
        SHA512
    }

    public interface IHashAlgorithmHelper
    {
        /// <summary>
        /// ハッシュアルゴリズム名からハッシュアルゴリズムタイプを取得するヘルパー
        /// </summary>
        FileHashAlgorithm GetAlgorithm(string algorithm);
        /// <summary>
        /// ハッシュアルゴリズムタイプからハッシュアルゴリズム名を取得するヘルパー
        /// </summary>
        string GetAlgorithmName(FileHashAlgorithm algorithmType);
        /// <summary>
        /// ハッシュアルゴリズ名からハッシュアルゴリズムの解説を取得するヘルパー
        /// </summary>
        string GetAlgorithmCaption(string algorithm);
    }

    /// <summary>
    /// ハッシュアルゴリズムから識別文字を取得する
    /// </summary>
    public class HashAlgorithmHelper : IHashAlgorithmHelper
    {
        /// <summary>
        /// ハッシュアルゴリズム名からハッシュアルゴリズムタイプを取得するヘルパー
        /// </summary>
        /// <param name="algorithm">ハッシュアルゴリズム名</param>
        /// <returns>ハッシュアルゴリズムタイプ</returns>
        public FileHashAlgorithm GetAlgorithm(string algorithm)
        {
            return algorithm switch
            {
                "SHA-256" => FileHashAlgorithm.SHA256,
                "SHA-384" => FileHashAlgorithm.SHA384,
                "SHA-512" => FileHashAlgorithm.SHA512,
                _ => FileHashAlgorithm.SHA256,
            };
        }

        /// <summary>
        /// ハッシュアルゴリズムタイプからハッシュアルゴリズム名を取得するヘルパー
        /// </summary>
        /// <param name="algorithmType">ハッシュアルゴリズム</param>
        /// <returns>ハッシュアルゴリズム名</returns>
        public string GetAlgorithmName(FileHashAlgorithm algorithmType)
        {
            return algorithmType switch
            {
                FileHashAlgorithm.SHA256 => "SHA-256",
                FileHashAlgorithm.SHA384 => "SHA-384",
                FileHashAlgorithm.SHA512 => "SHA-512",
                _ => "SHA-256",
            };
        }

        /// <summary>
        /// ハッシュアルゴリズ名からハッシュアルゴリズムの解説を取得するヘルパー
        /// </summary>
        /// <param name="algorithm">ハッシュアルゴリズム名</param>
        /// <returns>ハッシュアルゴリズムの解説</returns>
        public string GetAlgorithmCaption(string algorithm)
        {
            return algorithm switch
            {
                "SHA-256" => Resources.HashAlgorithm_SHA256,
                "SHA-384" => Resources.HashAlgorithm_SHA384,
                "SHA-512" => Resources.HashAlgorithm_SHA512,
                _ => Resources.HashAlgorithm_SHA256,
            };
        }
    }
}
