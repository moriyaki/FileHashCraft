namespace FileHashCraft.Models.HashCalc
{
    public interface IDuplicateFilesCollection
    {
        /// <summary>
        /// 同一ファイル
        /// </summary>
        HashSet<HashFile> DuplicateFiles { get; }

        void AddDuplicateFile(HashFile file);

        void AddDuplicateFiles(IEnumerable<HashFile> files);
    }

    public class DuplicateFilesCollection : IDuplicateFilesCollection
    {
        public HashSet<HashFile> DuplicateFiles { get; set; } = [];

        /// <summary>
        /// 同一ファイルの管理への追加
        /// </summary>
        /// <param name="file">追加するファイル</param>
        public void AddDuplicateFile(HashFile file)
        {
            DuplicateFiles.Add(file);
        }

        /// <summary>
        /// 同一ファイルへの管理への一括追加
        /// </summary>
        /// <param name="files">追加するファイルコレクション</param>
        public void AddDuplicateFiles(IEnumerable<HashFile> files)
        {
            DuplicateFiles.UnionWith(files);
        }
    }
}