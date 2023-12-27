using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FilOps.Models;
using FilOps.ViewModels.DebugWindow;
using FilOps.ViewModels.FileSystemWatch;

namespace FilOps.ViewModels.ExplorerPage
{
    #region インターフェース
    public interface IExplorerPageViewModel
    {
        public void InitializeOnce();

        /// <summary>
        /// ツリービューへのアクセス
        /// </summary>
        public ObservableCollection<ExplorerItemViewModelBase> TreeRoot { get; set; }

        /// <summary>
        /// リストビューへのアクセス
        /// </summary>
        public ObservableCollection<ExplorerItemViewModelBase> ListItems { get; set; }

        /// <summary>
        /// ツリービューのチェックボックスの表示状態を取得
        /// </summary>
        public Visibility IsCheckBoxVisible { get; }

        /// <summary>
        /// カレントディレクトリのフルパスへのアクセス
        /// </summary>
        public string CurrentFullPath { get; set; }

        /*
        /// <summary>
        /// カレントディレクトリのツリービューアイテムへのアクセス
        /// </summary>
        public ExplorerTreeNodeViewModel? CurrentDirectoryItem { get; set; }
        */
        /// <summary>
        /// フォントサイズへのアクセス
        /// </summary>
        public double FontSize { get; set; }

        /*
        /// <summary>
        /// TreeViewItem が展開された時に展開マネージャに通知します。
        /// </summary>
        /// <param name="node">展開されたノード</param>
        public void AddDirectoryToExpandedDirectoryManager(ExplorerTreeNodeViewModel node);

        /// <summary>
        /// TreeViewItem が展開された時に展開解除マネージャに通知します。
        /// </summary>
        /// <param name="node">展開解除されたノード</param>
        public void RemoveDirectoryToExpandedDirectoryManager(ExplorerTreeNodeViewModel node);

        /// <summary>
        /// ノードが展開されているかどうかを調べます。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsExpandDirectory(ExplorerTreeNodeViewModel node);
        */

        /// <summary>
        /// WndProc をフックして、リムーバブルドライブの着脱を監視します。
        /// </summary>
        /// <param name="hwndSource">hwndSource?</param>
        public void HwndAddHook(HwndSource? hwndSource);

        /// <summary>
        /// WNdProc のフック解除して、アプリケーション終了に備えます。
        /// </summary>
        public void HwndRemoveHook();
    }
    #endregion インターフェース

    public partial class ExplorerPageViewModel : ObservableObject, IExplorerPageViewModel
    {
        #region データバインディング
        public ObservableCollection<ExplorerItemViewModelBase> TreeRoot { get; set; } = [];
        public ObservableCollection<ExplorerItemViewModelBase> ListItems { get; set; } = [];

        /// <summary>
        /// 「上へ」コマンド
        /// </summary>
        public DelegateCommand ToUpDirectory { get; set; }

        /// <summary>
        /// リストビュー更新コマンド
        /// </summary>
        public DelegateCommand ListViewUpdater { get; set; }

        /// <summary>
        /// リストビューダブルクリック時のコマンド
        /// </summary>
        public DelegateCommand FileListViewExecuted { get; set; }

        /// <summary>
        /// デバッグウィンドウを開く
        /// </summary>
        public DelegateCommand DebugOpen { get; set; }

