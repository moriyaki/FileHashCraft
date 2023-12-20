using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public interface IExplorerPageViewModel
    {
        public ObservableCollection<ExplorerItemViewModelBase> TreeRoot { get; set; }
        public ObservableCollection<ExplorerItemViewModelBase> ListFile { get; set; }
        public string CurrentDir { get; set; }
        public ExplorerTreeNodeViewModel? CurrentItem { get; set; }
        public DirectoryManager TreeViewManager { get; }
        public double FontSize { get; set; }
    }
    public class ExplorerPageViewModel : ObservableObject, IExplorerPageViewModel
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

        private readonly DirectoryManager _TreeViewManager = new(ManagementType.ForWatcher);
        public DirectoryManager TreeViewManager { get => _TreeViewManager; }

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

                FileSystemWatcherService.Instance.SetCurrentDirectoryWatcher(value.FullPath);
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
            InitializeOnce();
        }

        private void InitializeOnce()
        {
            foreach (var rootInfo in FileSystemManager.Instance.SpecialFolderScan())
            {
                var item = new ExplorerTreeNodeViewModel(rootInfo);
                TreeRoot.Add(item);
                TreeViewManager.AddDirectory(rootInfo.FullPath);
            }
            foreach (var rootInfo in FileSystemManager.DriveScan())
            {
                var item = new ExplorerTreeNodeViewModel(rootInfo);
                TreeRoot.Add(item);
                TreeViewManager.AddDirectory(rootInfo.FullPath);
                FileSystemWatcherService.Instance.AddRootDriveWatcher(item);
            }
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// 指定されたディレクトリのファイル情報を取得し、リストビューを更新します。
        /// </summary>
        /// <param name="path">ファイル情報を取得するディレクトリのパス</param>
        private void FolderFileListScan(string path)
        {
            // Files クラスを使用して指定ディレクトリのファイル情報を取得
            foreach (var folderFile in FileSystemManager.FileItemScan(path, true))
            {
                // フォルダやファイルの情報を ViewModel に変換
                var item = new ExplorerListItemViewModel(folderFile);

                // UI スレッドでリストビューを更新
                App.Current?.Dispatcher?.Invoke((Action)(() =>
                {
                    ListFile.Add(item);
                }));
            }
        }

        /// <summary>
        /// カレントディレクトリが変更されたときの処理を行います。
        /// </summary>
        /// <param name="changedPath">変更されたカレントディレクトリのパス</param>
        public ExplorerTreeNodeViewModel? FolderSelectedChanged(string changedPath)
        {
            // 選択するディレクトリのアイテム
            ExplorerTreeNodeViewModel? selectingVM = null;

            // パスの最後がディレクトリセパレータで終わる場合は除去
            changedPath = changedPath.Length == 3 ? changedPath : changedPath.TrimEnd(Path.DirectorySeparatorChar);

            // ルートディレクトリにある場合は選択状態に設定して終了
            var selectedRoot = TreeRoot.FirstOrDefault(root => Path.Equals(root.FullPath, changedPath));
            if (selectedRoot != null) { return selectedRoot as ExplorerTreeNodeViewModel; }

            // サブディレクトリ内の場合は一部一致するルートディレクトリを特定し、ルートディレクトリを展開
            var subDirectoryRoot = TreeRoot.FirstOrDefault(root => changedPath.Contains(root.FullPath));
            if (subDirectoryRoot == null) return null;

            selectingVM = subDirectoryRoot as ExplorerTreeNodeViewModel;
            if (selectingVM == null) return null;
            selectingVM.IsExpanded = true;


            var directories = GetDirectoryNames(changedPath).ToList();

            // パス内の各ディレクトリに対して処理を実行
            foreach (var directory in directories)
            {
                // 親ディレクトリの各子ディレクトリに対して処理を実行
                foreach (var child in selectingVM.Children)
                {
                    if (child.FullPath == directory)
                    {
                        selectingVM = child;
                        if (Path.Equals(directory, changedPath))
                        {
                            // カレントディレクトリが見つかった
                            return child;
                        }
                        else
                        {
                            // サブディレクトリを展開する
                            child.IsExpanded = true;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを取得するディレクトリ</param>
        /// <returns>親ディレクトリからのコレクション</returns>
        public static IEnumerable<string> GetDirectoryNames(string path)
        {
            // パスの区切り文字に関係なく分割する
            var pathSeparated = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string fullPath = string.Empty;

            foreach (var directoryName in pathSeparated)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    // ルートディレクトリの場合、区切り文字を含めて追加
                    fullPath = directoryName + Path.DirectorySeparatorChar;
                }
                else
                {
                    // パスを結合
                    fullPath = Path.Combine(fullPath, directoryName);
                }

                yield return fullPath;
            }
        }
    }
}
