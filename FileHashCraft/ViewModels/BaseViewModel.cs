using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        protected readonly IMessenger _Messanger;
        protected readonly ISettingsService _SettingsService;

        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public BaseViewModel()
        { throw new NotImplementedException(nameof(BaseViewModel)); }

        public BaseViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        )
        {
            _Messanger = messenger;
            _SettingsService = settingsService;

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;
            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);
        }

        private FontFamily _CurrentFontFamily;

        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _SettingsService.CurrentFont = value;
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
                _SettingsService.FontSize = value;
            }
        }
    }
}