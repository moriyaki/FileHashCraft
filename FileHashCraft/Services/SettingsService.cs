using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Services
{
    #region インターフェース

    public interface ISettingsService
    {
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        double Top { get; set; }

        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        double Left { get; set; }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// ディレクトリツリービューの幅
        /// </summary>
        double DirectoriesTreeViewWidth { get; set; }

        /// <summary>
        /// ファイル一覧リストボックスの幅
        /// </summary>
        double FilesListBoxWidth { get; set; }

        /// <summary>
        /// 重複ファイルを含むフォルダのリストボックスの幅
        /// </summary>
        double DupFilesDirsListBoxWidth { get; set; }

        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        double DupDirsFilesTreeViewWidth { get; set; }

        /// <summary>
        /// 選択されている言語
        /// </summary>
        string SelectedLanguage { get; set; }

        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        string HashAlgorithm { get; set; }

        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        bool IsReadOnlyFileInclude { get; set; }

        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        bool IsHiddenFileInclude { get; set; }

        /// <summary>
        /// 0 サイズのファイルを削除するかどうか
        /// </summary>
        bool IsZeroSizeFileDelete { get; set; }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        bool IsEmptyDirectoryDelete { get; set; }

        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        FontFamily CurrentFont { get; set; }

        /// <summary>
        /// フォントのサイズ
        /// </summary>
        double FontSize { get; set; }

        /// <summary>
        /// 設定できるフォントサイズのコレクションを取得します。
        /// </summary>
        IEnumerable<double> GetSelectableFontSize();

        /// <summary>
        /// フォントサイズを大きくします。
        /// </summary>
        void FontSizePlus();

        /// <summary>
        /// フォントサイズを小さくします。
        /// </summary>
        void FontSizeMinus();

        /// <summary>
        /// 設定を読み込みます。
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// フォントを設定します
        /// </summary>
        void SendCurrentFont(FontFamily font);

        /// <summary>
        /// フォントサイズを設定します。
        /// </summary>
        void SendFontSize(double fontSize);
    }

    #endregion インターフェース

    public class SettingsService : ObservableObject, ISettingsService
    {
        #region コンストラクタ

        private readonly string appName = "FileHashCraft";
        private readonly string settingXMLFile = "settings.xml";
        private readonly string settingsFilePath;
        private readonly IMessenger _Messanger;
        private readonly IHashAlgorithmHelper _HashAlgorithmHelper;

        public SettingsService(
            IMessenger messenger,
            IHashAlgorithmHelper hashAlgorithmHelper
        )
        {
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            settingsFilePath = Path.Combine(localAppDataPath, settingXMLFile);
            _Messanger = messenger;
            _HashAlgorithmHelper = hashAlgorithmHelper;

            _HashAlgorithm = _HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256);
            LoadSettings();

            SendCurrentFont(CurrentFont);
            SendFontSize(FontSize);

            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFont = m.CurrentFontFamily);
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
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
        private double _DirectoriesTreeViewWidth = 300d;

        public double DirectoriesTreeViewWidth
        {
            get => _DirectoriesTreeViewWidth;
            set
            {
                if (value == _DirectoriesTreeViewWidth) { return; }
                _DirectoriesTreeViewWidth = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// ファイル一覧リストボックスの幅
        /// </summary>
        private double _FilesListBoxWidth = 300d;

        public double FilesListBoxWidth
        {
            get => _FilesListBoxWidth;
            set
            {
                if (value == _FilesListBoxWidth) { return; }
                _FilesListBoxWidth = value;
                SaveSettings();
            }
        }

        /// <summary>
        /// ファイル一覧リストボックスの幅
        /// </summary>
        private double _DupFilesDirsListBoxWidth = 400d;

        public double DupFilesDirsListBoxWidth
        {
            get => _DupFilesDirsListBoxWidth;
            set
            {
                if (value == _DupFilesDirsListBoxWidth) { return; }
                _DupFilesDirsListBoxWidth = value;
                SaveSettings();
            }
        }

        private double _DupDirsFilesTreeViewWidth = 500d;

        public double DupDirsFilesTreeViewWidth
        {
            get => _DupDirsFilesTreeViewWidth;
            set
            {
                if (value == _DupDirsFilesTreeViewWidth) { return; }
                _DupDirsFilesTreeViewWidth = value;
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
                if (value == _HashAlgorithm) { return; }
                _HashAlgorithm = value;
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
                SaveSettings();
            }
        }

        /// <summary>
        /// フォントのサイズ
        /// </summary>
        private double _FontSize = SystemFonts.MessageFontSize;

        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) return;
                _FontSize = value;
                SendFontSize(value);
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
                        DirectoriesTreeViewWidth = Convert.ToDouble(root.Element("DirectoriesTreeViewWidth")?.Value);
                        DupFilesDirsListBoxWidth = Convert.ToDouble(root.Element("DupFilesDirsListBoxWidth")?.Value);
                        DupDirsFilesTreeViewWidth = Convert.ToDouble(root.Element("DupDirsFilesTreeViewWidth")?.Value);
                        FilesListBoxWidth = Convert.ToDouble(root.Element("FilesListViewWidth")?.Value);
                        IsReadOnlyFileInclude = Convert.ToBoolean(root.Element("IsReadOnlyFileInclude")?.Value);
                        IsHiddenFileInclude = Convert.ToBoolean(root.Element("IsHiddenFileInclude")?.Value);
                        IsZeroSizeFileDelete = Convert.ToBoolean(root.Element("IsZeroSizeFileDelete")?.Value);
                        IsEmptyDirectoryDelete = Convert.ToBoolean(root.Element("IsEmptyDirectoryDelete")?.Value);
                        SelectedLanguage = root.Element("SelectedLanguage")?.Value ?? "ja-JP";
                        HashAlgorithm = root.Element("HashAlgorithm")?.Value ?? _HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256);

                        var fontFamilyName = root.Element("CurrentFont")?.Value ?? string.Empty;
                        CurrentFont = new FontFamilyConverter().ConvertFromString(fontFamilyName) as FontFamily ?? SystemFonts.MessageFontFamily;
                        FontSize = Convert.ToDouble(root.Element("FontSize")?.Value);
                    }
                }
                // XMLが正当ではないときはデフォルトの値を使う
                catch (XmlException) { }
            }
            else
            {
                SaveSettings();
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
                        new XElement("DirectoriesTreeViewWidth", DirectoriesTreeViewWidth),
                        new XElement("FilesListViewWidth", FilesListBoxWidth),
                        new XElement("DupDirsFilesTreeViewWidth", DupDirsFilesTreeViewWidth),
                        new XElement("DupFilesDirsListBoxWidth", DupFilesDirsListBoxWidth),
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

        #region ウィンドウ設定

        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="currentFont">フォントファミリー</param>
        public void SendCurrentFont(FontFamily currentFont)
        {
            _Messanger.Send(new CurrentFontFamilyChangedMessage(currentFont));
        }

        /// <summary>
        /// フォントサイズを設定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        public void SendFontSize(double fontSize)
        {
            _Messanger.Send(new FontSizeChangedMessage(fontSize));
        }

        #endregion ウィンドウ設定
    }
}