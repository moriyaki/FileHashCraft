using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FilOps.ViewModels;

namespace FilOps.Views
{
    /// <summary>
    /// ExplorerPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ExplorerPage : Page
    {
        public ExplorerPage()
        {
            InitializeComponent();
            viewModel = new ExplorerPageViewModel();
            DataContext = viewModel;
        }

        private readonly ExplorerPageViewModel viewModel;
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                if (viewModel is not null)
                {
                    if (e.Delta > 0)
                    {
                        viewModel.FontSize += 1;
                    }
                    else
                    {
                        viewModel.FontSize -= 1;
                    }
                }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        private void DirectoryTreeRoot_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ExplorerTreeNodeViewModel)
            {
                var item = e.NewValue as ExplorerTreeNodeViewModel;

                // 対応するTreeViewItemを取得
                var treeViewItem = FindTreeViewItem(DirectoryTreeRoot, item);
                // 対応するTreeViewItemが存在する場合、それを表示するようにスクロール
                treeViewItem?.BringIntoView();
            }
        }

        private static TreeViewItem? FindTreeViewItem(ItemsControl parent, ExplorerTreeNodeViewModel? data)
        {
            TreeViewItem? result = null;
            foreach (object item in parent.Items)
            {
                if (parent.ItemContainerGenerator?.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {

                    if (treeViewItem.DataContext == data)
                    {
                        return treeViewItem;
                    }

                    // 子アイテムを再帰的に検索
                    result = FindTreeViewItem(treeViewItem, data);
                    if (result != null)
                    {
                        break;
                    }
                }
            }


            return result;
        }
    }
}
