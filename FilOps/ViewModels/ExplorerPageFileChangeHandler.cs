using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public partial class ExplorerPageViewModel
    {
        #region FileSystemWatcherの宣言
        private readonly FileSystemWatcher CurrentWatcher = new();
        private readonly NotifyFilters CurrentNotifyFilter
            = NotifyFilters.FileName
            | NotifyFilters.LastWrite
            | NotifyFilters.Size;

        /// <summary>
        /// ドライブ内のディレクトリ変更を監視するインスタンス
        /// </summary>
        private readonly List<FileSystemWatcher> DrivesWatcher = [];

        /// <summary>
        /// ドライブ内のディレクトリ変更を通知するフィルタ
        /// </summary>
        private readonly NotifyFilters DriveNotifyFilter
            = NotifyFilters.DirectoryName
            | NotifyFilters.LastWrite;
        #endregion FileSystemWatcherの宣言

        #region リムーバブルディスクの着脱処理
        /// <summary>
        /// リムーバブルドライブの追加または挿入処理をします。
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター</param>
        public void InsertOpticalDriveMedia(char driveLetter)
        {
            var path = driveLetter + @":\";
            var indexToInsert = 0;

            foreach (var drive in TreeRoot)
            {
                if (drive.FullPath == path)
                {
                    var driveInfo = new DriveInfo(path);
                    if (driveInfo.IsReady && Directory.Exists(path))
                    {
                        var task = Task.Run(() =>
                        {
                            drive.IsReady = true;
                            var retries = 10;
                            while (retries > 0)
                            {
                                if (drive.Icon == WindowsAPI.GetIcon(path) || drive.Name == WindowsAPI.GetDisplayName(path))
                                {
                                    retries--;
                                    Task.Delay(100);
                                    continue;
                                }
                                break;
                            }
                            App.Current?.Dispatcher.Invoke((Action)(() =>
                            {
                                drive.Icon = WindowsAPI.GetIcon(path);
                                drive.Name = WindowsAPI.GetDisplayName(path);
                                drive.HasChildren = Directory.EnumerateDirectories(path).Any();
                            }));
                        });
                        return;
                    }
                }
                else
                {
                    if (path.CompareTo(drive.FullPath) < 0)
                    {
                        var fi = FileSystemManager.GetFileInformationFromDirectorPath(path);
                        var newTreeItem = new ExplorerTreeNodeViewModel(this, fi);
                        TreeRoot.Insert(indexToInsert, newTreeItem);
                        return;
                    }
                }
                indexToInsert++;
            }
            // 最終ドライブの場合
            var addFi = FileSystemManager.GetFileInformationFromDirectorPath(path);
            var addNewTreeItem = new ExplorerTreeNodeViewModel(this, addFi);
            TreeRoot.Add(addNewTreeItem);
        }

        /// <summary>
        /// リムーバブルメディアの削除またはイジェクト処理を行います
        /// </summary>
        /// <param name="driveLetter">リムーバブルドライブのドライブレター></param>
        public void EjectOpticalDriveMedia(char driveLetter)
        {
            var path = driveLetter + @":\";
            var driveInfo = new DriveInfo(path);
            var isCDRom = driveInfo.DriveType == DriveType.CDRom;

            var drive = TreeRoot.FirstOrDefault(c => c.FullPath == path);

            if (drive != null)
            {
                if (isCDRom)
                {
                    // 光学ドライブの場合
                    var task = Task.Run(() =>
                    {
                        var retries = 10;
                        while (retries > 0)
                        {
                            if (drive.Icon == WindowsAPI.GetIcon(path) || drive.Name == WindowsAPI.GetDisplayName(path))
                            {
                                retries--;
                                Task.Delay(100);
                                continue;
                            }
                            break;
                        }
                        App.Current?.Dispatcher.Invoke((Action)(() =>
                        {
                            drive.Icon = WindowsAPI.GetIcon(path);
                            drive.Name = WindowsAPI.GetDisplayName(path);
                            drive.HasChildren = false;
                            drive.IsReady = false;
                        }));
                    });
                }
                else
                {
                    // 取り外し可能なメディアの場合
                    TreeRoot.Remove(drive);
                }
            }
        }
        #endregion リムーバブルディスクの着脱処理

        #region ヘルパーメソッド
        /// <summary>
        /// ソート済みの位置に挿入するためのヘルパーメソッド
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

        /// <summary>
        /// 監視に引っかかっても、変更をするかどうかを取得する
        /// </summary>
        /// <param name="path">監視に引っかかったファイルパス</param>
        /// <returns>変更を反映するか否か</returns>
        private static bool IsSpecialFolderOrCannotAccess(string path)
        {
            if (path.Length == 3) { return false; }
            if (path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.Windows))) return true;
            if (path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))) return true;
            if (path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))) return true;
            if (path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))) return true;
            if (path.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))) return true;
            if (path.Contains(Path.GetTempPath())) return true;
            try
            {
                var hasDirectory = Directory.EnumerateFileSystemEntries(path).Any();
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException ||
                    ex is IOException)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion ヘルパーメソッド

        #region ドライブ監視関連
        /// <summary>
        /// ドライブに対してファイル変更監視の設定をする
        /// </summary>
        /// <param name="rootTreeItem"></param>
        private void AddRootDriveWatcher(ExplorerTreeNodeViewModel rootTreeItem)
        {
            var watcher = new FileSystemWatcher();
            
            if (rootTreeItem.IsReady)
            {
                watcher.Path = rootTreeItem.FullPath;
                watcher.NotifyFilter = DriveNotifyFilter;
                watcher.EnableRaisingEvents = true;
                watcher.IncludeSubdirectories = true;
                watcher.InternalBufferSize = 65536;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Renamed += OnRenamed;

                watcher.Error += OnError;

                DrivesWatcher.Add(watcher);
            }
        }

        /// <summary>
        /// FIleSystemWatcherエラーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"FileSystemWatcher エラー: {e.GetException()}");
        }

        /// <summary>
        /// ドライブルートのアイテムが削除された時の処理
        /// </summary>
        /// <param name="fullPath">削除されたファイルアイテムのフルパス</param>
        /// <returns>Task</returns>
        private async Task HandleRecycleBinChange(string fullPath)
        {
            // ドライブルートが削除を検出できない対策
            var rootPath = Path.GetPathRoot(fullPath);
            if (rootPath == null) { return; }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // ドライブルートのツリービューアイテムを取得
                var root = TreeRoot.FirstOrDefault(r => r.FullPath == rootPath);
                if (root != null)
                {
                    // 削除されたアイテムの取得
                    var dirs = Directory.EnumerateDirectories(rootPath);
                    var modifiedTreeItem = root.Children.Select(c => c.FullPath).Except(dirs);

                    var deletedTreeItem = root.Children.FirstOrDefault(c => c.FullPath == modifiedTreeItem.FirstOrDefault());
                    if (deletedTreeItem != null)
                    {
                        root.Children.Remove(deletedTreeItem);
                    }

                    // リストビューにも表示されていたら、そちらも更新
                    if (deletedTreeItem != null && root == CurrentItem)
                    {
                        var listItem = ListFile.FirstOrDefault(i => i.FullPath == deletedTreeItem.FullPath);
                        if (listItem != null)
                        {
                            ListFile.Remove(listItem);
                            //Debug.WriteLine($"List Item Delete {deletedTreeItem.FullPath}");
                        }
                    }
                    //Debug.WriteLine($"Root Deleted {modifiedTreeItem.FirstOrDefault()}");
                }
            });
        }

        /// <summary>
        /// 変更が加えられたファイルアイテムのディレクトリツリーアイテム
        /// </summary>
        /// <param name="path">ファイルアイテムのフルパス</param>
        /// <returns>変更する必要があるディレクトリツリーアイテム</returns>
        private ExplorerTreeNodeViewModel? FindChangedDirectoryTree(string path)
        {
            ExplorerTreeNodeViewModel? modifiedTreeItem = null;
            foreach (var dir in GetDirectoryNames(path))
            {
                if (dir.Length == 3)
                {
                    // ドライブの処理
                    modifiedTreeItem = TreeRoot.FirstOrDefault(root => root.FullPath == dir);
                    if (dir == path) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                }
                else if (modifiedTreeItem != null)
                {
                    // サブディレクトリの処理
                    modifiedTreeItem = modifiedTreeItem.Children.FirstOrDefault(child => child.FullPath == dir);
                    if (dir == path) { return modifiedTreeItem; }
                    if (modifiedTreeItem == null) { break; }
                    if (!modifiedTreeItem.IsExpanded) { break; }
                }
            }
            return null;
        }

        /// <summary>
        /// サブディレクトリが名前変更された時のUI処理
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="deletedItemNames">削除されたアイテムのコレクション</param>
        /// <param name="addedItemNames">追加されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async Task OnSubDirectoryRenamed(ExplorerTreeNodeViewModel modifiedTreeItem, IEnumerable<string> deletedItemNames,  IEnumerable<string> addedItemNames)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 名前変更されたツリービューアイテムの取得
                var renamedTreeItem = modifiedTreeItem.Children.FirstOrDefault(item => item.FullPath == deletedItemNames.FirstOrDefault());
                if (renamedTreeItem != null)
                {
                    // 新しい名前を取得
                    var renamedName = addedItemNames.FirstOrDefault();
                    if (renamedName != null && !string.IsNullOrEmpty(renamedName))
                    {
                        var oldName = renamedTreeItem.FullPath;

                        // 名前変更を反映
                        //Debug.WriteLine($"Renamed {oldName} to {renamedName}");
                        renamedTreeItem.FullPath = renamedName;

                        // リストビューにも表示されていたら、そちらも更新
                        if (CurrentItem != null && modifiedTreeItem == CurrentItem)
                        {
                            //Debug.WriteLine($"Renamed ListView {oldName} to {renamedName}");
                            var listItem = ListFile.FirstOrDefault(item => item.FullPath == oldName);
                            if (listItem != null) { listItem.FullPath = renamedName; }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// サブディレクトリが削除された時のUI処理
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="deletedItemNames">削除されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async Task OnSubDirectoryDeleted(ExplorerTreeNodeViewModel modifiedTreeItem, IEnumerable<string> deletedItemNames)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 削除されたツリービューアイテムの取得
                var deletedTreeItem = modifiedTreeItem.Children.FirstOrDefault(c => c.FullPath == deletedItemNames.FirstOrDefault());
                if (deletedTreeItem != null)
                {
                    // 削除されたツリービューアイテムの削除
                    modifiedTreeItem.Children.Remove(deletedTreeItem);

                    // リストビューにも表示されていたら、そちらも更新
                    if (CurrentItem != null && modifiedTreeItem == CurrentItem)
                    {
                        var listItem = ListFile.FirstOrDefault(item => item.FullPath == deletedTreeItem.FullPath);
                        if (listItem != null) { ListFile.Remove(listItem); }
                    }
                }
            });
        }

        /// <summary>
        /// サブディレクトリが追加された時のUI処理
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="addedItemNames">追加されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async Task OnSubDirectoryCreated(ExplorerTreeNodeViewModel modifiedTreeItem, IEnumerable<string> addedItemNames)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 追加されたファイル名の取得
                var addName = addedItemNames.FirstOrDefault();
                if (addName == null) { return; }

                // 追加されたファイルをツリービューアイテムにして追加
                var fi = FileSystemManager.GetFileInformationFromDirectorPath(addName);
                var addItem = new ExplorerTreeNodeViewModel(this, fi);
                Debug.WriteLine($"Add : {fi.FullPath}");
                int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, addItem);
                modifiedTreeItem.Children.Insert(newTreeIndex, addItem);

                // リストビューにも表示されていたら、そちらも更新
                if (CurrentItem != null && modifiedTreeItem == CurrentItem)
                {
                    var li = new ExplorerListItemViewModel(this, fi);
                    var newListItem = new ExplorerListItemViewModel(this, fi);
                    int newListIndex = FindIndexToInsert(ListFile, newListItem);
                    ListFile.Insert(newListIndex, li);
                }
            });
        }

        /// <summary>
        /// ファイルの変更通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">FileSystemEventArgs</param>
        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (IsSpecialFolderOrCannotAccess(e.FullPath)) return;

            /// ごみ箱の処理
            if (e.FullPath.Contains("$RECYCLE.BIN")) {
                await HandleRecycleBinChange(e.FullPath);
                return;
            }

            // 変更が加えられたディレクトリのツリーアイテムを取得
            var modifiedTreeItem = FindChangedDirectoryTree(e.FullPath);
            if (modifiedTreeItem == null) { return; }

            // カレントディレクトリかどうか
            var isCurrent = modifiedTreeItem == CurrentItem;

            var dirs = Directory.EnumerateDirectories(e.FullPath);
            var treeItems = modifiedTreeItem.Children
                .Where(c => !string.IsNullOrEmpty(c.FullPath))
                .Select(c => c.FullPath);

            var deletedItemNames = treeItems.Except(dirs);
            var addedItemNames = dirs.Except(treeItems);

            // リネーム処理
            if (deletedItemNames.Any() && addedItemNames.Any())
            {
                await OnSubDirectoryRenamed(modifiedTreeItem, deletedItemNames, addedItemNames);
                return;
            }

            if (deletedItemNames.Any())
            {
                await OnSubDirectoryDeleted(modifiedTreeItem, deletedItemNames);
                return;
            }

            if (addedItemNames.Any())
            {
                await OnSubDirectoryCreated(modifiedTreeItem, addedItemNames);
            }
        }

        /// <summary>
        /// パスがドライブルートかを確認する
        /// </summary>
        /// <param name="path">ドライブルートか確認するパス</param>
        /// <returns>ドライブルートかどうか</returns>
        private static bool IsRoot(string path)
        {
            var splitPath = path.Split('\\');
            return splitPath.Length == 2;
        }

        /// <summary>
        /// ドライブルートにファイルアイテムが作成された時の処理
        /// </summary>
        /// <param name="createdFile"></param>
        /// <returns></returns>
        private async Task OnRootCreated(string createdFile)
        {
            //Debug.WriteLine($"Enter Root Created : {rootPath}");
            var rootItem = TreeRoot.FirstOrDefault(item => item.FullPath == Path.GetPathRoot(createdFile));
            if (rootItem != null)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 新しいツリービューアイテムの作成
                    var fi = FileSystemManager.GetFileInformationFromDirectorPath(createdFile);
                    var addItem = new ExplorerTreeNodeViewModel(this, fi);
                    
                    // ソートされているので、そこに新しいツリービューアイテムを挿入
                    int newTreeIndex = FindIndexToInsert(rootItem.Children, addItem);
                    rootItem.Children.Insert(newTreeIndex, addItem);

                    // リストビューにも表示されていたら、そちらも更新
                    if (rootItem == CurrentItem)
                    {
                        var li = new ExplorerListItemViewModel(this, fi);
                        var newListItem = new ExplorerListItemViewModel(this, fi);
                        int newListIndex = FindIndexToInsert(ListFile, newListItem);
                        ListFile.Insert(newListIndex, li);
                    }
                });
            }
        }

        /// <summary>
        /// ドライブルートのファイルアイテムが削除された時の処理
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        private async Task OnRootRenamed(string oldPath, string newPath)
        {
            var rootItem = TreeRoot.FirstOrDefault(item => item.FullPath == Path.GetPathRoot(newPath));
            if (rootItem != null)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 名前変更されたツリービューアイテムの取得
                    var renamedItem = rootItem.Children.FirstOrDefault(item => item.FullPath == oldPath);
                    
                    // 名前変更の反映
                    if (renamedItem != null) { renamedItem.FullPath = newPath; }

                    // リストビューにも表示されていたら、そちらも更新
                    if (rootItem == CurrentItem)
                    {
                        var listItem = ListFile.FirstOrDefault(i => i.FullPath == oldPath);
                        if (listItem != null) { listItem.FullPath = newPath; }
                    }
                });
            }
        }

        /// <summary>
        /// ドライブルートのファイル作成通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">FileSystemEventArgs</param>
        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            if (!IsRoot(e.FullPath)) { return; }
            await OnRootCreated(e.FullPath);
        }

        /// <summary>
        /// ドライブルートのファイル名前変更通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">RenamedEventArgs</param>
        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;

            if (!IsRoot(e.FullPath)) { return; }
            await OnRootRenamed(e.OldFullPath, e.FullPath);
        }
        #endregion ドライブ監視関連

        #region カレントディレクトリ監視関連
        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をする
        /// </summary>
        /// <param name="rootTreeItem"></param>
        private void SetCurrentDirectoryWatcher(string currentDirectory)
        {
            // System.IO.FileNotFoundException: 'Error reading the C:\Windows\CSC directory.'

            try
            {
                CurrentWatcher.Created -= OnCurrentCreated;
                CurrentWatcher.Deleted -= OnCurrentDeleted;
                CurrentWatcher.Renamed -= OnCurrentRenamed;

                CurrentWatcher.Path = currentDirectory;
                CurrentWatcher.NotifyFilter = CurrentNotifyFilter;
                CurrentWatcher.EnableRaisingEvents = true;
                CurrentWatcher.IncludeSubdirectories = false;

                CurrentWatcher.Created += OnCurrentCreated;
                CurrentWatcher.Deleted += OnCurrentDeleted;
                CurrentWatcher.Renamed += OnCurrentRenamed;

                CurrentWatcher.Error += OnError;
            }
            catch (FileNotFoundException) { }
        }

        /// <summary>
        /// カレントディレクトリのファイルの作成通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCurrentCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;

            ///Debug.WriteLine($"Current Created : {e.FullPath}");
            // 新しいリストビューアイテムを作成する
            var fi = FileSystemManager.GetFileInformationFromDirectorPath(e.FullPath);
            var item = new ExplorerListItemViewModel(this, fi);
            var newListItem = new ExplorerListItemViewModel(this, fi);

            // リストビューに作成したアイテムを追加する
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                int newListIndex = FindIndexToInsert(ListFile, newListItem);
                ListFile.Insert(newListIndex, item);
            });
            
        }

        /// <summary>
        /// カレントディレクトリのファイル削除通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCurrentDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Deleted) return;
            if (File.Exists(e.FullPath)) { return; }

            //Debug.WriteLine($"Current Deleted : {e.FullPath}");
            // 削除されたアイテムを検索し、リストビューから削除する
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                var listItem = ListFile.FirstOrDefault(i => i.FullPath == e.FullPath);
                if (listItem != null) { ListFile.Remove(listItem); }
            });
        }

        /// <summary>
        /// カレントディレクトリのファイル名前変更通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">RenamedEventArgs</param>
        private void OnCurrentRenamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine($"Current Renamed : {e.OldFullPath} to {e.FullPath}");

            // 名前が変更されたアイテムを検索し、リストビューに新しい名前を反映する
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                var listItem = ListFile.FirstOrDefault(i => i.FullPath == e.OldFullPath);
                if (listItem != null) { listItem.FullPath = e.FullPath; }
            });
            
        }
        #endregion カレントディレクトリ監視関連
    }
}