        /// <summary>
        /// 選択されているリストビューのアイテム
        /// </summary>
        private ExplorerListItemViewModel? _SelectedListViewItem = null;
        public ExplorerListItemViewModel? SelectedListViewItem
        {
            get => _SelectedListViewItem;
            set => SetProperty(ref _SelectedListViewItem, value);
        }

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            private set => _IsCheckBoxVisible = value;
        }

        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        private string _currentDirectory = string.Empty;
        public string CurrentFullPath
        {
            get => _currentDirectory;
            set
            {
                string changedDirectory = value;

                if (_currentDirectory.Length == 1 && value.Length == 2)
                {
                    // 1文字から2文字になった時は、'\\' を追加する
                    changedDirectory = changedDirectory.ToUpper() + Path.DirectorySeparatorChar;
                }
                else if (changedDirectory.Length == 3)
                {
                    changedDirectory = value.ToUpper();
                    // 3文字なら返る
                    SetProperty(ref _currentDirectory, changedDirectory);
                    return;
                }

                // 可能なら、表示を大文字小文字正しいものを取得
                if (Directory.Exists(value))
                {
                    var dirName = Path.GetDirectoryName(changedDirectory);
                    if (dirName is not null)
                    {
                        var dirs = Directory.GetDirectories(dirName);
                        changedDirectory = dirs?.FirstOrDefault(dir => dir.Equals(value, StringComparison.OrdinalIgnoreCase)) ?? value;
                    }
                }

                // 同じ値ならセットしない
                if (Path.Equals(_currentDirectory, changedDirectory)) return;

                // 値のセット
                if (!SetProperty(ref _currentDirectory, changedDirectory)) return;

                // ディレクトリが存在するなら、CurrentItemを設定
                if (Directory.Exists(changedDirectory))
                {
                    //FolderSelectedChanged(value);
                    ToUpDirectory.RaiseCanExecuteChanged();
                    ListViewUpdater.Execute(null);
                    WeakReferenceMessenger.Default.Send(new CurrentChangeMessage(changedDirectory));
                }

            }
        }

        /*
        /// <summary>
        /// カレントディレクトリの情報
        /// </summary>
        private ExplorerTreeNodeViewModel? _CurrentIDirectorytem = null;
        public ExplorerTreeNodeViewModel? CurrentDirectoryItem
        {
            get => _CurrentIDirectorytem;
            set
            {
                if (value is null) { return; }
                if (_CurrentIDirectorytem == value) { return; }

                // 選択が変更されたら、明示的に今までの選択を外す
                if (_CurrentIDirectorytem is not null && _CurrentIDirectorytem != value)
                {
                    _CurrentIDirectorytem.IsSelected = false;
                }
                SetProperty(ref _CurrentIDirectorytem, value);

                // 「上へ」ボタンの可否をViewへ反映する
                ToUpDirectory.RaiseCanExecuteChanged();

                // カレントディレクトリが変更されたので、監視対象を移す
                _currentDirectoryWatcherService.SetCurrentDirectoryWatcher(value.FullPath);

                // 選択ディレクトリを移す
                value.IsSelected = true;

                // リストビューをカレントディレクトリに更新する
                ListViewUpdater.Execute(null);

                // カレントディレクトリの文字列を更新する
                CurrentDirectory = value.FullPath;

            }
        }
        */

        /// <summary>
        /// ユーザーコマンドの文字列
        /// </summary>
        private string _commandText = string.Empty;
        public string CommandText
        {
            get => _commandText;
            set => SetProperty(ref _commandText, value);
        }

