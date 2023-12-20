using System.Windows;
using FilOps.ViewModels;
using FilOps.Views;
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
    }
}