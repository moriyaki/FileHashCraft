using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels.ExplorerPage
{
    public interface IExplorerPageViewModel
    {
        public void InitializeOnce();
        public ObservableCollection<ExplorerItemViewModelBase> TreeRoot { get; set; }
        public ObservableCollection<ExplorerItemViewModelBase> ListFile { get; set; }
        public string CurrentDir { get; set; }
        public ExplorerTreeNodeViewModel? CurrentItem { get; set; }
        public ExpandedDirectoryManager ExpandDirManager { get; }
        public double FontSize { get; set; }

        // WndProcフック処理関連
        public void HwndAddHook(HwndSource? hwndSource);
        public void HwndRemoveHook();

        // リストビューアイテムを作成する(廃止予定)
        public ExplorerTreeNodeViewModel CreateTreeViewItem(string path);
        public ExplorerListItemViewModel CreateListViewItem(string path);

    }
    public partial class ExplorerPageViewModel : ObservableObject, IExplorerPageViewModel
    {
        #region データバインディング
        public ObservableCollection<ExplorerItemViewModelBase> TreeRoot { get; set; } = [];
        public ObservableCollection<ExplorerItemViewModelBase> ListFile { get; set; } = [];

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
        /// 選択されているリストビューのアイテム
        /// </summary>
        private ExplorerListItemViewModel? _SelectedListViewItem = null;
        public ExplorerListItemViewModel? SelectedListViewItem
        {
            get => _SelectedListViewItem;
            set => SetProperty(ref _SelectedListViewItem, value);
        }

        private readonly ExpandedDirectoryManager _ExpandDirManager = new();
        public ExpandedDirectoryManager ExpandDirManager { get => _ExpandDirManager; }

        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        private string _currentDir = string.Empty;
        public string CurrentDir
        {
            get => _currentDir;
            set
            {
                string changedDir = value;

                if (_currentDir.Length == 1 && value.Length == 2)
                {
                    // 1文字から2文字になった時は、'\\' を追加する
                    changedDir = changedDir.ToUpper() + Path.DirectorySeparatorChar;
                }
                else if (changedDir.Length < 3)
                {
                    // 3文字未満なら返る
                    SetProperty(ref _currentDir, value);
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
                if (Path.Equals(_currentDir, changedDir)) return;

                // 値のセット
                if (!SetProperty(ref _currentDir, changedDir)) return;

                // ディレクトリが存在するなら、CurrentItemを設定
                if (Directory.Exists(changedDir))
                {
                    CurrentItem = FolderSelectedChanged(changedDir);
                }
            }
        }

        /// <summary>
        /// カレントディレクトリの情報
        /// </summary>
        private ExplorerTreeNodeViewModel? _CurrentItem = null;
        public ExplorerTreeNodeViewModel? CurrentItem
        {
            get => _CurrentItem;
            set
            {
                if (value is null) { return; }
                if (_CurrentItem == value) { return; }

                // 選択が変更されたら、明示的に今までの選択を外す
                if (_CurrentItem is not null && _CurrentItem != value)
                {
                    _CurrentItem.IsSelected = false;
                }
                SetProperty(ref _CurrentItem, value);
                ToUpDirectory.RaiseCanExecuteChanged();


                FileWatcherService?.SetCurrentDirectoryWatcher(value.FullPath);
                value.IsSelected = true;
                ListViewUpdater.Execute(null);
                CurrentDir = value.FullPath;
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
        private IFileSystemWatcherService? FileWatcherService;
        #endregion データバインディング

        #region コンストラクタと初期化
        public ExplorerPageViewModel()
        {
            ToUpDirectory = new DelegateCommand(
                () => { if (CurrentItem != null) { CurrentItem = CurrentItem.Parent; } },
                () => { return CurrentItem != null && CurrentItem.Parent != null; }
            );

            ListViewUpdater = new DelegateCommand(async () => {
                ListFile.Clear();
                if (CurrentItem != null)
                {
                    await Task.Run(() => FolderFileListScan(CurrentItem.FullPath));
                }
            });

            FileListViewExecuted = new DelegateCommand(() => {
                if (SelectedListViewItem is not null)
                {
                    var newDir = SelectedListViewItem.FullPath;
                    if (Directory.Exists(newDir))
                    {
                        CurrentDir = newDir;
                    }
                }
            });
        }

        private bool IsInitialized = false;
        public void InitializeOnce()
        {
            if (!IsInitialized)
            {
                FileWatcherService = App.Current.Services.GetService<IFileSystemWatcherService>() ?? throw new ArgumentNullException(nameof(IFileSystemWatcherService));
                foreach (var rootInfo in FileSystemManager.Instance.SpecialFolderScan())
                {
                    var item = new ExplorerTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    ExpandDirManager.AddDirectory(rootInfo.FullPath);
                }
                foreach (var rootInfo in FileSystemManager.DriveScan())
                {
                    var item = new ExplorerTreeNodeViewModel(this, rootInfo);
                    TreeRoot.Add(item);
                    ExpandDirManager.AddDirectory(rootInfo.FullPath);
                    FileWatcherService.AddRootDriveWatcher(item);
                }
                IsInitialized = true;
            }
        }
        #endregion コンストラクタと初期化


        #region ファイルアイテム取得(廃止予定)
        /// <summary>
        /// フルパスからツリービューアイテムを作成する
        /// </summary>
        /// <param name="path">ファイルフルパス</param>
        /// <returns>ツリービューアイテム</returns>
        public ExplorerTreeNodeViewModel CreateTreeViewItem(string path)
        {
            var fileInformation = FileSystemManager.GetFileInformationFromDirectorPath(path);
            return new ExplorerTreeNodeViewModel(this, fileInformation);

        }

        /// <summary>
        /// フルパスからリストビューアイテムを作成する
        /// </summary>
        /// <param name="path">ファイルフルパス</param>
        /// <returns>リストビューアイテム</returns>
        public ExplorerListItemViewModel CreateListViewItem(string path)
        {
            var fileInformation = FileSystemManager.GetFileInformationFromDirectorPath(path);
            return new ExplorerListItemViewModel(this, fileInformation);
        }
        #endregion ファイルアイテム取得(廃止予定)

        #region ドライブ変更のフック処理
        // ページのHwndSourceを保持するための変数
        private HwndSource? hwndSource;

        public void HwndAddHook(HwndSource? hwndSource)
        {
            if (hwndSource != null) { hwndSource.AddHook(WndProc); }
            else { Debug.WriteLine("HwndSourceを取得できませんでした。"); }
        }

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
        /// カスタムのウィンドウプロシージャ
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

                    FileWatcherService?.InsertOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
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

                    FileWatcherService?.EjectOpticalDriveMedia(GetDriveLetter(volume.dbcv_unitmask));
                    break;
            }
            // デフォルトのウィンドウプロシージャに処理を渡す
            return IntPtr.Zero;
        }
        #endregion ドライブ変更のフック処理
    }
}
