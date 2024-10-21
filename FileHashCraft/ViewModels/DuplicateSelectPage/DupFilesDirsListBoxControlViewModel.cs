using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    #region インターフェース
    public interface IDupFilesDirsListBoxControlViewModel
    {
        ObservableCollection<DupFilesDirsListBoxItemViewModel> DuplicateDirectoryCollection { get; set; }
    }
    #endregion インターフェース

    public class DupFilesDirsListBoxControlViewModel : BaseViewModel, IDupFilesDirsListBoxControlViewModel
    {
        #region バインディング
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
                _settingsService.SendDupFilesDirsListBoxWidth(value);
            }
        }
        /// <summary>
        /// リストボックスのアイテムがクリックされた時のコマンド
        /// </summary>
        public RelayCommand<object> DuplicateDirectoryClickedCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        public DupFilesDirsListBoxControlViewModel() { throw new NotImplementedException(nameof(DupFilesDirsListBoxControlViewModel)); }

        public DupFilesDirsListBoxControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        ) : base(messenger, settingsService)
        {
            DuplicateDirectoryClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is DupFilesDirsListBoxItemViewModel checkBoxViewModel)
                {
                    if (parameter is DupFilesDirsListBoxItemViewModel item)
                    {
                        MessageBox.Show(item.DuplicateDirectory);
                    }
                }
            });

            var path = @"C:\users\moriyaki\";
            var item = new DupFilesDirsListBoxItemViewModel(_messenger, _settingsService)
            {
                Icon = WindowsAPI.GetIcon(path),
                DuplicateDirectory = path
            };
            DuplicateDirectoryCollection.Add(item);

            _DupFilesDirsListBoxWidth = settingsService.DupFilesDirsListBoxWidth;
        }
        #endregion コンストラクタ
    }
}
