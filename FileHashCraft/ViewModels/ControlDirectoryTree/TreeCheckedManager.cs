/*  DirectoryTreeCheckedDirectoryManager.cs

    ディレクトリのチェックボックスのチェック状態を管理します。
    
    初期状態：どこもチェックされていない
        true  が来たら、その下のディレクトリを全て管理にする、上位にいたら何もしない
        false が来たら、その下のディレクトリを全て管理から外す、上位下位はUI任せ
        null  が来たら、そのディレクトリを単独監視にする
 */

using System.Collections.ObjectModel;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.Modules
{
    public interface ITreeCheckedManager
    {
        /// <summary>
        /// 子ディレクトリを含む、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NestedDirectories { get; }
        /// <summary>
        /// 子ディレクトリを含まない、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NonNestedDirectories { get; }
        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べます。
        /// </summary>
        public bool IsChecked(string fullPath);
        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        public void CheckChanged(string fullPath, bool? checkedStatus);
        /// <summary>
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeViewModel> treeRoot);
        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        public void CreateCheckBoxManager(ObservableCollection<DirectoryTreeViewModel> treeRoot);
    }
    public class TreeCheckedManager : ITreeCheckedManager
    {
        #region リスト
        /// <summary>
        /// 子ディレクトリを含む、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NestedDirectories { get; } = [];
        /// <summary>
        /// 子ディレクトリを含まない、ファイルハッシュ取得対象のディレクトリを保持します。
        /// </summary>
        public List<string> NonNestedDirectories { get; } = [];
        #endregion リスト

        #region メソッド
        /// <summary>
        /// そのディレクトリがチェックされているかどうかを調べます。
        /// </summary>
        /// <param name="fullPath">チェックされているかを調べるディレクトリのフルパス</param>
        /// <returns>チェックされているかどうか</returns>
        public bool IsChecked(string fullPath)
        {
            if (NonNestedDirectories.Any(o => o == fullPath)) { return true; }
            return NestedDirectories.Any(d => fullPath.Contains(d));
        }

        /// <summary>
        /// ディレクトリのチェック状態を変化させます。
        /// </summary>
        /// <param name="fullPath">変化させるディレクトリのフルパス</param>
        /// <param name="checkedStatus">変化させる状態</param>
        public void CheckChanged(string fullPath, bool? checkedStatus)
        {
            switch (checkedStatus)
            {
                case true:
                    CheckedTrue(fullPath);
                    break;
                case false:
                    CheckedFalse(fullPath);
                    break;
                default:
                    CheckedNull(fullPath);
                    break;
            }
        }

        /// <summary>
        /// ディレクトリのチェック状態がチェック状態になった。
        /// </summary>
        /// <param name="fullPath">チェック状態になったディレクトリのフルパス</param>
        private void CheckedTrue(string fullPath)
        {
            if (NestedDirectories.Contains(fullPath)) { return; }

            // fullPath で始まる要素を全削除
            NestedDirectories.RemoveAll(c => c.StartsWith(fullPath));

            // 含まれているディレクトリで始まる fullPath なら追加しない
            if (NestedDirectories.Any(c => fullPath.StartsWith(c))) { return; }

            NonNestedDirectories.Remove(fullPath);
            NestedDirectories.Add(fullPath);
        }

        /// <summary>
        /// ディレクトリのチェック状態がチェック解除になった。
        /// </summary>
        /// <param name="fullPath"></param>
        private void CheckedFalse(string fullPath)
        {
            NonNestedDirectories.Remove(fullPath);
            NestedDirectories.RemoveAll(c => c.StartsWith(fullPath));
        }

        /// <summary>
        /// ディレクトリのチェック状態が混合状態になった。
        /// </summary>
        /// <param name="fullPath"></param>
        private void CheckedNull(string fullPath)
        {
            NestedDirectories.Remove(fullPath);
            if (NonNestedDirectories.Any(c => c == fullPath)) { return; }
            NonNestedDirectories.Add(fullPath);
        }
        #endregion メソッド

        #region チェックマネージャからチェック状態を反映
        /// <summary>
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager(ObservableCollection<DirectoryTreeViewModel> treeRoot)
        {
            // サブディレクトリを含む管理をしているディレクトリを巡回する
            foreach (var fullPath in NestedDirectories)
            {
                CheckStatusChange(fullPath, true, treeRoot);
            }
            // サブディレクトリを含まない管理をしているディレクトリを巡回する
            foreach (var fullPath in NonNestedDirectories)
            {
                CheckStatusChange(fullPath, null, treeRoot);
            }
        }

        /// <summary>
        /// ディレクトリのフルパスから、チェック状態を変更する
        /// </summary>
        /// <param name="fullPath">チェック状態を変更するディレクトリのフルパス</param>
        /// <param name="isChecked">変更するチェック状態</param>
        /// <returns>成功の可否</returns>
        private static bool CheckStatusChange(string fullPath, bool? isChecked, ObservableCollection<DirectoryTreeViewModel> treeRoot)
        {
            // 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得
            var dirs = DirectoryNameService.GetDirectoryNames(fullPath);

            // ドライブノードを取得する
            DirectoryTreeViewModel? node = treeRoot.FirstOrDefault(r => r.FullPath == dirs[0]);
            if (node == null) return false;
            node.KickChild();
            node.IsExpanded = true;

            if (node.FullPath == fullPath)
            {
                if (node.FullPath == fullPath)
                {
                    if (isChecked == true || isChecked == false)
                    {
                        node.IsChecked = isChecked;
                    }
                    else
                    {
                        node.IsCheckedForSync = null;
                    }
                    return true;
                }
            }

            // リストからドライブノードを除去する
            dirs.RemoveAt(0);
            foreach (var dir in dirs)
            {
                node = node.Children.FirstOrDefault(c => c.FullPath == dir);
                if (node == null) return false;

                node.KickChild();
                if (node.FullPath == fullPath)
                {
                    if (isChecked == true || isChecked == false)
                    {
                        node.IsChecked = isChecked;
                    }
                    else
                    {
                        node.IsCheckedForSync = null;
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion チェックマネージャからチェック状態を反映

        #region チェックボックスマネージャ登録
        /// <summary>
        /// チェックボックスマネージャの登録をします。
        /// </summary>
        public void CreateCheckBoxManager(ObservableCollection<DirectoryTreeViewModel> treeRoot)
        {
            foreach (var root in treeRoot)
            {
                RecursiveTreeNodeCheck(root);
            }
        }

        /// <summary>
        /// 再帰的にチェックされたアイテムのチェック状態を変更します。
        /// </summary>
        private void RecursiveTreeNodeCheck(DirectoryTreeViewModel node)
        {
            CheckChanged(node.FullPath, node.IsChecked);
            foreach (var child in node.Children)
            {
                if (child.FullPath != string.Empty)
                    RecursiveTreeNodeCheck(child);
            }
        }
        #endregion チェックボックスマネージャ登録
    }
}
