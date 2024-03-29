﻿/*  DirectoryTreeViewModel.cs

    ディレクトリツリービューのアイテム ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IDirectoryTreeViewModel
    {
        /// <summary>
        /// ファイル表示名を設定もしくは取得します。
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ファイルのアイコンを設定もしくは取得します。
        /// </summary>
        public BitmapSource? Icon { get; set; }
        /// <summary>
        /// ファイルのフルパスを取得します。
        /// </summary>
        public string FullPath { get; }
        /// <summary>
        /// 子ディレクトリを持っているかどうかを取得します。
        /// </summary>
        public bool HasChildren { get; }
        /// <summary>
        /// 子ディレクトリのコレクションを取得します。
        /// </summary>
        public ObservableCollection<DirectoryTreeViewModel> Children { get; }
        /// <summary>
        /// 子ディレクトリを取得します。
        /// </summary>
        public void KickChild(bool force = false);
        /// <summary>        /// <summary>
        /// ノードがチェックされているかどうかを設定もしくは取得します。
        /// </summary>
        public bool? IsChecked { get; set; }
        /// <summary>
        /// ノードが選択されているかどうかを取得します。
        /// </summary>
        public bool IsSelected { get; }
        /// <summary>
        /// ノードが展開されているかどうかを取得します。
        /// </summary>
        public bool IsExpanded { get; }
    }
    #endregion インターフェース
    public partial class DirectoryTreeViewModel : ObservableObject, IComparable<DirectoryTreeViewModel>, IDirectoryTreeViewModel
    {
        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入です。
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        public DirectoryTreeViewModel()
        {
            _messageServices = Ioc.Default.GetService<IMessageServices>() ?? throw new InvalidOperationException($"{nameof(IMessageServices)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m) => FontSize = m.FontSize);

            _UsingFont = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
        }

        /// <summary>
        /// コンストラクタで、ファイル情報の設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        public DirectoryTreeViewModel(FileItemInformation f) : this()
        {
            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;
        }
        /// <summary>
        /// コンストラクタで、ファイル情報、親ディレクトリの設定をします。
        /// </summary>
        /// <param name="f">ファイル情報</param>
        /// <param name="parent">親ディレクトリ</param>
        public DirectoryTreeViewModel(FileItemInformation f, DirectoryTreeViewModel parent) : this(f)
        {
            Parent = parent;
        }
        #endregion コンストラクタ

        #region メソッド
        /// <summary>
        /// ソートのための比較関数です。
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        public int CompareTo(DirectoryTreeViewModel? other)
        {
            return FullPath.CompareTo(other?.FullPath);
        }

        #endregion メソッド

        #region データバインディング
        /// <summary>
        /// ファイルの表示名
        /// </summary>
        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        private BitmapSource? _Icon;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
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
        private string _FullPath = string.Empty;
        public string FullPath
        {
            get => _FullPath;
            set
            {
                SetProperty(ref _FullPath, value);
                UpdatePropertiesFromFullPath();
            }
        }

        /// <summary>
        /// フルパスが変更されてアイコンやファイル種類を変更する
        /// </summary>
        private void UpdatePropertiesFromFullPath()
        {
            Name = WindowsAPI.GetDisplayName(FullPath);

            App.Current?.Dispatcher.Invoke(() =>
            {
                Icon = WindowsAPI.GetIcon(FullPath);
                FileType = WindowsAPI.GetType(FullPath);
            });
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _FileType = string.Empty;
        public string FileType
        {
            get => _FileType;
            set => SetProperty(ref _FileType, value);
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
        private bool _IsRemovable = false;
        public bool IsRemovable
        {
            get => _IsRemovable;
            set => SetProperty(ref _IsRemovable, value);
        }

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        private bool _IsDirectory;
        public bool IsDirectory
        {
            get => _IsDirectory;
            set => SetProperty(ref _IsDirectory, value);
        }

        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        private bool _HasChildren = false;
        public bool HasChildren
        {
            get => _HasChildren;
            set
            {
                if (!value)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _HasChildren, value) && Children.Count == 0)
                {
                    Children.Add(new DirectoryTreeViewModel() { Name = "【dummy】" });
                }
            }
        }

        /// <summary>
        /// このディレクトリの子ディレクトリのコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewModel> Children { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        public Visibility IsCheckBoxVisible
        {
            get
            {
                var controDirectoryTreeViewlViewModel = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new InvalidOperationException($"{nameof(IControDirectoryTreeViewlModel)} dependency not resolved.");
                return controDirectoryTreeViewlViewModel.IsCheckBoxVisible;
            }
        }

        /// <summary>
        /// ディレクトリのチェックボックスがチェックされているかどうか
        /// </summary>
        private bool? _IsChecked = false;
        public bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (FullPath?.Length == 0) { return; }
                if (value == _IsChecked) return;

                // サブディレクトリのチェック状態を変更する
                CheckCheckBoxStatusChanged(this, value);

                // 値をセットする
                SetProperty(ref _IsChecked, value, nameof(IsChecked));

                // 親ディレクトリのチェック状態を変更する
                ParentCheckBoxChange(this);

                // 特殊フォルダのチェック状態を変更する
                SyncSpecialDirectory(this, value);
            }
        }

        /// <summary>
        /// 他に影響を与えない形でディレクトリのチェックボックス状態を変更します。
        /// </summary>
        public bool? IsCheckedForSync
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value, nameof(IsChecked));
            }
        }

        /// <summary>
        /// ディレクトリの親ディレクトリ
        /// </summary>
        private DirectoryTreeViewModel? _Parent;
        public DirectoryTreeViewModel? Parent
        {
            get => _Parent;
            protected set => SetProperty(ref _Parent, value);
        }

        /// <summary>
        /// ディレクトリが選択されているかどうか
        /// </summary>
        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected != value)
                {
                    SetProperty(ref _IsSelected, value);
                    if (value)
                    {
                        var controDirectoryTreeViewlViewModel = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new InvalidOperationException($"{nameof(IControDirectoryTreeViewlModel)} dependency not resolved.");
                        controDirectoryTreeViewlViewModel.CurrentFullPath = this.FullPath;
                        if (HasChildren)
                        {
                            KickChild();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ディレクトリが展開されているかどうか
        /// </summary>
        private bool _IsExpanded;
        public bool IsExpanded
        {
            get => _IsExpanded;
            set
            {
                if (!HasChildren)
                {
                    Children.Clear();
                    return;
                }
                if (SetProperty(ref _IsExpanded, value) && IsReady)
                {
                    if (value && !_IsKicked) { KickChild(); }
                }
                var directorryTreeManager = Ioc.Default.GetService<ITreeManager>() ?? throw new InvalidOperationException($"{nameof(ITreeManager)} dependency not resolved.");
                if (value)
                {
                    foreach (var child in Children)
                    {
                        directorryTreeManager.AddExpandedDirectoryManager(child);
                        if (IsChecked == true) { child.IsChecked = true; }
                    }
                }
                else
                {
                    foreach (var child in Children)
                    {
                        directorryTreeManager.RemoveExpandedDirectoryManager(child);
                    }
                }
            }
        }

        /// <summary>
        /// 子ディレクトリを取得しているかどうか
        /// </summary>
        private bool _IsKicked;
        public bool IsKicked { get => _IsKicked; }

        /// <summary>
        /// 子ノードを設定する
        /// </summary>
        public void KickChild(bool force = false)
        {
            if (!_IsKicked || force)
            {
                Children.Clear();
                foreach (var childPath in FileManager.EnumerateDirectories(FullPath))
                {
                    var child = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(childPath);
                    var item = new DirectoryTreeViewModel(child, this);
                    Children.Add(item);
                }
            }
            _IsKicked = true;
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _UsingFont;
        public FontFamily CurrentFontFamily
        {
            get => _UsingFont;
            set
            {
                if (_UsingFont.Source == value.Source) { return; }
                SetProperty(ref _UsingFont, value);
                _messageServices.SendCurrentFont(value);
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
                _messageServices.SendFontSize(value);
            }
        }
        #endregion データバインディング
    }
}
