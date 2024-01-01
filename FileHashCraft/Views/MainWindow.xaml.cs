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
    public partial class MainWindow : Window
    {
        private ReturnPageEnum FromPage { get; set; } = ReturnPageEnum.PageExplorer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IMainWindowViewModel>();
            MainFrame.Navigate(new ExplorerPage());
            //MainFrame.Navigate(new SettingsPage());

            WeakReferenceMessenger.Default.Register<ToExplorerPage>(this, (_, _) => 
                MainFrame.Navigate(new ExplorerPage()));
            WeakReferenceMessenger.Default.Register<ReturnPageFromSettings>(this, (_, _) =>
            {
                switch (FromPage)
                {
                    case ReturnPageEnum.PageExplorer:
                        MainFrame.Navigate(new ExplorerPage());
                        break;
                    default:
                        break;
                }
            });
            WeakReferenceMessenger.Default.Register<ToSettingPage>(this, (_, message) =>
            {
                FromPage = message.ReturnPage;
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
            var explorerPageViewModel = Ioc.Default.GetService<PageExplorerViewModel>();
            explorerPageViewModel?.HwndRemoveHook();

            Application.Current.Shutdown();
        }
    }
}