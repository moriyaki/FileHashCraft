using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IPageSettingsViewModel;
    #endregion インターフェース

    public class PageSettingsViewModel : ObservableObject, IPageSettingsViewModel
    {
        #region バインディング
        /// <summary>
        ///  選択できる言語
        /// </summary>
        public ObservableCollection<Language> Languages { get; } =
            [
                new Language("en-US", "English"),
                new Language("ja-JP", "日本語"),
            ];

        /// <summary>
        /// 選択されている言語
        /// </summary>
        public string SelectedLanguage
        {
            get => _mainWindowViewModel.SelectedLanguage;
            set
            {
                _mainWindowViewModel.SelectedLanguage = value;
                var currentHashAlgorithms = SelectedHashAlgorithm;
                HashAlgorithms.Clear();
                HashAlgorithms =
                    [
                        new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                        new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                        new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
                    ];
                OnPropertyChanged(nameof(HashAlgorithms));
                SelectedHashAlgorithm = currentHashAlgorithms;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
            }
        }

        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } =
            [
                new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(HashAlgorithmHelper.GetAlgorithmName(Models.FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary
        public string SelectedHashAlgorithm
        {
            get => _mainWindowViewModel.HashAlgorithm;
            set
            {
                if (value == _mainWindowViewModel.HashAlgorithm) return;
                _mainWindowViewModel.HashAlgorithm = value;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
            }
        }

        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        public bool IsZeroSizeFileDelete
        {
            get => _mainWindowViewModel.IsZeroSizeFileDelete;
            set
            {
                if (_mainWindowViewModel.IsZeroSizeFileDelete == value) return;
                _mainWindowViewModel.IsZeroSizeFileDelete = value;
                OnPropertyChanged(nameof(IsZeroSizeFileDelete));
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        public bool IsEmptyDirectoryDelete
        {
            get => _mainWindowViewModel.IsEmptyDirectoryDelete;
            set
            {
                if (_mainWindowViewModel.IsEmptyDirectoryDelete == value) return;
                _mainWindowViewModel.IsEmptyDirectoryDelete = value;
                OnPropertyChanged(nameof(IsEmptyDirectoryDelete));
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsEmptyDirectoryDeleteClicked { get; set; }
        /// <summary>
        /// フォントの取得と設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _mainWindowViewModel.UsingFont;
            set
            {
                if (_mainWindowViewModel.UsingFont == value) return;
                _mainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの取得と設定
        /// </summary>
        public double FontSize
        {
            get => _mainWindowViewModel.FontSize;
            set
            {
                if (_mainWindowViewModel.FontSize == value) return;
                _mainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// フォントファミリーの一覧
        /// </summary>
        public ObservableCollection<FontFamily> FontFamilies { get; }

        /// <summary>
        /// フォントサイズの一覧
        /// </summary>
        public ObservableCollection<FontSize> FontSizes { get; } = [];

        /// <summary>
        /// エクスプローラー風画面にページに移動
        /// </summary>
        public DelegateCommand ReturnPage { get; set; }
        #endregion バインディング

        #region コンストラクタと初期化
        private readonly IMainWindowViewModel _mainWindowViewModel;

        public PageSettingsViewModel(
            IMainWindowViewModel mainViewModel)
        {
            _mainWindowViewModel = mainViewModel;

            // 利用言語の読み込み
            SelectedLanguage = _mainWindowViewModel.SelectedLanguage;

            // フォントの一覧取得とバインド
            FontFamilies = new ObservableCollection<FontFamily>(GetSortedFontFamilies());

            // フォントサイズの一覧取得とバインド
            foreach (var fontSize in _mainWindowViewModel.GetSelectableFontSize())
            {
                FontSizes.Add(new FontSize(fontSize));
            }

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new DelegateCommand(() => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new DelegateCommand(() => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // 「終了」で戻るページへのメッセージを送るコマンド
            ReturnPage = new DelegateCommand(
                () => WeakReferenceMessenger.Default.Send(new ReturnPageFromSettings()));

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, _) => OnPropertyChanged(nameof(UsingFont)));

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, _) => OnPropertyChanged(nameof(FontSize)));
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
            Name = algorithmCaption;
        }
    }
    #endregion 設定画面表示用クラス
}