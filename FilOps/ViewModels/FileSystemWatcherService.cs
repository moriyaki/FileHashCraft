using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public class FileSystemWatcherService
    {
        #region FileSystemWatcherの宣言
        private static FileSystemWatcherService? _instance;
        private static readonly object LockObject = new();

        public static FileSystemWatcherService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        // 既にコンストラクタで設定されている前提
                        var dummyVM = new ExplorerPageViewModel(null);
                        _instance ??= new FileSystemWatcherService(dummyVM);
                        dummyVM = null;
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// MainViewModel
        /// </summary>
        private readonly ExplorerPageViewModel ExplorerVM;

        private FileSystemWatcherService()
        {
            throw new NotImplementedException();
        }

        public FileSystemWatcherService(ExplorerPageViewModel vm)
        {
            ExplorerVM = vm;
        }

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
            var indexToInsert = -1;

            foreach (var drive in ExplorerVM.TreeRoot)
            {
                // TreeViewの挿入位置を変更
                indexToInsert++;

                // 光学ドライブを前提とする
                if (Path.Equals(drive.FullPath, path))
                {
                    var driveInfo = new DriveInfo(path);
                    if (!(driveInfo.IsReady && Directory.Exists(path))) continue;

                    var task = Task.Run(() =>
                    {
                        drive.IsReady = true;
                        // 変更されるまで 100ms 待機しながら 20回繰り返す
                        var retries = 20;
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
                        // ドライブが認識されアイコン等が変更された
                        App.Current?.Dispatcher?.Invoke(() =>
                        {
                            drive.Icon = WindowsAPI.GetIcon(path);
                            drive.Name = WindowsAPI.GetDisplayName(path);
                            drive.HasChildren = Directory.EnumerateDirectories(path).Any();
                        });
                    });
                    return;
                }
                else
                {
                    // 取り外し可能なメディアを前提とする
                    if (path.CompareTo(drive.FullPath) >= 0) continue;

                    var addNewInformation = FileSystemManager.GetFileInformationFromDirectorPath(path);
                    var newTreeItem = new ExplorerTreeNodeViewModel(ExplorerVM, addNewInformation);
                    ExplorerVM.TreeRoot.Insert(indexToInsert, newTreeItem);
                    return;
                }
            }
            // 最終ドライブの場合
            var fileInformation = FileSystemManager.GetFileInformationFromDirectorPath(path);
            var addNewTreeItem = new ExplorerTreeNodeViewModel(ExplorerVM, fileInformation);
            ExplorerVM.TreeRoot.Add(addNewTreeItem);
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

            var drive = ExplorerVM.TreeRoot.FirstOrDefault(c => c.FullPath == path);

            if (drive == null) return;
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
                    App.Current?.Dispatcher?.Invoke(() =>
                    {
                        drive.Icon = WindowsAPI.GetIcon(path);
                        drive.Name = WindowsAPI.GetDisplayName(path);
                        drive.HasChildren = false;
                        drive.IsReady = false;
                    });
                });
            }
            else
            {
                // 取り外し可能なメディアの場合
                ExplorerVM.TreeRoot.Remove(drive);
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

        #region カレントディレクトリ監視関連
        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をする
        /// </summary>
        /// <param name="rootTreeItem"></param>
        public void SetCurrentDirectoryWatcher(string currentDirectory)
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
            var item = new ExplorerListItemViewModel(ExplorerVM, fi);
            var newListItem = new ExplorerListItemViewModel(ExplorerVM, fi);

            // リストビューに作成したアイテムを追加する
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                int newListIndex = FindIndexToInsert(ExplorerVM.ListFile, newListItem);
                ExplorerVM.ListFile.Insert(newListIndex, item);
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
                var listItem = ExplorerVM.ListFile.FirstOrDefault(i => i.FullPath == e.FullPath);
                if (listItem != null) { ExplorerVM.ListFile.Remove(listItem); }
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
                var listItem = ExplorerVM.ListFile.FirstOrDefault(i => i.FullPath == e.OldFullPath);
                if (listItem != null) { listItem.FullPath = e.FullPath; }
            });

        }
        #endregion カレントディレクトリ監視関連

        #region ドライブ監視関連
        /// <summary>
        /// ドライブに対してファイル変更監視の設定をする
        /// </summary>
        /// <param name="rootTreeItem"></param>
        public void AddRootDriveWatcher(ExplorerTreeNodeViewModel rootTreeItem)
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
                var root = ExplorerVM.TreeRoot.FirstOrDefault(r => r.FullPath == rootPath);
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
                    if (deletedTreeItem != null && root == ExplorerVM.CurrentItem)
                    {
                        var listItem = ExplorerVM.ListFile.FirstOrDefault(i => i.FullPath == deletedTreeItem.FullPath);
                        if (listItem != null)
                        {
                            ExplorerVM.ListFile.Remove(listItem);
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
            foreach (var dir in ExplorerPageViewModel.GetDirectoryNames(path))
            {
                if (dir.Length == 3)
                {
                    // ドライブの処理
                    modifiedTreeItem = ExplorerVM.TreeRoot.FirstOrDefault(root => root.FullPath == dir);
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
        /// パスがドライブルートかを確認する
        /// </summary>
        /// <param name="path">ドライブルートか確認するパス</param>
        /// <returns>ドライブルートかどうか</returns>
        private static bool IsRoot(string path) => path.Split('\\').Length == 2;

        /// <summary>
        /// ドライブルートにファイルアイテムが作成された時の処理
        /// </summary>
        /// <param name="createdFile"></param>
        /// <returns></returns>
        private async Task OnRootCreated(string createdFile)
        {
            //Debug.WriteLine($"Enter Root Created : {rootPath}");
            var rootItem = ExplorerVM.TreeRoot.FirstOrDefault(item => item.FullPath == Path.GetPathRoot(createdFile));
            if (rootItem != null)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 新しいツリービューアイテムの作成
                    var fi = FileSystemManager.GetFileInformationFromDirectorPath(createdFile);
                    var addItem = new ExplorerTreeNodeViewModel(ExplorerVM, fi);

                    // ソートされているので、そこに新しいツリービューアイテムを挿入
                    int newTreeIndex = FindIndexToInsert<ExplorerTreeNodeViewModel>(rootItem.Children, addItem);
                    rootItem.Children.Insert(newTreeIndex, addItem);

                    // リストビューにも表示されていたら、そちらも更新
                    if (rootItem == ExplorerVM.CurrentItem)
                    {
                        var li = new ExplorerListItemViewModel(ExplorerVM, fi);
                        var newListItem = new ExplorerListItemViewModel(ExplorerVM, fi);
                        int newListIndex = FindIndexToInsert(ExplorerVM.ListFile, newListItem);
                        ExplorerVM.ListFile.Insert(newListIndex, li);
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
            var rootItem = ExplorerVM.TreeRoot.FirstOrDefault(item => item.FullPath == Path.GetPathRoot(newPath));
            if (rootItem != null)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 名前変更されたツリービューアイテムの取得
                    var renamedItem = rootItem.Children.FirstOrDefault(item => item.FullPath == oldPath);

                    // 名前変更の反映
                    if (renamedItem != null) { renamedItem.FullPath = newPath; }

                    // リストビューにも表示されていたら、そちらも更新
                    if (rootItem == ExplorerVM.CurrentItem)
                    {
                        var listItem = ExplorerVM.ListFile.FirstOrDefault(i => i.FullPath == oldPath);
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

        #region サブディレクトリ監視関連
        /// <summary>
        /// サブディレクトリが名前変更された時のUI処理
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="deletedItemName">削除されたアイテムのコレクション</param>
        /// <param name="addedItemName">追加されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async Task OnSubDirectoryRenamed(ExplorerTreeNodeViewModel modifiedTreeItem, string deletedItemName, string addedItemName)
        {
            //Debug.WriteLine($"Renamed : {deletedItemName} to {addedItemName");
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 名前変更されたツリービューアイテムの取得
                var renamedTreeItem = modifiedTreeItem.Children.FirstOrDefault(item => item.FullPath == deletedItemName);
                if (renamedTreeItem != null)
                {
                    // 新しい名前を取得
                    var newFullPath = addedItemName;
                    if (newFullPath != null && !string.IsNullOrEmpty(newFullPath))
                    {
                        var oldFullPath = renamedTreeItem.FullPath;

                        // 一度名前変更前のアイテムを除去
                        //Debug.WriteLine($"Renamed {oldName} to {renamedName}");
                        modifiedTreeItem.Children.Remove(renamedTreeItem);
                        renamedTreeItem.FullPath = newFullPath;

                        // 名前変更後のアイテムを再追加
                        int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, renamedTreeItem);
                        modifiedTreeItem.Children.Insert(newTreeIndex, renamedTreeItem);


                        // リストビューにも表示されていたら、そちらも更新
                        if (ExplorerVM.CurrentItem != null && modifiedTreeItem == ExplorerVM.CurrentItem)
                        {
                            //Debug.WriteLine($"Renamed ListView {oldName} to {renamedName}");
                            var listItem = ExplorerVM.ListFile.FirstOrDefault(item => item.FullPath == oldFullPath);
                            if (listItem != null) { listItem.FullPath = newFullPath; }
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
        private async Task OnSubDirectoryDeleted(ExplorerTreeNodeViewModel modifiedTreeItem, string deletedItemName)
        {
            //Debug.WriteLine($"Deleted : {deletedItemName}");
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 削除されたツリービューアイテムの取得
                var deletedTreeItem = modifiedTreeItem.Children.FirstOrDefault(c => c.FullPath == deletedItemName);
                if (deletedTreeItem != null)
                {
                    // 削除されたツリービューアイテムの削除
                    modifiedTreeItem.Children.Remove(deletedTreeItem);

                    // リストビューにも表示されていたら、そちらも更新
                    if (ExplorerVM.CurrentItem != null && modifiedTreeItem == ExplorerVM.CurrentItem)
                    {
                        var listItem = ExplorerVM.ListFile.FirstOrDefault(item => item.FullPath == deletedItemName);
                        if (listItem != null) { ExplorerVM.ListFile.Remove(listItem); }
                    }

                    // 特殊フォルダにも存在したら反映
                    FindChangedSpecialDirectoryTreeItem(deletedItemName);
                    //TODO
                }
            });
        }

        /// <summary>
        /// サブディレクトリが追加された時のUI処理
        /// </summary>
        /// <param name="modifiedTreeItem">親ツリービューアイテム</param>
        /// <param name="addedItemNames">追加されたアイテムのコレクション</param>
        /// <returns>Task</returns>
        private async Task OnSubDirectoryCreated(ExplorerTreeNodeViewModel modifiedTreeItem, string addedItemNames)
        {
            //Debug.WriteLine($"Created : {addedItemNames}");
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                // 追加されたファイルをツリービューアイテムにして追加
                var fileInformation = FileSystemManager.GetFileInformationFromDirectorPath(addedItemNames);
                var addItem = new ExplorerTreeNodeViewModel(ExplorerVM, fileInformation);
                Debug.WriteLine($"Add : {addedItemNames}");
                int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, addItem);
                modifiedTreeItem.Children.Insert(newTreeIndex, addItem);

                // リストビューにも表示されていたら、そちらも更新
                if (ExplorerVM.CurrentItem != null && modifiedTreeItem == ExplorerVM.CurrentItem)
                {
                    var listItems = new ExplorerListItemViewModel(ExplorerVM, fileInformation);
                    var newListItem = new ExplorerListItemViewModel(ExplorerVM, fileInformation);
                    int newListIndex = FindIndexToInsert(ExplorerVM.ListFile, newListItem);
                    ExplorerVM.ListFile.Insert(newListIndex, listItems);
                }
            });
        }

        /// <summary>
        /// ツリービューへの差分を取り、名前変更、追加、削除の処理に振り分ける
        /// </summary>
        /// <param name="modifiedTreeItem">変更されるツリービューアイテム</param>
        /// <param name="originalItems">変更前のコレクション</param>
        /// <param name="changedItems">変更後のコレクション</param>
        /// <returns></returns>
        private async Task ExecuteChangedTreeItemChange(ExplorerTreeNodeViewModel modifiedTreeItem,
            IEnumerable<string> originalItems, IEnumerable<string> changedItems)
        {
            // originalItems にあって changedItems に無いもの
            var deletedItemNames = originalItems.Except(changedItems);
            // changedItems にあって originalItems に無いもの
            var addedItemNames = changedItems.Except(originalItems);

            if (modifiedTreeItem != null)
            { 
                // originalItems も changedItems にもアイテムがある場合はリネーム処理
                if (deletedItemNames.Any() && addedItemNames.Any())
                {
                    await OnSubDirectoryRenamed(modifiedTreeItem, deletedItemNames.First(), addedItemNames.First());
                    return;
                }

                // originalItem にあり changedItem にない場合は削除処理
                if (deletedItemNames.Any())
                {
                    await OnSubDirectoryDeleted(modifiedTreeItem, deletedItemNames.First());
                }
                // originalItems にあり originalItem にない場合は追加処理
                if (addedItemNames.Any())
                {
                    await OnSubDirectoryCreated(modifiedTreeItem, addedItemNames.First());
                }
            }
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

            Debug.WriteLine($"Changed : {e.FullPath}");

            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            var modifiedTreeItem = FindChangedDirectoryTree(e.FullPath);
            // 変更が加えられたディレクトリ内にあるディレクトリのフルパスコレクションを取得
            var dirs = Directory.EnumerateDirectories(e.FullPath);

            FindChangedSpecialDirectoryTreeItem(e.FullPath);

            if (modifiedTreeItem != null)
            {
                // 変更が加えられる前のツリービューアイテムを取得
                var treeItems = modifiedTreeItem.Children
                    .Where(c => !string.IsNullOrEmpty(c.FullPath))
                    .Select(c => c.FullPath);

                if (treeItems != null)
                {
                    await ExecuteChangedTreeItemChange(modifiedTreeItem, treeItems, dirs);
                }
            }

            // 特殊ディレクトリフォルダの処理
            foreach (var modifiedSpecialTreeItems in FindChangedSpecialDirectoryTreeItem(e.FullPath))
            {
                var treeItems = modifiedSpecialTreeItems.Children
                    .Where(c => !string.IsNullOrEmpty(c.FullPath))
                    .Select(c => c.FullPath);

                if (treeItems != null)
                {
                    await ExecuteChangedTreeItemChange(modifiedSpecialTreeItems, treeItems, dirs);
                }
            }
        }

        /// <summary>
        /// 特殊ユーザーディレクトリルートからツリーアイテムを探す
        /// </summary>
        /// <param name="path">特殊ユーザーディレクトリが含まれることを期待するパス</param>
        /// <returns>特殊ユーザーディレクトリ内のツリーアイテム</returns>
        private List<ExplorerTreeNodeViewModel> FindChangedSpecialDirectoryTreeItem(string path)
        {
            var specialTreeItems = new List<ExplorerTreeNodeViewModel>();

            foreach (var rootInfo in FileSystemManager.Instance.SpecialFolderScan())
            {
                // 特殊ユーザーディレクトリのパスを持つアイテムを抽出
                var rootItem = ExplorerVM.TreeRoot.FirstOrDefault(item => item.FullPath == rootInfo.FullPath);

                if (rootItem != null && path.Contains(rootInfo.FullPath))
                {
                    Debug.WriteLine($"Searching : {WindowsAPI.GetDisplayName(rootInfo.FullPath)}");
                    if (Path.Equals(rootItem.FullPath, path))
                    {
                        // ルートで見つかった場合はリストに追加
                        Debug.WriteLine($"\tFound : {rootItem.FullPath}");
                        specialTreeItems.Add(rootItem);
                    }
                    else
                    {
                        // 再帰的にツリービューを検索
                        FindTreeChild(rootItem, path, specialTreeItems);
                        foreach (var item in specialTreeItems)
                        {
                            Debug.WriteLine($"\tFound : {item.FullPath}");
                        }
                    }
                }
            }
            return specialTreeItems;
        }

        /// <summary>
        /// 再帰的に、パスと等しいツリービューアイテムを探して SpecialTreeItem に追加する
        /// </summary>
        /// <param name="treeItem">検索中のツリービューアイテム</param>
        /// <param name="path">探し出すファイルのフルパス</param>
        /// <param name="specialTreeItems">見つかった時に追加するリストコレクション</param>
        private static void FindTreeChild(ExplorerTreeNodeViewModel treeItem, string path, List<ExplorerTreeNodeViewModel> specialTreeItems)
        {
            // 見つかったら終わり
            if (Path.Equals(treeItem.FullPath, path))
            {
                specialTreeItems.Add(treeItem);
                return;
            }

            // 開いてなければそこで終わり
            if (!treeItem.IsExpanded) return;

            foreach (var child in treeItem.Children)
            {
                FindTreeChild(child, path, specialTreeItems);
            }
        }
        #endregion サブディレクトリ監視関連
    }
}
