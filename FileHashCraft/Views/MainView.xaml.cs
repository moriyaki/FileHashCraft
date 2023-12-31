using System.Windows;
using FileHashCraft.ViewModels;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft
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
            DataContext = Ioc.Default.GetService<IMainWindowViewModel>();
            MainFrame.Navigate(new ExplorerPage());
            //MainFrame.Navigate(new SettingsPage());

            WeakReferenceMessenger.Default.Register<ToExplorerPage>(this, (__, _) => MainFrame.Navigate(new ExplorerPage()));
            WeakReferenceMessenger.Default.Register<ToSettingsPage>(this, (__, _) => MainFrame.Navigate(new SettingsPage()));
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