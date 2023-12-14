using System.Windows;
using System.Windows.Input;
using FilOps.ViewModels;
using FilOps.Views;

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
            mainFrame.Navigate(new ExplorerPage());
            _mainVM = new MainViewModel();
            DataContext = _mainVM;
        }
        private readonly MainViewModel _mainVM;
    }
}