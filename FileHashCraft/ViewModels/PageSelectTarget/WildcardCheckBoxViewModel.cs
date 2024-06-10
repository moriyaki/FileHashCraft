using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHashCraft.Services.Messages;
using FileHashCraft.Services;
using CefSharp.DevTools.Page;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IWildcardCheckBoxViewModel
    {
        /// <summary>
        /// チェックボックス状態を参照または変更する
        /// </summary>
        bool IsChecked { get; set; }

        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        string WildcardCriteria { get; set; }
    }

    public class WildcardCheckBoxViewModel : ObservableObject, IWildcardCheckBoxViewModel
    {
        #region バインディング
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

        /// <summary>
        /// チェックボックス
        /// </summary>
        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }

        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        private string _WildcardCriteria = string.Empty;
        public string WildcardCriteria
        {
            get => _WildcardCriteria;
            set => SetProperty(ref _WildcardCriteria, value);
        }
        #endregion バインディング

        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;

        public WildcardCheckBoxViewModel(
            IMessageServices messageServices,
            ISettingsService settingsService
        )
        {
            _messageServices = messageServices;
            _settingsService = settingsService;

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m) => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m) => FontSize = m.FontSize);

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
        }
        #endregion コンストラクタ
    }
}
