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
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public class HashListFileItems : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        public HashListFileItems()
        {
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");

            _currentFontFamily = _settingsService.CurrentFont;
            _fontSize = _settingsService.FontSize;

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChangedMessage>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChangedMessage>(this, (_, m)
                => FontSize = m.FontSize);
        }

        #region バインディング
        private string _fullPathFileName = string.Empty;
        public string FileFullPath
        {
            get => _fullPathFileName;
            set
            {
                SetProperty(ref _fullPathFileName, value);
                Icon = WindowsAPI.GetIcon(value);
            }
        }

        private BitmapSource? _icon = null;
        public BitmapSource? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string FileName
        {
            get => Path.GetFileName(_fullPathFileName);
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
        private SolidColorBrush _hashTargetColor = new(Colors.Transparent);
        public SolidColorBrush HashTargetColor
        {
            get => _hashTargetColor;
            set => SetProperty(ref _hashTargetColor, value);
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _currentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _currentFontFamily;
            set
            {
                if (_currentFontFamily.Source == value.Source) { return; }

                SetProperty(ref _currentFontFamily, value);
                _settingsService.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _fontSize;
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) { return; }

                SetProperty(ref _fontSize, value);
                _settingsService.SendFontSize(value);
            }
        }
        #endregion バインディング
    }
 }
