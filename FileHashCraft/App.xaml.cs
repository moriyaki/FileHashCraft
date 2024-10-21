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
using FileHashCraft.ViewModels.SelectTargetPage;
using FileHashCraft.ViewModels.HashCalcingPage;
using Microsoft.Extensions.DependencyInjection;
using FileHashCraft.Models.HashCalc;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.ViewModels.DuplicateSelectPage;

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

            services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            // Services
            services.AddSingleton<IFileSystemServices, FileSystemServices>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFileSystemWatcherService, FileSystemWatcherService>();
            services.AddSingleton<ICheckedTreeItemsManager, CheckedTreeItemsManager>();
            services.AddSingleton<ITreeExpandedManager, ExpandedTreeManager>();

            // ViewModel
            services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();
            services.AddSingleton<IDebugWindowViewModel, DebugWindowViewModel>();
            services.AddSingleton<ISettingsPageViewModel, SettingsPageViewModel>();
            services.AddSingleton<IHelpWindowViewModel, HelpWindowViewModel>();

            services.AddSingleton<IControDirectoryTreeViewlModel, ControDirectoryTreeViewModel>();
            services.AddTransient<IDirectoryTreeViewItemModel, DirectoryTreeViewItemModel>();
            services.AddSingleton<IDirectoryTreeManager, DirectoryTreeManager>();
            services.AddSingleton<ICurrentDirFileSystemWatcherService, CurrentDirFileSystemWatcherService>();
            services.AddSingleton<IFileWatcherService, DrivesFileSystemWatcherService>();

            services.AddSingleton<IExplorerPageViewModel, ExplorerPageViewModel>();
            services.AddTransient<IExplorerListItemViewModel, ExplorerListItemViewModel>();

            services.AddSingleton<IShowTargetInfoUserControlViewModel, ShowTargetInfoUserControlViewModel>();
            services.AddSingleton<ISelectTargetPageViewModel, SelectTargetPageViewModel>();
            services.AddSingleton<ISetExtentionControlViewModel, SetExtentionControlViewModel>();
            services.AddSingleton<ISetWildcardControlViewModel, SetWildcardControlViewModel>();
            services.AddTransient<IWildcardCriteriaItemViewModel, WildcardCriteriaItemViewModel>();
            services.AddSingleton<ISetRegexControlViewModel, SetRegexControlViewModel>();
            services.AddTransient<IRegexCriteriaItemViewModel, RegexCriteriaItemViewModel>();
            services.AddSingleton<ISetExpertControlViewModel, SetExpertControlViewModel>();

            services.AddSingleton<IHashCalcingPageViewModel, HashCalcingPageViewModel>();

            services.AddSingleton<IDuplicateSelectPageViewModel, DuplicateSelectPageViewModel>();
            services.AddSingleton<IDupFilesDirsListBoxControlViewModel, DupFilesDirsListBoxControlViewModel>();
            services.AddSingleton<IDupDirsFilesTreeViewControlViewModel, DupDirsFilesTreeViewControlViewModel>();
            services.AddSingleton<IDupLinkedDirsFilesTreeViewControlViewModel, DupLinkedDirsFilesTreeViewControlViewModel>();

            // Model
            services.AddSingleton<IExtentionManager, ExtentionManager>();
            services.AddSingleton<IFileManager, FileManager>();

            // Model - FileScan
            services.AddSingleton<IDirectoriesManager, DirectoriesManager>();
            services.AddSingleton<IFileSearchCriteriaManager, FileSearchCriteriaManager>();
            services.AddSingleton<IScanHashFiles, ScanHashFiles>();
            services.AddSingleton<IScannedFilesManager, ScannedFilesManager>();

            // Model - HashCalc
            services.AddSingleton<IFileHashCalc, FileHashCalc>();

            // Model - Helper
            services.AddSingleton<IExtentionTypeHelper, ExtentionTypeHelper>();
            services.AddSingleton<IHashAlgorithmHelper, HashAlgorithmHelper>();

            return services.BuildServiceProvider();
        }
    }
}
