using System.Runtime.InteropServices;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.FileSystemWatcherServices;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.ViewModels.PageSelectTarget;
using Microsoft.Extensions.DependencyInjection;

namespace FileHashCraft
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 古い DPI Aware
        /// </summary>
        /// <returns></returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessDPIAware();

        /// <summary>
        /// 新しいDPI Aware
        /// </summary>
        /// <param name="awareness"></param>
        /// <returns></returns>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        /// <summary>
        /// 新しい DPI Aware で使う引数
        /// </summary>
        private enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }

        /// <summary>
        /// サービスの登録
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);
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

            var v = new MainWindow();
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

        /// <summary>
        /// サービスをここで登録する
        /// </summary>
        /// <returns></returns>
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IMessageServices, MessageServices>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFileSystemWatcherService, FileSystemWatcherService>();
            services.AddSingleton<ITreeCheckedManager, TreeCheckedManager>();
            services.AddSingleton<ITreeExpandedManager, TreeExpandedManager>();

            // ViewModel
            services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();
            services.AddSingleton<IDebugWindowViewModel, DebugWindowViewModel>();
            services.AddSingleton<IPageSettingsViewModel, PageSettingsViewModel>();

            services.AddSingleton<IControDirectoryTreeViewlModel, ControDirectoryTreeViewModel>();
            services.AddTransient<IDirectoryTreeViewItemModel, DirectoryTreeViewItemModel>();
            services.AddSingleton<ITreeManager, TreeManager>();
            services.AddSingleton<ICurrentDirectoryFIleSystemWatcherService, CurrentDirectoryFIleSystemWatcherService>();
            services.AddSingleton<IFileWatcherService, DrivesFileSystemWatcherService>();

            services.AddSingleton<IPageExplorerViewModel, PageExplorerViewModel>();
            services.AddTransient<IExplorerListItemViewModel, ExplorerListItemViewModel>();

            services.AddSingleton<IPageSelectTargetViewModel, PageSelectTargetViewModel>();
            services.AddSingleton<IScanHashFiles, ScanHashFiles>();
            services.AddSingleton<IPageSelectTargetViewModelMain, PageSelectTargetViewModelMain>();
            services.AddSingleton<IPageSelectTargetViewModelExtention, PageSelectTargetViewModelExtention>();

            services.AddSingleton<IPageSelectTargetViewModelExpert, PageSelectTargetViewModelExpert>();

            // Model
            services.AddSingleton<IScannedFilesManager, ScannedFilesManager>();
            services.AddSingleton<IExtentionManager, ExtentionManager>();
            return services.BuildServiceProvider();
        }
    }
}
