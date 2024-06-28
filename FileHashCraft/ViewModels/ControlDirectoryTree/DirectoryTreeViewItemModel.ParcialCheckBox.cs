/*  DirectoryTreeViewModel.ParcialCheckBox.cs

    ディレクトリツリービューのチェックボックス状態を管理します。
 */
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    public partial class DirectoryTreeViewItemModel
    {
        #region 子ディレクトリのチェック管理
        /// <summary>
        /// TreeViewItem の CheckBox 状態が変更された時の処理をします。
        /// </summary>
        /// <param name="current">CheckBox のチェック状態が変更された TreeViewItem</param>
        /// <param name="value">変更された CheckBox 状態</param>
        public static void CheckCheckBoxStatusChanged(DirectoryTreeViewItemModel current, bool? value)
        {
            if (current == null) { return; }
            if (current.FullPath?.Length == 0) { return; }
            if (current.IsChecked == value) { return; }

            // 自分のチェック状態により処理を振り分け
            switch (value)
            {
                case true:
                    CheckBoxChangeToChecked(current);
                    break;
                case false:
                    CheckBoxChangeToUnchecked(current);
                    break;
                default:
                    //CheckBoxChangeToMixed(current);
                    break;
            }
        }

        /// <summary>
        /// 再帰的に子の CheckBox 状態を変更します。
        /// </summary>
        /// <param name="node">チェック状態を変更する ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された CheckBox 状態</param>
        private static void ChildCheckBoxStatusChanged(DirectoryTreeViewItemModel node, bool? value)
        {
            foreach (var child in node.Children)
            {
                if (child.IsChecked == value) continue;

                child.IsChecked = value;
                if (node.HasChildren && node._isKicked)
                {
                    // 子がいたら再帰処理
                    ChildCheckBoxStatusChanged(child, value);
                }
            }
        }
        /// <summary>
        /// TreeViewItem の CheckBox がチェックされた時の処理をします。
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        private static void CheckBoxChangeToChecked(DirectoryTreeViewItemModel current)
        {
            // CheckBox のチェックがされていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, true);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理をします。
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        private static void CheckBoxChangeToUnchecked(DirectoryTreeViewItemModel current)
        {
            // CheckBox のチェックが解除されていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, false);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理をします。
        /// </summary>
        private static void CheckBoxChangeToMixed()
        {
            // Mixed は子に反映させる必要がない
        }
        #endregion 子ディレクトリのチェック管理

        #region 親ディレクトリのチェック管理
        /// <summary>
        /// 変更が加えられた可能性があるカレントディレクトリの親ディレクトリリストを取得する
        /// </summary>
        /// <param name="currentNode">カレントディレクトリのアイテム</param>
        /// <returns>変更可能性があるディレクトリのリスト</returns>
        private static void ParentCheckBoxChange(DirectoryTreeViewItemModel currentNode)
        {
            var current = currentNode;
            var parent = current.Parent;
            var currentChecked = current.IsChecked;

            while (parent != null)
            {
                // true ディレクトリの存在チェック
                var childHasTrue = parent.Children.Where(c => c.FullPath != current.FullPath).Any(child => child.IsChecked == true);
                // false ディレクトリの存在チェック
                var childHasFalse = parent.Children.Where(c => c.FullPath != current.FullPath).Any(child => child.IsChecked == false);
                // null ディレクトリの存在チェック
                var childHasNull = parent.Children.Where(c => c.FullPath != current.FullPath).Any(child => child.IsChecked == null);

                /* ここは条件が複雑
                 *  自分が true の場合
                 *      親が true なら何もしないで処理終了(ただし、この状態は異常)
                 *      親が false なら何もしないで処理終了(既に親は管理状態にない)
                 *      親が null なら、親内の全フォルダがチェックされていたら true にして処理継続
                 *  自分が false の場合
                 *      親が true の場合、親をnull にして処理継続
                 *      親が false の場合な、処理終了(既に親も管理状態ではない)
                 *      親が null の場合、処理終了
                 *  自分が null の場合
                 *      親が true の場合、親は null にして処理継続
                 *      親が false の場合、処理終了
                 *      親が null の場合、処理終了
                 */
                switch (currentChecked)
                {
                    case true:
                        if (parent.IsChecked != null) { return; }
                        // 親が null なら、親内の全フォルダがチェックされていたら true、そうでなければ null
                        parent.IsChecked = currentChecked = childHasTrue && !childHasFalse && !childHasNull ? true : null;
                        break;
                    case false:
                        if (parent.IsChecked != true) { return; }
                        parent.IsChecked = null;
                        break;
                    default:
                        if (parent.IsChecked != true) { return; }
                        parent.IsChecked = null;
                        break;
                }

                current = parent;
                parent = parent.Parent;
                currentChecked = parent?.IsChecked;
            }
        }
        #endregion 親ディレクトリのチェック管理

        #region 特殊フォルダのチェック管理
        /// <summary>
        /// 変更を特殊フォルダに反映します。
        /// </summary>
        /// <param name="changedNode">特殊フォルダの TreeViewItem</param>
        /// <param name="value">変更された値</param>
        private void SyncSpecialDirectory(DirectoryTreeViewItemModel changedNode, bool? value)
        {
            var controlDirectoryTreeViewlViewModel = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new NullReferenceException(nameof(IControDirectoryTreeViewlModel));
            foreach (var root in controlDirectoryTreeViewlViewModel.TreeRoot)
            {
                ChangeExpandedSpecialFolder(root, changedNode.FullPath, value);
            }
        }

        /// <summary>
        /// 特殊フォルダを探し、見つかったらチェックを連動する
        /// </summary>
        /// <param name="searchNode">子を検索するノード</param>
        /// <param name="fullPath">検索するファイルのフルパス</param>
        /// <param name="value">IsCheckedに設定する値</param>
        private static void ChangeExpandedSpecialFolder(DirectoryTreeViewItemModel searchNode, string fullPath, bool? value)
        {
            // ノード自身が探しているパスなら反映します
            if (searchNode.FullPath.Contains(fullPath))
            {
                searchNode.IsChecked = value;
                return;
            }

            // TreeViewItem が展開されていたら、再帰的に検索します
            if (searchNode.IsExpanded || searchNode._isKicked)
            {
                foreach (var child in searchNode.Children)
                {
                    ChangeExpandedSpecialFolder(child, fullPath, value);
                }
            }
        }
        #endregion 特殊フォルダのチェック管理
    }
}
