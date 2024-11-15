﻿/* PageExplorerViewModel.cs

    Explorer 風の画面を提供する ViewModel を提供します。
 */

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Services;
using FileHashCraft.Services.FileSystemWatcherServices;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.ExplorerPage
{
    #region インターフェース

    public interface IExplorerPageViewModel
    {
        /// <summary>
        /// リストビューのアイテムコレクション
        /// </summary>
        ObservableCollection<ExplorerListItemViewModel> ListItems { get; set; }

        /// <summary>
        /// ツリービューのチェックボックスの表示状態
        /// </summary>
        Visibility IsCheckBoxVisible { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize();

        /// <summary>
        /// カレントディレクトリを取得または設定する
        /// </summary>
        string CurrentFullPath { get; set; }

        /// <summary>
        /// リムーバブルストレージ用のフック処理
        /// </summary>
        /// <param name="hwndSource"></param>
        void HwndAddHook(HwndSource? hwndSource);

        /// <summary>
        /// リムーバブルストレージ用のフック解除処理
        /// </summary>
        void HwndRemoveHook();
    }

    #endregion インターフェース

    public partial class ExplorerPageViewModel : BaseViewModel, IExplorerPageViewModel
    {
        #region バインディング

        public ObservableCollection<ExplorerListItemViewModel> ListItems { get; set; } = [];

        /// <summary>
        /// メニュー「設定」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string MenuSettings { get => ResourceService.GetString("MenuSettings"); }

        /// <summary>
        /// メニュー「ヘルプ」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string MenuHelp { get => ResourceService.GetString("MenuHelp"); }

        /// <summary>
        /// ラベル「カレントディレクトリ」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string LabelCurrentDirectory { get => ResourceService.GetString("LabelCurrentDirectory"); }

        /// <summary>
        /// ラベル「コマンド」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string LabelCommand { get => ResourceService.GetString("LabelCommand"); }

        /// <summary>
        /// ボタン「実効」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ButtonExecuteCommand { get => ResourceService.GetString("ButtonExecuteCommand"); }

        /// <summary>
        /// ボタン「ハッシュ管理」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ButtonHashManagement { get => ResourceService.GetString("ButtonHashManagement"); }

        /// <summary>
        /// リストビューラベル「ファイル名」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ListViewName { get => ResourceService.GetString("ListViewName"); }

        /// <summary>
        /// リストビューラベル「最終更新日」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ListViewDataModified { get => ResourceService.GetString("ListViewDataModified"); }

        /// <summary>
        /// リストビューラベル「ファイル種類」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ListViewType { get => ResourceService.GetString("ListViewType"); }

        /// <summary>
        /// リストビューラベル「ファイルサイズ」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ListViewSize { get => ResourceService.GetString("ListViewSize"); }

        /// <summary>
        /// 選択されているリストビューのアイテム
        /// </summary>
        [ObservableProperty]
        private ExplorerListItemViewModel? _selectedListViewItem = null;

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        public Visibility IsCheckBoxVisible { get; set; } = Visibility.Visible;

        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        private string _currentFullPath = string.Empty;

        public string CurrentFullPath
        {
            get => _currentFullPath;
            set
            {
                string changedDirectory = value;

                // 同じディレクトリなら値をセットして終了
                if (_currentFullPath.TrimEnd(Path.DirectorySeparatorChar) == changedDirectory.TrimEnd(Path.DirectorySeparatorChar))
                {
                    SetProperty(ref _currentFullPath, changedDirectory);
                    return;
                }
                var isDirectoreySeparatorEnd = value.EndsWith(Path.DirectorySeparatorChar);

                if (changedDirectory.Length <= 3)
                {
                    // 3文字なら大文字化
                    changedDirectory = value.ToUpper();
                }
                else if (Directory.Exists(value))
                {
                    // ディレクトリの大文字小文字正しいものを取得
                    var sepalatedPath = value.Split(Path.DirectorySeparatorChar);
                    var makeTruthPath = sepalatedPath[0].ToUpper();
                    for (var index = 1; changedDirectory.TrimEnd(Path.DirectorySeparatorChar).Length > makeTruthPath.Length; index++)
                    {
                        var dirs = Directory.EnumerateDirectories(makeTruthPath + Path.DirectorySeparatorChar);
                        makeTruthPath = makeTruthPath + Path.DirectorySeparatorChar + sepalatedPath[index];
                        makeTruthPath = dirs.FirstOrDefault(dir => dir.Contains(makeTruthPath, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
                    }

                    changedDirectory = makeTruthPath;

                    // 入力値がパス区切り文字で終わってたら追加
                    if (isDirectoreySeparatorEnd) { changedDirectory += Path.DirectorySeparatorChar; }
                }

                // 値のセット
                if (!SetProperty(ref _currentFullPath, changedDirectory)) return;
                // 異なるディレクトリに移動した時の処理
                if (Directory.Exists(changedDirectory))
                {
                    ToUpDirectory.NotifyCanExecuteChanged();
                    ListViewUpdater.Execute(null);
                    _FileSystemWatcherService.SetCurrentDirectoryWatcher(changedDirectory);
                    _FileSystemServices.NotifyChangeCurrentDirectory(changedDirectory);
                }
            }
        }

        /// <summary>
        /// ユーザーコマンドの文字列
        /// </summary>
        [ObservableProperty]
        private string _commandText = string.Empty;

        /// <summary>
        /// 「上へ」コマンド
        /// </summary>
        public RelayCommand ToUpDirectory { get; set; }

        /// <summary>
        /// リストビュー更新コマンド
        /// </summary>
        public RelayCommand ListViewUpdater { get; set; }

        /// <summary>
        /// リストビューダブルクリック時のコマンド
        /// </summary>
        public RelayCommand FileListViewExecuted { get; set; }

        /// <summary>
        /// ハッシュ管理ウィンドウ実行のコマンド
        /// </summary>
        public RelayCommand HashCalc { get; set; }

        /// <summary>
        /// 設定画面を開く
        /// </summary>
        public RelayCommand SettingsOpen { get; set; }

        /// <summary>
        /// デバッグウィンドウを開きます。
        /// </summary>
        public RelayCommand DebugOpen { get; set; }

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public RelayCommand HelpOpen { get; set; }

        #endregion バインディング

        #region コンストラクタと初期処理

        private bool IsExecuting = false;

        private readonly IFileSystemServices _FileSystemServices;
        private readonly IFileSystemWatcherService _FileSystemWatcherService;
        private readonly IDirectoryTreeManager _DirectoryTreeManager;
        private readonly IFileManager _FileManager;
        private readonly IHelpWindowViewModel _HelpWindowViewModel;
        private readonly IControDirectoryTreeViewlModel _ControDirectoryTreeViewlViewModel;

        public ExplorerPageViewModel(
            IMessenger messenger,
            IFileSystemServices fileSystemServices,
            ISettingsService settingsService,
            IFileSystemWatcherService fileSystemWatcherService,
            IDirectoryTreeManager directoryTreeManager,
            IFileManager fileManager,
            IHelpWindowViewModel helpWindowViewModel,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlModel
        ) : base(messenger, settingsService)
        {
            _FileSystemServices = fileSystemServices;
            _FileSystemWatcherService = fileSystemWatcherService;
            _DirectoryTreeManager = directoryTreeManager;
            _FileManager = fileManager;
            _HelpWindowViewModel = helpWindowViewModel;
            _ControDirectoryTreeViewlViewModel = controDirectoryTreeViewlModel;

            // 「上へ」ボタンのコマンド
            ToUpDirectory = new RelayCommand(
                () =>
                {
                    var ParentPath = Path.GetDirectoryName(CurrentFullPath);
                    if (ParentPath != null) { CurrentFullPath = ParentPath; }
                },
                () => Directory.Exists(Path.GetDirectoryName(CurrentFullPath))
            );

            // リストビューの更新コマンド
            ListViewUpdater = new RelayCommand(async () =>
            {
                ListItems.Clear();

                // フォルダやファイルの情報を ViewModel に変換して非同期で格納
                var items = await Task.Run(() =>
                {
                    var collectedItems = new List<ExplorerListItemViewModel>();
                    foreach (var folderFile in _FileManager.EnumerateFileSystemEntries(CurrentFullPath))
                    {
                        var info = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(folderFile);
                        collectedItems.Add(new ExplorerListItemViewModel(info));
                    }
                    return collectedItems;
                });

                // リストビューを更新
                foreach (var item in items)
                {
                    ListItems.Add(item);
                }
            });

            // リストビューアイテムがダブルクリックされた時のコマンド
            FileListViewExecuted = new RelayCommand(() =>
            {
                if (SelectedListViewItem is not null)
                {
                    var newDirectory = SelectedListViewItem.FullPath;
                    if (Directory.Exists(newDirectory))
                    {
                        _FileSystemServices.NotifyChangeCurrentDirectory(newDirectory);
                    }
                }
            });

            // ハッシュ管理ウィンドウ実行のコマンド
            HashCalc = new RelayCommand(() =>
            {
                _DirectoryTreeManager.CreateCheckBoxManager(_ControDirectoryTreeViewlViewModel.TreeRoot);
                _FileSystemServices.NavigateToSelectTargetPage();
            });

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
                _FileSystemServices.NavigateToSettingsPage(ReturnPageEnum.ExplorerPage));

            // デバッグウィンドウを開くコマンド
            DebugOpen = new RelayCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });

            // ヘルプウィンドウを開くコマンド
            HelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _HelpWindowViewModel.Initialize(HelpPage.Index);
            });

            // カレントディレクトリ変更のメッセージ受信
            _Messanger.Register<CurrentDirectoryChangedMessage>(this, (_, m)
                => CurrentFullPath = m.CurrentFullPath);

            // カレントディレクトリのアイテム作成のメッセージ受信
            _Messanger.Register<CurrentDirectoryItemCreatedMessage>(this, (_, m)
                => CurrentDirectoryItemCreated(m.CreatedFullPath));

            // カレントディレクトリのアイテム名前変更のメッセージ受信
            _Messanger.Register<CurrentDirectoryItemRenamedMessage>(this, (_, m)
                => CurrentDirectoryItemRenamed(m.OldFullPath, m.NewFullPath));

            // カレントディレクトリのアイテム削除のメッセージ受信
            _Messanger.Register<CurrentDirectoryItemDeletedMessage>(this, (_, m)
                => CurrentDirectoryItemDeleted(m.DeletedFullPath));

            Initialize();
        }

        /// <summary>
        /// 初期処理
        /// </summary>
        public void Initialize()
        {
            // 設定画面から戻ってきたなら初期化処理終了
            if (IsExecuting)
            {
                IsExecuting = false;
                return;
            }

            // ツリービューを初期化する
            _ControDirectoryTreeViewlViewModel.ClearRoot();
            _ControDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(true);

            // TreeViewにルートアイテムを登録する
            foreach (var rootInfo in SpecialFolderAndRootDrives.ScanSpecialFolders())
            {
                _ControDirectoryTreeViewlViewModel.AddRoot(rootInfo, true);
            }
            foreach (var rootInfo in SpecialFolderAndRootDrives.ScanDrives())
            {
                _ControDirectoryTreeViewlViewModel.AddRoot(rootInfo, true);
            }
            _DirectoryTreeManager.CheckStatusChangeFromCheckManager(_ControDirectoryTreeViewlViewModel.TreeRoot);
            /*
            //--------------------- 開発用自動化処理
            foreach (var root in _ControDirectoryTreeViewlViewModel.TreeRoot)
            {
                if (root.FullPath == @"E:\")
                {
                    root.KickChild();
                    foreach (var child in root.Children)
                    {
                        if (child.FullPath == @"E:\Videos")
                        {
                            child.KickChild();
                            child.IsChecked = true;
                        }
                    }
                }
                if (root.FullPath == @"H:\")
                {
                    root.KickChild();
                    foreach (var child in root.Children)
                    {
                        if (child.FullPath == @"H:\旧D_Drive")
                        {
                            child.KickChild();
                            foreach (var grandchild in child.Children)
                            {
                                if (grandchild.FullPath == @"H:\旧D_Drive\Software")
                                {
                                    grandchild.KickChild();
                                    grandchild.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }
            _DirectoryTreeManager.CreateCheckBoxManager(_ControDirectoryTreeViewlViewModel.TreeRoot);
            _FileSystemServices.NavigateToSelectTargetPage();
            //--------------------- 開発用自動化処理ここまで
            */
        }

        #endregion コンストラクタと初期処理

        #region リストビューのディレクトリ更新通知処理

        /// <summary>
        /// カレントディレクトリにディレクトリが追加された時の処理です。
        /// </summary>
        /// <param name="FullPath">作成されたディレクトリのフルパス</param>
        public async void CurrentDirectoryItemCreated(string FullPath)
        {
            if (CurrentFullPath != Path.GetDirectoryName(FullPath)) return;

            var fileInformation = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(FullPath);
            var addListItem = new ExplorerListItemViewModel(fileInformation);
            int newListIndex = FindIndexToInsert(ListItems, addListItem);
            await App.Current.Dispatcher.InvokeAsync(() => ListItems.Insert(newListIndex, addListItem));
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
                    DebugManager.ExceptionWrite($"Exception in CurrentDirectoryItemRenamed: {ex.Message}");
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
                    DebugManager.ExceptionWrite($"Exception in CurrentDirectoryItemRenamed: {ex.Message}");
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

        #endregion リストビューのディレクトリ更新通知処理

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
            else { DebugManager.ErrorWrite("HwndSourceを取得できませんでした。"); }
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
        private struct DEV_BROADCAST_VOLUME
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
        private unsafe IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
                            ptrToStructure = (object?)(*(DEV_BROADCAST_VOLUME*)lParam);
                            if (ptrToStructure != null)
                            {
                                volume = (DEV_BROADCAST_VOLUME)ptrToStructure;
                            }
                        }
                    }
                    catch (Exception ex) { DebugManager.ExceptionWrite($"WndProcで例外が発生しました: {ex.Message}"); }
                    _FileSystemWatcherService.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    break;

                case DBT.DBT_DEVICEREMOVECOMPLETE:
                    //ドライブが取り外されたされた時の処理を書く
                    try
                    {
                        if (lParam != IntPtr.Zero)
                        {
                            ptrToStructure = (object?)(*(DEV_BROADCAST_VOLUME*)lParam);
                            if (ptrToStructure != null)
                            {
                                volume = (DEV_BROADCAST_VOLUME)ptrToStructure;
                            }
                        }
                    }
                    catch (Exception ex) { DebugManager.ExceptionWrite($"WndProcで例外が発生しました: {ex.Message}"); }
                    _FileSystemWatcherService.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }

        #endregion ドライブ変更のフック処理
    }
}