/*  ExtensionOrTypeCheckBoxViewModel.cs

    拡張子、またはファイル種類のチェックボックスを扱うクラスです。
    ExtensionOrTypeCheckBoxBase を継承させた
    ExtensionCheckBox (拡張子チェックボックス) および ExtentionGroupCheckBoxViewModel (ファイル種類チェックボックス) を利用します。
 */

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IExtensionOrTypeCheckBoxBase
    {
        /// <summary>
        /// チェックボックス状態を参照または変更する
        /// </summary>
        public bool? IsChecked { get; set; }
        /// <summary>
        /// 拡張子またはそのグループを参照する
        /// </summary>
        public string ExtentionOrGroup { get; }
    }
    #endregion インターフェース

    /// <summary>
    /// 拡張子チェックボックスと、ファイル種類チェックボックスの基底クラス
    /// </summary>
    public class ExtensionOrTypeCheckBoxBase : ObservableObject, IExtensionOrTypeCheckBoxBase
    {
        #region コンストラクタ
        protected readonly IExtentionHelper _ExtentionManager;
        protected readonly ISearchManager _SearchManager;
        protected readonly IPageSelectTargetViewModel _PageSelectTargetFileViewModel;
        protected readonly IMainWindowViewModel _MainWindowViewModel;

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected ExtensionOrTypeCheckBoxBase()
        {
            _ExtentionManager = Ioc.Default.GetService<IExtentionHelper>() ?? throw new InvalidOperationException($"{nameof(IExtentionHelper)} dependency not resolved.");
            _SearchManager = Ioc.Default.GetService<ISearchManager>() ?? throw new InvalidOperationException($"{nameof(ISearchManager)} dependency not resolved.");
            _PageSelectTargetFileViewModel = Ioc.Default.GetService<IPageSelectTargetViewModel>() ?? throw new InvalidOperationException($"{nameof(IPageSelectTargetViewModel)} dependency not resolved.");
            _MainWindowViewModel = Ioc.Default.GetService<IMainWindowViewModel>() ?? throw new InvalidOperationException($"{nameof(IMainWindowViewModel)} dependency not resolved.");

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);
        }
        #endregion コンストラクタ

        #region バインディング
        /// <summary>
        /// チェックボックスから表示する文字列
        /// </summary>
        public string ExtentionOrFileTypeView
        {
            get
            {
                if (string.IsNullOrEmpty(ExtentionOrGroup))
                {
                    return $"{Resources.NoHaveExtentions} ({ExtentionCount})";
                }
                return $"{ExtentionOrGroup} ({ExtentionCount})";
            }
        }
        public int ExtentionCount { get; set; } = 0;

        /// <summary>
        /// 拡張子かファイル種類の文字列
        /// </summary>
        private string _ExtentionOrFileType = string.Empty;
        public string ExtentionOrGroup
        {
            get => _ExtentionOrFileType;
            set => SetProperty(ref _ExtentionOrFileType, value);
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _MainWindowViewModel.UsingFont;
            set
            {
                _MainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        public double FontSize
        {
            get => _MainWindowViewModel.FontSize;
            set
            {
                _MainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// チェックボックスのチェックの状態
        /// </summary>
        protected bool? _IsChecked = false;
        public virtual bool? IsChecked { get; set; }
        #endregion バインディング
    }
    //----------------------------------------------------------------------------------------
    /// <summary>
    /// ファイル拡張子のチェックボックスを扱うクラス
    /// </summary>
    public class ExtensionCheckBox : ExtensionOrTypeCheckBoxBase
    {
        #region バインディング
        /// <summary>
        /// 拡張子を設定し、該当するファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
         public ExtensionCheckBox(string extention) : base()
        {
            ExtentionOrGroup = extention;
            ExtentionCount = _ExtentionManager.GetExtentionsCount(extention);
        }

        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value == true)
                {
                    Task.Run(async () =>
                    {
                        await _SearchManager.AddCondition(SearchConditionType.Extention, ExtentionOrGroup);
                        _PageSelectTargetFileViewModel.ExtentionCountChanged();
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await _SearchManager.RemoveCondition(SearchConditionType.Extention, ExtentionOrGroup);
                        _PageSelectTargetFileViewModel.ExtentionCountChanged();
                    });
                }
                SetProperty(ref _IsChecked, value);
            }
        }
        #endregion バインディング
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// ファイル種類のチェックボックスを扱うクラス
    /// </summary>
    public class ExtentionGroupCheckBoxViewModel : ExtensionOrTypeCheckBoxBase
    {
        #region コンストラクタ
        /// <summary>
        /// その他のドキュメント専用のコンストラクタ
        /// </summary>
        public ExtentionGroupCheckBoxViewModel() : base()
        {
            FileType = FileGroupType.Others;
            ExtentionCount = _ExtentionManager.GetGroupExtentionCount(FileType);
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(FileGroupType.Others);
        }
        /// <summary>
        /// ファイルの種類用のコンストラクタ
        /// </summary>
        /// <param name="fileType">ファイルの種類</param>
        public ExtentionGroupCheckBoxViewModel(FileGroupType fileType) : base()
        {
            FileType = fileType;
            ExtentionCount = _ExtentionManager.GetGroupExtentionCount(FileType);
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(FileType);
        }
        #endregion コンストラクタ

        #region バインディング
        //private readonly List<string> _extensionList = [];
        private FileGroupType FileType { get; }
        /// <summary>
        /// チェックボックスの状態
        /// </summary>
        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value == true)
                {
                    _PageSelectTargetFileViewModel.ChangeCheckBoxGroup(true, _ExtentionManager.GetGroupExtentions(FileType));
                }
                else
                {
                    _PageSelectTargetFileViewModel.ChangeCheckBoxGroup(false, _ExtentionManager.GetGroupExtentions(FileType));
                }
                SetProperty(ref _IsChecked, value);
            }
        }
        #endregion バインディング
    }
}
