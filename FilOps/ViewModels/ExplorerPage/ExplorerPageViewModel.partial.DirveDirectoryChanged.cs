using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Media.Imaging;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
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
    public partial class ExplorerPageViewModel
    {
        #region ディレクトリ変更通知処理
        /// <summary>
        /// パスがドライブルートかを確認する
        /// </summary>
        /// <param name="path">ドライブルートか確認するパス</param>
        /// <returns>ドライブルートかどうか</returns>
        private static bool IsRoot(string path) => path.Split('\\').Length == 2;

        /// <summary>
        /// ディレクトリが変更された時の処理をします。
        /// 特に重要なのはドライブルートのアイテム削除は $RECYCLE.BIN を利用してしか不可で、
        /// 削除処理を DirectoryDeleted で取得できないため、ここでは削除処理を行います。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">DirectoryChangedEventArgs</param>
        private async void DirectoryChanged(object? sender, DirectoryChangedEventArgs e)
        {
            try
            {
                /// ごみ箱の変更通知はドライブルートのアイテム削除の可能性があるのでその処理
                if (e.FullPath.Contains("$RECYCLE.BIN"))
                {
                    await HandleRecycleBinChange(e.FullPath);
                    return;
                }

                // 変更が加えられたディレクトリの親ツリーアイテムを取得
                var modifiedTreeItem = FindChangedDirectoryTree(e.FullPath);
                // 変更が加えられたディレクトリ内にあるディレクトリのフルパスコレクションを取得
                var dirs = Directory.EnumerateDirectories(e.FullPath);

                FindChangedSpecialDirectoryTreeItem(e.FullPath);

                // 変更が加えられたディレクトリの親ディレクトリを取得できていれば
                if (modifiedTreeItem != null)
                {
                    // 変更が加えられる前のツリービューアイテムを取得
                    var treeItems = modifiedTreeItem.Children
                        .Where(c => !string.IsNullOrEmpty(c.FullPath))
                        .Select(c => c.FullPath);

                    if (treeItems != null)
                    {
                        // TreeViewのアイテム削除のために、新旧ディレクトリ構造の差分を取る
                        var deletedItemNames = treeItems.Except(dirs);
                        if (deletedItemNames.Any())
                        {
                            // treeItems にあって dirs にアイテムがない場合は削除処理
                            DirectoryDeleted(modifiedTreeItem, deletedItemNames.First());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in DirectoryChanged: {ex.Message}");
            }

        }

        /// <summary>
        /// ディレクトリが作成されたイベントを処理します。
        /// ドライブルートでも、サブディレクトリでも発生します。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">DirectoryChangedEventArgs</param>
        private async void DirectoryCreated(object? sender, DirectoryChangedEventArgs e)
        {
            if (e.FullPath.Contains("$RECYCLE.BIN")) { return; }

            // ディレクトリが作成された親ディレクトリのパスを取得
            var path = System.IO.Path.GetDirectoryName(e.FullPath);
            if (path != null)
            {
                // 変更が加えられたディレクトリの親ツリーアイテムを取得
                var modifiedTreeItem = FindChangedDirectoryTree(path);
                if (modifiedTreeItem != null)
                {
                    // 作成されたディレクトリを追加
                    var addTreeItem = CreateTreeViewItem(e.FullPath); ;

                    int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, addTreeItem);
                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            modifiedTreeItem.Children.Insert(newTreeIndex, addTreeItem);

                            // リストビューにも表示されていたら、そちらも更新
                            if (modifiedTreeItem.IsSelected)
                            {
                                var addListItem = CreateListViewItem(e.FullPath); ;
                                int newListIndex = FindIndexToInsert(ListItems, addListItem);
                                ListItems.Insert(newListIndex, addListItem);
                            }
                        }
                        catch (Exception ex) 
                        {
                            Debug.WriteLine($"Exception in DirectoryCreated: {ex.Message}");
                        }
                    });
                }
                // TODO : 特殊フォルダ内にも存在したら反映
                //FindChangedSpecialDirectoryTreeItem(e.FullPath);
            }
        }

        /// <summary>
        /// ディレクトリが名前変更されたイベントの処理をします。
        /// ドライブルートでも、サブディレクトリでも発生します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DirectoryRenamed(object? sender, DirectoryRenamedEventArgs e)
        {
            if (e.FullPath.Contains("$RECYCLE.BIN")) { return; }

            var path = System.IO.Path.GetDirectoryName(e.FullPath);
            if (path != null)
            {
                // 変更が加えられたディレクトリの親ツリーアイテムを取得
                var modifiedTreeItem = FindChangedDirectoryTree(path);
                if (modifiedTreeItem != null)
                {
                    // 名前変更されたツリービューアイテムの取得
                    var renamedTreeItem = modifiedTreeItem.Children.FirstOrDefault(item => item.FullPath == e.OldFullPath);
                    if (renamedTreeItem != null)
                    {
                        // 新しい名前を取得
                        var newFullPath = e.FullPath;
                        if (!string.IsNullOrEmpty(newFullPath))
                        {
                            var oldFullPath = renamedTreeItem.FullPath;

                            await App.Current.Dispatcher.InvokeAsync(() =>
                            {
                                try
                                {
                                    // 一度名前変更前のアイテムを除去
                                    modifiedTreeItem.Children.Remove(renamedTreeItem);

                                    // 名前変更後のアイテムを再追加
                                    renamedTreeItem.FullPath = newFullPath;
                                    int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, renamedTreeItem);
                                    modifiedTreeItem.Children.Insert(newTreeIndex, renamedTreeItem);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Exception in DirectoryRenamed: {ex.Message}");
                                }
                            });

                            // リストビューにも表示されていたら、そちらも更新
                            if (modifiedTreeItem.IsSelected)
                            {
                                var listItem = ListItems.FirstOrDefault(item => item.FullPath == e.OldFullPath);
                                if (listItem != null)
                                {
                                    await App.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        try
                                        {
                                            // 一度名前変更前のアイテムを除去
                                            ListItems.Remove(listItem);

                                            // 名前変更後のアイテムを再追加
                                            listItem.FullPath = newFullPath;
                                            int newListIndex = FindIndexToInsert(ListItems, listItem);
                                            ListItems.Insert(newListIndex, listItem);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Exception in DirectoryRenamed: {ex.Message}");
                                        }
                                    });
                                }
                            }
                        }
                    }
                    // TODO : 特殊フォルダ内にも存在したら反映
                    //FindChangedSpecialDirectoryTreeItem(e.FullPath);
                }
            }
        }

        /// <summary>
        /// サブディレクトリが削除された時の処理をします。
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="deletedItemName">削除されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async void DirectoryDeleted(ExplorerTreeNodeViewModel modifiedTreeItem, string deletedItemName)
        {
            // 削除されたツリービューアイテムの取得
            var deletedTreeItem = modifiedTreeItem.Children.FirstOrDefault(c => c.FullPath == deletedItemName);
            if (deletedTreeItem != null)
            {
                // 削除されたツリービューアイテムの削除
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        modifiedTreeItem.Children.Remove(deletedTreeItem);

                        // リストビューにも表示されていたら、そちらも更新
                        if (modifiedTreeItem.IsSelected)
                        {
                            var listItem = ListItems.FirstOrDefault(item => item.FullPath == deletedItemName);
                            if (listItem != null) { ListItems.Remove(listItem); }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception in DirectoryDeleted: {ex.Message}");
                    }

                });
                // TODO : 特殊フォルダ内にも存在したら反映
                //FindChangedSpecialDirectoryTreeItem(e.FullPath);
            }
        }

        /// <summary>
        /// ドライブルートのアイテムが削除された時の処理をします。
        /// これは、DirectoryCreated のイベントから、削除されたファイルを検出しています。
        /// </summary>
        /// <param name="fullPath">削除されたファイルアイテムのフルパス</param>
        /// <returns>Task</returns>
        private async Task HandleRecycleBinChange(string fullPath)
        {
            // ドライブルートが削除を検出できない対策
            if (!IsRoot(fullPath)) { return; }

            // ドライブルートのツリービューアイテムを取得
            if (TreeRoot.FirstOrDefault(r => r.FullPath == fullPath) is ExplorerTreeNodeViewModel root)
            {
                // 削除されたアイテムの取得
                var dirs = Directory.EnumerateDirectories(fullPath);
                var modifiedTreeItem = root.Children.Select(c => c.FullPath).Except(dirs);

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        var deletedTreeItem = root.Children.FirstOrDefault(c => c.FullPath == modifiedTreeItem.FirstOrDefault());
                        if (deletedTreeItem != null) { root.Children.Remove(deletedTreeItem); }

                        // リストビューにも表示されていたら、そちらも更新
                        if (deletedTreeItem != null && root == CurrentDirectoryItem)
                        {
                            var listItem = ListItems.FirstOrDefault(i => i.FullPath == deletedTreeItem.FullPath);
                            if (listItem != null) { ListItems.Remove(listItem); }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception in HandleRecycleBinChange: {ex.Message}");
                    }
                });
            }
        }
        #endregion ディレクトリ変更通知処理

        #region リムーバブルドライブドライブ変更通知処理
        /// <summary>
        /// リムーバブルドライブが追加または挿入された時の処理をします。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">DirectoryChangedEventArgs</param>
        private async void OpticalDriveMediaInserted(object? sender, DirectoryChangedEventArgs e)
        {
            try
            {
                var driveInfo = new DriveInfo(e.FullPath);
                var isCDRom = driveInfo.DriveType == DriveType.CDRom;
                var drive = TreeRoot.FirstOrDefault(c => c.FullPath == e.FullPath);

                BitmapSource? icon;
                string name;

                if (drive != null)
                {
                    icon = drive.Icon;
                    name = drive.Name;
                }
                else
                {
                    icon = WindowsAPI.GetIcon(e.FullPath);
                    name = WindowsAPI.GetDisplayName(e.FullPath);

                }

                // 変更されるまで 100ms 待機しながら 120回繰り返す
                var retries = 120;
                while (retries > 0)
                {
                    if (icon == WindowsAPI.GetIcon(e.FullPath) || name == WindowsAPI.GetDisplayName(e.FullPath))
                    {
                        retries--;
                        await Task.Delay(100);
                        continue;
                    }
                    break;
                }
                Debug.WriteLine($"Insert retries : {120 - retries}");
                if (isCDRom && drive != null)
                {
                    // 光学ドライブへの挿入処理
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        drive.Icon = WindowsAPI.GetIcon(e.FullPath);
                        drive.Name = WindowsAPI.GetDisplayName(e.FullPath);
                        drive.HasChildren = Directory.EnumerateDirectories(e.FullPath).Any();
                    });
                }
                else
                {
                    // 取り外し可能なメディアの追加処理
                    var addNewInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(e.FullPath);
                    var newTreeItem = new ExplorerTreeNodeViewModel(this, addNewInformation);
                    int newTreeIndex = FindIndexToInsert(TreeRoot, newTreeItem);
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        TreeRoot.Insert(newTreeIndex, newTreeItem);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in OpticalDriveMediaInserted: {ex.Message}");
            }

        }

        /// <summary>
        /// リムーバブルメディアがイジェクトされた時の処理をします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EjectOpticalDriveMedia(object? sender, DirectoryChangedEventArgs e)
        {
            try
            {
                var driveInfo = new DriveInfo(e.FullPath);
                var isCDRom = driveInfo.DriveType == DriveType.CDRom;
                var drive = TreeRoot.FirstOrDefault(c => c.FullPath == e.FullPath);

                // 挿入完了まで待機
                if (drive != null)
                {
                    // 変更されるまで 100ms 待機しながら 20回繰り返す
                    var retries = 120;
                    while (retries > 0)
                    {
                        if (drive.Icon == WindowsAPI.GetIcon(e.FullPath) ||
                            drive.Name == WindowsAPI.GetDisplayName(e.FullPath))
                        {
                            retries--;
                            await Task.Delay(100);
                            continue;
                        }
                        break;
                    }
                    Debug.WriteLine($"Eject retries : {120 - retries}");

                    if (isCDRom)
                    {
                        // 光学ドライブへからのイジェクト処理
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            drive.Icon = WindowsAPI.GetIcon(e.FullPath);
                            drive.Name = WindowsAPI.GetDisplayName(e.FullPath);
                            drive.HasChildren = false;
                        });
                    }
                    else
                    {
                        TreeRoot.Remove(drive);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in EjectOpticalDriveMedia: {ex.Message}");
            }
        }
        #endregion リムーバブルドライブドライブ変更通知処理

        #region TreeNode取得関連
        /// <summary>
        /// 変更が加えられたファイルアイテムのディレクトリツリーアイテムを検索します。
        /// </summary>
        /// <param name="fullPath">ファイルアイテムのパス</param>
        /// <returns>変更する必要があるディレクトリツリーアイテム</returns>
        private ExplorerTreeNodeViewModel? FindChangedDirectoryTree(string fullPath)
        {
            ExplorerTreeNodeViewModel? modifiedTreeItem = null;
            foreach (var dir in GetDirectoryNames(fullPath))
            {
                if (dir.Length == 3)
                {
                    // ドライブの処理
                    modifiedTreeItem = TreeRoot.FirstOrDefault(root => root.FullPath == dir) as ExplorerTreeNodeViewModel;
                    if (dir == fullPath) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                }
                else if (modifiedTreeItem != null)
                {
                    // サブディレクトリの処理
                    modifiedTreeItem = modifiedTreeItem.Children.FirstOrDefault(child => child.FullPath == dir);
                    if (dir == fullPath) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                    if (!modifiedTreeItem.IsExpanded) { break; }
                }
            }
            return null;
        }

        /// <summary>
        /// 特殊ユーザーディレクトリルートからツリーアイテムを探します。
        /// </summary>
        /// <param name="fullPath">特殊ユーザーディレクトリが含まれることを期待するパス</param>
        /// <returns>特殊ユーザーディレクトリ内のツリーアイテム</returns>
        private List<ExplorerTreeNodeViewModel> FindChangedSpecialDirectoryTreeItem(string fullPath)
        {
            var specialTreeItems = new List<ExplorerTreeNodeViewModel>();

            foreach (var rootInfo in FileSystemInfoManager.SpecialFolderScan())
            {
                // 特殊ユーザーディレクトリのパスを持つアイテムを抽出

                if (TreeRoot.FirstOrDefault(item => item.FullPath == rootInfo.FullPath) is
                    ExplorerTreeNodeViewModel rootItem && fullPath.Contains(rootInfo.FullPath))
                {
                    if (Equals(rootItem.FullPath, fullPath))
                    {
                        // ルートで見つかった場合はリストに追加
                        specialTreeItems.Add(rootItem);
                    }
                    else
                    {
                        // 再帰的にツリービューを検索
                        FindTreeChild(rootItem, fullPath, specialTreeItems);
                    }
                }
            }
            return specialTreeItems;
        }

        /// <summary>
        /// 再帰的に、パスと等しいツリービューアイテムを探して SpecialTreeItem に追加します。
        /// </summary>
        /// <param name="treeItem">検索中のツリービューアイテム</param>
        /// <param name="path">探し出すファイルのフルパス</param>
        /// <param name="specialTreeItems">見つかった時に追加するリストコレクション</param>
        private static void FindTreeChild(ExplorerTreeNodeViewModel treeItem, string path, List<ExplorerTreeNodeViewModel> specialTreeItems)
        {
            // 見つかったら終わり
            if (Equals(treeItem.FullPath, path))
            {
                specialTreeItems.Add(treeItem);
                return;
            }

            // 開いてなければそこで終わり
            if (!treeItem.IsExpanded) return;

            // 再帰的に検索する
            foreach (var child in treeItem.Children)
            {
                FindTreeChild(child, path, specialTreeItems);
            }
        }
        #endregion TreeNode取得関連
    }
}
