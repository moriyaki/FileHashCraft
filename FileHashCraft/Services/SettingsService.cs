using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.Helpers;

namespace FileHashCraft.Services
{
    #region インターフェース
    public interface ISettingsService
    {
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        public double Top { get; }
        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        public double Left { get; }
        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        public double Width { get; }
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        public double Height { get; }
        /// <summary>
        /// ツリービューの幅
        /// </summary>
        public double TreeWidth { get; }
        /// <summary>
        /// リストボックスの幅
        /// </summary>
        public double ListWidth { get; }
        /// <summary>
        /// 選択されている言語
        /// </summary>
        public string SelectedLanguage { get; }
        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        public string HashAlgorithm { get; }
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        public bool IsReadOnlyFileInclude { get; }
        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        public bool IsHiddenFileInclude { get; }
        /// <summary>
        /// 0 サイズのファイルを削除するかどうか
        /// </summary>
        public bool IsZeroSizeFileDelete { get; }
        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        public bool IsEmptyDirectoryDelete { get; }
        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        public FontFamily CurrentFont { get; }
        /// <summary>
        /// フォントのサイズ
        /// </summary>
        public double FontSize { get; }
        /// <summary>
        /// 設定できるフォントサイズのコレクションを取得します。
        /// </summary>
        public IEnumerable<double> GetSelectableFontSize();
        /// <summary>
        /// フォントサイズを大きくします。
        /// </summary>
        public void FontSizePlus();
        /// <summary>
        /// フォントサイズを小さくします。
        /// </summary>
        public void FontSizeMinus();
        /// <summary>
        /// 設定を読み込みます。
        /// </summary>
        public void LoadSettings();
    }
    #endregion インターフェース

    public class SettingsService :ObservableObject, ISettingsService
    {
        #region コンストラクタ
        private readonly string appName = "FileHashCraft";
        private readonly string settingXMLFile = "settings.xml";
        private readonly string settingsFilePath;

