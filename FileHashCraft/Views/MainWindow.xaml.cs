﻿using System.Windows;
using System.Windows.Navigation;
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

            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new PageExplorer());
            //MainFrame.Navigate(new PageSettings());
            //MainFrame.Navigate(new PageTargetFileSetting());

            // PageExplorer へ移動のメッセージ受信したので移動
            WeakReferenceMessenger.Default.Register<ToPageExplorer>(this, (_, _) =>
                MainFrame.Navigate(new PageExplorer()));
            // PageTargetFileSetting へ移動のメッセージ受信したので移動
            WeakReferenceMessenger.Default.Register<ToPageTargetFileSetting>(this, (_, _) =>
                MainFrame.Navigate(new PageTargetFileSetting()));
            // PageHashCalcing へ移動のメッセージ受信したので移動
            WeakReferenceMessenger.Default.Register<ToPageHashCalcing>(this, (_, _) =>
                MainFrame.Navigate(new PageHashCalcing()));
            // PageSetting への移動のメッセージ受信したので、戻り先を保存して移動
            WeakReferenceMessenger.Default.Register<ToPageSetting>(this, (_, message) =>
            {
                FromPage = message.ReturnPage;
                MainFrame.Navigate(new PageSettings());
            });
            // PageSetting の終了メッセージを受信したので、元のページへ移動
            WeakReferenceMessenger.Default.Register<ReturnPageFromSettings>(this, (_, _) =>
            {
                switch (FromPage)
                {
                    case ReturnPageEnum.PageExplorer:
                        MainFrame.Navigate(new PageExplorer());

                        break;
                    case ReturnPageEnum.PageTargetFileSelect:
                        MainFrame.Navigate(new PageTargetFileSetting());
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
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is PageExplorer)
            {
                var pageExplorer = Ioc.Default.GetService<IPageExplorerViewModel>();
                pageExplorer?.Initialize();
            }
            if (e.Content is PageTargetFileSetting)
            {
                var targetFileSetting = Ioc.Default.GetService<IPageSelectTargetFileViewModel>();
                targetFileSetting?.Initialize();
            }
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