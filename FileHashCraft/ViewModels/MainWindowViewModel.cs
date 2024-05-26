/*  MainWindowViewModel.cs

    メインウィンドウの ViewModel を提供します。
 */
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IMainWindowViewModel;
    #endregion インターフェース
    public class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        #region 初期設定
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;

        public MainWindowViewModel() { throw new NotImplementedException(); }

        public MainWindowViewModel(
            ISettingsService settingsService,
            IMessageServices messageServices)
        {
            _settingsService = settingsService;
            _messageServices = messageServices;

            // 設定を読み込む
            _settingsService.LoadSettings();
            Top = _settingsService.Top;
            Left = _settingsService.Left;
            Width = _settingsService.Width;
            Height = _settingsService.Height;
            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;

            WeakReferenceMessenger.Default.Register<WindowTopChanged>(this, (_, m) => Top = m.Top);
            WeakReferenceMessenger.Default.Register<WindowLeftChanged>(this, (_, m) => Left = m.Left);
            WeakReferenceMessenger.Default.Register<WindowWidthChanged>(this, (_, m) => Width = m.Width);
            WeakReferenceMessenger.Default.Register<WindowHeightChanged>(this, (_, m) => Height = m.Height);
        }
        #endregion 初期設定

        #region データバインディング
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        private double _Top = 100d;
        public double Top
        {
            get => _Top;
            set
            {
                if (value == _Top) { return; }
                SetProperty(ref _Top, value);
                _messageServices.SendWindowTop(value);
            }
        }
        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        private double _Left = 100d;
        public double Left
        {
            get => _Left;
            set
            {
                if (value == _Left) { return; }
                SetProperty(ref _Left, value);
                _messageServices.SendWindowLeft(value);
            }
        }
        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 1500d;
        public double Width
        {
            get => _Width;
            set
            {
                if (value == _Width) { return; }
                SetProperty(ref _Width, value);
                _messageServices.SendWindowWidth(value);
            }
        }
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _Height = 800d;
        public double Height
        {
            get => _Height;
            set
            {
                if (value == _Height) { return; }
                SetProperty(ref _Height, value);
                _messageServices.SendWindowHeight(value);
            }
        }
        /// <summary>
        /// フォントの変更
        /// </summary>
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (value == _CurrentFontFamily) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _messageServices.SendCurrentFont(value);
            }
        }
        /// <summary>
        /// フォントサイズの変更
        /// </summary>
        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (value == _FontSize) { return; }
                SetProperty(ref _FontSize, value, nameof(FontSize));
                _messageServices.SendFontSize(value);
            }
        }
        #endregion データバインディング
    }
}
