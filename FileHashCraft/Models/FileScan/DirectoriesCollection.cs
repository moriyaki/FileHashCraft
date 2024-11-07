namespace FileHashCraft.Models.FileScan
{
    #region インターフェース
    public interface IDirectoriesCollection
    {
        /// <summary>
        /// 全てのディレクトリです。
        /// </summary>
        List<string> Directories { get; set; }

        /// <summary>
        /// 子ディレクトリを含むチェックされたディレクトリです。
        /// </summary>
        List<string> NestedDirectories { get; set; }

        /// <summary>
        /// 子ディレクトリを含まないチェックされたディレクトリです。
        /// </summary>
        List<string> NonNestedDirectories { get; set; }
    }
    #endregion インターフェース

    public class DirectoriesCollection : IDirectoriesCollection
    {
        /// <summary>
        /// 全てのディレクトリです。
        /// </summary>
        public List<string> Directories { get; set; } = [];

        /// <summary>
        /// 子ディレクトリを含むチェックされたディレクトリです。
        /// </summary>
        public List<string> NestedDirectories { get; set; } = [];

        /// <summary>
        /// 子ディレクトリを含まないチェックされたディレクトリです。
        /// </summary>
        public List<string> NonNestedDirectories { get; set; } = [];
    }
}