        /// <summary>
        /// フォントサイズの変更
        /// </summary>
        public double FontSize
        {
            get => _mainViewModel.FontSize;
            set
            {
                _mainViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
        #endregion データバインディング

        #region コンストラクタと初期化
        private readonly IMainViewModel _mainViewModel;
        private readonly IDrivesFileSystemWatcherService _drivesFileSystemWatcherService;
        private readonly ICurrentDirectoryFIleSystemWatcherService _currentDirectoryWatcherService;
        private readonly IExpandedDirectoryManager _expandedDirectoryManager;
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        private readonly IFileSystemInformationManager _fileSystemInformationManager;
        public ExplorerPageViewModel(
            IDrivesFileSystemWatcherService driveWatcherService,
            ICurrentDirectoryFIleSystemWatcherService currentWatcherService,
            IExpandedDirectoryManager expandDirManager,
            ICheckedDirectoryManager checkedDirManager,
            IFileSystemInformationManager fileSystemInfoManager,
            IMainViewModel mainViewModel
            )
        {
            _drivesFileSystemWatcherService = driveWatcherService;
            _currentDirectoryWatcherService = currentWatcherService;
            _expandedDirectoryManager = expandDirManager;
            _checkedDirectoryManager = checkedDirManager;
            _fileSystemInformationManager = fileSystemInfoManager;
            _mainViewModel = mainViewModel;

            
            ToUpDirectory = new DelegateCommand(
                () => {
                    var ParentPath = Path.GetDirectoryName(CurrentFullPath);
                    if (ParentPath != null) { CurrentFullPath = ParentPath; }

                },
                () => { return Directory.Exists(Path.GetDirectoryName(CurrentFullPath)); }
            );


            ListViewUpdater = new DelegateCommand(async () =>
            {
                ListItems.Clear();
               await Task.Run(() =>
               {
                   foreach (var folderFile in FileSystemInformationManager.FileItemScan(CurrentFullPath, true))
                   {
                       // フォルダやファイルの情報を ViewModel に変換
                       var item = new ExplorerListItemViewModel(this, folderFile);

                       // UI スレッドでリストビューを更新
                       App.Current?.Dispatcher?.Invoke((Action)(() =>
                       {
                           ListItems.Add(item);
                       }));
                   }
               });

               
            });

            FileListViewExecuted = new DelegateCommand(() =>
            {
                if (SelectedListViewItem is not null)
                {
                    var newDirectory = SelectedListViewItem.FullPath;
                    if (Directory.Exists(newDirectory))
                    {
                        CurrentFullPath = newDirectory;
                    }
                }
            });

            DebugOpen = new DelegateCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                var debugWindowService = new DebugWindowService(debugWindow);
                debugWindowService.ShowDebugWindow();
            });

            WeakReferenceMessenger.Default.Register<CurrentChangeMessage>(this, (recipient, message) =>
            {
                CurrentFullPath = message.CurrentFullPath;
            });

            _drivesFileSystemWatcherService.Changed += DirectoryChanged;
            _drivesFileSystemWatcherService.Created += DirectoryCreated;
            _drivesFileSystemWatcherService.Renamed += DirectoryRenamed;
            _drivesFileSystemWatcherService.OpticalDriveMediaInserted += OpticalDriveMediaInserted;
            _drivesFileSystemWatcherService.OpticalDriveMediaEjected += EjectOpticalDriveMedia;

            _currentDirectoryWatcherService.Created += CurrentDirectoryItemCreated;
            _currentDirectoryWatcherService.Deleted += CurrentDirectoryItemDeleted;
            _currentDirectoryWatcherService.Renamed += CurrentDirectoryItemRenamed;
        }

        private bool IsInitialized = false;
        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        public void InitializeOnce()
        {
            if (!IsInitialized)
            {
                IsCheckBoxVisible = Visibility.Visible;
                foreach (var rootInfo in _fileSystemInformationManager.SpecialFolderScan())
                {
                    var item = new ExplorerTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    _expandedDirectoryManager.AddDirectory(rootInfo.FullPath);
                }
                foreach (var rootInfo in FileSystemInformationManager.DriveScan())
                {
                    var item = new ExplorerTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    _expandedDirectoryManager.AddDirectory(rootInfo.FullPath);
                    _drivesFileSystemWatcherService.SetRootDirectoryWatcher(rootInfo);
                }
                var debugWindowService = new DebugWindowService(new FilOps.Views.DebugWindow());
                debugWindowService.ShowDebugWindow();
                IsInitialized = true;
            }
        }
        #endregion コンストラクタと初期化

        #region アイテムの挿入位置を決定するヘルパー/これはこのまま
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
        #endregion アイテムの挿入位置を決定するヘルパー

