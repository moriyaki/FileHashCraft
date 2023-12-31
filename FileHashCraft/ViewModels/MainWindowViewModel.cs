using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.ViewModels.ExplorerPage;
using FileHashCraft.Views;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IMainWindowViewModel
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width {  get; set; }
        public double Height { get; set; }
        public double FontSize { get; set; }
        public FontFamily UsingFont { get; set; }
        /// <summary>
        /// フォントサイズを取得する
        /// </summary>
        /// <returns>設定できるフォントサイズリスト</returns>
        public IEnumerable<double> GetSelectableFontSize();

        /// <summary>
        /// フォントサイズを大きくする
        /// </summary>
        public void FontSizePlus();
        /// <summary>
        /// フォントサイズを小さくする
        /// </summary>
        public void FontSizeMinus();
    }
    #endregion インターフェース

    public class MainWindowViewModel : ObservableObject, IMainWindowViewModel
    {
        #region データバインディング
        /// <summary>
        /// ウィンドウの開始上位置
        /// </summary>
        private double _Top = 100d;
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }

        /// <summary>
        /// ウィンドウの開始左位置
        /// </summary>
        private double _Left = 100d;
        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 1200d;
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _Height = 800d;
        public double Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }

        /// <summary>
        /// フォントの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        private FontFamily _UsingFont = SystemFonts.MessageFontFamily;
        public FontFamily UsingFont
        {
            get => _UsingFont;
            set
            {
                if (value != _UsingFont)
                {
                    SetProperty(ref _UsingFont, value);
                    WeakReferenceMessenger.Default.Send(new FontChanged(value));

                }
            }
        }

        /// <summary>
        /// フォントサイズの変更
        /// MainWindowを参照できる所には以下のコード
        /// </summary>
        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (value != _FontSize)
                {
                    SetProperty(ref _FontSize, value, nameof(FontSize));
                    WeakReferenceMessenger.Default.Send(new FontSizeChanged(value));

                }
            }
        }
 
        /// <summary>
        /// ハッシュ計算アルゴリズムの変更
        /// </summary>
        private string _HashAlgorithm = "SHA-256";
        public string HashAlgorithm
        {
            get => _HashAlgorithm;
            set
            {
                SetProperty(ref _HashAlgorithm, value);
            }
        }
        #endregion データバインディング

        /*
        フォント関連の XAML はこう
        FontFamily="{Binding UsingFont}" FontSize="{Binding FontSize}"

        ViewModel ではこう

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
         */

        /// <summary>
        /// 設定できるフォントサイズ
        /// </summary>
        private readonly List<double> FontSizes =
            [8d, 9d, 10d, 10.5d, 11d, 12d, 13d, 14d, 15d, 16d, 18d, 20d, 21d, 22d, 24d];

        /// <summary>
        /// フォントサイズを取得する
        /// </summary>
        /// <returns>設定できるフォントサイズリスト</returns>
        public IEnumerable<double> GetSelectableFontSize()
        {
            foreach (var fontSize in FontSizes) { yield return fontSize; }
        }

        /// <summary>
        /// フォントサイズを大きくする
        /// </summary>
        public void FontSizePlus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == FontSizes.Count - 1) return;

            FontSize = FontSizes[index + 1];
        }
        /// <summary>
        /// フォントサイズを小さくする
        /// </summary>
        public void FontSizeMinus()
        {
            var index = FontSizes.IndexOf(FontSize);
            if (index == 0) return;

            FontSize = FontSizes[index - 1];
        }

    }
}
