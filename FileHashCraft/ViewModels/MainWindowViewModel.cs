using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IMainWindowViewModel
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double FontSize { get; set; }
        public string SelectedLanguage { get; set; }
        public string HashAlgorithm { get; set; }
        public FontFamily UsingFont { get; set; }
        /// <summary>
        /// フォントサイズを取得する
        /// </summary>
        /// <returns>設定できるフォントサイズリスト</returns>
        public IEnumerable<double> GetSelectableFontSize();

        /// <summary>
        /// フォントサイズを大きくする
        /// </summary>
        public void FontSizePlus();
        /// <summary>
        /// フォントサイズを小さくする
        /// </summary>
        public void FontSizeMinus();
    }
    #endregion インターフェース
    public class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        #region 初期設定
        private readonly string appName = "FileHashCraft";
        private readonly string settingXMLFile = "settings.xml";
        private readonly string settingsFilePath;

        public MainWindowViewModel()
        {
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            settingsFilePath = Path.Combine(localAppDataPath, settingXMLFile);

            LoadSettings();
        }
        #endregion 初期設定

        #region 設定ファイルの読み書き
        /// <summary>
        /// 設定ファイルからロード
        /// </summary>
        private void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                // 設定ファイルが存在する場合は読み込む
                XDocument doc = XDocument.Load(settingsFilePath);
                XElement? root = doc.Element("Settings");

                if (root != null)
                {
                    Top = Convert.ToDouble(root.Element("Top")?.Value);
                    Left = Convert.ToDouble(root.Element("Left")?.Value);
                    Width = Convert.ToDouble(root.Element("Width")?.Value);
                    Height = Convert.ToDouble(root.Element("Height")?.Value);
                    FontSize = Convert.ToDouble(root.Element("FontSize")?.Value);
                    SelectedLanguage = root.Element("SelectedLanguage")?.Value ?? "ja-JP";
                    HashAlgorithm = root.Element("HashAlgorithm")?.Value ?? "SHA-256";

                    var fontFamilyName = root.Element("UsingFont")?.Value ?? string.Empty;
                    UsingFont = new FontFamilyConverter().ConvertFromString(fontFamilyName) as FontFamily ?? SystemFonts.MessageFontFamily;
                }
            }
        }

        /// <summary>
        /// 設定ファイルに保存
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // 設定ファイルを保存
                XDocument doc = new(
                    new XElement("Settings",
                        new XElement("Top", Top),
                        new XElement("Left", Left),
                        new XElement("Width", Width),
                        new XElement("Height", Height),
                        new XElement("FontSize", FontSize),
                        new XElement("SelectedLanguage", SelectedLanguage),
                        new XElement("HashAlgorithm", HashAlgorithm),
                        new XElement("UsingFont", UsingFont)
                    )
                );

                // ディレクトリが存在しない場合は作成
                string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
                if (!Directory.Exists(localAppDataPath))
                {
                    Directory.CreateDirectory(localAppDataPath);
                }

                // 設定ファイルを保存
                doc.Save(settingsFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}");
            }
        }
        #endregion 設定ファイルの読み書き

        #region データバインディング
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        private double _Top = 100d;
        public double Top
        {
            get => _Top;
            set
            {
                SetProperty(ref _Top, value);
                SaveSettings();
            }
        }

        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        private double _Left = 100d;
        public double Left
        {
            get => _Left;
            set
            {
                SetProperty(ref _Left, value);
                SaveSettings();
            }
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 1200d;
        public double Width
        {
            get => _Width;
            set
            {
                SetProperty(ref _Width, value);
                SaveSettings();
            }
        }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _Height = 800d;
        public double Height
        {
            get => _Height;
            set
            {
                SetProperty(ref _Height, value);
                SaveSettings();
            }
        }

        /// <summary>
        /// 選択されている言語
        /// </summary>
        private string _SelectedLanguage = string.Empty;
        public string SelectedLanguage
        {
            get => _SelectedLanguage;
            set
            {
                SetProperty(ref _SelectedLanguage, value);
                ResourceService.Current.ChangeCulture(value);
                OnPropertyChanged("Resources");
                SaveSettings();
            }
        }

        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        private string _HashAlgorithm = "SHA-256";
        public string HashAlgorithm
        {
            get => _HashAlgorithm;
            set
            {
                if (value != _HashAlgorithm)
                {
                    SetProperty(ref _HashAlgorithm, value);
                    WeakReferenceMessenger.Default.Send(new HashAlgorithmChanged(value));
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        private FontFamily _UsingFont = SystemFonts.MessageFontFamily;
        public FontFamily UsingFont
        {
            get => _UsingFont;
            set
            {
                if (value != _UsingFont)
                {
                    SetProperty(ref _UsingFont, value);
                    WeakReferenceMessenger.Default.Send(new FontChanged(value));
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// フォントサイズの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (value != _FontSize)
                {
                    SetProperty(ref _FontSize, value, nameof(FontSize));
                    WeakReferenceMessenger.Default.Send(new FontSizeChanged(value));
                    SaveSettings();
                }
            }
        }
        #endregion データバインディング

        #region 各ウィンドウでのフォントとフォントサイズ設定方法
        /*
        フォント関連の XAML はこう
        FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"

        ViewModel ではこう

        /// <summary>
        /// フォントの設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _MainWindowViewModel.UsingFont;
            set
            {
                _MainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        public double FontSize
        {
            get => _MainWindowViewModel.FontSize;
            set
            {
                _MainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }
         */
        #endregion 各ウィンドウでのフォントとフォントサイズ設定方法

        #region メソッド
        /// <summary>
        /// 設定できるフォントサイズ
        /// </summary>
        private readonly List<double> FontSizes =
            [8d, 9d, 10d, 10.5d, 11d, 12d, 13d, 14d, 15d, 16d, 18d, 20d, 21d, 22d, 24d];

        /// <summary>
        /// フォントサイズを取得する
        /// </summary>
        /// <returns>設定できるフォントサイズリスト</returns>
        public IEnumerable<double> GetSelectableFontSize()
        {
            foreach (var fontSize in FontSizes) { yield return fontSize; }
        }

        /// <summary>
        /// フォントサイズを大きくする
        /// </summary>
        public void FontSizePlus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == FontSizes.Count - 1) return;

            FontSize = FontSizes[index + 1];
        }
        /// <summary>
        /// フォントサイズを小さくする
        /// </summary>
        public void FontSizeMinus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == 0) return;

            FontSize = FontSizes[index - 1];
        }
        #endregion メソッド
    }
}
