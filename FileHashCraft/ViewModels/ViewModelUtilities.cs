using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileHashCraft.ViewModels
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
        public string HashAlgorithm { get; } = "SHA-256";

        public HashAlgorithmChanged() { throw new NotImplementedException(); }

        public HashAlgorithmChanged(string hashAlgorithm)
        {
            HashAlgorithm = hashAlgorithm;
        }
    }
    #endregion MainView用

    #region ページ移動用

    /// <summary>
    /// エクスプローラー風画面にページに移動するメッセージ
    /// </summary>
    public class ToExplorerPage
    {
        public ToExplorerPage() { }
    }

    /// <summary>
    /// どこに戻るかの列挙子
    /// </summary>
    public enum ReturnPageEnum
    {
        PageExplorer,
        PageSettings,
        PageHashCalcing,
    }

    /// <summary>
    /// 戻るページを指定して設定画面に移動するメッセージ
    /// </summary>
    public class ToSettingPage
    {
        public ReturnPageEnum ReturnPage { get; }

        public ToSettingPage() { throw new NotImplementedException(); }
        public ToSettingPage(ReturnPageEnum returnPage)
        {
            ReturnPage = returnPage;
        }
    }

    /// <summary>
    /// 元のページへの移動メッセージ
    /// </summary>
    public class ReturnPageFromSettings
    {
        public ReturnPageFromSettings() { }
    }

    #endregion ページ移動用

    #region DirectoryTreeViewControlViewModelから発信
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
    #endregion FileChangeTreeSync用

    #region DelegateCommand
    /*
    //使い方
    public class YourViewModel : ObservableObject
    {
        public YourViewModel()
        {
            YourCommand = new DelegateCommand<string>(ExecuteYourCommand, CanExecuteYourCommand);
        }

        public ICommand YourCommand { get; }

        private void ExecuteYourCommand(string parameter)
        {
            // Your command logic here
        }

        private bool CanExecuteYourCommand(string parameter)
        {
            // Your can execute logic here
            return true;
        }
    }
    */

    /// <summary>
    /// 型なしの ICommand実装
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand()
        {
            throw new NotImplementedException();
        }

        public DelegateCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 型ありの ICommand実装
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand()
        {
            throw new NotImplementedException();
        }

        public DelegateCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter != null && _canExecute((T)parameter));
        }

        public void Execute(object? parameter)
        {
            if (parameter != null)
            {
                _execute((T)parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion DelegateCommand
}
