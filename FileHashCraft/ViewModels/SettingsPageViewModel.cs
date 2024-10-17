/*  PageSettingsViewModel.cs

    設定画面の ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface ISettingsPageViewModel;
    #endregion インターフェース

    public class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
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
        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                _settingsService.SendLanguage(_selectedLanguage);
                var currentHashAlgorithms = SelectedHashAlgorithm;
                HashAlgorithms.Clear();
                HashAlgorithms =
                [
                    new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                    new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                    new(_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
                ];
                OnPropertyChanged(nameof(HashAlgorithms));
                SelectedHashAlgorithm = currentHashAlgorithms;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
            }
        }

        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } = [];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        private string _selectedHashAlgorithm;
        public string SelectedHashAlgorithm
        {
            get => _selectedHashAlgorithm;
            set
            {
                if (value == _selectedHashAlgorithm) return;

                SetProperty(ref _selectedHashAlgorithm, value);
                _settingsService.SendHashAlogrithm(value);
            }
        }
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        private bool _isReadOnlyFileInclude;
        public bool IsReadOnlyFileInclude
        {
            get => _isReadOnlyFileInclude;
            set
            {
                if (_isReadOnlyFileInclude == value) return;
                SetProperty(ref _isReadOnlyFileInclude, value);
                _settingsService.SendReadOnlyFileInclude(value);
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _isHiddenFileInclude;
        public bool IsHiddenFileInclude
        {
            get => _isHiddenFileInclude;
            set
            {
                if (_isHiddenFileInclude == value) return;
                SetProperty(ref _isHiddenFileInclude, value);
                _settingsService.SendHiddenFileInclude(value);
            }
        }
        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _isZeroSizeFileDelete;
        public bool IsZeroSizeFileDelete
        {
            get => _isZeroSizeFileDelete;
            set
            {
                if (_isZeroSizeFileDelete == value) return;
                SetProperty(ref _isZeroSizeFileDelete, value);
                _settingsService.SendZeroSizeFileDelete(value);
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _isEmptyDirectoryDelete;
        public bool IsEmptyDirectoryDelete
        {
            get => _isEmptyDirectoryDelete;
            set
            {
                if (_isEmptyDirectoryDelete == value) return;
                SetProperty(ref _isEmptyDirectoryDelete, value);
                _settingsService.SendEmptyDirectoryDelete(value);
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
        #endregion バインディング

        #region コンストラクタと初期化
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IHashAlgorithmHelper _hashAlgorithmHelper;

        public SettingsPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileSystemServices fileSystemServices,
            IHashAlgorithmHelper hashAlgorithmHelper
        ) : base(messenger, settingsService)
        {
            _fileSystemServices = fileSystemServices;
            _hashAlgorithmHelper = hashAlgorithmHelper;

            HashAlgorithms =
            [
                new (_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new (_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new (_hashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

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
                () => _fileSystemServices.NavigateReturnPageFromSettings());

            // 読み取り専用ファイルを利用するかどうかが変更されたメッセージ受信
            _messenger.Register<ReadOnlyFileIncludeChangedMessage>(this, (_, m)
                => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかが変更されたメッセージ受信
            _messenger.Register<HiddenFileIncludeChangedMessage>(this, (_, m)
                => IsHiddenFileInclude = m.HiddenFileInclude);

            // 0サイズファイルを削除するかどうかが変更されたメッセージ受信
            _messenger.Register<ZeroSizeFileDeleteChangedMessage>(this, (_, m)
                => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);

            _selectedLanguage = _settingsService.SelectedLanguage;
            _selectedHashAlgorithm = _settingsService.HashAlgorithm;
            _isReadOnlyFileInclude = _settingsService.IsReadOnlyFileInclude;
            _isHiddenFileInclude = _settingsService.IsHiddenFileInclude;
            _isZeroSizeFileDelete = _settingsService.IsZeroSizeFileDelete;
            _isEmptyDirectoryDelete = _settingsService.IsEmptyDirectoryDelete;
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
        public Language() { throw new NotImplementedException(nameof(Language)); }

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
        public FontSize() { throw new NotImplementedException(nameof(FontSize)); }
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
        public HashAlgorithm() { throw new NotImplementedException(nameof(HashAlgorithm)); }
        public HashAlgorithm(string algorithm, string algorithmCaption)
        {
            Algorithm = algorithm;
            Name = algorithmCaption;
        }
    }
    #endregion 設定画面表示用クラス
}