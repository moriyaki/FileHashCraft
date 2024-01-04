namespace FileHashCraft.Models
{
    public class FileHashManager
    {
        public FileHashManager() { }
    }

    [Serializable]
    public class FileHashXML
    {
        public int Version = 1;
        public List<FileHashInformation> Files = [];
    }
    [Serializable]
    public class FileHashInformation
    {
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        private DateTime _LastModiried;
        public DateTime LastModified
        {
            get => _LastModiried;
            set => _LastModiried = value.ToUniversalTime();
        }
        public string SHA256_Hash { get; set; } = string.Empty;
        public string SHA384_Hash { get; set; } = string.Empty;
        public string SHA512_Hash { get; set; } = string.Empty;
    }
}
