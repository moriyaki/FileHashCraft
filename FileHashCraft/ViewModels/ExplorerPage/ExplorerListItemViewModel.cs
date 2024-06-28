/*  ExplorerListItemViewModel.cs

    Explorer 風画面の ListView のアイテム ViewModel を提供します。
 */
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.ExplorerPage
{
    public interface IExplorerListItemViewModel
    {
        /// <summary>
        /// ファイルのフルパスを取得します。
        /// </summary>
        public string FullPath { get; }
    }

    public class ExplorerListItemViewModel : ObservableObject, IComparable<ExplorerListItemViewModel>, IExplorerListItemViewModel
    {
        #region コンストラクタ
        /// <summary>
        /// コンストラクタで渡されるIExplorerPageViewModel
        /// </summary>
        private readonly IFileSystemServices _messageServices;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入です。
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        public ExplorerListItemViewModel()
        {
            _messageServices = Ioc.Default.GetService<IFileSystemServices>() ?? throw new InvalidOperationException($"{nameof(IFileSystemServices)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");

            _currentFontFamily = _settingsService.CurrentFont;
            _fontSize = _settingsService.FontSize;

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
        }

        /// <summary>
        /// コンストラクタで、ファイル情報の設定をします。
        /// </summary>
        /// <param name="f">FileItemInformation</param>
        public ExplorerListItemViewModel(FileItemInformation f) : this()
        {
            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;

            LastModifiedDate = f.LastModifiedDate;
            FileSize = f.FileSize;
        }
        #endregion コンストラクタ

        #region メソッド
        /// <summary>
        /// ソートのための比較関数です。
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        public int CompareTo(ExplorerListItemViewModel? other)
        {
            return FullPath.CompareTo(other?.FullPath);
        }
        #endregion メソッド

        #region バインディング
        /// <summary>
        /// ファイルの表示名
        /// </summary>
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// ファイル実体名
        /// </summary>
        public string FileName
        {
            get => Path.GetFileName(FullPath);
        }

        /// <summary>
        /// ファイルまたはフォルダのフルパス
        /// </summary>
        private string _fullPath = string.Empty;
        public string FullPath
        {
            get => _fullPath;
            set
            {
                SetProperty(ref _fullPath, value);
                Name = WindowsAPI.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = WindowsAPI.GetIcon(FullPath);
                    FileType = WindowsAPI.GetType(FullPath);
                }));
            }
        }

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        private BitmapSource? _icon = null;
        public BitmapSource? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _fileType = string.Empty;
        public string FileType
        {
            get => _fileType;
            set => SetProperty(ref _fileType, value);
        }

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        protected bool _hasChildren = false;
        public virtual bool HasChildren
        {
            get => _hasChildren;
            set => SetProperty(ref _hasChildren, value);
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        protected bool _isReady = false;
        public virtual bool IsReady
        {
            get => _isReady;
            set => SetProperty(ref _isReady, value);
        }

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        private bool _isRemovable = false;
        public bool IsRemovable
        {
            get => _isRemovable;
            set => SetProperty(ref _isRemovable, value);
        }

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        private bool _isDirectory = false;
        public bool IsDirectory
        {
            get => _isDirectory;
            set => SetProperty(ref _isDirectory, value);
        }

        /// <summary>
        /// 表示用の更新日時文字列
        /// </summary>
        public string LastFileUpdate
        {
            get => LastModifiedDate?.ToString("yy/MM/dd HH:mm") ?? string.Empty;
        }

        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        private long? _fileSize = null;
        public long? FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        /// <summary>
        /// 更新日時
        /// </summary>
        private DateTime? _lastModifiedDate = null;
        public DateTime? LastModifiedDate
        {
            get => _lastModifiedDate;
            set => SetProperty(ref _lastModifiedDate, value);
        }

        /// <summary>
        /// チェックボックス
        /// </summary>
        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 書式化したファイルのサイズ文字列
        /// </summary>
        public string FormattedFileSize
        {
            get
            {
                if (FileSize == null || IsDirectory) return string.Empty;

                var kb_filesize = (long)FileSize / 1024;
                return kb_filesize.ToString("N") + "KB";
            }
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _currentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _currentFontFamily;
            set
            {
                if (_currentFontFamily.Source == value.Source) { return; }

                SetProperty(ref _currentFontFamily, value);
                _settingsService.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _fontSize;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) { return; }

                SetProperty(ref _fontSize, value);
                _settingsService.SendFontSize(value);
            }
        }
        #endregion バインディング
    }
}
