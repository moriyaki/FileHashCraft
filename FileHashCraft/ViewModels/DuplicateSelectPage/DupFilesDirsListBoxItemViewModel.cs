using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

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

        #endregion バインディング

        #region コンストラクタ

        protected readonly IMessenger _Messanger;
        protected readonly ISettingsService _SettingsService;

        public DupFilesDirsListBoxItemViewModel()
        {
            _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            _SettingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));

            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m) => FontSize = m.FontSize);

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;
        }

        #endregion コンストラクタ
    }
}