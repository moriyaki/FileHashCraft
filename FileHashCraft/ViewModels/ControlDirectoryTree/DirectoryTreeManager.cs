using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.ControlDirectoryTree
{
    public interface IDirectoryTreeManager
    {
        /// <summary>
        /// 子ディレクトリを含むチェックされたディレクトリです。
        /// </summary>
        public List<string> NestedDirectories { get; }
        /// <summary>
        /// 子ディレクトリを含まないチェックされたディレクトリです。
        /// </summary>
        public List<string> NonNestedDirectories { get; }
        /// <summary>
        /// そのディレクトリがチェックされているかどうか。
        /// </summary>
        public bool IsChecked(string fullPath);
        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        public void CheckChanged(string fullPath, bool? checkedStatus);

        /// <summary>
        /// 全てのディレクトリです。
        /// </summary>
        public List<string> Directories { get; }
        /// <summary>
        /// 展開されたディレクトリかどうか。
        /// </summary>
        public bool IsExpandedDirectory(string path);
        /// <summary>
        /// 特殊フォルダの子ディレクトリかどうか。
        /// </summary>
        public bool HasSpecialSubFolder(string fullPath);
        /// <summary>
        /// ディレクトリを追加します。
        /// </summary>
        public void AddDirectory(string fullPath);
        /// <summary>
        /// ディレクトリを削除します。
        /// </summary>
        public void RemoveDirectory(string fullPath);
    }

    public class DirectoryTreeManager : IDirectoryTreeManager
    {
        private readonly DirectoryTreeCheckedDirectoryManager _CheckedDirectoryManager;
        private readonly DirectoryTreeExpandedDirectoryManager _ExpandedDirectoryManager;

        public DirectoryTreeManager() { throw new NotImplementedException(); }

        public DirectoryTreeManager(ISpecialFolderAndRootDrives specialFolderAndRootDrives)
        {
            _CheckedDirectoryManager = new DirectoryTreeCheckedDirectoryManager();
            _ExpandedDirectoryManager = new DirectoryTreeExpandedDirectoryManager(specialFolderAndRootDrives);
        }

        #region リスト
        public List<string> NestedDirectories { get; } = [];
        public List<string> NonNestedDirectories { get; } = [];
        #endregion リスト

        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックされているかを調べるディレクトリのフルパス</param>
        /// <returns>チェックされているかどうか</returns>
        public bool IsChecked(string fullPath) => _CheckedDirectoryManager.IsChecked(fullPath);

        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        /// <param name="fullPath">変化させるディレクトリのフルパス</param>
        /// <param name="checkedStatus">変化させる状態</param>
        public void CheckChanged(string fullPath, bool? checkedStatus) => _CheckedDirectoryManager.CheckChanged(fullPath, checkedStatus);

        /// <summary>
        /// 登録したディレクトリのリストを取得します。
        /// </summary>
        /// <returns></returns>
        public List<string> Directories { get => _ExpandedDirectoryManager.Directories; }

        /// <summary>
        /// 展開されたディレクトリかどうかを調べます。
        /// </summary>
        /// <param name="path">チェックするディレクトリのフルパス</param>
        /// <returns>展開されたディレクトリかどうか</returns>
        public bool IsExpandedDirectory(string path)=> _ExpandedDirectoryManager.IsExpandedDirectory(path);

        /// <summary>
        /// 特殊フォルダの配下かどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックするディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool HasSpecialSubFolder(string fullPath) => _ExpandedDirectoryManager.HasSpecialSubFolder(fullPath);

        /// <summary>
        /// 指定したパスを管理対象に追加します。
        /// </summary>
        /// <param name="fullPath">追加するディレクトリのフルパス</param>
        public void AddDirectory(string fullPath) => _ExpandedDirectoryManager.AddDirectory(fullPath);

        /// <summary>
        /// 指定したパスを管理対象から外します。
        /// </summary>
        /// <param name="fullPath">削除するディレクトリのフルパス</param>
        public void RemoveDirectory(string fullPath) => _ExpandedDirectoryManager.RemoveDirectory(fullPath);
    }
}
