using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModelWildcard
    {
        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        ObservableCollection<WildcardCheckBoxViewModel> WildcardCollection { get; set; }
    }
    #endregion インターフェース

    public enum WildcardSearchErrorStatus
    {
        None,
        Empty,
        TooManyAsterisk,
        NotAllowedCharacter,
    }

    public class PageSelectTargetViewModelWildcard : ObservableObject, IPageSelectTargetViewModelWildcard
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        public ObservableCollection<WildcardCheckBoxViewModel> WildcardCollection { get; set; } = [];

        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        private WildcardSearchErrorStatus _WildcardSearchErrorStatus = WildcardSearchErrorStatus.None;
        public WildcardSearchErrorStatus WildcardSearchErrorStatus
        {
            get => _WildcardSearchErrorStatus;
            set
            {
                SetProperty(ref _WildcardSearchErrorStatus, value);
                switch (value)
                {
                    case WildcardSearchErrorStatus.None:
                        WildcardBackground = Brushes.Transparent;
                        WildcardErrorOutput = string.Empty;
                        break;
                    case WildcardSearchErrorStatus.Empty:
                        WildcardBackground = Brushes.Red;
                        WildcardErrorOutput = $"{Resources.LabelWildcardError_Empty}";
                        break;
                    case WildcardSearchErrorStatus.TooManyAsterisk:
                        WildcardBackground = Brushes.Red;
                        WildcardErrorOutput = $"{Resources.LabelWildcardError_TooManyAsterisk}";
                        break;
                    case WildcardSearchErrorStatus.NotAllowedCharacter:
                        WildcardBackground = Brushes.Red;
                        WildcardErrorOutput = $"{Resources.LabelWildcardError_NotAllowedCharacter}";
                        break;
                }
            }
        }

        /// <summary>
        /// ワイルドカード検索文字列
        /// </summary>
        private string _WildcardSearch = string.Empty;
        public string WildcardSearch
        {
            get => _WildcardSearch;
            set
            {
                SetProperty(ref _WildcardSearch, value);
                WildcardAddOrModifyCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ワイルドカードエラー出力の背景色
        /// </summary>
        private Brush _WildcardBackground = Brushes.Transparent;
        public Brush WildcardBackground
        {
            get => _WildcardBackground;
            set => SetProperty(ref _WildcardBackground, value);
        }

        /// <summary>
        /// ワイルドカードエラー出力
        /// </summary>
        private string _WildcardErrorOutput = string.Empty;
        public string WildcardErrorOutput
        {
            get => _WildcardErrorOutput;
            set => SetProperty(ref _WildcardErrorOutput, value);
        }

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public RelayCommand WildcardHelpOpen { get; set; }
        /// <summary>
        /// ワイルドカード検索条件のチェックボックスラベルが選択された時の処理をします。
        /// </summary>
        public RelayCommand<object> WildcardCheckBoxClickedCommand { get; set; }

        public RelayCommand WildcardAddOrModifyCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        private readonly IPageSelectTargetViewModelMain _pageSelectTargetViewModelMain;
        private readonly IHelpWindowViewModel _helpWindowViewModel;

        public PageSelectTargetViewModelWildcard()
        {
            throw new NotImplementedException("PageSelectTargetViewModelWildcard");
        }

        public PageSelectTargetViewModelWildcard(
            IMessageServices messageServices,
            ISettingsService settingsService,
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain,
            IHelpWindowViewModel helpWindowViewModel)
        {
            _messageServices = messageServices;
            _settingsService = settingsService;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;
            _helpWindowViewModel = helpWindowViewModel;

            // ヘルプ画面を開きます
            WildcardHelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // ワイルドカード条件の文字列が選択された時、チェックボックス状態を切り替えます
            WildcardCheckBoxClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is WildcardCheckBoxViewModel checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.Invoke(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });

            WildcardAddOrModifyCommand = new RelayCommand(
                () => { },
                () => IsWildcardCriteriaConditionCorrent()
            );

            // 試験用にアイテムを一つ追加してます(実装後削除します)
            var dummyWildcard = new WildcardCheckBoxViewModel(_messageServices, _settingsService)
            {
                IsChecked = false,
                WildcardCriteria = "*.mp4",
            };
            WildcardCollection.Add(dummyWildcard);
        }
        #endregion コンストラクタ

        private bool IsWildcardCriteriaConditionCorrent()
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(WildcardSearch))
            {
                WildcardSearchErrorStatus = WildcardSearchErrorStatus.Empty;
                return false;
            }

            // アスタリスクの数をチェックする
            var filenameWithoutExtentionAsteriskCount = Path.GetFileNameWithoutExtension(WildcardSearch).Count(c => c == '*');
            var filenameExtentionAsteriskCount = Path.GetExtension(WildcardSearch).Count(c => c == '*');
            if (filenameWithoutExtentionAsteriskCount > 1 || filenameExtentionAsteriskCount > 1)
            {
                WildcardSearchErrorStatus = WildcardSearchErrorStatus.TooManyAsterisk;
                return false;
            }

            // Windowsファイルシステムで禁止されてる文字列が含まれているかチェックする
            char[] invalidChars = ['\\', '/', ':', '"', '<', '>', '|'];
            foreach (var invalidChar in invalidChars)
            {
                if (WildcardSearch.Contains(invalidChar))
                {
                    WildcardSearchErrorStatus = WildcardSearchErrorStatus.NotAllowedCharacter;
                    return false;
                }
            }

            // ワイルドカード検索条件文字列に問題はなかった
            WildcardSearchErrorStatus = WildcardSearchErrorStatus.None;
            return true;
        }
    }
}
