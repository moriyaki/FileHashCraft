using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.Properties;

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

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected ExtensionOrTypeCheckBoxBase()
        {
            _ExtentionManager = Ioc.Default.GetService<IExtentionHelper>() ?? throw new InvalidOperationException($"{nameof(IExtentionHelper)} dependency not resolved.");
            _SearchManager = Ioc.Default.GetService<ISearchManager>() ?? throw new InvalidOperationException($"{nameof(ISearchManager)} dependency not resolved.");
            _PageSelectTargetFileViewModel = Ioc.Default.GetService<IPageSelectTargetViewModel>() ?? throw new InvalidOperationException($"{nameof(IPageSelectTargetViewModel)} dependency not resolved.");
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
                    _SearchManager.AddCondition(SearchConditionType.Extention, ExtentionOrGroup);
                    _PageSelectTargetFileViewModel.ExtentionCountChanged();
                }
                else
                {
                    _SearchManager.RemoveCondition(SearchConditionType.Extention, ExtentionOrGroup);
                    _PageSelectTargetFileViewModel.ExtentionCountChanged();
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
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(FileGroupType.Others);

            _extensionList = _ExtentionManager.GetExtensions().ToList();

            var fileTypeHelper = new FileTypeHelper();
            // ファイルの種類ごとに除外する拡張子を取得
            var exclusionsByFileType = new Dictionary<FileGroupType, IEnumerable<string>>
            {
                { FileGroupType.Movies, fileTypeHelper.GetFileGroupExtention(FileGroupType.Movies) },
                { FileGroupType.Pictures, fileTypeHelper.GetFileGroupExtention(FileGroupType.Pictures) },
                { FileGroupType.Sounds, fileTypeHelper.GetFileGroupExtention(FileGroupType.Sounds) },
                { FileGroupType.Documents, fileTypeHelper.GetFileGroupExtention(FileGroupType.Documents) },
                { FileGroupType.Applications, fileTypeHelper.GetFileGroupExtention(FileGroupType.Applications) },
                { FileGroupType.Archives, fileTypeHelper.GetFileGroupExtention(FileGroupType.Archives) },
                { FileGroupType.SourceCodes, fileTypeHelper.GetFileGroupExtention(FileGroupType.SourceCodes) },
                { FileGroupType.Registrations, fileTypeHelper.GetFileGroupExtention(FileGroupType.Registrations) },
            };

            // 各種類の拡張子を一括で除外
            foreach (var kvp in exclusionsByFileType)
            {
                _extensionList.RemoveAll(extension => kvp.Value.Contains(extension));
            }
            ExtentionCount = 0;
            foreach (var extension in _extensionList)
            {
                ExtentionCount += _ExtentionManager.GetExtentionsCount(extension);
            }
        }
        /// <summary>
        /// ファイルの種類用のコンストラクタ
        /// </summary>
        /// <param name="fileType">ファイルの種類</param>
        public ExtentionGroupCheckBoxViewModel(FileGroupType fileType) : base()
        {
            var fileTypeHelper = new FileTypeHelper();
            _extensionList = fileTypeHelper.GetFileGroupExtention(fileType);
            ExtentionCount = 0;
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(fileType);
            foreach (var extention in _extensionList)
            {
                ExtentionCount += _ExtentionManager.GetExtentionsCount(extention);
                DebugManager.InfoWrite(extention);
            }
        }
        #endregion コンストラクタ

        #region バインディング
        private readonly List<string> _extensionList = [];
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
                    _PageSelectTargetFileViewModel.ChangeCheckBoxGroup(true, _extensionList);
                }
                else
                {
                    _PageSelectTargetFileViewModel.ChangeCheckBoxGroup(false, _extensionList);
                }
                SetProperty(ref _IsChecked, value);
            }
        }
        #endregion バインディング
    }
}
