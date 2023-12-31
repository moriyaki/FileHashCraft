using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DirectoryTreeViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryTreeViewControl : UserControl
    {
        public DirectoryTreeViewControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDirectoryTreeViewControlViewModel>();
        }

        /// <summary>
        /// ツリービューのディレクトリ選択状況が変わった時、選択されたアイテムまでスクロールします
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectoryTreeRoot_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is DirectoryTreeViewModel)
            {
                if (e.NewValue is not DirectoryTreeViewModel item) { return; }

                // 対応するTreeViewItemを取得
                var treeViewItem = FindTreeViewItem(DirectoryTreeRoot, item);
                // 対応するTreeViewItemが存在する場合、それを表示するようにスクロール
                treeViewItem?.BringIntoView();
            }
        }

        /// <summary>
        /// 選択されているアイテムを再帰的に検索して取得します。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static TreeViewItem? FindTreeViewItem(ItemsControl parent, DirectoryTreeViewModel? data)
        {
            TreeViewItem? result = null;
            foreach (object item in parent.Items)
            {
                if (parent.ItemContainerGenerator?.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {
                    if (treeViewItem.DataContext == data) { return treeViewItem; }

                    // 子アイテムを再帰的に検索
                    result = FindTreeViewItem(treeViewItem, data);
                    if (result != null) { break; }
                }
            }
            return result;
        }
    }
}
