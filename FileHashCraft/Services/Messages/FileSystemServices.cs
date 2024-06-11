using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.Services.Messages
{
    public interface IFileSystemServices
    {
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
    public class FileSystemServices : IFileSystemServices
    {
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
