/*  ControDirectoryTreeViewlViewModel.PartialDirectoryChanged

    ディレクトリの TreeView でドライブまたはディレクトリが変更された時の処理をします。

 */
using System.Collections.ObjectModel;
using System.IO;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    /* このクラスの監視結果：
     *
     * ドライブルートのディレクトリ作成：DirectoryCreated
     * ドライブルートのディレクトリ名前変更：DirectoryRenamed
     * ドライブルートのディレクトリ削除：RECYCLED.BIN DirectoryChanged
     *
     * サブディレクトリのディレクトリ作成：親ディレクトリ DirectoryChanged & 対象ディレクトリ DirectoryCreated
     * サブディレクトリのディレクトリ名前変更：親ディレクトリ DirectoryChanged & 対象ディレクトリ DirectoryRenamed
     * サブディレクトリのディレクトリ削除：親ディレクトリ DirectoryChanged & RECYCLED.BIN DirectoryChanged
     *
     * → DirectoryDeletedは必要なし
     */
    public partial class ControDirectoryTreeViewModel
    {
        #region ディレクトリ変更通知処理
        /// <summary>
        /// ディレクトリが変更された時の処理をします。
        /// 特に重要なのはドライブルートのアイテム削除は $RECYCLE.BIN を利用してしか不可で、
        /// 削除処理を DirectoryDeleted で取得できないため、ここでは削除処理を行います。
        /// サブディレクトリの削除もここで行います。
        /// </summary>
        /// <param name="fullPath">変更されたディレクトリのフルパス</param>
        private async Task DirectoryChanged(string fullPath)
        {
            var directory = Path.GetDirectoryName(fullPath);

            try
            {
                var deletedPath = fullPath;
                /// ごみ箱の変更通知はドライブルートのアイテム削除の可能性があるのでその処理
                if (fullPath.Contains("$RECYCLE.BIN"))
                {
                    deletedPath = Path.GetPathRoot(deletedPath);
                }
                if (deletedPath == null) { return; }

                // 変更が加えられたディレクトリの親ツリーアイテムを取得
                var modifiedParentItem = FindChangedDirectoryTree(deletedPath);
                var modifiedSpecialParent = FindChangedSpecialDirectoryTreeItem(deletedPath);

                // 変更が加えられたディレクトリ内にあるディレクトリのフルパスコレクションを取得
                var dirs = Directory.EnumerateDirectories(deletedPath);

                // 変更が加えられる前のツリービューアイテムを取得
                List<string>? treeItems = null;
                if (modifiedParentItem != null)
                {
                    treeItems = modifiedParentItem.Children
                        .Where(c => !string.IsNullOrEmpty(c.FullPath))
                        .Select(c => c.FullPath)
                        .ToList();
                }
                else
                {
                    foreach (var item in modifiedSpecialParent)
                    {
                        treeItems = item.Children
                            .Where(c => !string.IsNullOrEmpty(c.FullPath))
                            .Select(c => c.FullPath)
                            .ToList();
                        if (treeItems.Count > 0) break;
                    }
                }
                if (treeItems == null) { return; }

                // TreeViewのアイテム削除のために、新旧ディレクトリ構造の差分を取る
                var deletedItemNames = treeItems.Except(dirs);
                if (deletedItemNames.Any())
                {
                    // treeItems にあって dirs にアイテムがない場合は削除処理
                    if (modifiedParentItem != null)
                    {
                        await DirectoryItemDeleted(modifiedParentItem, deletedItemNames.First());
                    }
                    // 特殊フォルダも削除
                    foreach (var parent in modifiedSpecialParent)
                    {
                        await DirectoryItemDeleted(parent, deletedItemNames.First());
                    }
                }
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"Exception in DirectoryChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// サブディレクトリが削除された時の処理をします。
        /// </summary>
        /// <param name="modifiedParentItem">親ツリービューアイテム</param>
        /// <param name="deletedItemFullPath">削除されたアイテムのフルパス</param>
        /// <returns>Task</returns>
        private async Task DirectoryItemDeleted(DirectoryTreeViewItemModel modifiedParentItem, string deletedItemFullPath)
        {
            // 削除されたツリービューアイテムの取得
            var deletedTreeItem = modifiedParentItem.Children.FirstOrDefault(c => c.FullPath == deletedItemFullPath);
            if (deletedTreeItem == null) { return; }

            // カレントディレクトリに削除メッセージを送信
            _messageServices.SendCurrentItemDeleted(deletedItemFullPath);

            // 削除されたツリービューアイテムの削除
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // TODO : 特殊フォルダ内にも存在したら反映
                    modifiedParentItem.Children.Remove(deletedTreeItem);
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryDeleted: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// ディレクトリが作成されたイベントを処理します。
        /// ドライブルートでも、サブディレクトリでも発生します。
        /// </summary>
        /// <param name="fullPath">作成されたディレクトリのフルパス</param>
        private async Task DirectoryCreated(string fullPath)
        {
            if (fullPath.Contains("$RECYCLE.BIN")) { return; }

            // ディレクトリが作成された親ディレクトリのパスを取得
            var createdPath = Path.GetDirectoryName(fullPath);
            if (createdPath == null) { return; }

            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            var modifiedParentItem = FindChangedDirectoryTree(createdPath);
            var modifiedSpecialParent = FindChangedSpecialDirectoryTreeItem(createdPath);

            var fileInformation = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(fullPath);
            var addTreeItem = new DirectoryTreeViewItemModel(fileInformation);

            // カレントディレクトリに作成メッセージを送信
            _messageServices.SendCurrentItemCreated(fullPath);

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 作成されたディレクトリを追加
                    if (modifiedParentItem != null)
                    {
                        int newTreeIndex = FindIndexToInsert(modifiedParentItem.Children, addTreeItem);
                        modifiedParentItem.Children.Insert(newTreeIndex, addTreeItem);
                    }
                    // 特殊フォルダにも追加
                    foreach (var parent in modifiedSpecialParent)
                    {
                        int newTreeIndex = FindIndexToInsert(parent.Children, addTreeItem);
                        parent.Children.Insert(newTreeIndex, addTreeItem);
                    }
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryCreated: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// ディレクトリが名前変更されたイベントの処理をします。
        /// ドライブルートでも、サブディレクトリでも発生します。
        /// </summary>
        private async Task DirectoryRenamed(string oldFullPath, string newFullPath)
        {
            if (newFullPath.Contains("$RECYCLE.BIN")) { return; }

            var renamedPath = Path.GetDirectoryName(newFullPath);
            if (renamedPath == null) { return; }

            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            var modifiedParentItem = FindChangedDirectoryTree(renamedPath);
            var modifiedSpecialParent = FindChangedSpecialDirectoryTreeItem(renamedPath);

            // 名前変更されたツリービューアイテムの取得
            DirectoryTreeViewItemModel? renamedTreeItem = null;
            if (modifiedParentItem != null)
            {
                renamedTreeItem = modifiedParentItem.Children.FirstOrDefault(item => item.FullPath == oldFullPath);
            }
            else
            {
                foreach (var parent in modifiedSpecialParent)
                {
                    renamedTreeItem = parent.Children.FirstOrDefault(item => item.FullPath == oldFullPath);
                }
            }
            if (renamedTreeItem == null) { return; }

            // カレントディレクトリに名前変更メッセージを送信
            _messageServices.SendCurrentItemRenamed(oldFullPath, newFullPath);
            int newTreeIndex = 0;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 一度名前変更前のアイテムを除去
                    if (modifiedParentItem != null)
                    {
                        modifiedParentItem.Children.Remove(renamedTreeItem);
                        newTreeIndex = FindIndexToInsert(modifiedParentItem.Children, renamedTreeItem);
                    }
                    foreach (var parent in modifiedSpecialParent)
                    {
                        parent.Children.Remove(renamedTreeItem);
                        newTreeIndex = FindIndexToInsert(parent.Children, renamedTreeItem);
                    }
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryRenamed Remove: {ex.Message}");
                }
            });

            // 名前変更後のアイテムを再追加
            renamedTreeItem.FullPath = newFullPath;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // TODO : 特殊フォルダ内にも存在したら反映
                    // 名前変更後のアイテムを追加
                    modifiedParentItem?.Children.Insert(newTreeIndex, renamedTreeItem);
                    foreach (var parent in modifiedSpecialParent)
                    {
                        parent.Children.Insert(newTreeIndex, renamedTreeItem);
                    }
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryRenamed Insert: {ex.Message}");
                }
            });
        }
        #endregion ディレクトリ変更通知処理

        #region リムーバブルドライブドライブ変更通知処理
        /// <summary>
        /// リムーバブルドライブが追加または挿入された時の処理をします。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブが挿入されたフルパス</param>
        private async Task OpticalDriveMediaInserted(string fullPath)
        {
            try
            {
                var driveInfo = new DriveInfo(fullPath);
                var isCDRom = driveInfo.DriveType == DriveType.CDRom;
                if (!isCDRom)
                {
                    var newItem = new FileItemInformation
                    {
                        FullPath = fullPath
                    };
                    var insertedItem = new DirectoryTreeViewItemModel(newItem);
                    int newTreeIndex = FindIndexToInsert(TreeRoot, insertedItem);
                    TreeRoot.Insert(newTreeIndex, insertedItem);
                    return;
                }
                var drive = TreeRoot.FirstOrDefault(c => c.FullPath == fullPath);
                if (drive == null) return;

                drive.Icon = drive.Icon;
                drive.Name = drive.Name;

                // 変更されるまで 100ms 待機しながら 120回繰り返す
                var retries = 120;
                while (retries > 0)
                {
                    if (drive.Icon == WindowsAPI.GetIcon(fullPath) || drive.Name == WindowsAPI.GetDisplayName(fullPath))
                    {
                        retries--;
                        await Task.Delay(100);
                        continue;
                    }
                    break;
                }
                drive = TreeRoot.FirstOrDefault(c => c.FullPath == fullPath);
                if (drive == null) return;
                if (isCDRom)
                {
                    // 光学ドライブへの挿入処理
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        drive.Icon = WindowsAPI.GetIcon(fullPath);
                        drive.Name = WindowsAPI.GetDisplayName(fullPath);
                        drive.HasChildren = Directory.EnumerateDirectories(fullPath).Any();
                    });
                }
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"Exception in OpticalDriveMediaInserted: {ex.Message}");
            }
        }

        private readonly object treeRootLock = new();

        /// <summary>
        /// リムーバブルメディアがイジェクトされた時の処理をします。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブがイジェクトされたフルパス</param>
        private async Task OpticalDriveMediaEjected(string fullPath)
        {
            try
            {
                var driveInfo = new DriveInfo(fullPath);
                var isCDRom = driveInfo.DriveType == DriveType.CDRom;
                var drive = TreeRoot.FirstOrDefault(c => c.FullPath == fullPath);

                if (drive != null)
                {
                    if (isCDRom)
                    {
                        // 変更されるまで 100ms 待機しながら 20回繰り返す
                        var retries = 120;
                        while (retries > 0)
                        {
                            if (drive.Icon == WindowsAPI.GetIcon(fullPath) ||
                                drive.Name == WindowsAPI.GetDisplayName(fullPath))
                            {
                                retries--;
                                await Task.Delay(100);
                                continue;
                            }
                            break;
                        }

                        // 光学ドライブへからのイジェクト処理
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            drive.Icon = WindowsAPI.GetIcon(fullPath);
                            drive.Name = WindowsAPI.GetDisplayName(fullPath);
                            drive.HasChildren = false;
                        });
                    }
                    else
                    {
                        //App.Current?.Dispatcher?.Invoke(() => TreeRoot.Remove(drive));
                        await App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            lock(treeRootLock)
                            {
                                drive = TreeRoot.FirstOrDefault(c => c.FullPath == fullPath);
                                if (drive != null)
                                {
                                    TreeRoot.Remove(drive);
                                }
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"Exception in EjectOpticalDriveMedia: {ex.Message}");
            }
        }
        #endregion リムーバブルドライブドライブ変更通知処理

        #region TreeNode取得関連
        /// <summary>
        /// 変更が加えられたファイルアイテムのドライブディレクトリツリーアイテムを検索します。
        /// </summary>
        /// <param name="fullPath">ファイルアイテムのパス</param>
        /// <returns>変更する必要があるディレクトリツリーアイテム</returns>
        private DirectoryTreeViewItemModel? FindChangedDirectoryTree(string fullPath, string rootPath = "")
        {
            DirectoryTreeViewItemModel? modifiedTreeItem = null;
            foreach (var dir in DirectoryNameService.GetDirectoryNames(fullPath, rootPath))
            {
                if (modifiedTreeItem == null)
                {
                    // ドライブの処理
                    modifiedTreeItem = TreeRoot.FirstOrDefault(root => root.FullPath == dir);
                    if (dir == fullPath) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                }
                else
                {
                    // サブディレクトリの処理
                    modifiedTreeItem = modifiedTreeItem.Children.FirstOrDefault(child => child.FullPath == dir);
                    if (dir == fullPath) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                }
            }
            return null;
        }

        /// <summary>
        /// 特殊ユーザーディレクトリルートからツリーアイテムを探します。
        /// </summary>
        /// <param name="fullPath">特殊ユーザーディレクトリが含まれることを期待するパス</param>
        /// <returns>特殊ユーザーディレクトリ内のツリーアイテム</returns>
        private HashSet<DirectoryTreeViewItemModel> FindChangedSpecialDirectoryTreeItem(string fullPath)
        {
            var specialTreeItems = new HashSet<DirectoryTreeViewItemModel>();

            foreach (var rootInfo in SpecialFolderAndRootDrives.ScanSpecialFolders())
            {
                // 特殊ユーザーディレクトリのパスを持つアイテムを抽出
                if (fullPath.Contains(rootInfo.FullPath))
                {
                    var rootItem = TreeRoot.FirstOrDefault(item => item.FullPath == rootInfo.FullPath);
                    if (rootItem == null) continue;
                    var foundTreeItem = FinddDirectoryTreeViewItem(rootItem, fullPath);
                    if (foundTreeItem != null)
                    {
                        specialTreeItems.Add(foundTreeItem);
                    }
                }
            }

            return specialTreeItems;
        }

        private static DirectoryTreeViewItemModel? FinddDirectoryTreeViewItem(DirectoryTreeViewItemModel root, string fullPath)
        {
            if (root.FullPath == fullPath) return root;
            DirectoryTreeViewItemModel? child = root;
            while (true)
            {
                child = child.Children.FirstOrDefault(item => fullPath.Contains(item.FullPath));
                if (child == null) return null;
                if (child.FullPath == fullPath) return child;
            }
        }

        /// <summary>
        /// ソート済みの位置に挿入するためのヘルパーメソッド、挿入する位置を取得します。
        /// </summary>
        /// <typeparam name="T">検索するObservableCollectionの型</typeparam>
        /// <param name="collection">コレクション</param>
        /// <param name="newItem">新しいアイテム</param>
        /// <returns>挿入する位置</returns>
        private static int FindIndexToInsert<T>(ObservableCollection<T> collection, T newItem) where T : IComparable<T>
        {
            int indexToInsert = 0;
            foreach (var item in collection)
            {
                if (item.CompareTo(newItem) >= 0) { break; }
                indexToInsert++;
            }
            return indexToInsert;
        }
        #endregion TreeNode取得関連
    }
}
