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
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
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
    public class ExtensionOrTypeCheckBoxBase : ObservableObject, IExtensionOrTypeCheckBoxBase
    {
        #region コンストラクタ
        protected readonly ISettingsService _settingsService;
        protected readonly IExtentionManager _extentionManager;
        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected ExtensionOrTypeCheckBoxBase(
            ISettingsService settingsService,
            IExtentionManager extentionManager)
        {
            _settingsService = settingsService;
            _extentionManager = extentionManager;

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m) => FontSize = m.FontSize);

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
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
        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
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
                _settingsService.SendCurrentFont(value);
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
                _settingsService.SendFontSize(value);
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
        ISettingsService settingsService,
        IExtentionManager extentionManager
        ) : ExtensionOrTypeCheckBoxBase(settingsService, extentionManager)
    {
        #region コンストラクタと初期化

        /// <summary>
        /// 拡張子を設定し、該当するファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void Initialize(string extention)
        {
            Name = extention;
            ExtentionCount = _extentionManager.GetExtentionsCount(extention);
        }
        #endregion 初期化

        #region バインディング
        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value);
                if (value == true)
                {
                    FileSearchCriteriaManager.AddCriteriaExtention(Name);
                    WeakReferenceMessenger.Default.Send(new ExtentionChechReflectToGroup(Name));
                    WeakReferenceMessenger.Default.Send(new ExtentionCheckChangedToListBox(Name, true));
                }
                else
                {
                    FileSearchCriteriaManager.RemoveCriteriaExtention(Name);
                    WeakReferenceMessenger.Default.Send(new ExtentionUnchechReflectToGroup(Name));
                    WeakReferenceMessenger.Default.Send(new ExtentionCheckChangedToListBox(Name, false));
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
        ISettingsService settingsService,
        IExtentionManager extentionManager
        ) : ExtensionOrTypeCheckBoxBase(settingsService, extentionManager)
    {
        #region 初期化

        /// <summary>
        /// その他ドキュメント用初期化
        /// </summary>
        public void Initialize()
        {
            FileType = FileGroupType.Others;
            ExtentionCount = _extentionManager.GetExtentionGroupCount(FileType);
            Name = ExtentionTypeHelper.GetFileGroupName(FileGroupType.Others);
        }

        /// <summary>
        /// ファイル拡張子グループ用初期化
        /// </summary>
        /// <param name="fileType">ファイル拡張子グループ</param>
        public void Initialize(FileGroupType fileType)
        {
            FileType = fileType;
            ExtentionCount = _extentionManager.GetExtentionGroupCount(FileType);
            Name = ExtentionTypeHelper.GetFileGroupName(FileType);
        }
        #endregion 初期化

        #region バインディング
        public FileGroupType FileType { get; set; }
        /// <summary>
        /// チェックボックスの状態
        /// </summary>
        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value);
                if (value == true)
                {
                    WeakReferenceMessenger.Default.Send(new ExtentionGroupChecked(true, _extentionManager.GetGroupExtentions(FileType)));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new ExtentionGroupChecked(false, _extentionManager.GetGroupExtentions(FileType)));
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
