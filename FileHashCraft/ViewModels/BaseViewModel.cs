using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        protected readonly IMessenger _messenger;
        protected readonly ISettingsService _settingsService;
        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public BaseViewModel() { throw new NotImplementedException(nameof(BaseViewModel)); }

        public BaseViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        )
        {
            _messenger = messenger;
            _settingsService = settingsService;

            _currentFontFamily = _settingsService.CurrentFont;
            _fontSize = _settingsService.FontSize;
            // フォント変更メッセージ受信
            _messenger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _messenger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
        }

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
    }
}
