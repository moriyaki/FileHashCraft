using System.Windows;
using FilOps.ViewModels;
using FilOps.ViewModels.ExplorerPage;
using FilOps.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace FilOps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainView : Window
    {
　      public MainView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IMainViewModel>();
            MainFrame.Navigate(new ExplorerPage());

            WeakReferenceMessenger.Default.Register<ToExplorerPage>(this, (recipient, message) =>
            {
                MainFrame.Navigate(new ExplorerPage());
            });
            WeakReferenceMessenger.Default.Register<ToSettingsPage>(this, (recipient, message) =>
            {
                MainFrame.Navigate(new SettingsPage());
            });

        }

        /// <summary>
        /// ウィンドウを閉じる時の後処理
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">EventArgs</param>
        private void Window_Closed(object? sender, EventArgs e)
        {
            var explorerPageViewModel = Ioc.Default.GetService<ExplorerPageViewModel>();
            explorerPageViewModel?.HwndRemoveHook();
            
            Application.Current.Shutdown();
        }

    }
}