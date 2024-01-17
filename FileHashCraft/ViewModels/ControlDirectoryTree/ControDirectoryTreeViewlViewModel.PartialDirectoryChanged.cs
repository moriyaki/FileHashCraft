using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.FileSystemWatch;
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
    public partial class ControDirectoryTreeViewlViewModel
    {
        #region ディレクトリ変更通知処理
        /// <summary>
        /// ディレクトリが変更された時の処理をします。
        /// 特に重要なのはドライブルートのアイテム削除は $RECYCLE.BIN を利用してしか不可で、
        /// 削除処理を DirectoryDeleted で取得できないため、ここでは削除処理を行います。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">DirectoryChangedEventArgs</param>
        private void DirectoryChanged(object? sender, DirectoryChangedEventArgs e)
        {
            try
            {
                var deletedPath = e.FullPath;
                /// ごみ箱の変更通知はドライブルートのアイテム削除の可能性があるのでその処理
                if (e.FullPath.Contains("$RECYCLE.BIN"))
                {
                    deletedPath = Path.GetPathRoot(deletedPath);
                }
                if (deletedPath == null) { return; }

                // 変更が加えられたディレクトリの親ツリーアイテムを取得
                var modifiedTreeItem = FindChangedDirectoryTree(deletedPath);
                // 変更が加えられたディレクトリ内にあるディレクトリのフルパスコレクションを取得
                var dirs = Directory.EnumerateDirectories(deletedPath);
                FindChangedSpecialDirectoryTreeItem(deletedPath);

                // 変更が加えられたディレクトリの親ディレクトリを取得できていれば
                if (modifiedTreeItem == null) { return; }

                // 変更が加えられる前のツリービューアイテムを取得
                var treeItems = modifiedTreeItem.Children
                    .Where(c => !string.IsNullOrEmpty(c.FullPath))
                    .Select(c => c.FullPath);
                if (treeItems == null) { return; }

                // TreeViewのアイテム削除のために、新旧ディレクトリ構造の差分を取る
                var deletedItemNames = treeItems.Except(dirs);
                if (deletedItemNames.Any())
                {
                    // treeItems にあって dirs にアイテムがない場合は削除処理
                    DirectoryDeleted(modifiedTreeItem, deletedItemNames.First());
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
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="deletedItemName">削除されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private static async void DirectoryDeleted(DirectoryTreeViewModel modifiedTreeItem, string deletedItemName)
        {
            // 削除されたツリービューアイテムの取得
            var deletedTreeItem = modifiedTreeItem.Children.FirstOrDefault(c => c.FullPath == deletedItemName);
            if (deletedTreeItem == null) { return; }

            // ディレクトリ名前変更メッセージを送信
            WeakReferenceMessenger.Default.Send(new DirectoryDeleted(deletedItemName));

            // 削除されたツリービューアイテムの削除
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    modifiedTreeItem.Children.Remove(deletedTreeItem);
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryDeleted: {ex.Message}");
                }
            });
            // TODO : 特殊フォルダ内にも存在したら反映
            //FindChangedSpecialDirectoryTreeItem(e.FullPath);
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
            var path = Path.GetDirectoryName(e.FullPath);
            if (path == null) { return; }

            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            var modifiedTreeItem = FindChangedDirectoryTree(path);
            if (modifiedTreeItem == null) { return; }

            // 作成されたディレクトリを追加
            var fileInformation = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(e.FullPath);
            var addTreeItem = new DirectoryTreeViewModel(fileInformation);
            int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, addTreeItem);

            // ディレクトリ作成メッセージを送信
            WeakReferenceMessenger.Default.Send(new DirectoryCreated(e.FullPath));

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    modifiedTreeItem.Children.Insert(newTreeIndex, addTreeItem);
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryCreated: {ex.Message}");
                }
            });
            // TODO : 特殊フォルダ内にも存在したら反映
            //FindChangedSpecialDirectoryTreeItem(e.FullPath);
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
            if (path == null) { return; }

            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            var modifiedTreeItem = FindChangedDirectoryTree(path);
            if (modifiedTreeItem == null) { return; }

            // 名前変更されたツリービューアイテムの取得
            var renamedTreeItem = modifiedTreeItem.Children.FirstOrDefault(item => item.FullPath == e.OldFullPath);
            if (renamedTreeItem == null) { return; }

            // ディレクトリ名前変更メッセージを送信
            WeakReferenceMessenger.Default.Send(new DirectoryRenamed(e.OldFullPath, e.FullPath));

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 一度名前変更前のアイテムを除去
                    modifiedTreeItem.Children.Remove(renamedTreeItem);
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryRenamed Remove: {ex.Message}");
                }
            });

            // 名前変更後のアイテムを再追加
            renamedTreeItem.FullPath = e.FullPath;
            int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, renamedTreeItem);

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    modifiedTreeItem.Children.Insert(newTreeIndex, renamedTreeItem);
                }
                catch (Exception ex)
                {
                    DebugManager.ExceptionWrite($"Exception in DirectoryRenamed Insert: {ex.Message}");
                }
            });

            // TODO : 特殊フォルダ内にも存在したら反映
            //FindChangedSpecialDirectoryTreeItem(e.FullPath);
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

                if (drive == null) return;

                drive.Icon = drive.Icon;
                drive.Name = drive.Name;

                // 変更されるまで 100ms 待機しながら 120回繰り返す
                var retries = 120;
                while (retries > 0)
                {
                    if (drive.Icon == _WindowsAPI.GetIcon(e.FullPath) || drive.Name == _WindowsAPI.GetDisplayName(e.FullPath))
                    {
                        retries--;
                        await Task.Delay(100);
                        continue;
                    }
                    break;
                }
                DebugManager.InfoWrite($"Insert retries : {120 - retries}");
                if (isCDRom && drive != null)
                {
                    // 光学ドライブへの挿入処理
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        drive.Icon = _WindowsAPI.GetIcon(e.FullPath);
                        drive.Name = _WindowsAPI.GetDisplayName(e.FullPath);
                        drive.HasChildren = Directory.EnumerateDirectories(e.FullPath).Any();
                    });
                }
                else
                {
                    // 取り外し可能なメディアの追加処理
                    var addNewInformation = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(e.FullPath);
                    var newTreeItem = new DirectoryTreeViewModel(addNewInformation);
                    int newTreeIndex = FindIndexToInsert(TreeRoot, newTreeItem);
                    App.Current?.Dispatcher?.Invoke(() => TreeRoot.Insert(newTreeIndex, newTreeItem));
                }
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"Exception in OpticalDriveMediaInserted: {ex.Message}");
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
                        if (drive.Icon == _WindowsAPI.GetIcon(e.FullPath) ||
                            drive.Name == _WindowsAPI.GetDisplayName(e.FullPath))
                        {
                            retries--;
                            await Task.Delay(100);
                            continue;
                        }
                        break;
                    }
                    DebugManager.InfoWrite($"Eject retries : {120 - retries}");

                    if (isCDRom)
                    {
                        // 光学ドライブへからのイジェクト処理
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            drive.Icon = _WindowsAPI.GetIcon(e.FullPath);
                            drive.Name = _WindowsAPI.GetDisplayName(e.FullPath);
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
                DebugManager.ExceptionWrite($"Exception in EjectOpticalDriveMedia: {ex.Message}");
            }
        }
        #endregion リムーバブルドライブドライブ変更通知処理

        #region TreeNode取得関連
        /// <summary>
        /// 変更が加えられたファイルアイテムのディレクトリツリーアイテムを検索します。
        /// </summary>
        /// <param name="fullPath">ファイルアイテムのパス</param>
        /// <returns>変更する必要があるディレクトリツリーアイテム</returns>
        private DirectoryTreeViewModel? FindChangedDirectoryTree(string fullPath)
        {
            DirectoryTreeViewModel? modifiedTreeItem = null;
            foreach (var dir in GetDirectoryNames(fullPath))
            {
                if (dir.Length == 3)
                {
                    // ドライブの処理
                    modifiedTreeItem = TreeRoot.FirstOrDefault(root => root.FullPath == dir) as DirectoryTreeViewModel;
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
        private List<DirectoryTreeViewModel> FindChangedSpecialDirectoryTreeItem(string fullPath)
        {
            var specialTreeItems = new List<DirectoryTreeViewModel>();

            foreach (var rootInfo in _SpecialFolderAndRootDrives.ScanSpecialFolders())
            {
                // 特殊ユーザーディレクトリのパスを持つアイテムを抽出
                if (TreeRoot.FirstOrDefault(item => item.FullPath == rootInfo.FullPath) is
                    DirectoryTreeViewModel rootItem && fullPath.Contains(rootInfo.FullPath))
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
        private static void FindTreeChild(DirectoryTreeViewModel treeItem, string path, List<DirectoryTreeViewModel> specialTreeItems)
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
