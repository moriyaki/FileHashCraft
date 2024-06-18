using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.Services.Messages
{
    #region インターフェース
    public interface IFileSystemServices
    {
        /// <summary>
        /// カレントディレクトリを移動します。
        /// </summary>
        /// <param name="fullPath">移動先カレントディレクトリ</param>
        public void NotifyChangeCurrentDirectory(string fullPath);

        /// <summary>
        /// 設定画面ページに移動します。
        /// </summary>
        /// <param name="pageEnum"></param>
        public void NavigateToSettingsPage(ReturnPageEnum pageEnum);
        /// <summary>
        /// 設定画面ページから元の画面に移動します。
        /// </summary>
        public void NavigateReturnPageFromSettings();
        /// <summary>
        /// エクスプローラー風画面に移動します。
        /// </summary>
        public void NavigateToExplorerPage();
        /// <summary>
        /// ハッシュ取得絞り込み画面に移動します。
        /// </summary>
        public void NavigateToSelectTargetPage();
        /// <summary>
        /// ハッシュ計算画面に移動に移動します。
        /// </summary>
        public void NavigateToHashCalcingPage();

        /// <summary>
        /// カレントディレクトリにアイテムが追加された事をメッセージ送信します。
        /// </summary>
        public void NotifyCurrentItemCreatedMessage(string fullPath);
        /// <summary>
        /// カレントディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        public void NotifyCurrentItemDeletedMessage(string fullPath);
        /// <summary>
        /// カレントディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        public void NotifyCurrentItemRenamedMessage(string oldFullPath, string newFullPath);
        /// <summary>
        /// ディレクトリのアイテムが追加された事をメッセージ送信します。
        /// </summary>
        public void NotifyDirectoryItemCreatedMessage(string fullPath);
        /// <summary>
        /// ディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        public void NotifyDirectoryItemDeletedMessage(string fullPath);
        /// <summary>
        /// ディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        public void NotifyDirectoryItemRenamedMessage(string oldFullPathm, string newFullPathm);
        /// <summary>
        /// リムーバブルドライブの追加または挿入された事をメッセージ送信します。
        /// </summary>
        public void NotifyInsertOpticalMediaMessage(string fullPath);
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクトされた事をメッセージ送信します。
        /// </summary>
        public void NotifyEjectOpticalMediaMessage(string fullPath);
    }
    #endregion インターフェース

    public class FileSystemServices : IFileSystemServices
    {
        #region 移動処理
        /// <summary>
        /// カレントディレクトリを移動します。
        /// </summary>
        /// <param name="fullPath">移動先カレントディレクトリ</param>
        public void NotifyChangeCurrentDirectory(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryChangedMessage(fullPath));
        }
        /// <summary>
        /// 設定画面ページに移動します。
        /// </summary>
        /// <param name="pageEnum"></param>
        public void NavigateToSettingsPage(ReturnPageEnum pageEnum)
        {
            WeakReferenceMessenger.Default.Send(new ToSettingPageMessage(pageEnum));
        }
        /// <summary>
        /// 設定画面ページから元の画面に移動します。
        /// </summary>
        public void NavigateReturnPageFromSettings()
        {
            WeakReferenceMessenger.Default.Send(new ReturnPageFromSettingsMessage());
        }
        /// <summary>
        /// エクスプローラー風画面に移動します。
        /// </summary>
        public void NavigateToExplorerPage()
        {
            WeakReferenceMessenger.Default.Send(new ToExplorerPageMessage());
        }
        /// <summary>
        /// ハッシュ取得絞り込み画面に移動します。
        /// </summary>
        public void NavigateToSelectTargetPage()
        {
            WeakReferenceMessenger.Default.Send(new ToPageSelectTargetMessage());
        }
        /// <summary>
        /// ハッシュ計算画面に移動に移動します。
        /// </summary>
        public void NavigateToHashCalcingPage()
        {
            WeakReferenceMessenger.Default.Send(new ToHashCalcingPageMessage());
        }

        /// <summary>
        /// 同一ファイル選択ページに移動します。
        /// </summary>
        /*
        public void NavigateToSameFileSelectPage()
        {
            WeakReferenceMessenger.Default.Send(new ToSameFileSelectPageMessage());
        }
        */
        #endregion 移動処理

        #region ファイル監視
        /// <summary>
        /// カレントディレクトリにアイテムが追加された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">追加されたアイテムのフルパス</param>
        public void NotifyCurrentItemCreatedMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemCreatedMessage(fullPath));
        }

        /// <summary>
        /// カレントディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary
        /// <param name="fullPath">削除されたアイテムのフルパス</param>
        public void NotifyCurrentItemDeletedMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemDeletedMessage(fullPath));
        }

        /// <summary>
        /// カレントディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        /// <param name="oldFullPath">名前変更される前のアイテムのフルパス</param>
        /// <param name="newFullPath">名前変更された後のアイテムのフルパス</param>
        public void NotifyCurrentItemRenamedMessage(string oldFullPath, string newFullPath)
        {
            WeakReferenceMessenger.Default.Send(new CurrentDirectoryItemRenamedMessage(oldFullPath, newFullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが追加された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">追加されたアイテムのフルパス</param>
        public void NotifyDirectoryItemCreatedMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemCreatedMessage(fullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが削除された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">削除されたアイテムのフルパス</param>
        public void NotifyDirectoryItemDeletedMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemDeletedMessage(fullPath));
        }
        /// <summary>
        /// ディレクトリのアイテムが名前変更された事をメッセージ送信します。
        /// </summary>
        /// <param name="oldFullPath">名前変更される前のアイテムのフルパス</param>
        /// <param name="newFullPath">名前変更された後のアイテムのフルパス</param>
        public void NotifyDirectoryItemRenamedMessage(string oldFullPath, string newFullPath)
        {
            WeakReferenceMessenger.Default.Send(new DirectoryItemRenamedMessage(oldFullPath, newFullPath));
        }

        /// <summary>
        /// リムーバブルドライブの追加または挿入された事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブのフルパス</param>
        public void NotifyInsertOpticalMediaMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new OpticalDriveMediaInsertedMessage(fullPath));
        }
        /// <summary>
        /// リムーバブルメディアの削除またはイジェクトされた事をメッセージ送信します。
        /// </summary>
        /// <param name="fullPath">リムーバブルドライブのフルパス</param>
        public void NotifyEjectOpticalMediaMessage(string fullPath)
        {
            WeakReferenceMessenger.Default.Send(new OpticalDriveMediaEjectedMessage(fullPath));
        }
        #endregion ファイル監視
    }
}
