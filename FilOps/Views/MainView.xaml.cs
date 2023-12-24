using System.Windows;
using FilOps.ViewModels;
using FilOps.ViewModels.ExplorerPage;
using FilOps.Views;
using FilOps.ViewModels.DebugWindow;
using Microsoft.Extensions.DependencyInjection;

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
            DataContext = App.Current.Services.GetService<IMainViewModel>();
            MainFrame.Navigate(new ExplorerPage());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var explorerPageViewModel = App.Current.Services.GetService<IExplorerPageViewModel>();
            explorerPageViewModel?.HwndRemoveHook();

            var debugViewModel = App.Current.Services.GetService<IDebugWindowViewModel>();
            debugViewModel?.Cancel();

            Application.Current.Shutdown();
        }
    }
}