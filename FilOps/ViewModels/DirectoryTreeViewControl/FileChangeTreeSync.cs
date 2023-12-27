using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FilOps.Models;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.ViewModels.DirectoryTreeViewControl
{
    public interface IFileChangeTreeSync
    {

    }
    public class FileChangeTreeSync : IFileChangeTreeSync
    {
         public FileChangeTreeSync()
        {
            // ディレクトリ作成メッセージ
            WeakReferenceMessenger.Default.Register<DirectoryCreated>(this, (recipient, message) =>
            {
                OnDirectoryCreated(message.FullPath);
            });

            // ディレクトリ名前変更メッセージ
            WeakReferenceMessenger.Default.Register<DirectoryRenamed>(this, (recipient, message) =>
            {
                OnDirectoryRenamed(message.OldFullPath, message.NewFullPath);
            });

            // 何らかのサブディレクトリ削除メッセージ
            WeakReferenceMessenger.Default.Register<SomethingDirectoryDeleted>(this, (recipient, message) =>
            {
                OnSomethingDirectoryDeleted(message.ParentFullPath);
            });

            // ドライブ追加/変更メッセージ
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaInserted>(this, (recipient, message) =>
            {
                OnOpticalDriveMediaInserted(message.FullPath);
            });

            // ドライブイジェクトメッセージ
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaEjected>(this, (recipient, message) =>
            {

            });
        }

        #region ディレクトリ操作
        /// <summary>
        /// ディレクトリが外部から作成通知が来たときの処理をします。
        /// </summary>
        /// <param name="fullPath">作成されたディレクトリのフルパス</param>
        private void OnDirectoryCreated(string fullPath)
        {
            if (fullPath.Contains("$RECYCLE.BIN")) { return; }

            // ディレクトリが作成された親ディレクトリのパスを取得
            var path = System.IO.Path.GetDirectoryName(fullPath);
            if (path == null) { return; }


            WeakReferenceMessenger.Default.Send(new AddDirectory(fullPath));
            // 変更が加えられたディレクトリの親ツリーアイテムを取得
            /*
            var modifiedTreeItem = _DirectoryTreeViewControlViewModel.FindChangedDirectoryTree(path);
            if (modifiedTreeItem == null) { return; }

            // 作成されたディレクトリを追加
            var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(e.FullPath);
            var addTreeItem = new ExplorerTreeNodeViewModel(this, fileInformation);

            int newTreeIndex = FindIndexToInsert(modifiedTreeItem.Children, addTreeItem);
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    modifiedTreeItem.Children.Insert(newTreeIndex, addTreeItem);

                    // リストビューにも表示されていたら、そちらも更新
                    if (modifiedTreeItem.IsSelected)
                    {
                        var fileInformation = FileSystemInformationManager.GetFileInformationFromDirectorPath(e.FullPath);
                        var addListItem = new ExplorerListItemViewModel(this, fileInformation);
                                
                        int newListIndex = FindIndexToInsert(ListItems, addListItem);
                        ListItems.Insert(newListIndex, addListItem);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in DirectoryCreated: {ex.Message}");
                }
            });
            */
        }
        /// <summary>
        /// ディレクトリが外部から名前変更通知が来たときの処理をします。
        /// </summary>
        /// <param name="OldFullPath">古いディレクトリのフルパス</param>
        /// <param name="NewFullPath">新しいディレクトリのフルパス</param>
        public void OnDirectoryRenamed(string OldFullPath, string NewFullPath)
        {
        }

        /// <summary>
        /// 何らかのディレクトリが外部から削除された時の処理をします。
        /// </summary>
        /// <param name="FullPath">削除された親のディレクトリフルパス</param>
        private void OnSomethingDirectoryDeleted(string parentFullPath)
        {
        }
        #endregion ディレクトリ操作

        #region ドライブ操作
        /// <summary>
        /// ドライブが追加/変更された時の処理をします。
        /// </summary>
        /// <param name="fullPath"></param>
        private void OnOpticalDriveMediaInserted(string fullPath)
        {

        }

        /// <summary>
        /// ドライブがイジェクトされた時の処理をします。
        /// </summary>
        /// <param name="fullPath"></param>
        private void OnOpticalDriveMediaEjected(string fullPath)
        {

        }
        #endregion ドライブ操作


    }
}
