using System.DirectoryServices.ActiveDirectory;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels
{
    public interface IMainViewModel 
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width {  get; set; }
        public double Height { get; set; }
        public double FontSize { get; set; }
    }

    public class MainViewModel : ObservableObject, IMainViewModel
    {
        public MainViewModel() { }

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
                }
            }
        }
    }
}
