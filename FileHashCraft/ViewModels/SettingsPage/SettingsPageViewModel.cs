using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Properties;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface ISettingsPageViewModel
    {
    }
    #endregion インターフェース

    public class SettingsPageViewModel : ObservableObject, ISettingsPageViewModel
    {
        #region バインディング
        /// <summary>
        ///  選択できる言語
        /// </summary>
        public ObservableCollection<Language> Languages { get; private set; } =
            [
                new Language("en-US", "English"),
                new Language("ja-JP", "日本語"),
            ];

        /// <summary>
        /// 選択されている言語
        /// </summary>
        private string _SelectedLanguage = string.Empty;
        public string SelectedLanguage
        {
            get => _SelectedLanguage;
            set
            {
                ResourceService.Current.ChangeCulture(value);
                OnPropertyChanged("Resources");
                SetProperty(ref _SelectedLanguage, value);
            }
        }

        /// <summary>
        /// フォントファミリーの一覧
        /// </summary>
        public ObservableCollection<FontFamily> FontFamilies { get; private set; }

        /// <summary>
        /// 選択されているフォント
        /// </summary>
        private FontFamily _SelectedFontFamily = SystemFonts.MessageFontFamily;
        public FontFamily SelectedFontFamily
        {
            get => _SelectedFontFamily;
            set
            {
                UsingFont = value;
                SetProperty(ref _SelectedFontFamily, value);
            }
        }

        /// <summary>
        /// フォントサイズの一覧
        /// </summary>
        public ObservableCollection<FontSize> FontSizes { get; private set; } = [];

        /// <summary>
        /// 選択されたフォントサイズ
        /// </summary>
        private double _SelectedFontSize = SystemFonts.MessageFontSize;
        public double SelectedFontSize
        {
            get => _SelectedFontSize;
            set
            {
                if (SelectedFontSize != value)
                {
                    FontSize = value;
                    SetProperty(ref _SelectedFontSize, value);
                }
            }
        }

        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; private set; } =
            [
                new("SHA-256", Resources.HashAlgorithm_SHA256),
                new("SHA-3", Resources.HashAlgorithm_SHA_3),
                new("MD5", Resources.HashAlgorithm_MD5),
            ];

        private string _SelectedHashAlgorithm = "SHA-256";
        public string SelectedHashAlgorithm
        {
            get => _SelectedHashAlgorithm;
            set
            {
                SetProperty(ref _SelectedHashAlgorithm, value);
            }
        }

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

        /// <summary>
        /// エクスプローラー風画面にページに移動
        /// </summary>
        public DelegateCommand ToExplorer { get; set; }
        #endregion バインディング

        #region コンストラクタと初期化
        private readonly IMainWindowViewModel _MainWindowViewModel;

        public SettingsPageViewModel(
            IMainWindowViewModel mainViewModel)
        {
            _MainWindowViewModel = mainViewModel;

            // TODO : MainViewModel から言語を読み込むようにする
            SelectedLanguage = CultureInfo.CurrentCulture.Name;

            // フォントの一覧取得とバインド
            FontFamilies = new ObservableCollection<FontFamily>(GetSortedFontFamilies());
            // TODO : MainViewModel からフォント設定を読み込むようにする

            // フォントサイズの一覧取得とバインド
            foreach (var fontSize in _MainWindowViewModel.GetSelectableFontSize())
            {
                FontSizes.Add(new FontSize(fontSize));
            }
            // TODO : MainViewModel からフォントサイズを読み込むようにする

            ToExplorer = new DelegateCommand(() => { WeakReferenceMessenger.Default.Send(new ToExplorerPage()); });

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (recipient, message) =>
            {
                FontSize = message.FontSize;
                SelectedFontSize = message.FontSize;
            });

        }

        /// <summary>
        /// 日本語と英語のフォントを取得する
        /// </summary>
        /// <returns>ソートされたフォントのリスト</returns>
        private static IEnumerable<FontFamily> GetSortedFontFamilies()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            CultureInfo cultureUS = new("en-US");

            // フォント名の重複判定に利用
            List<string> uriName = [];
            // 取得したフォントのリスト
            IList<FontFamily> fontFamilyList = [];
            foreach (var font in Fonts.SystemFontFamilies)
            {
                var typefaces = font.GetTypefaces();
                foreach (var typeface in typefaces)
                {
                    _ = typeface.TryGetGlyphTypeface(out GlyphTypeface glyphType);
                    if (glyphType == null) continue;

                    // フォントに日本語名がなければ英語名
                    string fontName = glyphType.Win32FamilyNames[culture] ?? glyphType.Win32FamilyNames[cultureUS];

                    //フォント名で重複判定
                    var uri = glyphType.FontUri;
                    if (!uriName.Any(f => f == fontName))
                    {
                        uriName.Add(fontName);
                        fontFamilyList.Add(new(uri, fontName));
                    }
                }
            }
            return fontFamilyList.OrderBy(family => family.Source);
        }
        #endregion コンストラクタと初期化
    }

    #region 設定画面表示用クラス
    /// <summary>
    /// 言語の選択用クラス
    /// </summary>
    public class Language
    {
        public string Lang { get; }
        public string Name { get; }
        public Language() { throw new NotImplementedException(); }

        public Language(string lang, string name)
        {
            Lang = lang;
            Name = name;
        }
    }

    /// <summary>
    /// フォントサイズの選択用クラス
    /// </summary>
    public class FontSize
    {
        public double Size { get; }
        public string SizeString
        {
            get
            {
                if (Size == Math.Floor(Size))
                {
                    return Math.Floor(Size).ToString() + " px";
                }
                else
                {
                    return Size.ToString() + "px";
                }
            }
        }
        public FontSize() { throw new NotImplementedException(); }
        public FontSize(double size)
        {
            Size = size;
        }
    }

    /// <summary>
    /// ハッシュ計算アルゴリズムの選択
    /// </summary>
    public class HashAlgorithm
    {
        public string Algorithm { get; }
        public string Name { get; }
        public HashAlgorithm() { throw new NotImplementedException(); }
        public HashAlgorithm(string algorithm, string algorithmCaption)
        {
            Algorithm = algorithm;
            Name = $"{algorithm} ({algorithmCaption})";

        }
    }
    #endregion 設定画面表示用クラス
}