using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Services
{
    #region インターフェース
    public interface ISettingsService
    {
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        double Top { get; }
        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        double Left { get; }
        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        double Width { get; }
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        double Height { get; }
        /// <summary>
        /// ツリービューの幅
        /// </summary>
        double TreeWidth { get; }
        /// <summary>
        /// リストボックスの幅
        /// </summary>
        double ListWidth { get; }
        /// <summary>
        /// 選択されている言語
        /// </summary>
        string SelectedLanguage { get; }
        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        string HashAlgorithm { get; }
        /// <summary>
        ///  読み取り専用ファイルを対象にするかどうか
        /// </summary>
        bool IsReadOnlyFileInclude { get; }
        /// <summary>
        /// 隠しファイルを対象にするかどうか
        /// </summary>
        bool IsHiddenFileInclude { get; }
        /// <summary>
        /// 0 サイズのファイルを削除するかどうか
        /// </summary>
        bool IsZeroSizeFileDelete { get; }
        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        bool IsEmptyDirectoryDelete { get; }
        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        FontFamily CurrentFont { get; }
        /// <summary>
        /// フォントのサイズ
        /// </summary>
        double FontSize { get; }
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
        /// ウィンドウの上位置を設定します。
        /// </summary>
        void SendWindowTop(double top);
        /// <summary>
        /// ウィンドウの左位置を設定します。
        /// </summary>
        void SendWindowLeft(double left);
        /// <summary>
        /// ウィンドウの幅を設定します。
        /// </summary>
        void SendWindowWidth(double width);
        /// <summary>
        /// ウィンドウの高さを設定します。
        /// </summary>
        void SendWindowHeight(double height);
        /// <summary>
        /// ツリービューの幅を設定します。
        /// </summary>
        void SendTreeWidth(double width);
        /// <summary>
        /// リストボックスの幅を設定します。
        /// </summary>
        void SendListWidth(double width);
        /// <summary>
        /// 利用言語を設定します。
        /// </summary>
        void SendLanguage(string language);
        /// <summary>
        /// ファイルのハッシュアルゴリズムを設定します。
        /// </summary>
        void SendHashAlogrithm(string hashAlogrithm);
        /// <summary>
        /// 削除対象に読み取り専用ファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="readOnlyFileInclude">読み取り専用ファイルを含むかどうか</param>
        void SendReadOnlyFileInclude(bool readOnlyFileInclude);
        /// <summary>
        /// 削除対象に隠しファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="hiddenFileInclude">隠しファイルを含むかどうか</param>
        void SendHiddenFileInclude(bool hiddenFileInclude);
        /// <summary>
        /// 削除対象に0サイズのファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="zeroSizeFileDelete">0サイズファイルを含むかどうか</param>
        void SendZeroSizeFileDelete(bool zeroSizeFileDelete);
        /// <summary>
        /// 削除対象に空のディレクトリを含むかどうかを設定します。
        /// </summary>
        /// <param name="emptyDirectoryDelete">空のディレクトリを含むかどうか</param>
        void SendEmptyDirectoryDelete(bool emptyDirectoryDelete);
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

    public class SettingsService :ObservableObject, ISettingsService
    {
        #region コンストラクタ
        private readonly string appName = "FileHashCraft";
        private readonly string settingXMLFile = "settings.xml";
        private readonly string settingsFilePath;
        private readonly IMessenger _messenger;

        public SettingsService(IMessenger messenger)
        {
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            settingsFilePath = Path.Combine(localAppDataPath, settingXMLFile);
            _messenger = messenger;

            LoadSettings();

            SendWindowTop(Top);
            SendWindowLeft(Left);
            SendWindowWidth(Width);
            SendWindowHeight(Height);
            SendTreeWidth(TreeWidth);
            SendListWidth(ListWidth);
            SendLanguage(SelectedLanguage);
            SendHashAlogrithm(HashAlgorithm);
            SendReadOnlyFileInclude(IsReadOnlyFileInclude);
            SendHiddenFileInclude(IsHiddenFileInclude);
            SendZeroSizeFileDelete(IsZeroSizeFileDelete);
            SendEmptyDirectoryDelete(IsEmptyDirectoryDelete);
            SendCurrentFont(CurrentFont);
            SendFontSize(FontSize);

            _messenger.Register<WindowTopChangedMessage>(this, (_, m) => Top = m.Top);
            _messenger.Register<WindowLeftChangedMessage>(this, (_, m) => Left = m.Left);
            _messenger.Register<WindowWidthChangedMessage>(this, (_, m) => Width = m.Width);
            _messenger.Register<WindowHeightChangedMessage>(this, (_, m) => Height = m.Height);

            _messenger.Register<TreeWidthChangedMessage>(this, (_, m) => TreeWidth = m.TreeWidth);
            _messenger.Register<ListWidthChangedMessage>(this, (_, m) => ListWidth = m.ListWidth);

            _messenger.Register<SelectedLanguageChangedMessage>(this, (_, m) => SelectedLanguage = m.SelectedLanguage);
            _messenger.Register<HashAlgorithmChangedMessage>(this, (_, m) => HashAlgorithm = m.HashAlgorithm);

            _messenger.Register<ReadOnlyFileIncludeChangedMessage>(this, (_, m) => IsReadOnlyFileInclude = m.ReadOnlyFileInclude);
            _messenger.Register<HiddenFileIncludeChangedMessage>(this, (_, m) => IsHiddenFileInclude = m.HiddenFileInclude);
            _messenger.Register<ZeroSizeFileDeleteChangedMessage>(this, (_, m) => IsZeroSizeFileDelete = m.ZeroSizeFileDelete);
            _messenger.Register<EmptyDirectoryDeleteChangedMessage>(this, (_, m) => IsEmptyDirectoryDelete = m.EmptyDirectoryDelete);

            _messenger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFont = m.CurrentFontFamily);
            _messenger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
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
                SendTreeWidth(value);
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
                SendListWidth(value);
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
                SendLanguage(value);
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
                SendHashAlogrithm(value);
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
                SendReadOnlyFileInclude(value);
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
                SendHiddenFileInclude(value);
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
                SendZeroSizeFileDelete(value);
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
                SendEmptyDirectoryDelete(value);
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
                SendCurrentFont(value);
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

        #region ウィンドウ設定
        /// <summary>
        /// ウィンドウの上位置を設定します。
        /// </summary>
        /// <param name="top">ウィンドウの上位置</param>
        public void SendWindowTop(double top)
        {
            _messenger.Send(new WindowTopChangedMessage(top));
        }
        /// <summary>
        /// ウィンドウの左位置を設定します。
        /// </summary>
        /// <param name="left">ウィンドウの左位置</param>
        public void SendWindowLeft(double left)
        {
            _messenger.Send(new WindowLeftChangedMessage(left));
        }
        /// <summary>
        /// ウィンドウの幅を設定します。
        /// </summary>
        /// <param name="width">ウィンドウの幅</param>
        public void SendWindowWidth(double width)
        {
            _messenger.Send(new WindowWidthChangedMessage(width));
        }
        /// <summary>
        /// ウィンドウの高さを設定します。
        /// </summary>
        /// <param name="height">ウィンドウの高さ</param>
        public void SendWindowHeight(double height)
        {
            _messenger.Send(new WindowHeightChangedMessage(height));
        }
        /// <summary>
        /// ツリービューの幅を設定します。
        /// </summary>
        /// <param name="width">ツリービューの幅</param>
        public void SendTreeWidth(double width)
        {
            _messenger.Send(new TreeWidthChangedMessage(width));
        }
        /// <summary>
        /// リストボックスの幅を設定します。
        /// </summary>
        /// <param name="width">リストボックスの幅</param>
        public void SendListWidth(double width)
        {
            _messenger.Send(new ListWidthChangedMessage(width));
        }
        /// <summary>
        /// 利用言語を設定します。
        /// </summary>
        /// <param name="language"></param>
        public void SendLanguage(string language)
        {
            _messenger.Send(new SelectedLanguageChangedMessage(language));
        }
        /// <summary>
        /// ファイルのハッシュアルゴリズムを設定します。
        /// </summary>
        /// <param name="hashAlogrithm">ファイルのハッシュアルゴリズム</param>
        public void SendHashAlogrithm(string hashAlogrithm)
        {
            _messenger.Send(new HashAlgorithmChangedMessage(hashAlogrithm));
        }
        /// <summary>
        /// 削除対象に読み取り専用ファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="readOnlyFileInclude">読み取り専用ファイルを含むかどうか</param>
        public void SendReadOnlyFileInclude(bool readOnlyFileInclude)
        {
            _messenger.Send(new ReadOnlyFileIncludeChangedMessage(readOnlyFileInclude));
        }
        /// <summary>
        /// 削除対象に隠しファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="hiddenFileInclude">隠しファイルを含むかどうか</param>
        public void SendHiddenFileInclude(bool hiddenFileInclude)
        {
            _messenger.Send(new HiddenFileIncludeChangedMessage(hiddenFileInclude));
        }
        /// <summary>
        /// 削除対象に0サイズのファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="zeroSizeFileDelete">0サイズファイルを含むかどうか</param>
        public void SendZeroSizeFileDelete(bool zeroSizeFileDelete)
        {
            _messenger.Send(new ZeroSizeFileDeleteChangedMessage(zeroSizeFileDelete));
        }
        /// <summary>
        /// 削除対象に空のディレクトリを含むかどうかを設定します。
        /// </summary>
        /// <param name="emptyDirectoryDelete">空のディレクトリを含むかどうか</param>
        public void SendEmptyDirectoryDelete(bool emptyDirectoryDelete)
        {
            _messenger.Send(new EmptyDirectoryDeleteChangedMessage(emptyDirectoryDelete));
        }

        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="currentFont">フォントファミリー</param>
        public void SendCurrentFont(FontFamily currentFont)
        {
            _messenger.Send(new CurrentFontFamilyChangedMessage(currentFont));
        }
        /// <summary>
        /// フォントサイズを設定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        public void SendFontSize(double fontSize)
        {
            _messenger.Send(new FontSizeChangedMessage(fontSize));
        }
        #endregion ウィンドウ設定
    }
}
