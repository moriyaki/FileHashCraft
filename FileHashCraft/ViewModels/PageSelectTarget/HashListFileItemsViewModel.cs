/*  HashListFileItemsViewModel.cs

    ハッシュ対象を表示するリストボックスのアイテム ViewModel です。
*/
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public class HashListFileItems : ObservableObject
    {
        #region バインディング
        private string _FullPathFileName = string.Empty;
        public string FileFullPath
        {
            get => _FullPathFileName;
            set
            {
                SetProperty(ref _FullPathFileName, value);
                Icon = WindowsAPI.GetIcon(value);
            }
        }

        private BitmapSource? _Icon = null;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        public string FileName
        {
            get => Path.GetFileName(_FullPathFileName);
        }

        private bool _IsHashTarget = false;
        public bool IsHashTarget
        {
            get => _IsHashTarget;
            set => SetProperty(ref _IsHashTarget, value);
        }

        /// <summary>
        /// 背景色：ハッシュ検索対象になっていたら変更される
        /// </summary>
        private SolidColorBrush _HashTargetColor = new(Colors.LightCyan);
        public SolidColorBrush HashTargetColor
        {
            get => _HashTargetColor;
            set => SetProperty(ref _HashTargetColor, value);
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

        private readonly IMainWindowViewModel _MainWindowViewModel;
        public HashListFileItems()
        {
            _MainWindowViewModel = Ioc.Default.GetService<IMainWindowViewModel>() ?? throw new NullReferenceException(nameof(IMainWindowViewModel));
        }
    }
 }
