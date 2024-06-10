using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.Wpf;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public class HelpWindowViewModel : ObservableObject, IHelpWindowViewModel
    {
        #region バインディング
        /// <summary>
        /// 画面の上端設定
        /// </summary>
        private double _Top = 450d;
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        public double _Left = 400d;
        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 400d;
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _Height = 800d;
        public double Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }

        /// <summary>
        /// 表示するヘルプHTML
        /// </summary>
        private string _HtmlFile = "index.html";
        public string HtmlFile
        {
            get => _HtmlFile;
            set => SetProperty(ref _HtmlFile, value);
        }
        #endregion バインディング

        private readonly ISettingsService _settingsService;
        public HelpWindowViewModel()
        {
            throw new NotImplementedException("HelpWindowViewModel");
        }

        public HelpWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// ウィンドウ位置の初期化
        /// </summary>
        public void Initialize(HelpPage help)
        {
            Top = _settingsService.Top;
            Left = _settingsService.Left + _settingsService.Width;
            Width = _settingsService.Width / 2;
            Height = _settingsService.Height;

            NavigateToHtmlFile(help);
        }

        private string GetHtmlFileFromHelpPage(HelpPage helpTab)
        {
            return _settingsService.SelectedLanguage switch
            {
                "ja-JP" => helpTab switch
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
