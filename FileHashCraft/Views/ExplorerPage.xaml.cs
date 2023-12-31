using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.ViewModels;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.ViewModels.FileSystemWatch;

namespace FileHashCraft.Views
{
    /// <summary>
    /// ExplorerPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ExplorerPage : Page
    {
        public ExplorerPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IExplorerPageViewModel>();
        }

        // とりあえずのデバッグウィンドウ開く処理
        private void DebugClick(object sender, RoutedEventArgs e)
        {
            var debugWindow = new Views.DebugWindow();
            debugWindow.Show();
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
                var mainViewVM = Ioc.Default.GetService<IMainWindowViewModel>();
                if (mainViewVM is not null)
                {
                    if (e.Delta > 0) { mainViewVM.FontSizePlus(); }
                    else { mainViewVM.FontSizeMinus(); }
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

        private IDrivesFileSystemWatcherService? FileWatcherService;

        /// <summary>
        /// ウィンドウがロードされた時のイベント、カスタムのウィンドウプロシージャをフックする
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ExplorerPage_Loaded(object sender, RoutedEventArgs e)
        {
            var explorerVM = Ioc.Default.GetService<IExplorerPageViewModel>();
            if (explorerVM == null) { throw new NullReferenceException(nameof(explorerVM)); }

            FileWatcherService = Ioc.Default.GetService<IDrivesFileSystemWatcherService>();
            if (FileWatcherService == null) { throw new NullReferenceException(nameof(FileWatcherService)); }

            if (explorerVM != null)
            {
                explorerVM.CurrentFullPath = WindowsAPI.GetPath(KnownFolder.User);
            }
            // HwndSourceを取得
            HwndSource? hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            explorerVM?.HwndAddHook(hwndSource);
        }
    }
}
