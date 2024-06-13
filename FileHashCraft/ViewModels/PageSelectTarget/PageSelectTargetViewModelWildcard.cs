using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
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
        AlreadyRegistered,
        TooManyAsterisk,
        NotAllowedCharacter,
    }

    public class PageSelectTargetViewModelWildcard : BaseViewModel, IPageSelectTargetViewModelWildcard
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
                    case WildcardSearchErrorStatus.AlreadyRegistered:
                        WildcardBackground = Brushes.Red;
                        WildcardErrorOutput = $"{Resources.LabelWildcardError_AlreadyRegistered}";
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
        /// リストボックスで編集中の時は入力不可の色にする
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
                AddWildcardCommand.NotifyCanExecuteChanged();
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
        public RelayCommand AddWildcardCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を編集します。
        /// </summary>
        public RelayCommand ModifyCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を削除します。
        /// </summary>
        public RelayCommand RemoveCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IPageSelectTargetViewModelMain _pageSelectTargetViewModelMain;

        public PageSelectTargetViewModelWildcard()
        {
            throw new NotImplementedException("PageSelectTargetViewModelWildcard");
        }

        public PageSelectTargetViewModelWildcard(
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain
        ) : base(settingsService)
        {
            _helpWindowViewModel = helpWindowViewModel;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;

            // ヘルプ画面を開きます
            WildcardHelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // ワイルドカード追加コマンド
            AddWildcardCommand = new RelayCommand(
                () => AddWildcardCriteria(),
                () => IsWildcardCriteriaConditionCorrent(WildcardSearchCriteriaText) && _pageSelectTargetViewModelMain.Status == FileScanStatus.Finished
            );

            // ワイルドカード条件一覧から編集モードにします。
            ModifyCommand = new RelayCommand(
                () => SelectedItems[0].IsEditMode = true,
                () => SelectedItems.Count == 1
            );

            // ワイルドカード条件一覧削除します。
            RemoveCommand = new RelayCommand(
                () => RemoveWildcardCriteria(),
                () => SelectedItems.Count > 0
            );

            WeakReferenceMessenger.Default.Register<FileScanFinished>(this, (_, _) =>
                AddWildcardCommand.NotifyCanExecuteChanged());

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
                RemoveCommand.NotifyCanExecuteChanged();
            });

            // リストボックスアイテムが編集された時のエラーチェック
            WeakReferenceMessenger.Default.Register<WildcardSelectedChangedCriteria>(this, (_, m) =>
                m.Reply(IsWildcardCriteriaConditionCorrent(m.WildcardCriteria, m.OriginalWildcardCriteria)));
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public void AddWildcardCriteria()
        {
            var newWildcard = new WildcardItemViewModel(_settingsService)
            {
                WildcardCriteria = WildcardSearchCriteriaText,
            };
            WildcardItems.Add(newWildcard);
            FileSearchCriteriaManager.AddCriteria(WildcardSearchCriteriaText, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
            WildcardSearchCriteriaText = string.Empty;
        }

        /// <summary>
        /// ワイルドカード検索条件を削除します。
        /// </summary>
        public void RemoveWildcardCriteria()
        {
            foreach (var item in SelectedItems)
            {
                FileSearchCriteriaManager.RemoveCriteria(item.WildcardCriteria, FileSearchOption.Wildcard);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
                WildcardItems.Remove(item);
            }
            SelectedItems.Clear();
            ModifyCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// ワイルドカード検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更されたワイルドカード検索条件</param>
        public void ModefyWildcardCriteria(WildcardItemViewModel modifiedItem)
        {
            FileSearchCriteriaManager.RemoveCriteria(modifiedItem.OriginalWildcardCriteria, FileSearchOption.Wildcard);
            FileSearchCriteriaManager.AddCriteria(modifiedItem.WildcardCriteria, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
        }

        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        /// <param name="pattern">チェックするワイルドカード文字列</param>
        /// <param name="originalPattern">(必要ならば)元のワイルドカード文字列</param>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public bool IsWildcardCriteriaConditionCorrent(string pattern, string originalPattern = "")
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(pattern))
            {
                WildcardSearchErrorStatus = WildcardSearchErrorStatus.Empty;
                return false;
            }

            // 既に登録されているかをチェックする
            if (pattern != originalPattern)
            {
                foreach (var item in WildcardItems)
                {
                    if (item.WildcardCriteria == pattern)
                    {
                        WildcardSearchErrorStatus = WildcardSearchErrorStatus.AlreadyRegistered;
                        return false;
                    }
                }
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
                ModefyWildcardCriteria(listItem);
            }
        }
    }
}
