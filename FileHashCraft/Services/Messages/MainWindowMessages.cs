/*  MainWindowMessages.cs

    メインウィンドウに関わるメッセージングに利用するメッセージを配置しています。
 */

using System.Windows.Media;

namespace FileHashCraft.Services.Messages
{
    #region MainView用メッセージ

    /// <summary>
    /// フォントの変更メッセージ
    /// </summary>
    public class CurrentFontFamilyChangedMessage
    {
        public FontFamily CurrentFontFamily { get; }

        public CurrentFontFamilyChangedMessage()
        { throw new NotImplementedException(nameof(CurrentFontFamilyChangedMessage)); }

        public CurrentFontFamilyChangedMessage(FontFamily currentFontFamily) => CurrentFontFamily = currentFontFamily;
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontSizeChangedMessage
    {
        public double FontSize { get; }

        public FontSizeChangedMessage()
        { throw new NotImplementedException(nameof(FontSizeChangedMessage)); }

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

        public CurrentDirectoryChangedMessage()
        { throw new NotImplementedException(nameof(CurrentDirectoryChangedMessage)); }

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

        public ToSettingPageMessage()
        { throw new NotImplementedException(nameof(ToSettingPageMessage)); }

        public ToSettingPageMessage(ReturnPageEnum returnPage) => ReturnPage = returnPage;
    }

    /// <summary>
    /// 元のページへの移動メッセージ
    /// </summary>
    public class ReturnPageFromSettingsMessage;

    #endregion ページ移動用メッセージ
}