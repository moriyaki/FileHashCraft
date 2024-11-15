﻿/*  ExplorerListItemViewModel.cs

    Explorer 風画面の ListView のアイテム ViewModel を提供します。
 */

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.ExplorerPage
{
    public interface IExplorerListItemViewModel
    {
        /// <summary>
        /// ファイルのフルパスを取得します。
        /// </summary>
        public string FullPath { get; }
    }

    public partial class ExplorerListItemViewModel : ObservableObject, IComparable<ExplorerListItemViewModel>, IExplorerListItemViewModel
    {
        #region コンストラクタ

        /// <summary>
        /// コンストラクタで渡されるIExplorerPageViewModel
        /// </summary>
        private readonly IMessenger _Messanger;

        private readonly IFileSystemServices _FileSystemServices;
        private readonly ISettingsService _SettingsService;

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入です。
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        public ExplorerListItemViewModel()
        {
            _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            _FileSystemServices = Ioc.Default.GetService<IFileSystemServices>() ?? throw new NullReferenceException(nameof(IFileSystemServices));
            _SettingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;

            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
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
        [ObservableProperty]
        private string _Name = string.Empty;

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
        private string _FullPath = string.Empty;

        public string FullPath
        {
            get => _FullPath;
            set
            {
                SetProperty(ref _FullPath, value);
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
        [ObservableProperty]
        private BitmapSource? _Icon = null;

        /// <summary>
        /// ファイルの種類
        /// </summary>
        [ObservableProperty]
        private string _FileType = string.Empty;

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        protected bool _HasChildren = false;

        public virtual bool HasChildren
        {
            get => _HasChildren;
            set => SetProperty(ref _HasChildren, value);
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        protected bool _IsReady = false;

        public virtual bool IsReady
        {
            get => _IsReady;
            set => SetProperty(ref _IsReady, value);
        }

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        [ObservableProperty]
        private bool _IsRemovable = false;

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        [ObservableProperty]
        private bool _IsDirectory = false;

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
        [ObservableProperty]
        private long? _FileSize = null;

        /// <summary>
        /// 更新日時
        /// </summary>
        [ObservableProperty]
        private DateTime? _LastModifiedDate = null;

        /// <summary>
        /// チェックボックス
        /// </summary>
        [ObservableProperty]
        private bool _IsChecked = false;

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
        private FontFamily _CurrentFontFamily;

        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _SettingsService.CurrentFont = value;
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _FontSize;

        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) { return; }

                SetProperty(ref _FontSize, value);
                _SettingsService.FontSize = value;
            }
        }

        #endregion バインディング
    }
}