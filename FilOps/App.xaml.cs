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
        [System.Runtime.InteropServices.LibraryImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static partial bool SetProcessDPIAware();

        protected override void OnStartup(StartupEventArgs e)
        {
            SetProcessDPIAware(); // アプリケーションをDPI Awareに設定

            base.OnStartup(e);
            var v = new MainView();
            v.Show();
        }
    }
}
