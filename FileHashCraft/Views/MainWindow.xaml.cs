using System.Windows;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels;
using FileHashCraft.ViewModels.DuplicateSelectPage;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.ViewModels.HashCalcingPage;
using FileHashCraft.ViewModels.SelectTargetPage;
using FileHashCraft.Views;

namespace FileHashCraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        private ReturnPageEnum FromPage { get; set; } = ReturnPageEnum.ExplorerPage;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IMainWindowViewModel>();

            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new ExplorerPage());

            var _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));

            // PageExplorer へ移動のメッセージ受信したので移動
            _Messanger.Register<ToExplorerPageMessage>(this, (_, _) =>
                MainFrame.Navigate(new ExplorerPage()));
            // PageSelectTarget へ移動のメッセージ受信したので移動
            _Messanger.Register<ToPageSelectTargetMessage>(this, (_, _) =>
                MainFrame.Navigate(new SelectTargetPage()));
            // PageHashCalcing へ移動のメッセージ受信したので移動
            _Messanger.Register<ToHashCalcingPageMessage>(this, (_, _) =>
                MainFrame.Navigate(new HashCalcingPage()));
            // PageSameFileSelectSimple へ移動のメッセージ受信したので移動
            _Messanger.Register<ToDuplicateSelectPage>(this, (_, _) =>
                MainFrame.Navigate(new DuplicateSelectPage()));
            // PageSetting への移動のメッセージ受信したので、戻り先を保存して移動
            _Messanger.Register<ToSettingPageMessage>(this, (_, m) =>
            {
                FromPage = m.ReturnPage;
                MainFrame.Navigate(new SettingsPage());
            });
            // PageSetting の終了メッセージを受信したので、元のページへ移動
            _Messanger.Register<ReturnPageFromSettingsMessage>(this, (_, _) =>
            {
                switch (FromPage)
                {
                    case ReturnPageEnum.ExplorerPage:
                        MainFrame.Navigate(new ExplorerPage());

                        break;

                    case ReturnPageEnum.SelecTargettPage:
                        MainFrame.Navigate(new SelectTargetPage());
                        break;

                    case ReturnPageEnum.HashCalcingPage:
                        MainFrame.Navigate(new HashCalcingPage());
                        break;

                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// ページが遷移した時の処理
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">NavigationEventArgs</param>
        private async void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is ExplorerPage)
            {
                var pageExplorer = Ioc.Default.GetService<IExplorerPageViewModel>() ?? throw new NullReferenceException(nameof(IExplorerPageViewModel));
                pageExplorer?.Initialize();
                return;
            }
            if (e.Content is SelectTargetPage)
            {
                var targetFileSetting = Ioc.Default.GetService<ISelectTargetPageViewModel>() ?? throw new NullReferenceException(nameof(ISelectTargetPageViewModel));
                targetFileSetting.Initialize();
                return;
            }
            if (e.Content is HashCalcingPage)
            {
                var hashCalcing = Ioc.Default.GetService<IHashCalcingPageViewModel>() ?? throw new NullReferenceException(nameof(IHashCalcingPageViewModel));
                await hashCalcing.InitializeAsync();
                return;
            }
            if (e.Content is DuplicateSelectPage)
            {
                var dupSelect = Ioc.Default.GetService<IDuplicateSelectPageViewModel>() ?? throw new NullReferenceException(nameof(IDuplicateSelectPageViewModel));
                dupSelect.Initialize();
                return;
            }
        }

        /// <summary>
        /// ウィンドウを閉じる時の後処理
        /// </summary>
        /// <param name="sender">object?</param>
        /// <param name="e">EventArgs</param>
        private void Window_Closed(object? sender, EventArgs e)
        {
            var explorerPageViewModel = Ioc.Default.GetService<IExplorerPageViewModel>() ?? throw new NullReferenceException(nameof(IExplorerListItemViewModel));
            explorerPageViewModel.HwndRemoveHook();

            var messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            //messanger.UnregisterAll(this);

            Application.Current.Shutdown();
        }
    }
}