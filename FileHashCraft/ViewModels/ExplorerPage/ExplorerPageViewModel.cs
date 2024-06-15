/* PageExplorerViewModel.cs

    Explorer 風の画面を提供する ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.Helpers;
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
        #region データバインディング
        public ObservableCollection<ExplorerListItemViewModel> ListItems { get; set; } = [];

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
                    _fileSystemWatcherService.SetCurrentDirectoryWatcher(changedDirectory);
                    _fileSystemService.SendCurrentDirectoryChanged(changedDirectory);
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
        #endregion データバインディング

        #region コンストラクタと初期処理
        private bool IsExecuting = false;

        private readonly IFileSystemServices _fileSystemService;
        private readonly IFileSystemWatcherService _fileSystemWatcherService;
        private readonly ITreeManager _treeManager;
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IControDirectoryTreeViewlModel _controDirectoryTreeViewlViewModel;
        public ExplorerPageViewModel(
            IFileSystemServices fileSystemServices,
            ISettingsService settingsService,
            IFileSystemWatcherService fileSystemWatcherService,
            ITreeManager treeManager,
            IHelpWindowViewModel helpWindowViewModel,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlModel
        ) : base(settingsService)
        {
            _fileSystemService = fileSystemServices;
            _fileSystemWatcherService = fileSystemWatcherService;
            _treeManager = treeManager;
            _helpWindowViewModel = helpWindowViewModel;
            _controDirectoryTreeViewlViewModel = controDirectoryTreeViewlModel;

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
                await Task.Run(() =>
                {
                    foreach (var folderFile in FileManager.EnumerateFileSystemEntries(CurrentFullPath))
                    {
                        // フォルダやファイルの情報を ViewModel に変換
                        var info = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(folderFile);
                        var item = new ExplorerListItemViewModel(info);

                        // UI スレッドでリストビューを更新
                        App.Current?.Dispatcher?.Invoke(() => ListItems.Add(item));
                    }
                });
            });

            // リストビューアイテムがダブルクリックされた時のコマンド
            FileListViewExecuted = new RelayCommand(() =>
            {
                if (SelectedListViewItem is not null)
                {
                    var newDirectory = SelectedListViewItem.FullPath;
                    if (Directory.Exists(newDirectory))
                    {
                        _fileSystemService.SendCurrentDirectoryChanged(newDirectory);
                    }
                }
            });

            // ハッシュ管理ウィンドウ実行のコマンド
            HashCalc = new RelayCommand(() =>
            {
                _treeManager.CreateCheckBoxManager(_controDirectoryTreeViewlViewModel.TreeRoot);
                _fileSystemService.SendToSelectTargetPage();
            });

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
                _fileSystemService.SendToSettingsPage(ReturnPageEnum.PageExplorer));

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
                _helpWindowViewModel.Initialize(HelpPage.Index);
            });

            // カレントディレクトリ変更のメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChanged>(this, (_, m)
                => CurrentFullPath = m.CurrentFullPath);

            // カレントディレクトリのアイテム作成のメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryItemCreated>(this, (_, m)
                => CurrentDirectoryItemCreated(m.CreatedFullPath));

            // カレントディレクトリのアイテム名前変更のメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryItemRenamed>(this, (_, m)
                => CurrentDirectoryItemRenamed(m.OldFullPath, m.NewFullPath));

            // カレントディレクトリのアイテム削除のメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryItemDeleted>(this, (_, m)
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
            _controDirectoryTreeViewlViewModel.ClearRoot();
            _controDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(true);

            // TreeViewにルートアイテムを登録する
            foreach (var rootInfo in SpecialFolderAndRootDrives.ScanSpecialFolders())
            {
                _controDirectoryTreeViewlViewModel.AddRoot(rootInfo, true);
            }
            foreach (var rootInfo in SpecialFolderAndRootDrives.ScanDrives())
            {
                _controDirectoryTreeViewlViewModel.AddRoot(rootInfo, true);
            }
            _treeManager.CheckStatusChangeFromCheckManager(_controDirectoryTreeViewlViewModel.TreeRoot);

            // 開発用自動化処理
            foreach (var root in _controDirectoryTreeViewlViewModel.TreeRoot)
            {
                if (root.FullPath == @"G:\")
                {
                    root.KickChild();
                    foreach (var child in root.Children)
                    {
                        if (child.FullPath == @"G:\Anime_Season")
                        {
                            child.IsChecked = true;
                        }
                    }
                    _treeManager.CreateCheckBoxManager(_controDirectoryTreeViewlViewModel.TreeRoot);
                    _fileSystemService.SendToSelectTargetPage();
                }
            }
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
        #endregion リストビューの更新通知処理

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
                    catch (Exception ex) { DebugManager.ExceptionWrite($"WndProcで例外が発生しました: {ex.Message}"); }
                    _fileSystemWatcherService.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
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
                    catch (Exception ex) { DebugManager.ExceptionWrite($"WndProcで例外が発生しました: {ex.Message}"); }
                    _fileSystemWatcherService.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }
        #endregion ドライブ変更のフック処理
    }
}
