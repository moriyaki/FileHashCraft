using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilOps.Models;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.ViewModels.ExplorerPage
{
    public partial class ExplorerPageViewModel
    {
        #region カレントディレクトリ移動関連
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
        #endregion カレントディレクトリ移動関連

    }
}
