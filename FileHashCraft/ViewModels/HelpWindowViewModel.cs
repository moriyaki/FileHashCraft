using System.IO;
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

    public class HelpWindowViewModel : BaseViewModel, IHelpWindowViewModel
    {
        #region バインディング
        /// <summary>
        /// 画面の上端設定
        /// </summary>
        private double _top = 450d;
        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        public double _left = 400d;
        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _width = 400d;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _height = 800d;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /// <summary>
        /// 表示するヘルプHTML
        /// </summary>
        private string _htmlFile = "index.html";
        public string HtmlFile
        {
            get => _htmlFile;
            set => SetProperty(ref _htmlFile, value);
        }
        #endregion バインディング

        public HelpWindowViewModel() : base() { }

        public HelpWindowViewModel(ISettingsService settingsService) : base(settingsService) { }

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
