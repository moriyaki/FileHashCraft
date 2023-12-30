using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FilOps.ViewModels.ExplorerPage;
using FilOps.Views;

namespace FilOps.ViewModels
{
    public interface IMainViewModel 
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width {  get; set; }
        public double Height { get; set; }
        public double FontSize { get; set; }
        public FontFamily Font { get; set; }
        public IEnumerable<double> GetSelectableFontSize();
        public void FontSizePlus();
        public void FontSizeMinus();
    }

    public class MainViewModel : ObservableObject, IMainViewModel
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
        /// </summary>
        private FontFamily _Font = SystemFonts.MessageFontFamily;
        public FontFamily Font
        {
            get => _Font;
            set
            {
                SetProperty(ref _Font, value);
                WeakReferenceMessenger.Default.Send(new FontChanged(value));
            }
        }

        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                SetProperty(ref _FontSize, value, nameof(FontSize));
                WeakReferenceMessenger.Default.Send(new FontSizeChanged(value));
            }
        }

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

        /// <summary>
        /// 設定できるフォントサイズ
        /// </summary>
        private readonly List<double> FontSizes =
            [8d, 9d, 10d, 10.5d, 11d, 12d, 13d, 14d, 15d, 16d, 18d, 20d, 21d, 22d, 24d];

        /// <summary>
        /// フォントサイズを取得する
        /// </summary>
        /// <returns></returns>
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
