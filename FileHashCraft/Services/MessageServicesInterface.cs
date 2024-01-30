using System.Windows.Media;

namespace FileHashCraft.Services
{
    public interface IMessageServices
    {
        #region 重要設定
        /// <summary>
        /// ウィンドウの上位置を設定します。
        /// </summary>
        public void SendWindowTop(double top);
        /// <summary>
        /// ウィンドウの左位置を設定します。
        /// </summary>
        public void SendWindowLeft(double left);
        /// <summary>
        /// ウィンドウの幅を設定します。
        /// </summary>
        public void SendWindowWidth(double width);
        /// <summary>
        /// ウィンドウの高さを設定します。
        /// </summary>
        public void SendWindowHeight(double height);
        /// <summary>
        /// ツリービューの幅を設定します。
        /// </summary>
        public void SendTreeWidth(double width);
        /// <summary>
        /// リストボックスの幅を設定します。
        /// </summary>
        public void SendListWidth(double width);
        /// <summary>
        /// 利用言語を設定します。
        /// </summary>
        public void SendLanguage(string language);
        /// <summary>
        /// ファイルのハッシュアルゴリズムを設定します。
        /// </summary>
        public void SendHashAlogrithm(string hashAlogrithm);
        /// <summary>
        /// 削除対象に読み取り専用ファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="readOnlyFileInclude">読み取り専用ファイルを含むかどうか</param>
        public void SendReadOnlyFileInclude(bool readOnlyFileInclude);
        /// <summary>
        /// 削除対象に隠しファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="hiddenFileInclude">隠しファイルを含むかどうか</param>
        public void SendHiddenFileInclude(bool hiddenFileInclude);
        /// <summary>
        /// 削除対象に0サイズのファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="zeroSizeFileDelete">0サイズファイルを含むかどうか</param>
        public void SendZeroSizeFileDelete(bool zeroSizeFileDelete);
        /// <summary>
        /// 削除対象に空のディレクトリを含むかどうかを設定します。
        /// </summary>
        /// <param name="emptyDirectoryDelete">空のディレクトリを含むかどうか</param>
        public void SendEmptyDirectoryDelete(bool emptyDirectoryDelete);

        /// <summary>
        /// フォントを設定します
        /// </summary>
        public void SendCurrentFont(FontFamily font);
        /// <summary>
        /// フォントサイズを設定します。
        /// </summary>
        public void SendFontSize(double fontSize);
        #endregion 重要設定

        #region 移動処理
        /// <summary>
        /// カレントディレクトリを移動します。
        /// </summary>
        /// <param name="fullPath">移動先カレントディレクトリ</param>
        public void SendCurrentDirectoryChanged(string fullPath);

        /// <summary>
        /// 設定画面ページに移動します。
        /// </summary>
        /// <param name="pageEnum"></param>
        public void SendToSettingsPage(ReturnPageEnum pageEnum);
        /// <summary>
        /// 設定画面ページから元の画面に移動します。
        /// </summary>
        public void SendReturnPageFromSettings();
        /// <summary>
        /// エクスプローラー風画面に移動します。
        /// </summary>
        public void SendToExplorerPage();
        /// <summary>
        /// ハッシュ取得絞り込み画面に移動します。
        /// </summary>
        public void SendToSelectTargetPage();
        /// <summary>
        /// ハッシュ計算画面に移動に移動します。
        /// </summary>
        public void SendToHashCalcingPage();
        #endregion 移動処理

        #region ファイル監視
        /// <summary>
        /// カレントディレクトリにアイテムが追加された事をメッセージ送信します。
        /// </summary>
        public void SendCurrentItemCreated(string fullPath);
        /// <summary>
        /// カレントディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        public void SendCurrentItemDeleted(string fullPath);
        /// <summary>
        /// カレントディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        public void SendCurrentItemRenamed(string oldFullPath, string newFullPath);
        /// <summary>
        /// ディレクトリのアイテムが追加された事をメッセージ送信します。
        /// </summary>
        public void SendDirectoryItemCreated(string fullPath);
        /// <summary>
        /// ディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        public void SendDirectoryItemDeleted(string fullPath);
        /// <summary>
        /// ディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        public void SendDirectoryItemRenamed(string oldFullPathm, string newFullPathm);
        /// <summary>
        /// リムーバブルドライブの追加または挿入された事をメッセージ送信します。
        /// </summary>
        public void SendInsertOpticalMedia(string fullPath);
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクトされた事をメッセージ送信します。
        /// </summary>
        public void SendEjectOpticalMedia(string fullPath);
        #endregion ファイル監視
    }
}
