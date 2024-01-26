/*  DirectoryTreeExpandedDirectoryManager.cs

    ディレクトリツリーの展開状況を管理するクラスです。
 */

namespace FileHashCraft.ViewModels.Modules
{
    // TODO : 削除するパスの特殊フォルダは除外
    public class DirectoryTreeExpandedDirectoryManager
    {
        public DirectoryTreeExpandedDirectoryManager(ISpecialFolderAndRootDrives specialFolderAndRootDrives)
        {
            foreach (var rootInfo in specialFolderAndRootDrives.ScanSpecialFolders())
            {
                _specialDirectoriesRoot.Add(rootInfo.FullPath);
            }
        }

        #region 変数宣言
        /// <summary>
        /// 登録したディレクトリのリスト
        /// </summary>
        private readonly List<string> _normalDirectories = [];

        /// <summary>
        /// 登録した特殊フォルダのリスト
        /// </summary>
        private readonly List<string> _specialSubDirectories = [];

        /// <summary>
        /// /特殊フォルダのリスト
        /// </summary>
        private readonly List<string> _specialDirectoriesRoot = [];
        #endregion 変数宣言

        #region メソッドとプロパティ
        /// <summary>
        /// 登録したディレクトリのリストを取得します。
        /// </summary>
        /// <returns></returns>
        public List<string> Directories
        {
            get
            {
                var allList = new List<string>();
                allList.AddRange(_specialDirectoriesRoot);
                allList.AddRange(_specialSubDirectories);
                allList.AddRange(_normalDirectories);
                return allList;
            }
        }

        /// <summary>
        /// 展開されたディレクトリかどうかを調べます。
        /// </summary>
        /// <param name="path">チェックするディレクトリのフルパス</param>
        /// <returns>展開されたディレクトリかどうか</returns>
        public bool IsExpandedDirectory(string path) => HasDirectory(path) || IsSpecialSubFolder(path);

        /// <summary>
        /// 特殊フォルダかどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックするディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool IsSpecialFolder(string fullPath) => _specialDirectoriesRoot.Any(dir => dir == fullPath);

        /// <summary>
        /// 特殊フォルダに含まれているサブディレクトリかどうかを取得します。
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool IsSpecialSubFolder(string fullPath) => _specialDirectoriesRoot.Any(dir => dir == fullPath);

        /// <summary>
        /// 特殊フォルダか、そこに含まれているディレクトリかどうかを取得します。
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool IsSpecialSubFolderOrSub(string fullPath) => IsSpecialFolder(fullPath) || IsSpecialSubFolder(fullPath);

        /// <summary>
        /// 特殊フォルダの配下かどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックするディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool HasSpecialSubFolder(string fullPath) => _specialDirectoriesRoot.Any(root => fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// 特殊フォルダ以外で、ディレクトリがTreeViewで展開されているかどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックするディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool HasDirectory(string fullPath) => _normalDirectories.Any(dir => dir == fullPath);
        #endregion メソッドとプロパティ

        #region 追加削除メソッド
        /// <summary>
        /// 指定したパスを管理対象に追加します。
        /// </summary>
        /// <param name="fullPath">追加するディレクトリのフルパス</param>
        public void AddDirectory(string fullPath)
        {
            // 特殊フォルダそのものであれば登録しない
            if (IsSpecialFolder(fullPath)) { return; }

            // 特殊フォルダの配下時の処理
            if (IsSpecialSubFolder(fullPath) && !HasSpecialSubFolder(fullPath))
            {
                _specialSubDirectories.Add(fullPath);
                return;
            }

            if (!HasDirectory(fullPath))
            {
                _normalDirectories.Add(fullPath);
                return;
            }
        }

        /// <summary>
        /// 指定したパスを管理対象から外します。
        /// </summary>
        /// <param name="fullPath">削除するディレクトリのフルパス</param>
        public void RemoveDirectory(string fullPath)
        {
            // 特殊フォルダそのものであれば削除しない
            if (IsSpecialFolder(fullPath)) { return; }

            // 特殊フォルダの配下時の処理
            if (IsSpecialSubFolder(fullPath))
            {
                _specialSubDirectories.Remove(fullPath);
                return;
            }

            if (HasDirectory(fullPath))
            {
                _normalDirectories.Remove(fullPath);
                return;
            }
        }
        #endregion 追加削除メソッド
    }
}
