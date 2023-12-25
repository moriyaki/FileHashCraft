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
        /// TreeViewItem の CheckBox 状態が変更された時の処理
        /// </summary>
        /// <param name="current">CheckBox のチェック状態が変更された TreeViewItem</param>
        /// <param name="value">変更された CheckBox 状態</param>
        public void CheckCheckBoxStatusChanged(ExplorerTreeNodeViewModel current, bool? value)
        {
            if (current == null) { return ; }
            if (current.FullPath == string.Empty) { return ; }
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
                    CheckBoxChangeToMixed(current);
                    break;
            };
        }

        /// <summary>
        /// 再帰的に子の CheckBox 状態を変更する
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
        /// TreeViewItem の CheckBox がチェックされた時の処理
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private void CheckBoxChangeToChecked(ExplorerTreeNodeViewModel current)
        {
            // CheckBox のチェックがされていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, true);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private void CheckBoxChangeToUnchecked(ExplorerTreeNodeViewModel current)
        {
            // CheckBox のチェックが解除されていたら、再帰的に子を反映する
            ChildCheckBoxStatusChanged(current, false);
        }

        /// <summary>
        /// TreeViewItem の CheckBox がチェック解除された時の処理
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private void CheckBoxChangeToMixed(ExplorerTreeNodeViewModel current)
        {
            // Mixed は子に反映させる必要がない
        }



        #endregion 子ディレクトリのチェック管理

        private static ParentDirectoryCheckedChangedInfo NewParentDirCheckChangedInfo(ExplorerTreeNodeViewModel parent, bool? currentCheckStatus)
        {
            var parentInfo = new ParentDirectoryCheckedChangedInfo
            {
                CurrentChecked = currentCheckStatus,
                FullPath = parent.FullPath,
                Node = parent
            };
            return parentInfo;      
        }

        /// <summary>
        /// 変更が加えられた可能性があるカレントディレクトリの親ディレクトリリストを取得する
        /// </summary>
        /// <param name="current">カレントディレクトリのアイテム</param>
        /// <returns>変更可能性があるディレクトリのリスト</returns>
        private static List<ParentDirectoryCheckedChangedInfo> GetChangedParent(ExplorerTreeNodeViewModel current)
        {
            var changedParentNode = new List<ParentDirectoryCheckedChangedInfo>();
            var parent = current.Parent;
            var currentCheckStatus = current.IsChecked;

            /* 自分のチェックボックス状態が true なら
             *      親の状態が true なら、何もする必要がない
             *      親の状態が false なら、何もする必要がない
             *      親の状態が null なら、親が true 化する可能性があるので変更リストに加える
             * 自分のチェックボックス状態が false なら     
             *      親の状態が true なら、子の状態により親が null 化する可能性があるので変更リストに加える
             *      親の状態が false なら、何もする必要がない
             *      親の状態が null なら、親が false 化する可能性があるので変更リストに加える
             * 自分のチェックボックス状態が null なら
             *      親の状態が true なら、親が null 化する可能性があるのでリストに加える
             *      親の状態が false なら、親が null 化する可能性があるのでリストに加える
             *      親の状態が null なら、何もする必要がない
             */
            while (parent != null)
            {
                if (currentCheckStatus == true && parent.IsChecked == null)
                {
                    // 大元が true なら、親が null の場合のみ true 化する可能性がある
                    changedParentNode.Add(NewParentDirCheckChangedInfo(parent, currentCheckStatus));
                }
                else if (currentCheckStatus == false && parent.IsChecked != false)
                {
                    // 大元が false なら、親が false 以外では態変化する可能性がある
                    changedParentNode.Add(NewParentDirCheckChangedInfo(parent, currentCheckStatus));
                }
                else if (currentCheckStatus == null && parent.IsChecked != null)
                {
                    // 大元が null なら、親が null 以外では親が null 化する可能性がある
                    changedParentNode.Add(NewParentDirCheckChangedInfo(parent, currentCheckStatus));
                }
                else
                {
                    return changedParentNode;
                }
                parent = parent.Parent;
            }
            return changedParentNode;
        }
    }
}
