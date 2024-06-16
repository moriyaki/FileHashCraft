/*  MainWindowMessages.cs

    メインウィンドウに関わるメッセージングに利用するメッセージを配置しています。
 */
using System.Windows.Media;

namespace FileHashCraft.Services.Messages
{
    #region MainView用メッセージ
    /// <summary>
    /// ウィンドウの上位置を変更するメッセージ
    /// </summary>
    public class WindowTopChanged
    {
        public double Top { get; }
        public WindowTopChanged() { throw new NotImplementedException(nameof(WindowTopChanged)); }
        public WindowTopChanged(double top) => Top = top;
    }
    /// <summary>
    /// ウィンドウの左位置を変更するメッセージ
    /// </summary>
    public class WindowLeftChanged
    {
        public double Left { get; }
        public WindowLeftChanged() { throw new NotImplementedException(nameof(WindowLeftChanged)); }
        public WindowLeftChanged(double left) => Left = left;
    }
    /// <summary>
    /// ウィンドウの幅を変更するメッセージ
    public class WindowWidthChanged
    {
        public double Width { get; }
        public WindowWidthChanged() { throw new NotImplementedException(nameof(WindowWidthChanged)); }
        public WindowWidthChanged(double width) => Width = width;
    }
    /// <summary>
    /// ウィンドウの高さを変更するメッセージ
    /// </summary>
    public class WindowHeightChanged
    {
        public double Height { get; }
        public WindowHeightChanged() { throw new NotImplementedException(nameof(WindowHeightChanged)); }
        public WindowHeightChanged(double height) => Height = height;
    }
    /// <summary>
    /// ツリービュー幅の変更メッセージ
    /// </summary>
    public class TreeWidthChanged
    {
        public double TreeWidth { get; }

        public TreeWidthChanged() { throw new NotImplementedException(nameof(TreeWidthChanged)); }

        public TreeWidthChanged(double treeWidth) => TreeWidth = treeWidth;
    }
    /// <summary>
    /// リストボックス幅の変更メッセージ
    /// </summary>
    public class ListWidthChanged
    {
        public double ListWidth { get; }

        public ListWidthChanged() { throw new NotImplementedException(nameof(ListWidthChanged)); }

        public ListWidthChanged(double listWidth) => ListWidth = listWidth;
    }
    /// <summary>
    /// 選択されている言語変更のメッセージ
    /// </summary>
    public class SelectedLanguageChanged
    {
        public string SelectedLanguage { get; }
        public SelectedLanguageChanged() { throw new NotImplementedException(nameof(SelectedLanguageChanged)); }
        public SelectedLanguageChanged(string selectedLanguage) => SelectedLanguage = selectedLanguage;
    }

    /// <summary>
    /// ハッシュ計算アルゴリズムの変更メッセージ
    /// </summary>
    public class HashAlgorithmChanged
    {
        public string HashAlgorithm { get; }
        public HashAlgorithmChanged() { throw new NotImplementedException(nameof(HashAlgorithmChanged)); }
        public HashAlgorithmChanged(string hashAlgorithm) => HashAlgorithm = hashAlgorithm;
    }
    /// <summary>
    /// 読み取り専用ファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class ReadOnlyFileIncludeChanged
    {
        public bool ReadOnlyFileInclude { get; }
        public ReadOnlyFileIncludeChanged() { throw new NotImplementedException(nameof(ReadOnlyFileIncludeChanged)); }
        public ReadOnlyFileIncludeChanged(bool readOnlyFileInclude) => ReadOnlyFileInclude = readOnlyFileInclude;
    }
    /// <summary>
    /// 隠しファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class HiddenFileIncludeChanged
    {
        public bool HiddenFileInclude { get; }
        public HiddenFileIncludeChanged() { throw new NotImplementedException(nameof(HiddenFileIncludeChanged)); }
        public HiddenFileIncludeChanged(bool hiddenFileInclude) => HiddenFileInclude = hiddenFileInclude;
    }

    /// <summary>
    /// 0サイズのファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class ZeroSizeFileDeleteChanged
    {
        public bool ZeroSizeFileDelete { get; }
        public ZeroSizeFileDeleteChanged() { throw new NotImplementedException(nameof(ZeroSizeFileDeleteChanged)); }
        public ZeroSizeFileDeleteChanged(bool zeroSizeFileDelete) => ZeroSizeFileDelete = zeroSizeFileDelete;
    }

    /// <summary>
    /// 空のディレクトリを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class EmptyDirectoryDeleteChanged
    {
        public bool EmptyDirectoryDelete { get; }
        public EmptyDirectoryDeleteChanged() { throw new NotImplementedException(nameof(EmptyDirectoryDeleteChanged)); }
        public EmptyDirectoryDeleteChanged(bool emptyDirectoryDelete) => EmptyDirectoryDelete = emptyDirectoryDelete;
    }

    /// <summary>
    /// フォントの変更メッセージ
    /// </summary>
    public class CurrentFontFamilyChanged
    {
        public FontFamily CurrentFontFamily { get; }
        public CurrentFontFamilyChanged() { throw new NotImplementedException(nameof(CurrentFontFamilyChanged)); }
        public CurrentFontFamilyChanged(FontFamily currentFontFamily) => CurrentFontFamily = currentFontFamily;
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontSizeChanged
    {
        public double FontSize { get; }
        public FontSizeChanged() { throw new NotImplementedException(nameof(FontSizeChanged)); }
        public FontSizeChanged(double fontSize) => FontSize = fontSize;
    }
    #endregion MainView用メッセージ

    #region カレントディレクトリの移動
    /// <summary>
    /// カレントディレクトリの移動メッセージ
    /// </summary>
    public class CurrentDirectoryChanged
    {
        public string CurrentFullPath { get; } = string.Empty;
        public CurrentDirectoryChanged() { throw new NotImplementedException(nameof(CurrentDirectoryChanged)); }
        public CurrentDirectoryChanged(string currentFullPath) => CurrentFullPath = currentFullPath;
    }
    #endregion カレントディレクトリの移動

    #region ページ移動用メッセージ
    /// <summary>
    /// エクスプローラー風画面ページに移動するメッセージ
    /// </summary>
    public class ToExplorerPage;

    /// <summary>
    /// ハッシュ計算対象選択ページに移動するメッセージ
    /// </summary>
    public class ToPageSelectTarget;
    /// <summary>
    /// ハッシュ計算画面ページに移動するメッセージ
    /// </summary>
    public class ToHashCalcingPage;

    /// <summary>
    /// どこに戻るかの列挙子
    /// </summary>
    public enum ReturnPageEnum
    {
        PageExplorer,
        PageSettings,
        PageTargetSelect,
        PageHashCalcing,
    }

    /// <summary>
    /// 戻るページを指定して設定画面に移動するメッセージ
    /// </summary>
    public class ToSettingPage
    {
        public ReturnPageEnum ReturnPage { get; }
        public ToSettingPage() { throw new NotImplementedException(nameof(ToSettingPage)); }
        public ToSettingPage(ReturnPageEnum returnPage) => ReturnPage = returnPage;
    }

    /// <summary>
    /// 元のページへの移動メッセージ
    /// </summary>
    public class ReturnPageFromSettings;

    #endregion ページ移動用メッセージ
}
