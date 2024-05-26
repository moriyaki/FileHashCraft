/*  DirectoryTreeManager.cs

    TreeViewの展開状況とチェック状況をまとめて管理します。
    Facadeパターンを利用しています。
 */
using System.Collections.ObjectModel;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.ControlDirectoryTree
{
    public interface ITreeManager
    {
        // CheckedTreeManager
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
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeViewItemModel> treeRoot);
        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        public void CreateCheckBoxManager(ObservableCollection<DirectoryTreeViewItemModel> treeRoot);

        // ExpandedTreeManager
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
        /// ディレクトリノードを展開マネージャに追加します。
        /// </summary>
        public void AddExpandedDirectoryManager(DirectoryTreeViewItemModel node);
        /// <summary>
        /// ディレクトリノードを展開マネージャから削除します。
        /// </summary>
        public void RemoveExpandedDirectoryManager(DirectoryTreeViewItemModel node);
        /// <summary>
        /// ディレクトリを追加します。
        /// </summary>
        public void AddDirectory(string fullPath);
        /// <summary>
        /// ディレクトリを削除します。
        /// </summary>
        public void RemoveDirectory(string fullPath);
    }

    public class TreeManager : ITreeManager
    {
        public TreeManager() { throw new NotImplementedException(); }

        private readonly ITreeCheckedManager _treeCheckedManager;
        private readonly ITreeExpandedManager _treeExpandedManager;
        public TreeManager(
            ITreeCheckedManager treeCheckedManager,
            ITreeExpandedManager treeExpandedManager
            )
        {
            _treeCheckedManager = treeCheckedManager;
            _treeExpandedManager = treeExpandedManager;
        }
        //-----------------------------------TreeCheckedManager
        #region リスト
        /// <summary>
        /// 子ディレクトリを含む、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NestedDirectories { get => _treeCheckedManager.NestedDirectories; }
        /// <summary>
        /// 子ディレクトリを含まない、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NonNestedDirectories { get => _treeCheckedManager.NonNestedDirectories; }
        #endregion リスト

        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックされているかを調べるディレクトリのフルパス</param>
        /// <returns>チェックされているかどうか</returns>
        public bool IsChecked(string fullPath) => _treeCheckedManager.IsChecked(fullPath);

        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        /// <param name="fullPath">変化させるディレクトリのフルパス</param>
        /// <param name="checkedStatus">変化させる状態</param>
        public void CheckChanged(string fullPath, bool? checkedStatus) => _treeCheckedManager.CheckChanged(fullPath, checkedStatus);

        /// <summary>
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeViewItemModel> treeRoot)
            => _treeCheckedManager.CheckStatusChangeFromCheckManager(treeRoot);

        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        public void CreateCheckBoxManager(ObservableCollection<DirectoryTreeViewItemModel> treeRoot)
            => _treeCheckedManager.CreateCheckBoxManager(treeRoot);

        //-----------------------------------TreeExpandedManager
        /// <summary>
        /// 登録したディレクトリのリストを取得します。
        /// </summary>
        /// <returns></returns>
        public List<string> Directories { get => _treeExpandedManager.Directories; }

        /// <summary>
        /// 展開されたディレクトリかどうかを調べます。
        /// </summary>
        /// <param name="path">チェックするディレクトリのフルパス</param>
        /// <returns>展開されたディレクトリかどうか</returns>
        public bool IsExpandedDirectory(string path)=> _treeExpandedManager.IsExpandedDirectory(path);

        /// <summary>
        /// 特殊フォルダの配下かどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックするディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool HasSpecialSubFolder(string fullPath) => _treeExpandedManager.HasSpecialSubFolder(fullPath);

        /// <summary>
        /// ディレクトリノードを展開マネージャに追加します。
        /// </summary>
        /// <param name="node">追加するディレクトリノード</param>
        public void AddExpandedDirectoryManager(DirectoryTreeViewItemModel node) =>
           _treeExpandedManager.AddExpandedDirectoryManager(node);

        /// <summary>
        /// ディレクトリノードを展開マネージャから削除します。
        /// </summary>
        public void RemoveExpandedDirectoryManager(DirectoryTreeViewItemModel node) =>
            _treeExpandedManager.RemoveExpandedDirectoryManager(node);

        /// <summary>
        /// 指定したパスを管理対象に追加します。
        /// </summary>
        /// <param name="fullPath">追加するディレクトリのフルパス</param>
        public void AddDirectory(string fullPath) => _treeExpandedManager.AddDirectory(fullPath);

        /// <summary>
        /// 指定したパスを管理対象から外します。
        /// </summary>
        /// <param name="fullPath">削除するディレクトリのフルパス</param>
        public void RemoveDirectory(string fullPath) => _treeExpandedManager.RemoveDirectory(fullPath);
    }
}
