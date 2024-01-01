using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FileHashCraft.ViewModels
{
    public interface IPageTargetFileSelectViewModel
    {
    }

    public class PageTargetFileSelectViewModel : ObservableObject, IPageTargetFileSelectViewModel
    {
        #region バインディング
        /// <summary>
        /// フォントの設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _MainWindowViewModel.UsingFont;
            set
            {
                _MainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        public double FontSize
        {
            get => _MainWindowViewModel.FontSize;
            set
            {
                _MainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        /// <summary>
        /// 設定画面を開く
        /// </summary>
        public DelegateCommand SettingsOpen { get; set; }

        /// <summary>
        /// デバッグウィンドウを開く
        /// </summary>
        public DelegateCommand DebugOpen { get; set; }

        /// <summary>
        /// エクスプローラー画面に戻る
        /// </summary>
        public DelegateCommand ToPageExplorer { get; set; }

        /// <summary>
        /// ハッシュ計算画面に移動する
        /// </summary>
        public DelegateCommand ToPageHashCalcing { get; set; }
        #endregion バインディング

        private readonly IMainWindowViewModel _MainWindowViewModel;

        public PageTargetFileSelectViewModel() { throw new NotImplementedException(); }

        public PageTargetFileSelectViewModel(
            IMainWindowViewModel mainWindowViewModel)
        {
            _MainWindowViewModel = mainWindowViewModel;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new DelegateCommand(() =>
                WeakReferenceMessenger.Default.Send(new ToPageSetting(ReturnPageEnum.PageTargetFileSelect)));

            // デバッグウィンドウを開くコマンド
            DebugOpen = new DelegateCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });

            // エクスプローラー風画面に移動するコマンド
            ToPageExplorer = new DelegateCommand(() =>
                WeakReferenceMessenger.Default.Send(new ToPageExplorer()));

            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new DelegateCommand(() =>
                WeakReferenceMessenger.Default.Send(new ToPageHashCalcing()));

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) => UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) => FontSize = message.FontSize);
        }
    }
}
