using System.IO;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    public partial class ExplorerPageViewModel
    {
        #region カレントディレクトリ移動関連
        /// <summary>
        /// 指定されたディレクトリのファイル情報を取得し、リストビューを更新します。
        /// </summary>
        /// <param name="fullPath">ファイル情報を取得するディレクトリのパス</param>
        private void FolderFileListScan(string fullPath)
        {
            // Files クラスを使用して指定ディレクトリのファイル情報を取得
            foreach (var folderFile in FileSystemInformationManager.FileItemScan(fullPath, true))
            {
                // フォルダやファイルの情報を ViewModel に変換
                var item = new ExplorerListItemViewModel(this, folderFile);

                // UI スレッドでリストビューを更新
                App.Current?.Dispatcher?.Invoke((Action)(() =>
                {
                    ListItems.Add(item);
                }));
            }
        }

        /// <summary>
        /// カレントディレクトリが変更されたときの処理を行います。
        /// </summary>
        /// <param name="changedPath">変更されたカレントディレクトリのパス</param>
        public ExplorerTreeNodeViewModel? FolderSelectedChanged(string changedPath)
        {
            // パスの最後がディレクトリセパレータで終わる場合は除去
            changedPath = changedPath.Length == 3 ? changedPath : changedPath.TrimEnd(Path.DirectorySeparatorChar);

            // ルートディレクトリにある場合は選択状態に設定して終了
            var selectedRoot = TreeRoot.FirstOrDefault(root => Path.Equals(root.FullPath, changedPath));
            if (selectedRoot != null) { return selectedRoot as ExplorerTreeNodeViewModel; }

            // サブディレクトリ内の場合は一部一致するルートディレクトリを特定し、ルートディレクトリを展開
            var subDirectoryRoot = TreeRoot.FirstOrDefault(root => changedPath.Contains(root.FullPath));
            if (subDirectoryRoot == null) return null;

            if (subDirectoryRoot is not ExplorerTreeNodeViewModel selectingVM) return null;
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
        #endregion カレントディレクトリ移動関連

        #region カレントディレクトリのファイル変更通知関連
        /// <summary>
        /// カレントディレクトリにファイルが作成された
        /// ディレクトリの場合、TreeViewは全ドライブ監視が処理してくれる
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
        /// カレントディレクトリのファイルが削除された
        /// ディレクトリの場合、TreeViewは全ドライブ監視が処理してくれる
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
        /// カレントディレクトリのファイル名が変更された
        /// ディレクトリの場合、TreeViewは全ドライブ監視が処理してくれる
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
    }
}
