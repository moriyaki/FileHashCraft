using System.Collections.ObjectModel;
using System.Dynamic;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.DupSelectAndDelete;
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
                _settingsService.DupFilesDirsListBoxWidth = value;
            }
        }

        private int _SelectedDuplicateDirectoryIndex = -1;
        public int SelectedDuplicateDirectoryIndex
        {
            get => _SelectedDuplicateDirectoryIndex;
            set
            {
                SetProperty(ref _SelectedDuplicateDirectoryIndex, value);
                _dupFilesManager.GetDuplicateFiles(DuplicateDirectoryCollection[value].DuplicateDirectory);
            }
        }

        /// <summary>
        /// リストボックスのアイテムがクリックされた時のコマンド
        /// </summary>
        //public RelayCommand<object> DuplicateDirectoryClickedCommand { get; set; }
        #endregion バインディング

        #region コンストラクタと初期化
        private readonly IDupFilesManager _dupFilesManager;

        public DupFilesDirsListBoxViewModel() { throw new NotImplementedException(nameof(DupFilesDirsListBoxViewModel)); }

        public DupFilesDirsListBoxViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IDupFilesManager dupFilesManager
        ) : base(messenger, settingsService)
        {
            _dupFilesManager = dupFilesManager;
            _DupFilesDirsListBoxWidth = settingsService.DupFilesDirsListBoxWidth;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            var directories = _dupFilesManager.GetDirectories();
            foreach (var dir in directories)
            {
                var item = new DupFilesDirsListBoxItemViewModel()
                {
                    Icon = WindowsAPI.GetIcon(dir),
                    DuplicateDirectory = dir,
                };
                DuplicateDirectoryCollection.Add(item);
            }

            /*
            var path = @"C:\users\moriyaki\";
            var item = new DupFilesDirsListBoxItemViewModel(_messenger, _settingsService)
            {
                Icon = WindowsAPI.GetIcon(path),
                DuplicateDirectory = path
            };
            DuplicateDirectoryCollection.Add(item);
            */
        }
        #endregion コンストラクタと初期化
    }
}
