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
    }

    public class MainViewModel : ViewModelBase, IMainViewModel
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
        /// フォントサイズの変更
        /// </summary>
        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (8 <= value && value <= 24)
                {
                    SetProperty(ref _FontSize, value, nameof(FontSize));
                    WeakReferenceMessenger.Default.Send(new FontChanged(value));
                }
            }
        }

        private FontFamily _Font = SystemFonts.MessageFontFamily;
        public FontFamily Font
        {
            get => _Font;
            set => SetProperty(ref _Font, value);
        }
        #endregion データバインディング

        /*
        private Uri? _CurrentPageUri;

        public Uri? CurrentPageUri
        {
            get => _CurrentPageUri;
            set => SetProperty(ref _CurrentPageUri, value);
        }
        public void ToExplorerPage()
        {
            CurrentPageUri = new Uri("ExplorerPage.xaml", UriKind.Relative);
        }
        */
    }
}
