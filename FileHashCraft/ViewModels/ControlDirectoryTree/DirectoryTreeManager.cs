/*  DirectoryTreeManager.cs

    TreeViewの展開状況とチェック状況をまとめて管理します。
    Facadeパターンを利用しています。
 */
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.ControlDirectoryTree
{
    public interface IDirectoryTreeManager
    {
        // CheckedTreeManager
        /// <summary>
        /// そのディレクトリがチェックされているかどうか。
        /// </summary>
        bool IsChecked(string fullPath);
        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        void CheckChanged(string fullPath, bool? checkedStatus);
        /// <summary>
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeItem> treeRoot);
        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        void CreateCheckBoxManager(ObservableCollection<DirectoryTreeItem> treeRoot);

        // ExpandedTreeManager
        /// <summary>
        /// 展開されたディレクトリかどうか。
        /// </summary>
        bool IsExpandedDirectory(string path);
        /// <summary>
        /// 特殊フォルダの子ディレクトリかどうか。
        /// </summary>
        bool HasSpecialSubFolder(string fullPath);
        /// <summary>
        /// ディレクトリノードを展開マネージャに追加します。
        /// </summary>
        void AddExpandedDirectoryManager(DirectoryTreeItem node);
        /// <summary>
        /// ディレクトリノードを展開マネージャから削除します。
        /// </summary>
        void RemoveExpandedDirectoryManager(DirectoryTreeItem node);
        /// <summary>
        /// ディレクトリを追加します。
        /// </summary>
        void AddDirectory(string fullPath);
        /// <summary>
        /// ディレクトリを削除します。
        /// </summary>
        void RemoveDirectory(string fullPath);
    }

    public class DirectoryTreeManager : IDirectoryTreeManager
    {
        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public DirectoryTreeManager() { throw new NotImplementedException(nameof(DirectoryTreeManager)); }

        private readonly IMessenger _messenger;
        private readonly ICheckedTreeItemsManager _treeCheckedManager;
        private readonly ITreeExpandedManager _treeExpandedManager;
        private readonly IDirectoriesManager _directoriesManager;
        public DirectoryTreeManager(
            IMessenger messenger,
            ICheckedTreeItemsManager treeCheckedManager,
            ITreeExpandedManager treeExpandedManager,
            IDirectoriesManager directoriesManager
            )
        {
            _messenger = messenger;
            _treeCheckedManager = treeCheckedManager;
            _treeExpandedManager = treeExpandedManager;
            _directoriesManager = directoriesManager;

            _directoriesManager.Directories = _treeExpandedManager.Directories;
            _directoriesManager.NestedDirectories = _treeCheckedManager.NestedDirectories;
            _directoriesManager.NonNestedDirectories = _treeCheckedManager.NonNestedDirectories;

            _messenger.Register<AddToExpandDirectoryManagerMessage>(this, (_, m)
                => AddExpandedDirectoryManager(m.Child));

            _messenger.Register<RemoveFromExpandDirectoryManagerMessage>(this, (_, m)
                => RemoveExpandedDirectoryManager(m.Child));
        }
        //-----------------------------------TreeCheckedManager
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
        public void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeItem> treeRoot)
            => _treeCheckedManager.CheckStatusChangeFromCheckManager(treeRoot);

        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        public void CreateCheckBoxManager(ObservableCollection<DirectoryTreeItem> treeRoot)
            => _treeCheckedManager.CreateCheckBoxManager(treeRoot);

        //-----------------------------------TreeExpandedManager
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
        public void AddExpandedDirectoryManager(DirectoryTreeItem node) =>
           _treeExpandedManager.AddExpandedDirectoryManager(node);

        /// <summary>
        /// ディレクトリノードを展開マネージャから削除します。
        /// </summary>
        public void RemoveExpandedDirectoryManager(DirectoryTreeItem node) =>
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
