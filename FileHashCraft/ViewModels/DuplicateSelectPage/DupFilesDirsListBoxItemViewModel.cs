using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models;
using FileHashCraft.Services.Messages;
using FileHashCraft.Services;
using System.Windows.Media;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    public interface IDupFilesDirsListBoxItemViewModel;

    public partial class DupFilesDirsListBoxItemViewModel : ObservableObject, IDupFilesDirsListBoxItemViewModel
    {
        #region バインディング
        /// <summary>
        /// 重複ファイルがあるディレクトリのアイコン
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _Icon;

        /// <summary>
        /// 重複ファイルがあるディレクトリ名
        /// </summary>
        [ObservableProperty]
        private string _DuplicateDirectory = string.Empty;

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
                _settingsService.CurrentFont = value;
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
                _settingsService.FontSize = value;
            }
        }

        #endregion バインディング

        #region コンストラクタ
        protected readonly IMessenger _messenger;
        protected readonly ISettingsService _settingsService;

        public DupFilesDirsListBoxItemViewModel() { throw new NotImplementedException(nameof(DupFilesDirsListBoxItemViewModel)); }

        public DupFilesDirsListBoxItemViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        )
        {
            _messenger = messenger;
            _settingsService = settingsService;

            // フォント変更メッセージ受信
            _messenger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            _messenger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
        }
        #endregion コンストラクタ
    }
}
