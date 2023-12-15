using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public RelayCommand ToUpFolder { get; }

        /// <summary>
        /// リストビュー更新コマンド
        /// </summary>
        public RelayCommand ListViewUpdater { get; set; }

        /// <summary>
        /// リストビューダブルクリック時のコマンド
        /// </summary>
        public RelayCommand FileListViewExecuted { get; set; }

        /// <summary>
        /// 選択されているリストビュー
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
                if (SetProperty(ref _currentDir, value))
                {
                    ToUpFolder.CanExecute(null);
                    if (Directory.Exists(value))
                    {
                        FolderSelectedChanged(value);
                        ListViewUpdater.Execute(null);
                    }
                    else
                    {
                        ListFile.Clear();
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
                SetProperty(ref _CurrentItem, value);
                if (_CurrentItem is not null)
                {
                    _CurrentItem.IsSelected = false;
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
            ToUpFolder = new RelayCommand(
                () => { CurrentDir = CurrentItem?.Parent?.FullPath ?? CurrentDir; },
                () => { return CurrentItem?.Parent is not null; }
            );

            ListViewUpdater = new RelayCommand(
                async () => {
                    ListFile.Clear();
                    await Task.Run(() => FolderFileListScan(CurrentDir));

                },
                () => { return CurrentItem is not null; }
            );

            FileListViewExecuted = new RelayCommand(
                () => {
                    if (SelectedListViewItem is not null)
                    {
                        var newDir = Path.Combine(CurrentDir, SelectedListViewItem.Name);
                        if (Directory.Exists(newDir))
                        {
                            CurrentDir = newDir;
                        }
                    }
                }
            );

            foreach (var root in FileSystemManager.Instance.SpecialFolderScan())
            {
                var item = new ExplorerTreeNodeViewModel(this)
                {
                    FullPath = root.FullPath,
                    IsReady = root.IsReady,
                    HasChildren = root.HasChildren,
                };
                TreeRoot.Add(item);
            }
            var selected = true;
            foreach (var root in FileSystemManager.DriveScan())
            {
                var item = new ExplorerTreeNodeViewModel(this)
                {
                    FullPath = root.FullPath,
                    IsReady = root.IsReady,
                    HasChildren = root.HasChildren,
                };
                TreeRoot.Add(item);
                item.IsSelected = selected;
                selected = false;
            }
        }

        /// <summary>
        /// 指定されたディレクトリのファイル情報を取得し、リストビューを更新します。
        /// </summary>
        /// <param name="path">ファイル情報を取得するディレクトリのパス</param>
        private void FolderFileListScan(string path)
        {
            // Files クラスを使用して指定ディレクトリのファイル情報を取得
            foreach (var folderFile in FileSystemManager.Instance.GetFilesInformation(path, false))
            {
                // フォルダやファイルの情報を ViewModel に変換
                var item = new ExplorerListItemViewModel(this)
                {
                    FullPath = folderFile.FullPath,
                    LastModifiedDate = folderFile.LastModifiedDate,
                    FileSize = folderFile.FileSize,
                    IsDirectory = folderFile.IsDirectory,
                };

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
        public void FolderSelectedChanged(string changedPath)
        {
            // 選択するディレクトリのアイテム
            ExplorerTreeNodeViewModel? selectingVM = null;

            // パスの最後がディレクトリセパレータで終わる場合は除去
            string trueChangedPath = changedPath.TrimEnd(Path.DirectorySeparatorChar);

            // ルートディレクトリにある場合は選択状態に設定して終了
            foreach (var root in TreeRoot)
            {
                if (Path.Equals(root.FullPath, trueChangedPath))
                {
                    root.IsSelected = true;
                    return;
                }
                if (trueChangedPath.Contains(root.FullPath))
                {
                    // サブディレクトリ内の場合は一部一致するルートディレクトリを特定
                    selectingVM = root;
                    break;
                }
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

            if (selectingVM == null) return;

            // ルートディレクトリを展開
            selectingVM.IsExpanded = true;

            directories.RemoveAt(0);

            // パスの各ディレクトリに対して処理を実行
            foreach (var directory in directories)
            {
                foreach (var child in selectingVM.Children)
                {
                    if (child.FullPath == directory)
                    {
                        if (directory != trueChangedPath)
                        {
                            // サブディレクトリを展開して選択状態にする
                            child.IsExpanded = true;
                            selectingVM = child;
                            break;
                        }
                        else
                        {
                            // 最終ディレクトリを選択状態にする
                            child.IsSelected = true;
                            break;
                        }
                    }
                }
            }
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
