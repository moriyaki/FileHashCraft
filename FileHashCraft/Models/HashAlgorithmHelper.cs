namespace FileHashCraft.Models
{
    /// <summary>
    /// 洗濯できるハッシュアルゴリズム
    /// </summary>
    public enum HashAlgorithmType
    {
        SHA256,
        SHA384,
        SHA512
    }

    /// <summary>
    /// ハッシュアルゴリズムから識別文字を取得する
    /// </summary>
    public static class HashAlgorithmHelper
    {
        public static HashAlgorithmType GetHashAlgorithmType(string algorithm)
        {
            return algorithm switch
            {
                "SHA-256" => HashAlgorithmType.SHA256,
                "SHA-384" => HashAlgorithmType.SHA384,
                "SHA-512" => HashAlgorithmType.SHA512,
                _ => throw new ArgumentException("Invalid hash algorithm."),
            };
        }

        public static string GetHashAlgorithmName(HashAlgorithmType algorithmType)
        {
            return algorithmType switch
            {
                HashAlgorithmType.SHA256 => "SHA-256",
                HashAlgorithmType.SHA384 => "SHA-384",
                HashAlgorithmType.SHA512 => "SHA-512",
                _ => throw new ArgumentException("Invalid hash algorithm type."),
            };
        }
    }
}
