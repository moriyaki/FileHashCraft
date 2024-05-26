/*  CurrentDirectoryFIleSystemWatcherService.cs

    カレントディレクトリの変更を監視するクラスです。
 */
using System.IO;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Messages;

namespace FileHashCraft.Services.FileSystemWatcherServices
{
    #region インターフェース
    public interface ICurrentDirectoryFIleSystemWatcherService
    {
        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をします。
        /// </summary>
        public void SetCurrentDirectoryWatcher(string currentDirectory);
    }
    #endregion インターフェース
    public class CurrentDirectoryFIleSystemWatcherService : ICurrentDirectoryFIleSystemWatcherService
    {
        public CurrentDirectoryFIleSystemWatcherService() { throw new NotImplementedException(); }

        private readonly IMessageServices _messageServices;
        public CurrentDirectoryFIleSystemWatcherService(
            IMessageServices messageServices)
        {
            _messageServices = messageServices;
        }

        #region FileSystemWatcherの宣言
        private readonly FileSystemWatcher CurrentWatcher = new();

        /// <summary>
        /// カレントディレクトリに対してファイル変更監視の設定をします。
        /// </summary>
        /// <param name="currentDirectory">カレントディレクトリのフルパス</param>
        public void SetCurrentDirectoryWatcher(string currentDirectory)
        {
            // System.IO.FileNotFoundException: 'Error reading the C:\Windows\CSC directory.'
            if (!Path.Exists(currentDirectory)) return;
            try
            {
                CurrentWatcher.Created -= OnCurrentCreated;
                CurrentWatcher.Deleted -= OnCurrentDeleted;
                CurrentWatcher.Renamed -= OnCurrentRenamed;
                CurrentWatcher.Error -= OnError;

                CurrentWatcher.Path = currentDirectory;
                CurrentWatcher.NotifyFilter
                    = NotifyFilters.FileName
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Size;
                CurrentWatcher.EnableRaisingEvents = true;
                CurrentWatcher.IncludeSubdirectories = false;

                CurrentWatcher.Created += OnCurrentCreated;
                CurrentWatcher.Deleted += OnCurrentDeleted;
                CurrentWatcher.Renamed += OnCurrentRenamed;

                CurrentWatcher.Error += OnError;
            }
            catch (FileNotFoundException) { }
        }
        /// <summary>
        /// FIleSystemWatcherエラーイベントの処理をします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, ErrorEventArgs e)
        {
            DebugManager.ExceptionWrite($"FileSystemWatcher : {e.GetException()}");
        }
        #endregion FileSystemWatcherの宣言

        #region ファイル変更通知
        /// <summary>
        /// ファイルが作成された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCurrentCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            _messageServices.SendCurrentItemCreated(e.FullPath);
        }

        /// <summary>
        /// ファイルが削除された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCurrentDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Deleted) return;
            _messageServices.SendCurrentItemDeleted(e.FullPath);
        }

        /// <summary>
        /// ファイルが名前変更された通知の処理をします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">FileSystemEventArgs</param>
        private void OnCurrentRenamed(object sender, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;
            _messageServices.SendCurrentItemRenamed(e.OldFullPath, e.FullPath);
        }
        #endregion ファイル変更通知
    }
}
