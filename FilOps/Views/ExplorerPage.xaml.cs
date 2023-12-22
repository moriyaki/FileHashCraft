using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using FilOps.Models;
using FilOps.ViewModels;
using FilOps.ViewModels.DebugWindow;
using FilOps.ViewModels.ExplorerPage;
using Microsoft.Extensions.DependencyInjection;

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
            DataContext = App.Current.Services.GetService<IExplorerPageViewModel>();
        }

        // とりあえずのデバッグウィンドウ開く処理        
        private void DebugClick(object sender, RoutedEventArgs e)
        {
            var windowService = App.Current.Services.GetService<IWindowService>();
            windowService?.ShowDebugWindow();
        }

        /// <summary>
        /// Ctrl + マウスホイールで拡大縮小を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                var explorerVM = App.Current.Services.GetService<IExplorerPageViewModel>();
                if (explorerVM is not null)
                {
                    if (e.Delta > 0) { explorerVM.FontSize += 1;}
                    else             { explorerVM.FontSize -= 1; }
                }
                e.Handled = true;
            }
            else
                {
                    base.OnMouseWheel(e);
                }
            }
            /// <summary>
            /// ツリービューのディレクトリ選択状況が変わった時、選択されたアイテムまでスクロールします
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
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

        /// <summary>
        /// 選択されているアイテムを再帰的に検索して取得します。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static TreeViewItem? FindTreeViewItem(ItemsControl parent, ExplorerTreeNodeViewModel? data)
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

        /// <summary>
        /// ウィンドウプロシージャをオーバーライド
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += ExplorerPage_Loaded;
        }

        private IFileSystemWatcherService? FileWatcherService;

        /// <summary>
        /// ウィンドウがロードされた時のイベント、カスタムのウィンドウプロシージャをフックする
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ExplorerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var explorerVM = App.Current.Services.GetService<IExplorerPageViewModel>();
            if (explorerVM == null) { throw new NullReferenceException(nameof(explorerVM)); }
            explorerVM.InitializeOnce();

            FileWatcherService = App.Current.Services.GetService<IFileSystemWatcherService>();
            if (FileWatcherService == null) { throw new NullReferenceException(nameof(FileWatcherService)); }


            if (explorerVM != null)
            {
                explorerVM.CurrentDir = WindowsAPI.GetPath(KnownFolder.User);
            }

            HwndSource? hwndSource;

            // HwndSourceを取得
            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            explorerVM?.HwndAddHook(hwndSource);
        }
    }
}
