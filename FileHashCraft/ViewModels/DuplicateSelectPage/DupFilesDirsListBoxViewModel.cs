using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    #region インターフェース

    public interface IDupFilesDirsListBoxViewModel
    {
        /// <summary>
        /// 重複ファイルを持つディレクトリのコレクション
        /// </summary>
        ObservableCollection<DupFilesDirsListBoxItemViewModel> DuplicateDirectoryCollection { get; set; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize();
    }

    #endregion インターフェース

    public class DupFilesDirsListBoxViewModel : BaseViewModel, IDupFilesDirsListBoxViewModel
    {
        #region バインディング

        /// <summary>
        /// 重複ファイルを持つディレクトリのコレクション
        /// </summary>
        public ObservableCollection<DupFilesDirsListBoxItemViewModel> DuplicateDirectoryCollection { get; set; } = [];

        /// <summary>
        /// 重複ファイルを含むディレクトリ一覧ディレクトリの幅
        /// </summary>
        private double _DupFilesDirsListBoxWidth;

        public double DupFilesDirsListBoxWidth
        {
            get => _DupFilesDirsListBoxWidth;
            set
            {
                if (_DupFilesDirsListBoxWidth == value) return;
                SetProperty(ref _DupFilesDirsListBoxWidth, value);
                _SettingsService.DupFilesDirsListBoxWidth = value;
            }
        }

        private int _SelectedDuplicateDirectoryIndex = -1;

        public int SelectedDuplicateDirectoryIndex
        {
            get => _SelectedDuplicateDirectoryIndex;
            set
            {
                SetProperty(ref _SelectedDuplicateDirectoryIndex, value);
                _DupFilesManager.GetDuplicateFiles(DuplicateDirectoryCollection[value].DuplicateDirectory);
            }
        }

        #endregion バインディング

        #region コンストラクタと初期化

        private readonly IDuplicateFilesManager _DupFilesManager;

        public DupFilesDirsListBoxViewModel()
        { throw new NotImplementedException(nameof(DupFilesDirsListBoxViewModel)); }

        public DupFilesDirsListBoxViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IDuplicateFilesManager dupFilesManager
        ) : base(messenger, settingsService)
        {
            _DupFilesManager = dupFilesManager;
            _DupFilesDirsListBoxWidth = settingsService.DupFilesDirsListBoxWidth;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            var directories = _DupFilesManager.GetDirectories();
            foreach (var dir in directories)
            {
                var item = new DupFilesDirsListBoxItemViewModel()
                {
                    Icon = WindowsAPI.GetIcon(dir),
                    DuplicateDirectory = dir,
                };
                DuplicateDirectoryCollection.Add(item);
            }
        }

        #endregion コンストラクタと初期化
    }
}