using System.Diagnostics;
using System.Windows;

namespace FilOps.ViewModels.ExplorerPage
{
    /* 
     * TreeViewItem の IsChecked 用ディレクトリ管理
     * 
     * 前提条件：
     *      A ディレクトリ内に B, C, D があり、D の中に E が存在する。
     * 
     * 動作：
     *      CheckBox の状態が true か false なら、子はそのまま状態の変更の影響を受ける。
     *      親は子が全選択もしくは全解除されている場合はそれを反映し、そうでない場合は混合状態として単体監視に移行する。
     * 
     * 実例：
     *      A の CheckBox がチェックされたとき、自身をサブディレクトリを含む形で、
     *      チェック管理マネージャに登録する。ただし B, C, D, E は A の管理下にあるため、登録はしない。
     *      ただし、B,C,D,E も管理マネージャに問い合わせたら「登録されている」と返ってくる。
     * 
     *      C の CheckBox のチェックが解除された場合、C はチェック管理マネージャから解除し、
     *      A は CheckBox が混合状態となり、サブディレクトリを含まない登録に移行する。
     *      同時に、B, D をサブディレクトリを含む形で、チェック管理マネージャに登録する。
     * 
     *      C の CheckBox が再度チェックされたとき、A の子の状態をチェックし、
     *      既に登録されている B, D に加えて C が登録され、全ての子が登録されたため、
     *      A はチェック管理マネージャにサブディレクトリを含む形で登録する。
     *      同時に、サブディレクトリを含まない形での A と、A が管理している B, D は
     *      チェック管理マネージャから登録解除する。
     * 
     *      E の親である D の CheckBox のチェック状態を解除した場合、
     *      D がチェック管理マネージャから解除され、D のサブディレクトリ E も同時に解除される。
     *      
     *      ここで混合状態である A の CheckBox がチェックされたとき、
     *      A がサブディレクトリを含む形でチェック管理マネージャに再登録される。
     *      同時に、ここで管理マネージャに直接管理されている B,C は 登録解除される
     */
    public class ParentDirectoryCheckedChangedInfo
    {
        public string FullPath { get; set; } = string.Empty;
        public ExplorerTreeNodeViewModel? Node { get; set; }
        public bool? CurrentChecked { get; set; }
    }

    public partial class ExplorerTreeNodeViewModel
    {
        #region 子ディレクトリのチェック管理
        /// <summary>
        /// TreeViewItem の CheckBox 状態が変更された時の処理をします。
        /// </summary>
        /// <param name="current">CheckBox のチェック状態が変更された TreeViewItem</param>
        /// <param name="value">変更された CheckBox 状態</param>
        public static void CheckCheckBoxStatusChanged(ExplorerTreeNodeViewModel current, bool? value)
        {
            if (current == null) { return; }
            if (current.FullPath == string.Empty) { return; }
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
            };
        }

        /// <summary>
        /// 再帰的に子の CheckBox 状態を変更します。
        /// </summary>
        /// <param name="node">チェック状態を変更する ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された CheckBox 状態</param>
        private static void ChildCheckBoxStatusChanged(ExplorerTreeNodeViewModel node, bool? value)
        {
            foreach (var child in node.Children)
            {
                if (child.IsChecked != value)
                {
                    child.IsChecked = value;
                    if (node.HasChildren && node.IsKicked)
                    {
                        // 子がいたら再帰処理
                        ChildCheckBoxStatusChanged(child, value);
                    }
                }
            }
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェックされた時の処理をします。
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private static void CheckBoxChangeToChecked(ExplorerTreeNodeViewModel current)
        {
            // CheckBox のチェックがされていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, true);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理をします。
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private static void CheckBoxChangeToUnchecked(ExplorerTreeNodeViewModel current)
        {
            // CheckBox のチェックが解除されていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, false);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理をします。
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        /*
        private static void CheckBoxChangeToMixed(ExplorerTreeNodeViewModel current)
        {
            // Mixed は子に反映させる必要がない
        }
        */

        #endregion 子ディレクトリのチェック管理

        /// <summary>
        /// 変更が加えられた可能性があるカレントディレクトリの親ディレクトリリストを取得する
        /// </summary>
        /// <param name="current">カレントディレクトリのアイテム</param>
        /// <returns>変更可能性があるディレクトリのリスト</returns>
        /// 
        private static void ParentCheckBoxChange(ExplorerTreeNodeViewModel current)
        {
            var changedParentNode = new List<ParentDirectoryCheckedChangedInfo>();
            var parent = current.Parent;
            var currentChecked = current.IsChecked;
        
            while (parent != null)
            {
                // True ディレクトリの存在チェック
                var childHasTrue = parent.Children.Any(child => child.IsChecked == true);
                // False ディレクトリの存在チェック
                var childHasFalse = parent.Children.Any(child => child.IsChecked == false);

                // 大元が true 、親が null なら、親の全てのディレクトリがチェックされたら true 化する
                if (currentChecked == true && current.IsChecked == null)
                {
                    if (childHasTrue && !childHasFalse)
                    {
                        parent.IsChecked = true;
                    }
                }

                // 大元が false 、親が false 以外なら、状態変化する
                if (currentChecked == false)
                {
                    // 親が true なら、null になる
                    if (parent.IsChecked == true)
                    {
                        parent.IsChecked = null;
                    }

                    // 親が null なら親の配下ディレクトリが true を持つなら null となる
                    if (parent.IsChecked == null)
                    {
                        if (childHasTrue && childHasFalse)
                        {
                            parent.IsChecked = null;
                        }
                    }
                }

                // 大元が null なら、(親が null 以外では)親が null 化する可能性がある
                if (currentChecked == null && parent.IsChecked != null)
                {
                    if (childHasTrue && childHasTrue)
                    {
                        parent.IsChecked = null;
                    }
                }
                parent = parent.Parent;
            }
        }

        /// <summary>
        /// 変更を特殊フォルダに反映します。
        /// </summary>
        /// <param name="changedNode">特殊フォルダの TreeViewItem</param>
        /// <param name="value">変更された値</param>
        private void SyncSpecialDirectory(ExplorerTreeNodeViewModel changedNode, bool? value)
        {
            if (!ExplorerVM.IsExpandDirectory(changedNode)) { return; }
            
            foreach (var root in ExplorerVM.TreeRoot)
            {
                if (root is ExplorerTreeNodeViewModel specialFolder)
                {
                    ChangeExpandedSpecialFolder(specialFolder, changedNode.FullPath, value);
                }
            }
        }

        private static void ChangeExpandedSpecialFolder(ExplorerTreeNodeViewModel searchNode, string fullPath, bool? value)
        {
            // ノード自身が探しているパスなら反映します
            if (searchNode.FullPath == fullPath)
            {
                searchNode.IsCheckedForSync = value;
                return;
            }

            // TreeViewItem が展開されていたら、再帰的に検索します
            if (searchNode.IsExpanded || searchNode.IsKicked)
            {
                foreach (var child in searchNode.Children)
                {
                    ChangeExpandedSpecialFolder(child, fullPath, value);
                }
            }
        }
    }
}