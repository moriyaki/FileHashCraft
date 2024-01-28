/*  ViewModelUtilities.cs

    メッセージングに利用するメッセージ
    DelegateCommandの実装
    リソースサービスの実装
    
    をしているユーティリティクラスです。

 */
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;

namespace FileHashCraft.ViewModels.Modules
{
    #region MainView用メッセージ送受信
    /// <summary>
    /// カレントディレクトリの変更メッセージ
    /// </summary>
    public class CurrentChangeMessage
    {
        public string CurrentFullPath { get; } = string.Empty;
        public CurrentChangeMessage() { throw new NotImplementedException(); }
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
        public FontFamily UsingFont { get; } = SystemFonts.MessageFontFamily;

        public FontChanged() { throw new NotImplementedException(); }

        public FontChanged(FontFamily usingFont)
        {
            UsingFont = usingFont;
        }
    }

    /// <summary>
    /// フォントサイズの変更メッセージ
    /// </summary>
    public class FontSizeChanged
    {
        public double FontSize { get; } = SystemFonts.MessageFontSize;

        public FontSizeChanged() { throw new NotImplementedException(); }

        public FontSizeChanged(double fontSize)
        {
            FontSize = fontSize;
        }
    }

    /// <summary>
    /// ハッシュ計算アルゴリズムの変更メッセージ
    /// </summary>
    public class HashAlgorithmChanged
    {
        public string HashAlgorithm { get; } = HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256);

        public HashAlgorithmChanged() { throw new NotImplementedException(); }

        public HashAlgorithmChanged(string hashAlgorithm)
        {
            HashAlgorithm = hashAlgorithm;
        }
    }

    /// <summary>
    /// ツリービュー幅の変更メッセージ
    /// </summary>
    public class TreeWidthChanged
    {
        public double TreeWidth { get; } = 300;

        public TreeWidthChanged() { throw new NotImplementedException(); }

        public TreeWidthChanged(double treeWidth)
        {
            TreeWidth = treeWidth;
        }
    }

    /// <summary>
    /// リストボックス幅の変更メッセージ
    /// </summary>
    public class ListWidthChanged
    {
        public double ListWidth { get; } = 300;

        public ListWidthChanged() { throw new NotImplementedException(); }

        public ListWidthChanged(double listWidth)
        {
            ListWidth = listWidth;
        }
    }

    #endregion MainView用メッセージ送受信

    #region ページ移動用メッセージ送受信

    /// <summary>
    /// エクスプローラー風画面ページに移動するメッセージ
    /// </summary>
    public class ToPageExplorer
    {
        public ToPageExplorer() { }
    }

    /// <summary>
    /// ハッシュ計算対象選択ページに移動するメッセージ
    /// </summary>
    public class ToPageSelectTarget
    {
        public ToPageSelectTarget() { }
    }
    /// <summary>
    /// ハッシュ計算画面ページに移動するメッセージ
    /// </summary>
    public class ToPageHashCalcing
    {
        public ToPageHashCalcing() { }
    }

    /// <summary>
    /// どこに戻るかの列挙子
    /// </summary>
    public enum ReturnPageEnum
    {
        PageExplorer,
        PageSettings,
        PageTargetSelect,
        PageHashCalcing,
    }

    /// <summary>
    /// 戻るページを指定して設定画面に移動するメッセージ
    /// </summary>
    public class ToPageSetting
    {
        public ReturnPageEnum ReturnPage { get; }

        public ToPageSetting() { throw new NotImplementedException(); }
        public ToPageSetting(ReturnPageEnum returnPage)
        {
            ReturnPage = returnPage;
        }
    }

    /// <summary>
    /// 元のページへの移動メッセージ
    /// </summary>
    public class ReturnPageFromSettings
    {
        public string HashAlgorithm = string.Empty;
        public ReturnPageFromSettings() { }
    }

    #endregion ページ移動用メッセージ送受信

    #region ディレクトリ監視からメッセージ送受信
    /// <summary>
    /// ディレクトリ監視から、ディレクトリが作成されたメッセージ
    /// </summary>
    public class DirectoryCreated
    {
        public string FullPath { get; } = string.Empty;
        public DirectoryCreated() { throw new NotImplementedException(); }
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
        public DirectoryRenamed() { throw new NotImplementedException(); }
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
        public DirectoryDeleted() { throw new NotImplementedException(); }
        public DirectoryDeleted(string fullPath)
        {
            FullPath = fullPath;
        }
    }
    #endregion ディレクトリ監視からメッセージ送受信

    #region リソースサービス
    public class ResourceService : ObservableObject
    {
        public static ResourceService Current { get; } = new();
        public Resources Resources { get; } = new();

        /// <summary>
        /// 言語カルチャを変更する
        /// </summary>
        /// <param name="name">変更するカルチャ(例："ja-JP")</param>
        public void ChangeCulture(string name)
        {
            Resources.Culture = CultureInfo.GetCultureInfo(name);
            OnPropertyChanged(nameof(Resources));
        }
    }
    #endregion リソースサービス
}
