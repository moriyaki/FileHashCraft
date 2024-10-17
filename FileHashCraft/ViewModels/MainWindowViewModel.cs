/*  MainWindowViewModel.cs

    メインウィンドウの ViewModel を提供します。
 */
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IMainWindowViewModel;
    #endregion インターフェース
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        #region 初期設定
        public MainWindowViewModel() :base() { }

        public MainWindowViewModel(
            IMessenger messenger,
            ISettingsService settingsService) : base(messenger, settingsService)
        {
            // 設定を読み込む
            _settingsService.LoadSettings();
            Top = _settingsService.Top;
            Left = _settingsService.Left;
            Width = _settingsService.Width;
            Height = _settingsService.Height;

            _messenger.Register<WindowTopChangedMessage>(this, (_, m) => Top = m.Top);
            _messenger.Register<WindowLeftChangedMessage>(this, (_, m) => Left = m.Left);
            _messenger.Register<WindowWidthChangedMessage>(this, (_, m) => Width = m.Width);
            _messenger.Register<WindowHeightChangedMessage>(this, (_, m) => Height = m.Height);
        }
        #endregion 初期設定

        #region バインディング
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        private double _top = 100d;
        public double Top
        {
            get => _top;
            set
            {
                if (value == _top) { return; }
                SetProperty(ref _top, value);
                _settingsService.SendWindowTop(value);
            }
        }
        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        private double _left = 100d;
        public double Left
        {
            get => _left;
            set
            {
                if (value == _left) { return; }
                SetProperty(ref _left, value);
                _settingsService.SendWindowLeft(value);
            }
        }
        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _width = 1500d;
        public double Width
        {
            get => _width;
            set
            {
                if (value == _width) { return; }
                SetProperty(ref _width, value);
                _settingsService.SendWindowWidth(value);
            }
        }
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _height = 800d;
        public double Height
        {
            get => _height;
            set
            {
                if (value == _height) { return; }
                SetProperty(ref _height, value);
                _settingsService.SendWindowHeight(value);
            }
        }
        #endregion バインディング
    }
}
