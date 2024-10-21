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
    public class WindowTopChangedMessage
    {
        public double Top { get; }
        public WindowTopChangedMessage() { throw new NotImplementedException(nameof(WindowTopChangedMessage)); }
        public WindowTopChangedMessage(double top) => Top = top;
    }
    /// <summary>
    /// ウィンドウの左位置を変更するメッセージ
    /// </summary>
    public class WindowLeftChangedMessage
    {
        public double Left { get; }
        public WindowLeftChangedMessage() { throw new NotImplementedException(nameof(WindowLeftChangedMessage)); }
        public WindowLeftChangedMessage(double left) => Left = left;
    }
    /// <summary>
    /// ウィンドウの幅を変更するメッセージ
    public class WindowWidthChangedMessage
    {
        public double Width { get; }
        public WindowWidthChangedMessage() { throw new NotImplementedException(nameof(WindowWidthChangedMessage)); }
        public WindowWidthChangedMessage(double width) => Width = width;
    }
    /// <summary>
    /// ウィンドウの高さを変更するメッセージ
    /// </summary>
    public class WindowHeightChangedMessage
    {
        public double Height { get; }
        public WindowHeightChangedMessage() { throw new NotImplementedException(nameof(WindowHeightChangedMessage)); }
        public WindowHeightChangedMessage(double height) => Height = height;
    }
    /// <summary>
    /// ディレクトリツリービュー幅の変更メッセージ
    /// </summary>
    public class DirectoriesTreeViewWidthChangedMessage
    {
        public double DirectoriesTreeViewWidth { get; }

        public DirectoriesTreeViewWidthChangedMessage() { throw new NotImplementedException(nameof(DirectoriesTreeViewWidthChangedMessage)); }

        public DirectoriesTreeViewWidthChangedMessage(double treeViewWidth) => DirectoriesTreeViewWidth = treeViewWidth;
    }
    /// <summary>
    /// ファイル一覧リストボックス幅の変更メッセージ
    /// </summary>
    public class FilesListBoxWidthChangedMessage
    {
        public double FilesListBoxWidth { get; }

        public FilesListBoxWidthChangedMessage() { throw new NotImplementedException(nameof(FilesListBoxWidthChangedMessage)); }

        public FilesListBoxWidthChangedMessage(double filesListBoxWidth) => FilesListBoxWidth = filesListBoxWidth;
    }

    /// <summary>
    /// ファイル削除画面のディレクトリ一覧リストボックス幅の変更メッセージ
    /// </summary>
    public class DupFilesDirsListBoxWidthMessage
    {
        public double DupFilesDirsListBoxWidth { get; }
        public DupFilesDirsListBoxWidthMessage() { throw new NotImplementedException(nameof(DupFilesDirsListBoxWidthMessage)); }
        public DupFilesDirsListBoxWidthMessage(double dupFilesDirsListBoxWidth) => DupFilesDirsListBoxWidth = dupFilesDirsListBoxWidth;
    }

    /// <summary>
    /// ファイル削除画面の選択されたディレクトリ一覧ツリービュー幅の変更メッセージ
    /// </summary>
    public class DupDirsFilesTreeViewWidthMessage
    {
        public double DupDirsFilesTreeViewWidth { get; }
        public DupDirsFilesTreeViewWidthMessage() { throw new NotImplementedException(nameof(DupDirsFilesTreeViewWidthMessage)); }
        public DupDirsFilesTreeViewWidthMessage(double dupDirsFilesTreeViewWidth) => DupDirsFilesTreeViewWidth = dupDirsFilesTreeViewWidth;
    }

    /// <summary>
    /// 選択されている言語変更のメッセージ
    /// </summary>
    public class SelectedLanguageChangedMessage
    {
        public string SelectedLanguage { get; }
        public SelectedLanguageChangedMessage() { throw new NotImplementedException(nameof(SelectedLanguageChangedMessage)); }
        public SelectedLanguageChangedMessage(string selectedLanguage) => SelectedLanguage = selectedLanguage;
    }

    /// <summary>
    /// ハッシュ計算アルゴリズムの変更メッセージ
    /// </summary>
    public class HashAlgorithmChangedMessage
    {
        public string HashAlgorithm { get; }
        public HashAlgorithmChangedMessage() { throw new NotImplementedException(nameof(HashAlgorithmChangedMessage)); }
        public HashAlgorithmChangedMessage(string hashAlgorithm) => HashAlgorithm = hashAlgorithm;
    }
    /// <summary>
    /// 読み取り専用ファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class ReadOnlyFileIncludeChangedMessage
    {
        public bool ReadOnlyFileInclude { get; }
        public ReadOnlyFileIncludeChangedMessage() { throw new NotImplementedException(nameof(ReadOnlyFileIncludeChangedMessage)); }
        public ReadOnlyFileIncludeChangedMessage(bool readOnlyFileInclude) => ReadOnlyFileInclude = readOnlyFileInclude;
    }
    /// <summary>
    /// 隠しファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class HiddenFileIncludeChangedMessage
    {
        public bool HiddenFileInclude { get; }
        public HiddenFileIncludeChangedMessage() { throw new NotImplementedException(nameof(HiddenFileIncludeChangedMessage)); }
        public HiddenFileIncludeChangedMessage(bool hiddenFileInclude) => HiddenFileInclude = hiddenFileInclude;
    }

    /// <summary>
    /// 0サイズのファイルを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class ZeroSizeFileDeleteChangedMessage
    {
        public bool ZeroSizeFileDelete { get; }
        public ZeroSizeFileDeleteChangedMessage() { throw new NotImplementedException(nameof(ZeroSizeFileDeleteChangedMessage)); }
        public ZeroSizeFileDeleteChangedMessage(bool zeroSizeFileDelete) => ZeroSizeFileDelete = zeroSizeFileDelete;
    }

    /// <summary>
    /// 空のディレクトリを削除対象に含むかどうかの変更メッセージ
    /// </summary>
    public class EmptyDirectoryDeleteChangedMessage
    {
        public bool EmptyDirectoryDelete { get; }
        public EmptyDirectoryDeleteChangedMessage() { throw new NotImplementedException(nameof(EmptyDirectoryDeleteChangedMessage)); }
        public EmptyDirectoryDeleteChangedMessage(bool emptyDirectoryDelete) => EmptyDirectoryDelete = emptyDirectoryDelete;
    }

    /// <summary>
    /// フォントの変更メッセージ
    /// </summary>
    public class CurrentFontFamilyChangedMessage
    {
        public FontFamily CurrentFontFamily { get; }
        public CurrentFontFamilyChangedMessage() { throw new NotImplementedException(nameof(CurrentFontFamilyChangedMessage)); }
        public CurrentFontFamilyChangedMessage(FontFamily currentFontFamily) => CurrentFontFamily = currentFontFamily;
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontSizeChangedMessage
    {
        public double FontSize { get; }
        public FontSizeChangedMessage() { throw new NotImplementedException(nameof(FontSizeChangedMessage)); }
        public FontSizeChangedMessage(double fontSize) => FontSize = fontSize;
    }
    #endregion MainView用メッセージ

    #region カレントディレクトリの移動
    /// <summary>
    /// カレントディレクトリの移動メッセージ
    /// </summary>
    public class CurrentDirectoryChangedMessage
    {
        public string CurrentFullPath { get; } = string.Empty;
        public CurrentDirectoryChangedMessage() { throw new NotImplementedException(nameof(CurrentDirectoryChangedMessage)); }
        public CurrentDirectoryChangedMessage(string currentFullPath) => CurrentFullPath = currentFullPath;
    }
    #endregion カレントディレクトリの移動

    #region ページ移動用メッセージ
    /// <summary>
    /// エクスプローラー風画面ページに移動するメッセージ
    /// </summary>
    public class ToExplorerPageMessage;

    /// <summary>
    /// ハッシュ計算対象選択ページに移動するメッセージ
    /// </summary>
    public class ToPageSelectTargetMessage;
    /// <summary>
    /// ハッシュ計算画面ページに移動するメッセージ
    /// </summary>
    public class ToHashCalcingPageMessage;
    /// <summary>
    /// 同一ファイル画面選択ページに移動するメッセージ
    /// </summary>
    public class ToDuplicateSelectPage;

    /// <summary>
    /// どこに戻るかの列挙子
    /// </summary>
    public enum ReturnPageEnum
    {
        ExplorerPage,
        SettingsPage,
        SelecTargettPage,
        HashCalcingPage,
    }

    /// <summary>
    /// 戻るページを指定して設定画面に移動するメッセージ
    /// </summary>
    public class ToSettingPageMessage
    {
        public ReturnPageEnum ReturnPage { get; }
        public ToSettingPageMessage() { throw new NotImplementedException(nameof(ToSettingPageMessage)); }
        public ToSettingPageMessage(ReturnPageEnum returnPage) => ReturnPage = returnPage;
    }

    /// <summary>
    /// 元のページへの移動メッセージ
    /// </summary>
    public class ReturnPageFromSettingsMessage;

    #endregion ページ移動用メッセージ
}