        private readonly IMessageServices _messageServices;
        public SettingsService(IMessageServices messageServices)
        {
            _messageServices = messageServices;

            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            settingsFilePath = Path.Combine(localAppDataPath, settingXMLFile);

            LoadSettings();

            _messageServices.SendWindowTop(Top);
            _messageServices.SendWindowLeft(Left);
            _messageServices.SendWindowWidth(Width);
            _messageServices.SendWindowHeight(Height);
            _messageServices.SendTreeWidth(TreeWidth);
            _messageServices.SendListWidth(ListWidth);
            _messageServices.SendLanguage(SelectedLanguage);
            _messageServices.SendHashAlogrithm(HashAlgorithm);
            _messageServices.SendReadOnlyFileInclude(IsReadOnlyFileInclude);
            _messageServices.SendHiddenFileInclude(IsHiddenFileInclude);
            _messageServices.SendZeroSizeFileDelete(IsZeroSizeFileDelete);
            _messageServices.SendEmptyDirectoryDelete(IsEmptyDirectoryDelete);
            _messageServices.SendCurrentFont(CurrentFont);
            _messageServices.SendFontSize(FontSize);

            WeakReferenceMessenger.Default.Register<WindowTopChanged>(this, (_, m) => Top = m.Top);
            WeakReferenceMessenger.Default.Register<WindowLeftChanged>(this, (_, m) => Left = m.Left);
            WeakReferenceMessenger.Default.Register<WindowWidthChanged>(this, (_, m) => Width = m.Width);
            WeakReferenceMessenger.Default.Register<WindowHeightChanged>(this, (_, m) => Height = m.Height);

            WeakReferenceMessenger.Default.Register<TreeWidthChanged>(this, (_, m) => TreeWidth = m.TreeWidth);
            WeakReferenceMessenger.Default.Register<ListWidthChanged>(this, (_, m) => ListWidth = m.ListWidth);

            WeakReferenceMessenger.Default.Register<SelectedLanguageChanged>(this, (_, m) => SelectedLanguage = m.SelectedLanguage);
            WeakReferenceMessenger.Default.Register<HashAlgorithmChanged>(this, (_, m) => HashAlgorithm = m.HashAlgorithm);

            WeakReferenceMessenger.Default.Register<ReadOnlyFileIncludeChanged>(this, (_, m) => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);
            WeakReferenceMessenger.Default.Register<HiddenFileIncludeChanged>(this, (_, m) => IsHiddenFileInclude = m.HiddenFileInclude);
            WeakReferenceMessenger.Default.Register<ZeroSizeFileDeleteChanged>(this, (_, m) => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);
            WeakReferenceMessenger.Default.Register<EmptyDirectoryDeleteChanged>(this, (_, m) => IsEmptyDirectoryDelete = m.EmptyDirectoryDelete);

            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m) => CurrentFont = m.CurrentFontFamily);
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m) => FontSize = m.FontSize);
        }
        #endregion コンストラクタ

        #region プロパティ
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        private double _Top = 100d;
        public double Top
        {
            get => _Top;
            set
            {
                if (_Top == value) { return; }
                _Top = value;
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
                if (_Left == value) { return; }
                _Left = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 1500d;
        public double Width
        {
            get => _Width;
            set
            {
                if (_Width == value) { return; }
                _Width = value;
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
                if (_Height == value) { return; }
                _Height = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// ツリービューの幅
        /// </summary>
        private double _TreeWidth = 300d;
        public double TreeWidth
        {
            get => _TreeWidth;
            set
            {
                if (value == _TreeWidth) { return; }
                _TreeWidth = value;
                _messageServices.SendTreeWidth(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// リストボックスの幅
        /// </summary>
        private double _ListWidth = 300d;
        public double ListWidth
        {
            get => _ListWidth;
            set
            {
                if (value == _ListWidth) { return; }
                _ListWidth = value;
                _messageServices.SendListWidth(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// 選択されている言語
        /// </summary>
        private string _SelectedLanguage = "ja-JP";
        public string SelectedLanguage
        {
            get => _SelectedLanguage;
            set
            {
                if (value == _SelectedLanguage) { return; }
                _SelectedLanguage = value;
                ResourceService.Current.ChangeCulture(value);
                OnPropertyChanged("Resources");
                _messageServices.SendLanguage(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        private string _HashAlgorithm = HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256);
        public string HashAlgorithm
        {
            get => _HashAlgorithm;
            set
            {
                if (value == _HashAlgorithm) { return; }
                _HashAlgorithm = value;
                _messageServices.SendHashAlogrithm(value);
                SaveSettings();
            }
        }
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        private bool _IsReadOnlyFileInclude = false;
        public bool IsReadOnlyFileInclude
        {
            get => _IsReadOnlyFileInclude;
            set
            {
                if (value == _IsReadOnlyFileInclude) return;
                _IsReadOnlyFileInclude = value;
                _messageServices.SendReadOnlyFileInclude(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        private bool _IsHiddenFileInclude = false;
        public bool IsHiddenFileInclude
        {
            get => _IsHiddenFileInclude;
            set
            {
                if (value == _IsHiddenFileInclude) return;
                _IsHiddenFileInclude = value;
                _messageServices.SendHiddenFileInclude(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// 0 サイズのファイルを削除するかどうか
        /// </summary>
        private bool _IsZeroSizeFileDelete = false;
        public bool IsZeroSizeFileDelete
        {
            get => _IsZeroSizeFileDelete;
            set
            {
                if (value == _IsZeroSizeFileDelete) return;
                _IsZeroSizeFileDelete = value;
                _messageServices.SendZeroSizeFileDelete(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        private bool _IsEmptyDirectoryDelete = false;
        public bool IsEmptyDirectoryDelete
        {
            get => _IsEmptyDirectoryDelete;
            set
            {
                if (value == _IsEmptyDirectoryDelete) return;
                _IsEmptyDirectoryDelete = value;
                _messageServices.SendEmptyDirectoryDelete(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        private FontFamily _CurrentFont = SystemFonts.MessageFontFamily;
        public FontFamily CurrentFont
        {
            get => _CurrentFont;
            set
            {
                if (value.Source == _CurrentFont.Source) { return; }
                _CurrentFont = value;
                _messageServices.SendCurrentFont(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// フォントのサイズ
        /// </summary>
        private double _FontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) return;
                _FontSize = value;
                _messageServices.SendFontSize(value);
                SaveSettings();
            }
        }

        
        #endregion プロパティ

        #region フォントサイズの変更メソッド
        /// <summary>
        /// 設定できるフォントサイズ
        /// </summary>
        private readonly List<double> FontSizes =
            [8d, 9d, 10d, 10.5d, 11d, 12d, 13d, 14d, 15d, 16d, 18d, 20d, 21d, 22d, 24d];

        /// <summary>
        /// フォントサイズを取得します。
        /// </summary>
        /// <returns>設定できるフォントサイズリスト</returns>
        public IEnumerable<double> GetSelectableFontSize()
        {
            foreach (var fontSize in FontSizes) { yield return fontSize; }
        }

        /// <summary>
        /// フォントサイズを大きくします。
        /// </summary>
        public void FontSizePlus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == FontSizes.Count - 1) return;

            FontSize = FontSizes[index + 1];
        }
        /// <summary>
        /// フォントサイズを小さくします。
        /// </summary>
        public void FontSizeMinus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == 0) return;

            FontSize = FontSizes[index - 1];
        }
        #endregion フォントサイズの変更メソッド

        #region 設定ファイルの読み書き
        /// <summary>
        /// 設定ファイルからロード
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
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
                        TreeWidth = Convert.ToDouble(root.Element("TreeWidth")?.Value);
                        ListWidth = Convert.ToDouble(root.Element("ListWidth")?.Value);
                        IsReadOnlyFileInclude = Convert.ToBoolean(root.Element("IsReadOnlyFileInclude")?.Value);
                        IsHiddenFileInclude = Convert.ToBoolean(root.Element("IsHiddenFileInclude")?.Value);
                        IsZeroSizeFileDelete = Convert.ToBoolean(root.Element("IsZeroSizeFileDelete")?.Value);
                        IsEmptyDirectoryDelete = Convert.ToBoolean(root.Element("IsEmptyDirectoryDelete")?.Value);
                        SelectedLanguage = root.Element("SelectedLanguage")?.Value ?? "ja-JP";
                        HashAlgorithm = root.Element("HashAlgorithm")?.Value ?? HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256);

                        var fontFamilyName = root.Element("CurrentFont")?.Value ?? string.Empty;
                        CurrentFont = new FontFamilyConverter().ConvertFromString(fontFamilyName) as FontFamily ?? SystemFonts.MessageFontFamily;
                        FontSize = Convert.ToDouble(root.Element("FontSize")?.Value);
                    }
                }
                // XMLが正当ではないときはデフォルトの値を使う
                catch (XmlException) { }
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
                        new XElement("TreeWidth", TreeWidth),
                        new XElement("ListWidth", ListWidth),
                        new XElement("IsReadOnlyFileInclude", IsReadOnlyFileInclude),
                        new XElement("IsHiddenFileInclude", IsHiddenFileInclude),
                        new XElement("IsZeroSizeFileDelete", IsZeroSizeFileDelete),
                        new XElement("IsEmptyDirectoryDelete", IsEmptyDirectoryDelete),
                        new XElement("SelectedLanguage", SelectedLanguage),
                        new XElement("HashAlgorithm", HashAlgorithm),
                        new XElement("CurrentFont", CurrentFont),
                        new XElement("FontSize", FontSize)
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
                DebugManager.ExceptionWrite($"設定の保存中にエラーが発生しました: {ex.Message}");
            }
        }
        #endregion 設定ファイルの読み書き
    }
}