        #region 展開マネージャへの追加削除処理(全コメント済)
        /*
        /// <summary>
        /// TreeViewItem が展開された時に展開マネージャに通知します。
        /// </summary>
        /// <param name="node">展開されたノード</param>
        public void AddDirectoryToExpandedDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            _expandedDirectoryManager.AddDirectory(node.FullPath);
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    AddDirectoryToExpandedDirectoryManager(child);
                }
            }
        }

        /// <summary>
        /// TreeViewItem が展開された時に展開解除マネージャに通知します。
        /// </summary>
        /// <param name="node">展開解除されたノード</param>
        public void RemoveDirectoryToExpandedDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            _expandedDirectoryManager.RemoveDirectory(node.FullPath);
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    RemoveDirectoryToExpandedDirectoryManager(child);
                }
            }
        }

        /// <summary>
        /// ノードが展開されているかどうかを調べます。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsExpandDirectory(ExplorerTreeNodeViewModel node)
        {
            return _expandedDirectoryManager.IsExpandedDirectory(node.FullPath);
        }
        */
        #endregion 展開マネージャへの追加削除処理

        #region ファイルアイテム作成/削除する
        /// <summary>
        /// フルパスからツリービューアイテムを作成する。
        /// </summary>
        /// <param name="fullPath">ファイルフルパス</param>
        /// <returns>ツリービューアイテム</returns>
        public ExplorerTreeNodeViewModel CreateTreeViewItem(string fullPath)
        {
            var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(fullPath);
            return new ExplorerTreeNodeViewModel(this, fileInformation);

        }

        /// <summary>
        /// フルパスからリストビューアイテムを作成する。
        /// </summary>
        /// <param name="fullPath">ファイルフルパス</param>
        /// <returns>リストビューアイテム</returns>
        public ExplorerListItemViewModel CreateListViewItem(string fullPath)
        {
            var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(fullPath);
            return new ExplorerListItemViewModel(this, fileInformation);
        }
        #endregion ファイルアイテム作成

