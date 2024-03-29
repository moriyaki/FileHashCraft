﻿/*  HashListFileItemsViewModel.cs

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

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public class HashListFileItems : ObservableObject
    {
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        public HashListFileItems()
        {
            _messageServices = Ioc.Default.GetService<IMessageServices>() ?? throw new InvalidOperationException($"{nameof(IMessageServices)} dependency not resolved.");
            _settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new InvalidOperationException($"{nameof(ISettingsService)} dependency not resolved.");

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);
            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m)
                => FontSize = m.FontSize);
        }

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
                _messageServices.SendCurrentFont(value);
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
                _messageServices.SendFontSize(value);
            }
        }
        #endregion バインディング
    }
 }
