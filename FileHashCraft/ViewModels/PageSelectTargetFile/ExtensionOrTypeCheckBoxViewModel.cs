﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.Properties;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
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

    /// <summary>
    /// 拡張子チェックボックスと、ファイル種類チェックボックスの基底クラス
    /// </summary>
    public class ExtensionOrTypeCheckBoxBase : ObservableObject, IExtensionOrTypeCheckBoxBase
    {
        protected readonly IPageSelectTargetFileViewModel _pageSelectTargetFileViewModel;
        protected readonly IScanHashFilesClass _scanHashFilesClass;

        /// <summary>
        /// 必ず通すサービスロケータによる依存性注入
        /// </summary>
        /// <exception cref="InvalidOperationException">インターフェースがnullという異常発生</exception>
        protected ExtensionOrTypeCheckBoxBase()
        {
            _pageSelectTargetFileViewModel = Ioc.Default.GetService<IPageSelectTargetFileViewModel>() ?? throw new InvalidOperationException($"{nameof(IPageSelectTargetFileViewModel)} dependency not resolved.");
            _scanHashFilesClass = Ioc.Default.GetService<IScanHashFilesClass>() ?? throw new InvalidOperationException($"{nameof(IScanHashFilesClass)} dependency not resolved.");
        }
        /// <summary>
        /// チェックボックスから表示する文字列
        /// </summary>
        public string ExtentionOrFileTypeView
        {
            get
            {
                if (string.IsNullOrEmpty(ExtentionOrGroup))
                {
                    return $"{Resources.NoHaveExtentions} ({_ExtentionCount})";
                }
                return $"{ExtentionOrGroup} ({_ExtentionCount})";
            }
        }

        /// <summary>
        /// 拡張子の数を取得する
        /// </summary>
        protected int _ExtentionCount = 0;
        public int ExtentionCount { get => _ExtentionCount; }

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
        /// 現在のハッシュアルゴリズム
        /// </summary>
        private FileHashAlgorithm _CurrentHashAlgorithm = FileHashAlgorithm.SHA256;
        public FileHashAlgorithm CurrentHashAlgorithm
        {
            get => _CurrentHashAlgorithm;
            set
            {
                if (_CurrentHashAlgorithm != value)
                {
                    _CurrentHashAlgorithm = value;
                    // TODO : ハッシュ切り替えられた時の処理を入れる
                }
            }
        }

        /// <summary>
        /// チェックボックスのチェックの状態
        /// </summary>
        protected bool? _IsChecked = false;
        public virtual bool? IsChecked { get; set; }
    }
    //----------------------------------------------------------------------------------------
    /// <summary>
    /// ファイル拡張子のチェックボックスを扱うクラス
    /// </summary>
    public class ExtensionCheckBox : ExtensionOrTypeCheckBoxBase
    {
        /// <summary>
        /// 拡張子と利用しているハッシュアルゴリズムを設定します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <param name="fileHashAlgorithm">ファイルのハッシュアルゴリズム</param>
        public ExtensionCheckBox(string extention, FileHashAlgorithm fileHashAlgorithm) : base()
        {
            SetExtention(extention);
            CurrentHashAlgorithm = fileHashAlgorithm;
        }

        /// <summary>
        /// 拡張子を設定し、該当するファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        private void SetExtention(string extention)
        {
            ExtentionOrGroup = extention;
            _ExtentionCount = FileHashManager.Instance.GetExtentionsCount(extention, CurrentHashAlgorithm);
        }

        public override bool? IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value == true)
                {
                    _pageSelectTargetFileViewModel.ChangeExtentionCount(_ExtentionCount);
                }
                else
                {
                    _pageSelectTargetFileViewModel.ChangeExtentionCount(-_ExtentionCount);
                }
                SetProperty(ref _IsChecked, value);
            }
        }
    }

    //----------------------------------------------------------------------------------------
    /// <summary>
    /// ファイル種類のチェックボックスを扱うクラス
    /// </summary>
    public class ExtentionGroupCheckBoxViewModel : ExtensionOrTypeCheckBoxBase
    {
        /// <summary>
        /// その他のドキュメント専用のコンストラクタ
        /// </summary>
        /// <param name="fileHashAlgorithm">ファイルのハッシュアルゴリズム</param>
        public ExtentionGroupCheckBoxViewModel(FileHashAlgorithm fileHashAlgorithm) : base()
        {
            CurrentHashAlgorithm = fileHashAlgorithm;
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(FileGroupType.Others);
            _currentFileType = FileGroupType.Others;

            _extensionList = FileHashManager.Instance.GetExtensions(fileHashAlgorithm).ToList();

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
                { FileGroupType.SourceCodes, fileTypeHelper.GetFileGroupExtention(FileGroupType.SourceCodes) }
            };

            // 各種類の拡張子を一括で除外
            foreach (var kvp in exclusionsByFileType)
            {
                _extensionList.RemoveAll(extension => kvp.Value.Contains(extension));
            }
            _ExtentionCount = 0;
            foreach (var extension in _extensionList)
            {
                _ExtentionCount += FileHashManager.Instance.GetExtentionsCount(extension, fileHashAlgorithm);
            }
        }
        /// <summary>
        /// ファイルの種類用のコンストラクタ
        /// </summary>
        /// <param name="fileHashAlgorithm">ファイルのハッシュアルゴリズム</param>
        /// <param name="fileType">ファイルの種類</param>
        public ExtentionGroupCheckBoxViewModel(FileHashAlgorithm fileHashAlgorithm, FileGroupType fileType) : base()
        {
            CurrentHashAlgorithm = fileHashAlgorithm;

            var fileTypeHelper = new FileTypeHelper();
            _extensionList = fileTypeHelper.GetFileGroupExtention(fileType);
            _ExtentionCount = _extensionList.Count;
            ExtentionOrGroup = FileTypeHelper.GetFileGroupName(fileType);
            SetFileTypeCount(fileType);
        }

        /// <summary>
        /// ファイルの種類に対応するファイルの数を取得します。
        /// </summary>
        /// <param name="fileType">ファイルの種類</param>
        private void SetFileTypeCount(FileGroupType fileType)
        {
            var fileTypeHelper = new FileTypeHelper();
            _extensionList = fileTypeHelper.GetFileGroupExtention(fileType);
            _ExtentionCount = 0;
            _currentFileType = fileType;
            foreach (var extention in _extensionList)
            {
                _ExtentionCount += FileHashManager.Instance.GetExtentionsCount(extention, CurrentHashAlgorithm);
            }
        }

        private FileGroupType _currentFileType = FileGroupType.Others;
        private List<string> _extensionList = [];
        //private readonly List<string> IgnoreExtensionList = [];
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
                    _pageSelectTargetFileViewModel.ChangeCheckBoxGroupChanged(true, _extensionList);
                }
                else
                {
                    _pageSelectTargetFileViewModel.ChangeCheckBoxGroupChanged(false, _extensionList);
                }
                SetProperty(ref _IsChecked, value);
            }
        }
    }
}