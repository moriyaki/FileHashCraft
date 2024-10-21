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
        private BitmapSource? _icon;

        /// <summary>
        /// 重複ファイルがあるディレクトリ名
        /// </summary>
        [ObservableProperty]
        private string _duplicateDirectory = string.Empty;

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _currentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _currentFontFamily;
            set
            {
                if (_currentFontFamily.Source == value.Source) { return; }

                SetProperty(ref _currentFontFamily, value);
                _settingsService.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _fontSize;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) { return; }

                SetProperty(ref _fontSize, value);
                _settingsService.SendFontSize(value);
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

            _currentFontFamily = _settingsService.CurrentFont;
            _fontSize = _settingsService.FontSize;
        }
        #endregion コンストラクタ
    }
}
