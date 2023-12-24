using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

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
    public partial class ExplorerTreeNodeViewModel
    {
        /// <summary>
        /// TreeViewItem の CheckBox 状態が変更された時の処理
        /// </summary>
        /// <param name="current">CheckBox のチェック状態が変更された TreeViewItem</param>
        /// <param name="value">変更された CheckBox 状態</param>
        public void CheckStatusChanged(ExplorerTreeNodeViewModel current, bool? value)
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
            ExplorerVM.AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(current);
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
            ExplorerVM.RemoveDirectoryWithSubdirectoriesToCheckedDirectoryManager(current);
        }

        /// <summary>
        /// TreeViewItem の CheckBox が混合状態に変更された時の処理
        /// </summary>
        /// <param name="current">ExplorerTreeNodeViewModel</param>
        /// <param name="value">変更された値</param>
        private void CheckBoxChangeToMixed(ExplorerTreeNodeViewModel current)
        {
            // 混合状態にされたので、自分自身を単体監視に移行し、チェックされているディレクトリを全体監視に移行
            ExplorerVM.RemoveDirectoryOnlyToCheckedDirectoryManager(current);
            ExplorerVM.AddDirectoryOnlyToCheckedDirectoryManager(current);
            foreach (var child in current.Children)
            {
                if (child.IsChecked == true)
                {
                    ExplorerVM.AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(child);
                }
            }
        }
    }
}
