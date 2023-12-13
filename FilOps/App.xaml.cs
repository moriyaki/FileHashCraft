using System.Configuration;
using System.Data;
using System.Windows;
using FilOps.ViewModels;

namespace FilOps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var v = new MainView();
            var vm = new MainViewModel();
            v.DataContext = vm;
            v.Show();
        }
    }
}