        #region カレントディレクトリのファイル変更通知関連/これはこのまま
        /// <summary>
        /// カレントディレクトリにファイルが作成されたディレクトリの場合、
        /// TreeViewは全ドライブ監視が処理してくれます。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">作成されたファイルのフルパスが入っている</param>
        public void CurrentDirectoryItemCreated(object? sender, CurrentDirectoryFileChangedEventArgs e)
        {
            // 追加されたファイルの情報を取得する
            var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(e.FullPath);

            // リストビューに追加されたファイルを追加する
            var newListItem = new ExplorerListItemViewModel(this, fileInformation);
            int newListIndex = FindIndexToInsert(ListItems, newListItem);
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                ListItems.Insert(newListIndex, newListItem);
            });
        }

        /// <summary>
        /// カレントディレクトリのファイルが削除されたディレクトリの場合、
        /// TreeViewは全ドライブ監視が処理してくれます。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">削除されたファイルのフルパスが入っている</param>
        public void CurrentDirectoryItemDeleted(object? sender, CurrentDirectoryFileChangedEventArgs e)
        {
            // リストビューの削除されたアイテムを探す
            var listItem = ListItems.FirstOrDefault(i => i.FullPath == e.FullPath);

            // リストビューから削除されたファイルを取り除く
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                if (listItem != null) { ListItems.Remove(listItem); }
            });
        }

        /// <summary>
        /// カレントディレクトリのファイル名が変更されたディレクトリの場合、
        /// TreeViewは全ドライブ監視が処理してくれます。
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">名前変更されたファイルの新旧フルパスが入っている</param>

        public void CurrentDirectoryItemRenamed(object? sender, CurrentDirectoryFileRenamedEventArgs e)
        {
            // リストビューの名前変更されたアイテムを探す
            var listItem = ListItems.FirstOrDefault(i => i.FullPath == e.OldFullPath);

            // リストビューに新しい名前を反映する
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                if (listItem != null) { listItem.FullPath = e.FullPath; }
            });
        }
        #endregion カレントディレクトリのファイル変更通知関連

        #region ドライブ変更のフック処理/これはこのまま
        // ページのHwndSourceを保持するための変数
        private HwndSource? hwndSource;

        /// <summary>
        /// WndProc をフックして、リムーバブルドライブの着脱を監視します。
        /// </summary>
        /// <param name="hwndSource">hwndSource?</param>
        public void HwndAddHook(HwndSource? hwndSource)
        {
            if (hwndSource != null) { hwndSource.AddHook(WndProc); }
            else { Debug.WriteLine("HwndSourceを取得できませんでした。"); }
        }

        /// <summary>
        /// WNdProc のフック解除して、アプリケーション終了に備えます。
        /// </summary>
        public void HwndRemoveHook()
        {
            if (hwndSource != null)
            {
                hwndSource.RemoveHook(WndProc);
                hwndSource = null;
            }
        }

        /// <summary>
        /// DEV_BROADCAST_VOLUME.dbcv_unitmask からドライブレターを取得します。
        /// </summary>
        /// <param name="unitMask"></param>
        /// <returns></returns>
        private static char GetDriveLetter(uint unitMask)
        {
            for (int i = 0; i < 26; i++)
            {
                uint mask = (uint)(1 << i);
                if ((unitMask & mask) != 0) { return (char)('A' + i); }
            }
            return (char)(0);
        }

        /// <summary>
        /// 論理ボリュームに関する情報
        /// </summary>
        struct DEV_BROADCAST_VOLUME
        {
            public uint dbcv_size;
            public uint dbcv_devicetype;
            public uint dbcv_reserved;
            public uint dbcv_unitmask;
        }

        /// <summary>
        /// デバイス管理イベント
        /// </summary>
        private enum DBT
        {
            DBT_DEVICEARRIVAL = 0x8000,
            DBT_DEVICEQUERYREMOVE = 0x8001,
            DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
            DBT_DEVICEREMOVEPENDING = 0x8003,
            DBT_DEVICEREMOVECOMPLETE = 0x8004,
        }

        /// <summary>
        /// カスタムのウィンドウプロシージャ、ドライブの装着の取り外しを監視します。
        /// </summary>
        /// <param name="hwnd">IntPtr</param>
        /// <param name="msg">int</param>
        /// <param name="wParam">IntPtr</param>
        /// <param name="lParam">IntPtr</param>
        /// <param name="handled">ref bool</param>
        /// <returns>IntPtr</returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // メッセージに対する処理を追加
            Object? ptrToStructure;
            DEV_BROADCAST_VOLUME volume;
            volume.dbcv_size = 0;
            volume.dbcv_devicetype = 0;
            volume.dbcv_reserved = 0;
            volume.dbcv_unitmask = 0;

            DBT DBT_wParam;
            if (Environment.Is64BitProcess) { DBT_wParam = (DBT)wParam.ToInt64(); }
            else { DBT_wParam = (DBT)wParam.ToInt32(); }

            switch (DBT_wParam)
            {
                case DBT.DBT_DEVICEARRIVAL:
                    //ドライブが装着された時の処理を書く
                    try
                    {
                        if (lParam != IntPtr.Zero)
                        {
                            ptrToStructure = Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));
                            if (ptrToStructure != null)
                            {
                                volume = (DEV_BROADCAST_VOLUME)ptrToStructure;
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine($"WndProcで例外が発生しました: {ex.Message}"); }
                    _drivesFileSystemWatcherService.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
                    break;
                case DBT.DBT_DEVICEREMOVECOMPLETE:
                    //ドライブが取り外されたされた時の処理を書く
                    try
                    {
                        if (lParam != IntPtr.Zero)
                        {
                            ptrToStructure = Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));
                            if (ptrToStructure != null)
                            {
                                volume = (DEV_BROADCAST_VOLUME)ptrToStructure;
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine($"WndProcで例外が発生しました: {ex.Message}"); }
                    _drivesFileSystemWatcherService?.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }
        #endregion ドライブ変更のフック処理
    }

}
