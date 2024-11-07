/*  HashListFileItemsViewModel.cs

    ハッシュ対象を表示するリストボックスのアイテム ViewModel です。
*/

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public partial class HashListFileItems : ObservableObject
    {
        private readonly IMessenger _Messanger;
        private readonly ISettingsService _SettingsService;

        public HashListFileItems()
        {
            _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            _SettingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));

            _CurrentFontFamily = _SettingsService.CurrentFont;
            _FontSize = _SettingsService.FontSize;

            // フォント変更メッセージ受信
            _Messanger.Register<CurrentFontFamilyChangedMessage>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            _Messanger.Register<FontSizeChangedMessage>(this, (_, m)
                => FontSize = m.FontSize);
        }

        #region バインディング

        /// <summary>
        /// ファイルのフルパス名
        /// </summary>
        private string _FileFullPath = string.Empty;

        public string FileFullPath
        {
            get => _FileFullPath;
            set
            {
                SetProperty(ref _FileFullPath, value);
                Icon = WindowsAPI.GetIcon(value);
                OnPropertyChanged(nameof(Icon));
            }
        }

        [ObservableProperty]
        private BitmapSource? _Icon = null;

        public string FileName
        {
            get => Path.GetFileName(_FileFullPath);
        }

        private bool _isHashTarget = false;

        public bool IsHashTarget
        {
            get => _isHashTarget;
            set
            {
                SetProperty(ref _isHashTarget, value);
                if (value)
                {
                    HashTargetColor = new(Colors.LightCyan);
                }
                else
                {
                    HashTargetColor = new(Colors.Transparent);
                }
            }
        }

        /// <summary>
        /// 背景色：ハッシュ検索対象になっていたら変更される
        /// </summary>
        private SolidColorBrush _HashTargetColor = new(Colors.Transparent);

        public SolidColorBrush HashTargetColor
        {
            get => _HashTargetColor;
            set => SetProperty(ref _HashTargetColor, value);
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _CurrentFontFamily;

        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _SettingsService.CurrentFont = value;
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _FontSize;

        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) { return; }
                SetProperty(ref _FontSize, value);
                _SettingsService.FontSize = value;
            }
        }

        #endregion バインディング
    }
}