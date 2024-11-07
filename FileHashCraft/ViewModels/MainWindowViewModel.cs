/*  MainWindowViewModel.cs

    メインウィンドウの ViewModel を提供します。
 */

using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;

namespace FileHashCraft.ViewModels
{
    #region インターフェース

    public interface IMainWindowViewModel;

    #endregion インターフェース

    public class MainWindowViewModel : BaseViewModel, IMainWindowViewModel
    {
        #region 初期設定

        public MainWindowViewModel() : base()
        {
        }

        public MainWindowViewModel(
            IMessenger messenger,
            ISettingsService settingsService) : base(messenger, settingsService)
        {
            // 設定を読み込む
            _SettingsService.LoadSettings();
            Top = _SettingsService.Top;
            Left = _SettingsService.Left;
            Width = _SettingsService.Width;
            Height = _SettingsService.Height;
        }

        #endregion 初期設定

        #region バインディング

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
                _SettingsService.Top = value;
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
                _SettingsService.Left = value;
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
                _SettingsService.Width = value;
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
                _SettingsService.Height = value;
            }
        }

        #endregion バインディング
    }
}