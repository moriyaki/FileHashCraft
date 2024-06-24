namespace FileHashCraft.Models.FileScan
{
    public interface IDirectoriesManager
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
    public class DirectoriesManager : IDirectoriesManager
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
