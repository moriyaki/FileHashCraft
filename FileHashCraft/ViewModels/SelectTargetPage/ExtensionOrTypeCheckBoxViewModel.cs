/*  ExtensionOrTypeCheckBoxViewModel.cs

    拡張子、またはファイル拡張子グループのチェックボックスを扱うクラスです。
    ExtensionOrTypeCheckBoxBase を継承させた
    ExtensionCheckBox (拡張子チェックボックス) および
    ExtentionGroupCheckBoxViewModel (拡張子グループチェックボックス) を利用します。
 */

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    #region インターフェース

    public interface IExtensionOrTypeCheckBoxBase
    {
        /// <summary>
        /// チェックボックス状態を参照または変更する
        /// </summary>
        bool? IsChecked { get; set; }

        /// <summary>
        /// 拡張子またはそのグループを参照する
        /// </summary>
        string Name { get; }
    }

    #endregion インターフェース

    /// <summary>
    /// 拡張子チェックボックスと、拡張子グループチェックボックスの基底クラス
    /// </summary>
    public partial class BaseExtensionOrTypeCheckBox : ObservableObject, IExtensionOrTypeCheckBoxBase
    {
        #region コンストラクタ

        protected readonly IMessenger _Messanger;
        protected readonly ISettingsService _SettingsService;
        protected readonly IExtentionManager _ExtentionManager;
        protected readonly IFileSearchCriteriaManager _FileSearchCriteriaManager;

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected BaseExtensionOrTypeCheckBox(
            IMessenger messenger,
            ISettingsService settingsService,
            IExtentionManager extentionManager,
            IFileSearchCriteriaManager fileSearchCriteriaManager
        )
        {
            _Messanger = messenger;
            _SettingsService = settingsService;
            _ExtentionManager = extentionManager;
            _FileSearchCriteriaManager = fileSearchCriteriaManager;

            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;
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
                if (string.IsNullOrEmpty(Name))
                {
                    return $"{Resources.NoHaveExtentions} ({ExtentionCount})";
                }
                return $"{Name} ({ExtentionCount})";
            }
        }

        public int ExtentionCount { get; set; } = 0;

        /// <summary>
        /// 拡張子か拡張子グループの文字列
        /// </summary>
        [ObservableProperty]
        private string _Name = string.Empty;

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

        /// <summary>
        /// チェックボックスのチェックの状態
        /// </summary>
        protected bool? _IsChecked = false;

        public virtual bool? IsChecked { get; set; }

        public virtual bool? IsCheckedForce { get; set; }

        #endregion バインディング
    }

    //----------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// ファイル拡張子のチェックボックスを扱うクラス
    /// </summary>
    public class ExtensionCheckBoxViewModel(
        IMessenger messenger,
        ISettingsService settingsService,
        IExtentionManager extentionManager,
        IFileSearchCriteriaManager fileSearchCriteriaManager
        ) : BaseExtensionOrTypeCheckBox(messenger, settingsService, extentionManager, fileSearchCriteriaManager)
    {
        #region コンストラクタと初期化

        /// <summary>
        /// 拡張子を設定し、該当するファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void Initialize(string extention)
        {
            Name = extention;
            ExtentionCount = _ExtentionManager.GetExtentionsCount(extention);
        }

        #endregion コンストラクタと初期化

        #region バインディング

        /// <summary>
        /// setterのメッセージは親のViewModelへの送信、拡張子グループのチェックボックス遷移などを行う
        /// </summary>
        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value);
                if (value == true)
                {
                    _FileSearchCriteriaManager.AddCriteria(Name, FileSearchOption.Extention);
                    _Messanger.Send(new ExtentionCheckReflectToGroupMessage(Name));
                    _Messanger.Send(new ExtentionCheckChangedToListBoxMessage());
                }
                else
                {
                    _FileSearchCriteriaManager.RemoveCriteria(Name, FileSearchOption.Extention);
                    _Messanger.Send(new ExtentionUncheckReflectToGroupMessage(Name));
                    _Messanger.Send(new ExtentionCheckChangedToListBoxMessage());
                }
            }
        }

        public override bool? IsCheckedForce
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value, nameof(IsChecked));
        }

        #endregion バインディング
    }

    //----------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 拡張子グループのチェックボックスを扱うクラス
    /// </summary>
    public class ExtentionGroupCheckBoxViewModel(
        IMessenger messenger,
        ISettingsService settingsService,
        IExtentionManager extentionManager,
        IFileSearchCriteriaManager fileSearchCriteriaManager
        ) : BaseExtensionOrTypeCheckBox(messenger, settingsService, extentionManager, fileSearchCriteriaManager)
    {
        #region 初期化

        /// <summary>
        /// その他ドキュメント用初期化
        /// </summary>
        public void Initialize()
        {
            FileType = FileGroupType.Others;
            ExtentionCount = _ExtentionManager.GetExtentionGroupCount(FileType);
            Name = _ExtentionManager.GetFileGroupName(FileGroupType.Others);
        }

        /// <summary>
        /// ファイル拡張子グループ用初期化
        /// </summary>
        /// <param name="fileType">ファイル拡張子グループ</param>
        public void Initialize(FileGroupType fileType)
        {
            FileType = fileType;
            ExtentionCount = _ExtentionManager.GetExtentionGroupCount(FileType);
            Name = _ExtentionManager.GetFileGroupName(FileType);
        }

        #endregion 初期化

        #region バインディング

        public FileGroupType FileType { get; set; }

        /// <summary>
        /// チェックボックスの状態、拡張子グループのチェックボックスが変更されたら、拡張子チェックボックスにも変更
        /// </summary>
        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value);
                if (value == true)
                {
                    _Messanger.Send(new ExtentionGroupCheckedMessage(true, _ExtentionManager.GetGroupExtentions(FileType)));
                }
                else
                {
                    _Messanger.Send(new ExtentionGroupCheckedMessage(false, _ExtentionManager.GetGroupExtentions(FileType)));
                }
            }
        }

        public override bool? IsCheckedForce
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value, nameof(IsChecked));
        }

        #endregion バインディング
    }
}