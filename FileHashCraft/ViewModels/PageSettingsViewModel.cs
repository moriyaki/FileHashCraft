/*  PageSettingsViewModel.cs

    設定画面の ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

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
        private string _SelectedLanguage;
        public string SelectedLanguage
        {
            get => _SelectedLanguage;
            set
            {
                _SelectedLanguage = value;
                _messageServices.SendLanguage(_SelectedLanguage);
                var currentHashAlgorithms = SelectedHashAlgorithm;
                HashAlgorithms.Clear();
                HashAlgorithms =
                [
                    new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                    new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                    new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
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
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
        ];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        private string _SelectedHashAlgorithm;
        public string SelectedHashAlgorithm
        {
            get => _SelectedHashAlgorithm;
            set
            {
                if (value == _SelectedHashAlgorithm) return;

                SetProperty(ref _SelectedHashAlgorithm, value);
                _messageServices.SendHashAlogrithm(value);
            }
        }
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        private bool _IsReadOnlyFileInclude;
        public bool IsReadOnlyFileInclude
        {
            get => _IsReadOnlyFileInclude;
            set
            {
                if (_IsReadOnlyFileInclude == value) return;
                SetProperty(ref _IsReadOnlyFileInclude, value);
                _messageServices.SendReadOnlyFileInclude(value);
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _IsHiddenFileInclude;
        public bool IsHiddenFileInclude
        {
            get => _IsHiddenFileInclude;
            set
            {
                if (_IsHiddenFileInclude == value) return;
                SetProperty(ref _IsHiddenFileInclude, value);
                _messageServices.SendHiddenFileInclude(value);
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _IsZeroSizeFileDelete;
        public bool IsZeroSizeFileDelete
        {
            get => _IsZeroSizeFileDelete;
            set
            {
                if (_IsZeroSizeFileDelete == value) return;
                SetProperty(ref _IsZeroSizeFileDelete, value);
                _messageServices.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _IsEmptyDirectoryDelete;
        public bool IsEmptyDirectoryDelete
        {
            get => _IsEmptyDirectoryDelete;
            set
            {
                if (_IsEmptyDirectoryDelete == value) return;
                SetProperty(ref _IsEmptyDirectoryDelete, value);
                _messageServices.SendEmptyDirectoryDelete(value);
            }
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _messageServices.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _FontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) { return; }
                SetProperty(ref _FontSize, value);
                _messageServices.SendFontSize(value);
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
        public RelayCommand ReturnPage { get; set; }
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsReadOnlyFileIncludeClicked { get; set; }
        /// <summary>
        /// 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
        /// </summary>
        public RelayCommand IsHiddenFileIncludeClicked { get; set; }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public RelayCommand IsEmptyDirectoryDeleteClicked { get; set; }
        #endregion コマンド

        #region コンストラクタと初期化
        private readonly ISettingsService _settingsService;
        private readonly IMessageServices _messageServices;

        public PageSettingsViewModel(
            ISettingsService settingsService,
            IMessageServices messageServices
        )
        {
            _settingsService = settingsService;
            _messageServices = messageServices;

            // フォントの一覧取得とバインド
            FontFamilies = new ObservableCollection<FontFamily>(GetSortedFontFamilies());

            // フォントサイズの一覧取得とバインド
            foreach (var fontSize in _settingsService.GetSelectableFontSize())
            {
                FontSizes.Add(new FontSize(fontSize));
            }
            // 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsReadOnlyFileIncludeClicked = new RelayCommand(()
                => IsReadOnlyFileInclude = !IsReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsHiddenFileIncludeClicked = new RelayCommand(()
                => IsHiddenFileInclude = !IsHiddenFileInclude);

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new RelayCommand(()
                => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new RelayCommand(()
                => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // 「終了」で戻るページへのメッセージを送るコマンド
            ReturnPage = new RelayCommand(
                () => _messageServices.SendReturnPageFromSettings());

            // 読み取り専用ファイルを利用するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<ReadOnlyFileIncludeChanged>(this, (_, m)
                => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<HiddenFileIncludeChanged>(this, (_, m)
                => IsHiddenFileInclude = m.HiddenFileInclude);

            // 0サイズファイルを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<ZeroSizeFileDeleteChanged>(this, (_, m)
                => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);

            //空ディレクトリを削除するかどうかが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<EmptyDirectoryDeleteChanged>(this, (_, m)
                => IsEmptyDirectoryDelete = m.EmptyDirectoryDelete);

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily );

            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m)
                => FontSize = m.FontSize);

            _SelectedLanguage = _settingsService.SelectedLanguage;
            _SelectedHashAlgorithm = _settingsService.HashAlgorithm;
            _IsReadOnlyFileInclude = _settingsService.IsReadOnlyFileInclude;
            _IsHiddenFileInclude = _settingsService.IsHiddenFileInclude;
            _IsZeroSizeFileDelete = _settingsService.IsZeroSizeFileDelete;
            _IsEmptyDirectoryDelete = _settingsService.IsEmptyDirectoryDelete;

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
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