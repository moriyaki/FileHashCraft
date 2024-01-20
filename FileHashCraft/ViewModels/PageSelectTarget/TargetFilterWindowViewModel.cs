using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface ITargetFilterWindowViewModel
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
    public class TargetFilterWindowViewModel : ObservableObject, ITargetFilterWindowViewModel
    {
        #region バインディング
        /// <summary>
        /// 画面の上端設定
        /// </summary>
        private double _Top = 450d;
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        public double _Left = 400d;
        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _Width = 400d;
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
        /// フォントの取得と設定
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
        /// フォントサイズの取得と設定
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
        #endregion バインディング

        #region コンストラクタと初期化
        /// <summary>
        /// ICheckedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly IMainWindowViewModel _MainWindowViewModel;
        public TargetFilterWindowViewModel(
            IMainWindowViewModel mainWindowViewModel)
        {
            _MainWindowViewModel = mainWindowViewModel;

            Top = _MainWindowViewModel.Top;
            Left = _MainWindowViewModel.Left + mainWindowViewModel.Width;
            Width = _MainWindowViewModel.Width / 2;
            Height = _MainWindowViewModel.Height;
        }
        #endregion コンストラクタと初期化
    }
}
