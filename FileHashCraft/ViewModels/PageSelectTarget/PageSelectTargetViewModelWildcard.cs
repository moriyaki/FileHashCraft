using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModelWildcard
    {
        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        ObservableCollection<WildcardItemViewModel> WildcardItems { get; set; }
        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        ObservableCollection<WildcardItemViewModel> SelectedItems { get; set; }
        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        WildcardSearchErrorStatus WildcardSearchErrorStatus { get; }
        /// <summary>
        /// ワイルドカード検索条件を追加します
        /// </summary>
        void AddWildcardCriteria();
        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れる
        /// </summary>
        void LeaveListBoxWildcardCriteria();
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
        public ObservableCollection<WildcardItemViewModel> WildcardItems { get; set; } = [];

        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        public ObservableCollection<WildcardItemViewModel> SelectedItems { get; set; } = [];

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
        /// リストボックスで編集中の時は入力不可にする
        /// </summary>
        public Brush WildcardCiriteriaBackgroudColor
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    foreach (var item in SelectedItems)
                    {
                        if (item.IsEditMode)
                        {
                            return Brushes.LightGray;
                        }
                    }
                }
                return Brushes.Transparent;
            }
        }

        /// <summary>
        /// ワイルドカード新規検索文字列
        /// </summary>
        private string _WildcardSearchCriteriaText = string.Empty;
        public string WildcardSearchCriteriaText
        {
            get => _WildcardSearchCriteriaText;
            set
            {
                SetProperty(ref _WildcardSearchCriteriaText, value);
                WildcardAddCommand.NotifyCanExecuteChanged();
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
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public RelayCommand WildcardAddCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を編集する
        /// </summary>
        public RelayCommand ModifyCommand { get; set; }
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

            // ヘルプ画面を開きます
            WildcardHelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // ワイルドカード追加コマンド
            WildcardAddCommand = new RelayCommand(
                () => AddWildcardCriteria(),
                () => IsWildcardCriteriaConditionCorrent(WildcardSearchCriteriaText)
            );

            // 編集モードにします。
            ModifyCommand = new RelayCommand(
                () => SelectedItems[0].IsEditMode = true,
                () => SelectedItems.Count == 1
            );

            // リストボックスアイテムの編集状態から抜けた時の処理
            WeakReferenceMessenger.Default.Register<IsEditModeChanged>(this, (_, _) =>
                OnPropertyChanged(nameof(WildcardCiriteriaBackgroudColor)));

            // リストボックスの選択状態が変わった時の処理
            WeakReferenceMessenger.Default.Register<IsSelectedChanged>(this, (_, m) =>
            {
                if (m.IsSelected)
                {
                    SelectedItems.Add(m.SelectedItem);
                }
                else
                {
                    SelectedItems.Remove(m.SelectedItem);
                }
                ModifyCommand.NotifyCanExecuteChanged();
            });

            WeakReferenceMessenger.Default.Register<WildcardSelectedCriteria>(this, (_, m) =>
                m.Reply(IsWildcardCriteriaConditionCorrent(m.WildcardCriteria)));

            // 試験用にアイテムを一つ追加してます(実装後削除します)
            var dummyWildcard = new WildcardItemViewModel(_messageServices, _settingsService)
            {
                WildcardCriteria = "*.mp4",
            };
            WildcardItems.Add(dummyWildcard);

            var dummyWildcard2 = new WildcardItemViewModel(_messageServices, _settingsService)
            {
                WildcardCriteria = "*.*",
            };
            WildcardItems.Add(dummyWildcard2);
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード検索条件を追加します
        /// </summary>
        public void AddWildcardCriteria()
        {
            var newWildcard = new WildcardItemViewModel(_messageServices, _settingsService)
            {
                WildcardCriteria = WildcardSearchCriteriaText,
            };
            WildcardItems.Add(newWildcard);
            WildcardSearchCriteriaText = string.Empty;
        }

        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public bool IsWildcardCriteriaConditionCorrent(string pattern)
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(pattern))
            {
                WildcardSearchErrorStatus = WildcardSearchErrorStatus.Empty;
                return false;
            }

            // アスタリスクの数をチェックする
            var filenameWithoutExtentionAsteriskCount = Path.GetFileNameWithoutExtension(pattern).Count(c => c == '*');
            var filenameExtentionAsteriskCount = Path.GetExtension(pattern).Count(c => c == '*');
            if (filenameWithoutExtentionAsteriskCount > 1 || filenameExtentionAsteriskCount > 1)
            {
                WildcardSearchErrorStatus = WildcardSearchErrorStatus.TooManyAsterisk;
                return false;
            }

            // Windowsファイルシステムで禁止されてる文字列が含まれているかチェックする
            char[] invalidChars = ['\\', '/', ':', '"', '<', '>', '|'];
            foreach (var invalidChar in invalidChars)
            {
                if (pattern.Contains(invalidChar))
                {
                    WildcardSearchErrorStatus = WildcardSearchErrorStatus.NotAllowedCharacter;
                    return false;
                }
            }

            // ワイルドカード検索条件文字列に問題はなかった
            WildcardSearchErrorStatus = WildcardSearchErrorStatus.None;
            return true;
        }

        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れる
        /// </summary>
        public void LeaveListBoxWildcardCriteria()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
            }
        }
    }
}
