using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilOps.ViewModels
{
    /* 
        メッセージ送信サンプル
            WeakReferenceMessenger.Default.Send(new FontChanged(value));
        メッセージ受信サンプル
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (recipient, message) =>
            {
                FontSize = message.FontSize;
            });
     */
    #region MainView用
    /// <summary>
    /// カレントディレクトリの変更メッセージ
    /// </summary>
    public class CurrentChangeMessage
    {
        public string CurrentFullPath { get; } = string.Empty;
        public CurrentChangeMessage() { }
        public CurrentChangeMessage(string currentFullPath)
        {
            CurrentFullPath = currentFullPath;
        }
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontChanged
    {
        public double FontSize { get; } = SystemFonts.MessageFontSize;

        public FontChanged() { }

        public FontChanged(double fontSize)
        {
            FontSize = fontSize;
        }
    }
    #endregion MainView用

    #region FileChangeTreeSync用

    /// <summary>
    /// ディレクトリ監視から、ディレクトリが作成されたメッセージ
    /// </summary>
    public class DirectoryCreated
    {
        public string FullPath { get; } = string.Empty;
        public DirectoryCreated()
        {
            throw new NotImplementedException();
        }
        public DirectoryCreated(string fullPath)
        {
            FullPath = fullPath;
        }
    }

    /// <summary>
    /// ディレクトリ監視から、ディレクトリ名が変更されたメッセージ
    /// </summary>
    public class DirectoryRenamed
    {
        public string OldFullPath { get; } = string.Empty;
        public string NewFullPath { get; } = string.Empty;
        public DirectoryRenamed()
        {
            throw new NotSupportedException();
        }
        public DirectoryRenamed(string oldPath, string newPath)
        {
            OldFullPath = oldPath;
            NewFullPath = newPath;
        }
    }

    /// <summary>
    /// ディレクトリ監視から、何らかのディレクトリが削除されたメッセージ
    /// </summary>
    public class SomethingDirectoryDeleted
    {
        public string ParentFullPath { get; } = string.Empty;
        public SomethingDirectoryDeleted()
        {
            throw new NotImplementedException();
        }
        public SomethingDirectoryDeleted(string parentFullPath)
        {
            ParentFullPath = parentFullPath;
        }
    }

    /// <summary>
    /// ディレクトリ監視から、リムーバブルドライブが追加/挿入されたメッセージ
    /// </summary>
    public class OpticalDriveMediaInserted
    {
        public string FullPath { get; } = string.Empty;
        public OpticalDriveMediaInserted() 
        {
            throw new NotImplementedException();
        }
        public OpticalDriveMediaInserted(string fullPath)
        {
            FullPath = fullPath;
        }
    }

    /// <summary>
    /// ディレクトリ監視から、リムーバブルドライブがイジェクトされたメッセージ
    /// </summary>
    public class OpticalDriveMediaEjected
    {
        public string FullPath { get; } = string.Empty;
        public OpticalDriveMediaEjected()
        {
            throw new NotImplementedException();
        }
        public OpticalDriveMediaEjected(string fullPath)
        {
            FullPath = fullPath;
        }
    }
    #endregion FileChangeTreeSync用

    #region ViewModel用
    /// <summary>
    /// ディレクトリが追加されたメッセージ
    /// </summary>
    public class AddDirectory
    {
        public string FullPath { get; } = string.Empty;
        public AddDirectory()
        {
            throw new NotImplementedException();
        }
        public AddDirectory(string fullPath)
        {
            FullPath = fullPath;
        }
    }
    #endregion ViewModel用

}
