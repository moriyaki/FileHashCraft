using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Services;
using FileHashCraft.Services.FileSystemWatcherServices;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.Views
{
    /// <summary>
    /// ExplorerPage.xaml の相互作用ロジック
    /// </summary>
    public partial class PageExplorer : Page
    {
        public PageExplorer()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IPageExplorerViewModel>();
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
                var settingsService = Ioc.Default.GetService<ISettingsService>();
                if (settingsService is not null)
                {
                    if (e.Delta > 0) { settingsService.FontSizePlus(); }
                    else { settingsService.FontSizeMinus(); }
                }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
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

        /// <summary>
        /// ウィンドウがロードされた時のイベント、カスタムのウィンドウプロシージャをフックする
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ExplorerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var explorerVM = Ioc.Default.GetService<IPageExplorerViewModel>() ?? throw new NullReferenceException(nameof(IPageExplorerViewModel));
            var fileWatcherService = Ioc.Default.GetService<IFileWatcherService>() ?? throw new NullReferenceException(nameof(IFileWatcherService));
            explorerVM.CurrentFullPath = WindowsAPI.GetPath(KnownFolder.User);

            // HwndSourceを取得
            HwndSource? hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            explorerVM?.HwndAddHook(hwndSource);
        }

        /// <summary>
        /// スプリッタが移動された時、TreeViewの横幅を設定する
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var controlTree = Ioc.Default.GetService<IControDirectoryTreeViewlModel>();
            if (controlTree == null) { throw new NullReferenceException(nameof(controlTree)); }

            controlTree.TreeWidth = ExplorerTreeView.ActualWidth;
        }
    }
}
