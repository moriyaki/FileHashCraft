using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public class ExplorerPageViewModel : ObservableObject
    {
        #region データバインディング
        /// <summary>
        /// ツリービューのルートのコレクション
        /// </summary>
        private ObservableCollection<ExplorerTreeNodeViewModel> _TreeRoot = [];
        public ObservableCollection<ExplorerTreeNodeViewModel> TreeRoot
        {
            get => _TreeRoot;
            set => SetProperty(ref _TreeRoot, value);
        }

        /// <summary>
        /// リストビューのコレクション
        /// </summary>
        private ObservableCollection<ExplorerListItemViewModel> _ListFile = [];
        public ObservableCollection<ExplorerListItemViewModel> ListFile
        {
            get => _ListFile;
            set => SetProperty(ref _ListFile, value);
        }

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
        /// ディレクトリ変更を監視するインスタンス
        /// </summary>
        private readonly FileSystemWatcher CurrentDirectoryWatcher = new();

        /// <summary>
        /// ディレクトリ変更を通知するフィルタ
        /// </summary>
        private readonly NotifyFilters CurrentDirectoryNotifyFilter 
            = NotifyFilters.DirectoryName
            | NotifyFilters.FileName
            | NotifyFilters.LastWrite
            | NotifyFilters.Size;

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

                // 表示を大文字小文字正しいものを取得
                if (Directory.Exists(value))
                {
                    var dirName = Path.GetDirectoryName(changedDir);
                    if (dirName is not null)
                    {
                        var dirs = Directory.GetDirectories(dirName) ?? [""];
                        if (dirs is not null)
                        {
                            changedDir = dirs.FirstOrDefault(dir => dir.Equals(value, StringComparison.OrdinalIgnoreCase)) ?? value;
                        }
                    }
                    else
                    {
                        changedDir = value.ToUpper();
                    }
                }

                if (!SamePath(_currentDir, changedDir)) 
                {
                    if (SetProperty(ref _currentDir, changedDir))
                    {
                        
                        if (Directory.Exists(changedDir))
                        {
                            CurrentItem = FolderSelectedChanged(changedDir);
                        }
                    }
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
                //OnPropertyChanged(nameof(ToUpEnabled));

                SetCurrentDirectoryWatcher(value);
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
                if (10 <= value && value <= 18)
                {
                    SetProperty(ref _FontSize, value);
                }
            }
        }
        #endregion データバインディング

        public ExplorerPageViewModel()
        {
            ToUpDirectory = new DelegateCommand(
                () => {
                    if (CurrentItem != null)
                    {
                        CurrentItem = CurrentItem.Parent;
                    }
                },
                () =>
                {
                    return CurrentItem != null && CurrentItem.Parent != null;
                }
            );

            ListViewUpdater = new DelegateCommand(
                async () => {
                    ListFile.Clear();
                    if ( CurrentItem != null )
                    {
                        await Task.Run(() => FolderFileListScan(CurrentItem.FullPath));
                    }
                }
            );

            FileListViewExecuted = new DelegateCommand(
                () => {
                    if (SelectedListViewItem is not null)
                    {
                        var newDir = SelectedListViewItem.FullPath;
                        if (Directory.Exists(newDir))
                        {
                            CurrentDir = newDir;
                        }
                    }
                }
            );
            foreach (var root in FileSystemManager.Instance.SpecialFolderScan())
            {
                var item = new ExplorerTreeNodeViewModel(this, root);
                TreeRoot.Add(item);
            }
            var selected = true;
            foreach (var root in FileSystemManager.DriveScan())
            {
                var item = new ExplorerTreeNodeViewModel(this, root);
                TreeRoot.Add(item);
                item.IsSelected = selected;
                if (selected)
                {
                    CurrentDir = item.FullPath;
                    selected = false;
                }
            }
        }

        /// <summary>
        /// ファイルの作成通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            if (CurrentItem != null)
            {
                MessageBox.Show($"CurrentDirectory : {CurrentItem.FullPath}");
            }
            MessageBox.Show($"Created : {Path.GetFileName(e.FullPath)}");
        }

        /// <summary>
        /// ファイルの削除通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Deleted) return;
            MessageBox.Show($"Deleted : {Path.GetFileName(e.FullPath)}");
        }

        /// <summary>
        /// ファイルの名前変更通知が入った場合のイベントメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenamed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;
            MessageBox.Show($"Renamed : {e.FullPath}");
        }

        /// <summary>
        /// ディレクトリの変更を全監視する設定をする
        /// </summary>
        /// <param name="treeItem"></param>
        private void SetCurrentDirectoryWatcher(ExplorerTreeNodeViewModel treeItem)
        {
            if (treeItem.IsReady)
            {
                CurrentDirectoryWatcher.Created -= OnCreated;
                CurrentDirectoryWatcher.Deleted -= OnDeleted;
                CurrentDirectoryWatcher.Renamed -= OnRenamed;

                CurrentDirectoryWatcher.Path = treeItem.FullPath;
                CurrentDirectoryWatcher.Filter = "*.*";
                CurrentDirectoryWatcher.NotifyFilter = CurrentDirectoryNotifyFilter;
                CurrentDirectoryWatcher.EnableRaisingEvents = true;

                CurrentDirectoryWatcher.Created += OnCreated;
                CurrentDirectoryWatcher.Deleted += OnDeleted;
                CurrentDirectoryWatcher.Renamed += OnRenamed;
            }
        }

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
                var item = new ExplorerListItemViewModel(this, folderFile);

                // UI スレッドでリストビューを更新
                App.Current?.Dispatcher.Invoke((Action)(() =>
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
            string trueChangedPath = changedPath.Length == 3 ? changedPath : changedPath.TrimEnd(Path.DirectorySeparatorChar);

            // ルートディレクトリにある場合は選択状態に設定して終了
            var selectedRoot = TreeRoot.FirstOrDefault(root => Path.Equals(root.FullPath, trueChangedPath));
            if (selectedRoot != null)
            {
                return selectedRoot;
            }
            // サブディレクトリ内の場合は一部一致するルートディレクトリを特定
            var subDirectoryRoot = TreeRoot.FirstOrDefault(root => trueChangedPath.Contains(root.FullPath));
            if (subDirectoryRoot != null)
            {
                selectingVM = subDirectoryRoot;
            }

            var directories = GetDirectoryNames(trueChangedPath).ToList();

            // サブディレクトリ内のルートディレクトリを特定
            foreach (var root in TreeRoot)
            {
                if (Path.Equals(root.FullPath, trueChangedPath))
                {
                    selectingVM = root;
                    break;
                }
            }

            if (selectingVM is null) return null;

            // ルートディレクトリを展開
            selectingVM.IsExpanded = true;

            //directories.RemoveAt(0);

            // パスの各ディレクトリに対して処理を実行
            foreach (var directory in directories)
            {
                foreach (var child in selectingVM.Children)
                {
                    if (child.FullPath == directory)
                    {
                        selectingVM = child;
                        if (directory != trueChangedPath)
                        {
                            // サブディレクトリを展開する
                            child.IsExpanded = true;
                        }
                        else
                        {
                            return child;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 末尾の "\" 有無に関係なく、同じパスを指しているかどうか
        /// </summary>
        /// <param name="path1">比較するパス</param>
        /// <param name="path2">比較されるパス</param>
        /// <returns>同じパスかどうか</returns>
        public static bool SamePath(string path1, string path2)
        {
            string truePath1 = path1.TrimEnd(Path.PathSeparator).ToLower();
            string truePath2 = path2.TrimEnd(Path.PathSeparator).ToLower();

            return truePath1 == truePath2;
        }

        /// <summary>
        /// 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを取得するディレクトリ</param>
        /// <returns>親ディレクトリからのコレクション</returns>
        private static IEnumerable<string> GetDirectoryNames(string path)
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
