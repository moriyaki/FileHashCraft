﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using FilOps.ViewModels.DebugWindow;

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
        /// カレントディレクトリのフルパスへのアクセス
        /// </summary>
        public string CurrentDirectory { get; set; }

        /// <summary>
        /// カレントディレクトリのツリービューアイテムへのアクセス
        /// </summary>
        public ExplorerTreeNodeViewModel? CurrentDirectoryItem { get; set; }

        /// <summary>
        /// フォントサイズへのアクセス
        /// </summary>
        public double FontSize { get; set; }

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

        /// <summary>
        /// TreeViewItem の CheckBox がチェックされた時、子ディレクトリを含む形でをチェックマネージャに追加します。
        /// </summary>
        /// <param name="node">CHeckBOx がチェックされた TreeViewItem</param>
        public void AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(ExplorerTreeNodeViewModel node);

        /// <summary>
        /// TreeViewItem の CheckBox が状態変更した時、自分自身のディレクトリのみをチェックマネージャに追加します。
        /// </summary>
        /// <param name="node">CheckBox の状態が変更された TreeViewItem</param>
        public void AddDirectoryOnlyToCheckedDirectoryManager(ExplorerTreeNodeViewModel node);

        /// <summary>
        /// TreeViewItem の CheckBox が状態変更した時、チェックマネージャーから削除します。
        /// </summary>
        /// <param name="node">CheckBox の状態が変更された TreeViewItem</param>
        public void RemoveDirectoryFromDirectoryManager(ExplorerTreeNodeViewModel node);

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
        /// カレントディレクトリ
        /// </summary>
        private string _currentDirectory = string.Empty;
        public string CurrentDirectory
        {
            get => _currentDirectory;
            set
            {
                string changedDir = value;

                if (_currentDirectory.Length == 1 && value.Length == 2)
                {
                    // 1文字から2文字になった時は、'\\' を追加する
                    changedDir = changedDir.ToUpper() + Path.DirectorySeparatorChar;
                }
                else if (changedDir.Length < 3)
                {
                    // 3文字未満なら返る
                    SetProperty(ref _currentDirectory, value);
                    return;
                }

                // 可能なら、表示を大文字小文字正しいものを取得
                if (Directory.Exists(value))
                {
                    var dirName = Path.GetDirectoryName(changedDir);
                    if (dirName is not null)
                    {
                        var dirs = Directory.GetDirectories(dirName);
                        changedDir = dirs?.FirstOrDefault(dir => dir.Equals(value, StringComparison.OrdinalIgnoreCase)) ?? value;
                    }
                }

                // 同じ値ならセットしない
                if (Path.Equals(_currentDirectory, changedDir)) return;

                // 値のセット
                if (!SetProperty(ref _currentDirectory, changedDir)) return;

                // ディレクトリが存在するなら、CurrentItemを設定
                if (Directory.Exists(changedDir))
                {
                    CurrentDirectoryItem = FolderSelectedChanged(changedDir);
                }
            }
        }

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
                CurrentWatcherService.SetCurrentDirectoryWatcher(value.FullPath);

                // 選択ディレクトリを移す
                value.IsSelected = true;

                // リストビューをカレントディレクトリに更新する
                ListViewUpdater.Execute(null);

                // カレントディレクトリの文字列を更新する
                CurrentDirectory = value.FullPath;
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
        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (8 <= value && value <= 24)
                {
                    SetProperty(ref _FontSize, value);
                }
            }
        }
        #endregion データバインディング

        #region コンストラクタと初期化
        private readonly IDrivesFileSystemWatcherService DriveWatcherService;
        private readonly ICurrentDirectoryFIleSystemWatcherService CurrentWatcherService;
        private readonly IExpandedDirectoryManager ExpandDirManager;
        private readonly ICheckedDirectoryManager CheckedDirManager;
        private readonly IFileSystemInformationManager FileSystemInfoManager;
        private readonly IMainViewModel MainViewModel;
        public ExplorerPageViewModel(
            IDrivesFileSystemWatcherService driveWatcherService,
            ICurrentDirectoryFIleSystemWatcherService currentWatcherService,
            IExpandedDirectoryManager expandDirManager,
            ICheckedDirectoryManager checkedDirManager,
            IFileSystemInformationManager fileSystemInfoManager,
            IMainViewModel mainViewModel
            )
        {
            DriveWatcherService = driveWatcherService;
            CurrentWatcherService = currentWatcherService;
            ExpandDirManager = expandDirManager;
            CheckedDirManager = checkedDirManager;
            FileSystemInfoManager = fileSystemInfoManager;
            MainViewModel = mainViewModel;

            ToUpDirectory = new DelegateCommand(
                () => { if (CurrentDirectoryItem != null) { CurrentDirectoryItem = CurrentDirectoryItem.Parent; } },
                () => { return CurrentDirectoryItem != null && CurrentDirectoryItem.Parent != null; }
            );

            ListViewUpdater = new DelegateCommand(async () =>
            {
                ListItems.Clear();
                if (CurrentDirectoryItem != null)
                {
                    await Task.Run(() => FolderFileListScan(CurrentDirectoryItem.FullPath));
                }
            });

            FileListViewExecuted = new DelegateCommand(() =>
            {
                if (SelectedListViewItem is not null)
                {
                    var newDir = SelectedListViewItem.FullPath;
                    if (Directory.Exists(newDir))
                    {
                        CurrentDirectory = newDir;
                    }
                }
            });

            DebugOpen = new DelegateCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                var debugWindowService = new DebugWindowService(debugWindow);
                debugWindowService.ShowDebugWindow();
            });

            DriveWatcherService.Changed += DirectoryChanged;
            DriveWatcherService.Created += DirectoryCreated;
            DriveWatcherService.Renamed += DirectoryRenamed;
            DriveWatcherService.OpticalDriveMediaInserted += OpticalDriveMediaInserted;
            DriveWatcherService.OpticalDriveMediaEjected += EjectOpticalDriveMedia;

            CurrentWatcherService.Created += CurrentDirectoryItemCreated;
            CurrentWatcherService.Deleted += CurrentDirectoryItemDeleted;
            CurrentWatcherService.Renamed += CurrentDirectoryItemRenamed;

            /*
            // TreeView の遅延処理用
            _updateTimer = new System.Timers.Timer(300); // 300ミリ秒の遅延
            _updateTimer.Elapsed += HandleUpdateTimerElapsed;
            */
        }

        private bool IsInitialized = false;
        /// <summary>
        /// 初期化処理を行います。
        /// </summary>
        public void InitializeOnce()
        {
            if (!IsInitialized)
            {
                foreach (var rootInfo in FileSystemInfoManager.SpecialFolderScan())
                {
                    var item = new SpecialFolderTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    ExpandDirManager.AddDirectory(rootInfo.FullPath);
                }
                foreach (var rootInfo in FileSystemInformationManager.DriveScan())
                {
                    var item = new DirectoryTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    ExpandDirManager.AddDirectory(rootInfo.FullPath);
                    DriveWatcherService.SetRootDirectoryWatcher(rootInfo);
                }
                IsInitialized = true;
            }
        }
        #endregion コンストラクタと初期化

        #region アイテムの挿入位置を決定するヘルパー
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

        #region 展開マネージャへの追加削除処理
        /// <summary>
        /// TreeViewItem が展開された時に展開マネージャに通知します。
        /// </summary>
        /// <param name="node">展開されたノード</param>
        public void AddDirectoryToExpandedDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            ExpandDirManager.AddDirectory(node.FullPath);
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
            ExpandDirManager.RemoveDirectory(node.FullPath);
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
            return ExpandDirManager.IsExpandedDirectory(node.FullPath);
        }
        #endregion 展開マネージャへの追加削除処理

        #region チェックマネージャへの追加削除処理
        /// <summary>
        /// TreeViewItem の CheckBox がチェックされた時、子ディレクトリを含む形でをチェックマネージャに追加する。
        /// </summary>
        /// <param name="node">CHeckBOx がチェックされた TreeViewItem</param>
        public void AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            CheckedDirManager.AddDirectoryWithSubdirectories(node.FullPath);
        }

        /// <summary>
        /// TreeViewItem の CheckBox が状態変更した時、自分自身のディレクトリのみをチェックマネージャに追加する。
        /// </summary>
        /// <param name="node">CheckBox の状態が変更された TreeViewItem</param>
        public void AddDirectoryOnlyToCheckedDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            CheckedDirManager.AddDirectoryOnly(node.FullPath);
        }

        /// <summary>
        /// TreeViewItem の CheckBox が状態変更した時、チェックマネージャから削除する。
        /// </summary>
        /// <param name="node">CheckBox の状態が変更された TreeViewItem</param>
        public void RemoveDirectoryFromDirectoryManager(ExplorerTreeNodeViewModel node)
        {
            CheckedDirManager.RemoveDirectory(node.FullPath);
        }
        #endregion  チェックマネージャへの追加削除処理
 
        #region ファイルアイテム作成
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

        #region ドライブ変更のフック処理
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
                    DriveWatcherService.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
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
                    DriveWatcherService?.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }
        #endregion ドライブ変更のフック処理
    }

}