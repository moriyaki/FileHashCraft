using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModelWildcard;
    #endregion インターフェース

    public class PageSelectTargetViewModelWildcard : ObservableObject, IPageSelectTargetViewModelWildcard
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索文字列
        /// </summary>
        private string _WildcardSearch = string.Empty;
        public string WildcardSearch
        {
            get => _WildcardSearch;
            set => SetProperty(ref _WildcardSearch, value);
        }

        /// <summary>
        /// ヘルプの実行
        /// </summary>
        public RelayCommand WildcardHelpOpen { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        private readonly IHelpWindowViewModel _helpWindowViewModel;

        public PageSelectTargetViewModelWildcard()
        {
            throw new NotImplementedException("PageSelectTargetViewModelWildcard");
        }

        public PageSelectTargetViewModelWildcard(
            IMessageServices messageServices,
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel)
        {
            _messageServices = messageServices;
            _settingsService = settingsService;
            _helpWindowViewModel = helpWindowViewModel;

            WildcardHelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカードを含むファイル名をRegex型に変換する
        /// </summary>
        /// <param name="WildcardPattern">ワイルドカードを含むファイル名</param>
        /// <returns>Regex型</returns>
        public static Regex WildcardToRegexPattern(string WildcardPattern)
        {
            if (WildcardPattern.Contains('.'))
            {
                // ワイルドカードに拡張子がある時の処理
                var fileName = Path.GetFileNameWithoutExtension(WildcardPattern);
                var fileNamePattern = Regex.Escape(fileName).Replace("\\*", ".*").Replace("\\?", ".");

                var fileExtention = Path.GetExtension(WildcardPattern);
                if (fileExtention == ".*")
                {
                    // 拡張子全ての時、例えば"file"と"file.txt"の両方をヒットさせる
                    return new Regex("^" + fileNamePattern + "(?:\\..+)?$", RegexOptions.IgnoreCase);
                }
                else
                {
                    var extensionPattern = Regex.Escape(fileExtention).Replace("\\*", ".*").Replace("\\?", ".");
                    return new Regex("^" + fileNamePattern + extensionPattern + "$", RegexOptions.IgnoreCase);
                }
            }
            else
            {
                // ワイルドカードに拡張子がない時の処理
                var fileNamePattern = Regex.Escape(WildcardPattern).Replace("\\*", ".*").Replace("\\?", ".");
                return new Regex("^" + fileNamePattern + "$", RegexOptions.IgnoreCase);
            }
        }
    }
}
