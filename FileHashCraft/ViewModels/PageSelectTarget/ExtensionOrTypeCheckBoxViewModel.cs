/*  ExtensionOrTypeCheckBoxViewModel.cs

    拡張子、またはファイル拡張子グループのチェックボックスを扱うクラスです。
    ExtensionOrTypeCheckBoxBase を継承させた
    ExtensionCheckBox (拡張子チェックボックス) および 
    ExtentionGroupCheckBoxViewModel (拡張子グループチェックボックス) を利用します。
 */

using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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
        public bool? IsChecked { get; set; }
        /// <summary>
        /// 拡張子またはそのグループを参照する
        /// </summary>
        public string ExtentionOrGroup { get; }
    }
    #endregion インターフェース

    /// <summary>
    /// 拡張子チェックボックスと、拡張子グループチェックボックスの基底クラス
    /// </summary>
    public class ExtensionOrTypeCheckBoxBase : ObservableObject, IExtensionOrTypeCheckBoxBase
    {
        #region コンストラクタ
        protected readonly IScannedFilesManager _scannedFilesManager;
        protected readonly IMessageServices _messageServices;
        protected readonly ISettingsService _settingsService;
        protected readonly IExtentionManager _extentionManager;
        protected readonly IPageSelectTargetViewModel _pageSelectTargetFileViewModel;
        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected ExtensionOrTypeCheckBoxBase()
        {
            _scannedFilesManager = Ioc.Default.GetService<IScannedFilesManager>() ?? throw new InvalidOperationException($"{nameof(IScannedFilesManager)} dependency not resolved.");
            _messageServices = Ioc.Default.GetService<IMessageServices>() ?? throw new InvalidOperationException($"{nameof(IMessageServices)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");
            _extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new InvalidOperationException($"{nameof(IExtentionManager)} dependency not resolved.");
            _pageSelectTargetFileViewModel = Ioc.Default.GetService<IPageSelectTargetViewModel>() ?? throw new InvalidOperationException($"{nameof(IPageSelectTargetViewModel)} dependency not resolved.");

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
                if (string.IsNullOrEmpty(ExtentionOrGroup))
                {
                    return $"{Resources.NoHaveExtentions} ({ExtentionCount})";
                }
                return $"{ExtentionOrGroup} ({ExtentionCount})";
            }
        }
        public int ExtentionCount { get; set; } = 0;

        /// <summary>
        /// 拡張子か拡張子グループの文字列
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
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }

                SetProperty(ref _CurrentFontFamily, value);
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
        /// <summary>
        /// チェックボックスのチェックの状態
        /// </summary>
        protected bool? _IsChecked = false;
        public virtual bool? IsChecked { get; set; }

        public virtual bool? IsCheckedForce { get; set; }
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
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new InvalidOperationException($"{nameof(IExtentionManager)} dependency not resolved.");
            ExtentionCount = extentionManager.GetExtentionsCount(extention);
        }

        public async Task HandleChecked()
        {
            _scannedFilesManager.AddCondition(SearchConditionType.Extention, ExtentionOrGroup);
            await Task.Run(() =>
            {
                foreach (var extentionFile in _scannedFilesManager.AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), ExtentionOrGroup, StringComparison.OrdinalIgnoreCase)))
                {
                    if (extentionFile.ConditionCount == 0)
                    {
                        _scannedFilesManager.AllConditionFiles.Add(extentionFile);
                    }
                    extentionFile.ConditionCount++;
                }
            });
            await _pageSelectTargetFileViewModel.ViewModelExtention.CheckExtentionReflectToGroup(ExtentionOrGroup);
            await _pageSelectTargetFileViewModel.ViewModelExtention.ExtentionCountChanged();
            await _pageSelectTargetFileViewModel.ViewModelMain.ChangeExtensionToListBox(ExtentionOrGroup, true);
        }

        public async Task HandleUnchecked()
        {
            _scannedFilesManager.RemoveCondition(SearchConditionType.Extention, ExtentionOrGroup);
            await Task.Run(() =>
            {
                foreach (var extentionFile in _scannedFilesManager.AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), ExtentionOrGroup, StringComparison.OrdinalIgnoreCase)))
                {
                    if (--extentionFile.ConditionCount == 0)
                    {
                        _scannedFilesManager.AllConditionFiles.Remove(extentionFile);
                    }
                }
            });
            await _pageSelectTargetFileViewModel.ViewModelExtention.UncheckExtentionReflectToGroup(ExtentionOrGroup);
            await _pageSelectTargetFileViewModel.ViewModelExtention.ExtentionCountChanged();
            await _pageSelectTargetFileViewModel.ViewModelMain.ChangeExtensionToListBox(ExtentionOrGroup, false);
        }

        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                SetProperty(ref _IsChecked, value);

                if (value == true)
                {
                    Task.Run(async() => await HandleChecked());
                }
                else
                {
                    Task.Run(async () => await HandleUnchecked());
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

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// 拡張子グループのチェックボックスを扱うクラス
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
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new InvalidOperationException($"{nameof(IExtentionManager)} dependency not resolved.");
            ExtentionCount = extentionManager.GetExtentionGroupCount(FileType);
            ExtentionOrGroup = ExtentionTypeHelper.GetFileGroupName(FileGroupType.Others);
        }
        /// <summary>
        /// ファイル拡張子グループ用のコンストラクタ
        /// </summary>
        /// <param name="fileType">ファイルの拡張子グループ</param>
        public ExtentionGroupCheckBoxViewModel(FileGroupType fileType) : base()
        {
            FileType = fileType;
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new InvalidOperationException($"{nameof(IExtentionManager)} dependency not resolved.");
            ExtentionCount = extentionManager.GetExtentionGroupCount(FileType);
            ExtentionOrGroup = ExtentionTypeHelper.GetFileGroupName(FileType);
        }
        #endregion コンストラクタ

        #region バインディング
        public FileGroupType FileType { get; }
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
                    _pageSelectTargetFileViewModel.ViewModelExtention.ChangeCheckBoxGroup(true, _extentionManager.GetGroupExtentions(FileType));
                }
                else
                {
                    _pageSelectTargetFileViewModel.ViewModelExtention.ChangeCheckBoxGroup(false, _extentionManager.GetGroupExtentions(FileType));
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
