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
using FilOps.ViewModels.DirectoryTreeViewControl;
using FilOps.ViewModels.FileSystemWatch;

namespace FilOps.ViewModels.ExplorerPage
{
    #region インターフェース
    public interface IExplorerPageViewModel
    {
        /// <summary>
        /// リストビューへのアクセス
        /// </summary>
        public ObservableCollection<ExplorerListItemViewModel> ListItems { get; set; }

        /// <summary>
        /// ツリービューのチェックボックスの表示状態を取得
        /// </summary>
        public Visibility IsCheckBoxVisible { get; }

        /// <summary>
        /// カレントディレクトリのフルパスへのアクセス
        /// </summary>
        public string CurrentFullPath { get; set; }

        /// <summary>
        /// フォントサイズへのアクセス
        /// </summary>
        public double FontSize { get; set; }

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
        public ObservableCollection<ExplorerListItemViewModel> ListItems { get; set; } = [];

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

                if (changedDirectory.Length <= 3)
                {
                    // 3文字なら大文字化
                    changedDirectory = value.ToUpper();
                }
                else
                {
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
            get => _MainViewModel.FontSize;
            set
            {
                _MainViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
        #endregion データバインディング

        #region コンストラクタ
        private readonly ICurrentDirectoryFIleSystemWatcherService _CurrentDirectoryWatcherService;
        private readonly IDrivesFileSystemWatcherService _DrivesFileSystemWatcherService;
        private readonly IExpandedDirectoryManager _ExpandedDirectoryManager;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly IDirectoryTreeViewControlViewModel _DirectoryTreeViewControlViewModel;
        private readonly IMainViewModel _MainViewModel;
        public ExplorerPageViewModel(
            ICurrentDirectoryFIleSystemWatcherService currentDirectoryFIleSystemWatcherService,
            IDrivesFileSystemWatcherService drivesFileSystemWatcherService,
            IExpandedDirectoryManager expandedDirectoryManager,
            ICheckedDirectoryManager checkedDirectoryManager,
            IDirectoryTreeViewControlViewModel directoryTreeViewControlViewModel,
            IMainViewModel mainViewModel
            )
        {
            _CurrentDirectoryWatcherService = currentDirectoryFIleSystemWatcherService;
            _DrivesFileSystemWatcherService = drivesFileSystemWatcherService;
            _ExpandedDirectoryManager = expandedDirectoryManager;
            _CheckedDirectoryManager = checkedDirectoryManager;
            _DirectoryTreeViewControlViewModel = directoryTreeViewControlViewModel;
            _MainViewModel = mainViewModel;

            // 「上へ」ボタンのコマンド
            ToUpDirectory = new DelegateCommand(
                () => {
                    var ParentPath = Path.GetDirectoryName(CurrentFullPath);
                    if (ParentPath != null) { CurrentFullPath = ParentPath; }

                },
                () => { return Directory.Exists(Path.GetDirectoryName(CurrentFullPath)); }
            );

            // リストビューの更新コマンド
            ListViewUpdater = new DelegateCommand(async () =>
            {
                ListItems.Clear();
               await Task.Run(() =>
               {
                   foreach (var folderFile in FileSystemInformationManager.ScanFileItems(CurrentFullPath, true))
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

            // リストビューアイテムがダブルクリックされた時のコマンド
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

            // デバッグウィンドウを開くコマンド
            DebugOpen = new DelegateCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                var debugWindowService = new DebugWindowService(debugWindow);
                debugWindowService.ShowDebugWindow();
            });

            // TreeViewにルートアイテムを登録する
            foreach (var rootInfo in FileSystemInformationManager.ScanSpecialFolders())
            {
                _DirectoryTreeViewControlViewModel.AddRoot(rootInfo);
            }
            foreach (var rootInfo in FileSystemInformationManager.ScanDrives())
            {
                _DirectoryTreeViewControlViewModel.AddRoot(rootInfo);
            }

            // カレントディレクトリ変更のメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentChangeMessage>(this, (recipient, message) =>
            {
                CurrentFullPath = message.CurrentFullPath;
            });

            _CurrentDirectoryWatcherService.Created += CurrentDirectoryItemCreated;
            _CurrentDirectoryWatcherService.Deleted += CurrentDirectoryItemDeleted;
            _CurrentDirectoryWatcherService.Renamed += CurrentDirectoryItemRenamed;

            // ディレクトリ作成のメッセージ受信
            WeakReferenceMessenger.Default.Register<DirectoryCreated>(this, (recipient, message) =>
            {
                CurrentDirectoryItemCreated(message.FullPath);
            });

            // ディレクトリ名前変更のメッセージ受信
            WeakReferenceMessenger.Default.Register<DirectoryRenamed>(this, (recipient, message) =>
            {
                CurrentDirectoryItemRenamed(message.OldFullPath, message.NewFullPath);
            });

            // ディレクトリ削除のメッセージ受信
            WeakReferenceMessenger.Default.Register<DirectoryDeleted>(this, (recipient, message) =>
            {
                CurrentDirectoryItemDeleted(message.FullPath);
            });
        }
        #endregion コンストラクタ

        #region リストビューのディレクトリ更新通知処理
        /// <summary>
        /// カレントディレクトリにディレクトリが追加された時の処理です。
        /// </summary>
        /// <param name="FullPath">作成されたディレクトリのフルパス</param>
        public async void CurrentDirectoryItemCreated(string FullPath)
        {
            if (CurrentFullPath != Path.GetDirectoryName(FullPath)) return;

            var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(FullPath);
            var addListItem = new ExplorerListItemViewModel(this, fileInformation);
            int newListIndex = FindIndexToInsert(ListItems, addListItem);
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                ListItems.Insert(newListIndex, addListItem);
            });
        }

        /// <summary>
        /// カレントディレクトリのディレクトリが名前変更された時の処理です。
        /// </summary>
        /// <param name="OldFullPath">古いディレクトリのフルパス</param>
        /// <param name="NewFullPath">新しいディレクトリのフルパス</param>
        public async void CurrentDirectoryItemRenamed(string OldFullPath, string NewFullPath)
        {
            // リストビューにも表示されていたら、そちらも更新
            if (CurrentFullPath != Path.GetDirectoryName(NewFullPath)) return;

            var listItem = ListItems.FirstOrDefault(item => item.FullPath == OldFullPath);
            if (listItem == null) return;

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // 一度名前変更前のアイテムを除去
                    ListItems.Remove(listItem);
                
                    // 名前変更後のアイテムを再追加
                    listItem.FullPath = NewFullPath;
                    int newListIndex = FindIndexToInsert(ListItems, listItem);
                    ListItems.Insert(newListIndex, listItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in DirectoryRenamed: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// カレントディレクトリのディレクトリが削除された時の処理です。
        /// </summary>
        /// <param name="FullPath"></param>
        public async void CurrentDirectoryItemDeleted(string FullPath)
        {
            if (CurrentFullPath != Path.GetDirectoryName(FullPath)) return;
            var listItem = ListItems.FirstOrDefault(item => item.FullPath == FullPath);
            if (listItem == null) { return; }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    ListItems.Remove(listItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in DirectoryRenamed: {ex.Message}");
                }
            });
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
        #endregion リストビューの更新通知処理

        #region リストビューのファイル更新通知処理
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
        #endregion リストビューのファイル更新通知処理

        #region ドライブ変更のフック処理
        /// <summary>
        /// ページのHwndSourceを保持するための変数
        /// </summary>
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
                    _DrivesFileSystemWatcherService.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
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
                    _DrivesFileSystemWatcherService?.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }
        #endregion ドライブ変更のフック処理
    }
}
