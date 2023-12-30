using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
    /// フォントの変更メッセージ
    /// </summary>
    public class FontChanged
    {
        public FontFamily Font { get; } = SystemFonts.MessageFontFamily;

        public FontChanged() { }

        public FontChanged(FontFamily font)
        {
            Font = font;
        }
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontSizeChanged
    {
        public double FontSize { get; } = SystemFonts.MessageFontSize;

        public FontSizeChanged() { }

        public FontSizeChanged(double fontSize)
        {
            FontSize = fontSize;
        }
    }

    #endregion MainView用

    #region ページ移動用
    public class ToExplorerPage
    {
        public ToExplorerPage() { }
    }

    public class ToSettingsPage
    {
        public ToSettingsPage() { }
    }

    #endregion ページ移動用

    #region DirectoryTreeViewControlViewModelから発信
    /// <summary>
    /// ディレクトリ監視から、ディレクトリが作成されたメッセージ
    /// </summary>
    public class DirectoryCreated
    {
        public string FullPath { get; } = string.Empty;
        public DirectoryCreated() { }
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
        public DirectoryRenamed() { }
        public DirectoryRenamed(string oldPath, string newPath)
        {
            OldFullPath = oldPath;
            NewFullPath = newPath;
        }
    }

    /// <summary>
    /// ディレクトリ監視から、ディレクトリが削除されたメッセージ
    /// </summary>
    public class DirectoryDeleted
    {
        public string FullPath { get; } = string.Empty;
        public DirectoryDeleted() { }
        public DirectoryDeleted(string fullPath)
        {
            FullPath = fullPath;
        }
    }
    #endregion FileChangeTreeSync用
}
