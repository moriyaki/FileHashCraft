using System.Windows;
using FilOps.ViewModels;
using FilOps.ViewModels.ExplorerPage;
using FilOps.Views;
using FilOps.ViewModels.DebugWindow;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FilOps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
　      public MainView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IMainViewModel>();
            MainFrame.Navigate(new ExplorerPage());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var explorerPageViewModel = Ioc.Default.GetService<ExplorerPageViewModel>();
            explorerPageViewModel?.HwndRemoveHook();
            

            var debugViewModel = Ioc.Default.GetService<IDebugWindowViewModel>();
            debugViewModel?.Cancel();

            Application.Current.Shutdown();
        }
    }
}