﻿using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;

namespace FileHashCraft.ViewModels
{
    public enum HelpPage
    {
        Index,
        Wildcard,
        Regex,
    }

    #region インターフェース

    public interface IHelpWindowViewModel
    {
        double Top { get; set; }
        double Left { get; set; }

        /// <summary>
        /// ウィンドウ位置の初期化
        /// </summary>
        void Initialize(HelpPage help);

        /// <summary>
        /// 指定したヘルプファイルを開く
        /// </summary>
        /// <param name="help">ヘルプファイル</param>
        void NavigateToHtmlFile(HelpPage help);
    }

    #endregion インターフェース

    public partial class HelpWindowViewModel : BaseViewModel, IHelpWindowViewModel
    {
        #region バインディング

        /// <summary>
        /// 画面の上端設定
        /// </summary>
        [ObservableProperty]
        private double _Top = 450d;

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        [ObservableProperty]
        public double _Left = 400d;

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        [ObservableProperty]
        private double _Width = 400d;

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        [ObservableProperty]
        private double _Height = 800d;

        /// <summary>
        /// 表示するヘルプHTML
        /// </summary>
        [ObservableProperty]
        private string _HtmlFile = "index.html";

        #endregion バインディング

        public HelpWindowViewModel() : base()
        {
        }

        public HelpWindowViewModel(IMessenger messenger, ISettingsService settingsService) : base(messenger, settingsService)
        {
        }

        /// <summary>
        /// ウィンドウ位置の初期化
        /// </summary>
        public void Initialize(HelpPage help)
        {
            Top = _SettingsService.Top;
            Left = _SettingsService.Left + _SettingsService.Width;
            Width = _SettingsService.Width / 2;
            Height = _SettingsService.Height;

            NavigateToHtmlFile(help);
        }

        private string GetHtmlFileFromHelpPage(HelpPage helpTab)
        {
            return _SettingsService.SelectedLanguage switch
            {
                "ja" => helpTab switch
                {
                    HelpPage.Wildcard => "wildcard_ja.html",
                    HelpPage.Regex => "regex_ja.html",
                    _ => "index_ja.html",
                },
                _ => helpTab switch
                {
                    HelpPage.Wildcard => "wildcard.html",
                    HelpPage.Regex => "regex.html",
                    _ => "index.html",
                },
            };
        }

        /// <summary>
        /// 指定したヘルプファイルを開く
        /// </summary>
        /// <param name="help">ヘルプファイル</param>
        public void NavigateToHtmlFile(HelpPage help)
        {
            var htmlFile = GetHtmlFileFromHelpPage(help);
            var htmlDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpDocuments");
            var htmlFullPath = Path.Combine(htmlDirectory, htmlFile);
            HtmlFile = new Uri(htmlFullPath).AbsoluteUri;
        }
    }
}