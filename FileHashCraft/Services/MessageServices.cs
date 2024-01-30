using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.Services
{
    public class MessageServices : IMessageServices
    {
        #region 重要設定
        /// <summary>
        /// ウィンドウの上位置を設定します。
        /// </summary>
        /// <param name="top">ウィンドウの上位置</param>
        public void SendWindowTop(double top)
        {
            WeakReferenceMessenger.Default.Send(new WindowTopChanged(top));
        }
        /// <summary>
        /// ウィンドウの左位置を設定します。
        /// </summary>
        /// <param name="left">ウィンドウの左位置</param>
        public void SendWindowLeft(double left)
        {
            WeakReferenceMessenger.Default.Send(new WindowLeftChanged(left));
        }
        /// <summary>
        /// ウィンドウの幅を設定します。
        /// </summary>
        /// <param name="width">ウィンドウの幅</param>
        public void SendWindowWidth(double width)
        {
            WeakReferenceMessenger.Default.Send(new WindowWidthChanged(width));
        }
        /// <summary>
        /// ウィンドウの高さを設定します。
        /// </summary>
        /// <param name="height">ウィンドウの高さ</param>
        public void SendWindowHeight(double height)
        {
            WeakReferenceMessenger.Default.Send(new WindowHeightChanged(height));
        }
        /// <summary>
        /// ツリービューの幅を設定します。
        /// </summary>
        /// <param name="width">ツリービューの幅</param>
        public void SendTreeWidth(double width)
        {
            WeakReferenceMessenger.Default.Send(new TreeWidthChanged(width));
        }
        /// <summary>
        /// リストボックスの幅を設定します。
        /// </summary>
        /// <param name="width">リストボックスの幅</param>
        public void SendListWidth(double width)
        {
            WeakReferenceMessenger.Default.Send(new ListWidthChanged(width));
        }
        /// <summary>
        /// 利用言語を設定します。
        /// </summary>
        /// <param name="language"></param>
        public void SendLanguage(string language)
        {
            WeakReferenceMessenger.Default.Send(new SelectedLanguageChanged(language));
        }
        /// <summary>
        /// ファイルのハッシュアルゴリズムを設定します。
        /// </summary>
        /// <param name="hashAlogrithm">ファイルのハッシュアルゴリズム</param>
        public void SendHashAlogrithm(string hashAlogrithm)
        {
            WeakReferenceMessenger.Default.Send(new HashAlgorithmChanged(hashAlogrithm));
        }
        /// <summary>
        /// 削除対象に読み取り専用ファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="readOnlyFileInclude">読み取り専用ファイルを含むかどうか</param>
        public void SendReadOnlyFileInclude(bool readOnlyFileInclude)
        {
            WeakReferenceMessenger.Default.Send(new ReadOnlyFileIncludeChanged(readOnlyFileInclude));
        }
        /// <summary>
        /// 削除対象に隠しファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="hiddenFileInclude">隠しファイルを含むかどうか</param>
        public void SendHiddenFileInclude(bool hiddenFileInclude)
        {
            WeakReferenceMessenger.Default.Send(new HiddenFileIncludeChanged(hiddenFileInclude));
        }
        /// <summary>
        /// 削除対象に0サイズのファイルを含むかどうかを設定します。
        /// </summary>
        /// <param name="zeroSizeFileDelete">0サイズファイルを含むかどうか</param>
        public void SendZeroSizeFileDelete(bool zeroSizeFileDelete)
        {
            WeakReferenceMessenger.Default.Send(new ZeroSizeFileDeleteChanged(zeroSizeFileDelete));
        }
        /// <summary>
        /// 削除対象に空のディレクトリを含むかどうかを設定します。
        /// </summary>
        /// <param name="emptyDirectoryDelete">空のディレクトリを含むかどうか</param>
        public void SendEmptyDirectoryDelete(bool emptyDirectoryDelete)
        {
            WeakReferenceMessenger.Default.Send(new EmptyDirectoryDeleteChanged(emptyDirectoryDelete));
        }

        /// <summary>
        /// フォントを設定します
        /// </summary>
        /// <param name="currentFont">フォントファミリー</param>
        public void SendCurrentFont(FontFamily currentFont)
        {
            WeakReferenceMessenger.Default.Send(new CurrentFontFamilyChanged(currentFont));
        }
        /// <summary>
        /// フォントサイズを設定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        public void SendFontSize(double fontSize)
        {
            WeakReferenceMessenger.Default.Send(new FontSizeChanged(fontSize));
        }
        #endregion 重要設定

        #region 移動処理
        /// <summary>
        /// カレントディレクトリを移動します。
        /// </summary>
        /// <param name="fullPath">移動先カレントディレクトリ</param>
        public void SendCurrentDirectoryChanged(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryChanged(fullPath));
        }
        /// <summary>
        /// 設定画面ページに移動します。
        /// </summary>
        /// <param name="pageEnum"></param>
        public void SendToSettingsPage(ReturnPageEnum pageEnum)
        {
            WeakReferenceMessenger.Default.Send(new ToSettingPage(pageEnum));
        }
        /// <summary>
        /// 設定画面ページから元の画面に移動します。
        /// </summary>
        public void SendReturnPageFromSettings()
        {
            WeakReferenceMessenger.Default.Send(new ReturnPageFromSettings());
        }
        /// <summary>
        /// エクスプローラー風画面に移動します。
        /// </summary>
        public void SendToExplorerPage()
        {
            WeakReferenceMessenger.Default.Send(new ToExplorerPage());
        }
        /// <summary>
        /// ハッシュ取得絞り込み画面に移動します。
        /// </summary>
        public void SendToSelectTargetPage()
        {
            WeakReferenceMessenger.Default.Send(new ToPageSelectTarget());
        }
        /// <summary>
        /// ハッシュ計算画面に移動に移動します。
        /// </summary>
        public void SendToHashCalcingPage()
        {
            WeakReferenceMessenger.Default.Send(new ToHashCalcingPage());
        }

        #endregion 移動処理

        #region ファイル監視
        /// <summary>
        /// カレントディレクトリにアイテムが追加された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">追加されたアイテムのフルパス</param>
        public void SendCurrentItemCreated(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemCreated(fullPath));
        }

        /// <summary>
        /// カレントディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary
        /// <param name="fullPath">削除されたアイテムのフルパス</param>
        public void SendCurrentItemDeleted(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemDeleted(fullPath));
        }

        /// <summary>
        /// カレントディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        /// <param name="oldFullPath">名前変更される前のアイテムのフルパス</param>
        /// <param name="newFullPath">名前変更された後のアイテムのフルパス</param>
        public void SendCurrentItemRenamed(string oldFullPath, string newFullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemRenamed(oldFullPath, newFullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが追加された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">追加されたアイテムのフルパス</param>
        public void SendDirectoryItemCreated(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemCreated(fullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">削除されたアイテムのフルパス</param>
        public void SendDirectoryItemDeleted(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemDeleted(fullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        /// <param name="oldFullPath">名前変更される前のアイテムのフルパス</param>
        /// <param name="newFullPath">名前変更された後のアイテムのフルパス</param>
        public void SendDirectoryItemRenamed(string oldFullPath, string newFullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemRenamed(oldFullPath, newFullPath));
        }

        /// <summary>
        /// リムーバブルドライブの追加または挿入された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブのフルパス</param>
        public void SendInsertOpticalMedia(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new OpticalDriveMediaInserted(fullPath));
        }
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクトされた事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブのフルパス</param>
        public void SendEjectOpticalMedia(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new OpticalDriveMediaEjected(fullPath));
        }
        #endregion ファイル監視
    }
}
