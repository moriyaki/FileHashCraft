using System.Runtime.InteropServices;
using System.Windows;
using FilOps.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 古いDPI Aware
        /// </summary>
        /// <returns></returns>
        [System.Runtime.InteropServices.LibraryImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static partial bool SetProcessDPIAware();

        /// <summary>
        /// 新しいDPI Aware
        /// </summary>
        /// <param name="awareness"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }


        public App()
        {
            Services = ConfigureServices();
        }

        /// <summary>
        /// 機動処理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // アプリケーションをDPI Awareに設定
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 3)
            {
                // 新しい SetProcessDpiAwareness
                SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
            }
            else
            {
                // Windows 8.1以前の場合は、SetProcessDpiAwareを使用する（非推奨）
                SetProcessDPIAware();
            }

            base.OnStartup(e);

            var v = new MainView();
            v.Show();
        }

        /// <summary>
        /// 現在の App インスタンスを使うために取得する
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// サービスプロバイダ
        /// </summary>
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IMainViewModel, MainViewModel>();
            services.AddSingleton<IExplorerPageViewModel, ExplorerPageViewModel>();

            return services.BuildServiceProvider();
        }   
    }
}
