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
    public interface ISetWildcardControlViewModel
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
        WildcardSearchErrorStatus SearchErrorStatus { get; }
        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        void AddCriteria();
        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れます。
        /// </summary>
        void LeaveListBoxCriteria();
        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "");
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

    public class SetWildcardControlViewModel : BaseViewModel, ISetWildcardControlViewModel
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
        private WildcardSearchErrorStatus _SearchErrorStatus = WildcardSearchErrorStatus.None;
        public WildcardSearchErrorStatus SearchErrorStatus
        {
            get => _SearchErrorStatus;
            set
            {
                SetProperty(ref _SearchErrorStatus, value);
                switch (value)
                {
                    case WildcardSearchErrorStatus.None:
                        SearchErrorBackground = Brushes.Transparent;
                        SearchCriteriaErrorOutput = string.Empty;
                        break;
                    case WildcardSearchErrorStatus.Empty:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_Empty}";
                        break;
                    case WildcardSearchErrorStatus.AlreadyRegistered:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_AlreadyRegistered}";
                        break;
                    case WildcardSearchErrorStatus.TooManyAsterisk:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_TooManyAsterisk}";
                        break;
                    case WildcardSearchErrorStatus.NotAllowedCharacter:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_NotAllowedCharacter}";
                        break;
                }
            }
        }

        /// <summary>
        /// リストボックスで編集中の時は入力不可の色にする
        /// </summary>
        public Brush CiriteriaAddTextBoxBackgroudColor
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
        private string _SearchCriteriaText = string.Empty;
        public string SearchCriteriaText
        {
            get => _SearchCriteriaText;
            set
            {
                SetProperty(ref _SearchCriteriaText, value);
                AddCriteriaCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ワイルドカードエラー出力の背景色
        /// </summary>
        private Brush _SearchErrorBackground = Brushes.Transparent;
        public Brush SearchErrorBackground
        {
            get => _SearchErrorBackground;
            set => SetProperty(ref _SearchErrorBackground, value);
        }

        /// <summary>
        /// ワイルドカードエラー出力
        /// </summary>
        private string _SearchCriteriaErrorOutput = string.Empty;
        public string SearchCriteriaErrorOutput
        {
            get => _SearchCriteriaErrorOutput;
            set => SetProperty(ref _SearchCriteriaErrorOutput, value);
        }

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public RelayCommand HelpOpenCommand { get; set; }
        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public RelayCommand AddCriteriaCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を編集します。
        /// </summary>
        public RelayCommand ModifyCriteriaCommand { get; set; }

        /// <summary>
        /// 登録したワイルドカード検索条件を削除します。
        /// </summary>
        public RelayCommand RemoveCriteriaCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IHelpWindowViewModel _helpWindowViewModel;
        private readonly IShowTargetInfoUserControlViewModel _pageSelectTargetViewModelMain;

        public SetWildcardControlViewModel()
        {
            throw new NotImplementedException("PageSelectTargetViewModelWildcard");
        }

        public SetWildcardControlViewModel(
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(settingsService)
        {
            _helpWindowViewModel = helpWindowViewModel;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;

            // ヘルプ画面を開きます
            HelpOpenCommand = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // ワイルドカードを追加します。
            AddCriteriaCommand = new RelayCommand(
                () => AddCriteria(),
                () => IsCriteriaConditionCorrent(SearchCriteriaText) && _pageSelectTargetViewModelMain.Status == FileScanStatus.Finished
            );

            // ワイルドカード条件一覧から編集モードにします。
            ModifyCriteriaCommand = new RelayCommand(
                () => SelectedItems[0].IsEditMode = true,
                () => SelectedItems.Count == 1
            );

            // ワイルドカード条件一覧削除します。
            RemoveCriteriaCommand = new RelayCommand(
                () => RemoveCriteria(),
                () => SelectedItems.Count > 0
            );

            WeakReferenceMessenger.Default.Register<FileScanFinished>(this, (_, _) =>
                AddCriteriaCommand.NotifyCanExecuteChanged());

            // リストボックスアイテムの編集状態から抜けた時の処理をします。
            WeakReferenceMessenger.Default.Register<IsEditModeChanged>(this, (_, _) =>
                OnPropertyChanged(nameof(CiriteriaAddTextBoxBackgroudColor)));
            // リストボックスの選択状態が変わった時の処理をします。
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
                ModifyCriteriaCommand.NotifyCanExecuteChanged();
                RemoveCriteriaCommand.NotifyCanExecuteChanged();
            });

            // リストボックスアイテムが編集された時のエラーチェックをします。
            WeakReferenceMessenger.Default.Register<SelectedChangedCriteria>(this, (_, m) =>
                m.Reply(IsCriteriaConditionCorrent(m.WildcardCriteria, m.OriginalWildcardCriteria)));
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public void AddCriteria()
        {
            var newWildcard = new WildcardItemViewModel(_settingsService)
            {
                Criteria = SearchCriteriaText,
            };
            WildcardItems.Add(newWildcard);
            FileSearchCriteriaManager.AddCriteria(SearchCriteriaText, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
            SearchCriteriaText = string.Empty;
        }

        /// <summary>
        /// ワイルドカード検索条件を削除します。
        /// </summary>
        public void RemoveCriteria()
        {
            foreach (var item in SelectedItems)
            {
                FileSearchCriteriaManager.RemoveCriteria(item.Criteria, FileSearchOption.Wildcard);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
                WildcardItems.Remove(item);
            }
            SelectedItems.Clear();
            ModifyCriteriaCommand.NotifyCanExecuteChanged();
            LeaveListBoxCriteria();
            WeakReferenceMessenger.Default.Send<NewCriteriaFocus>(new NewCriteriaFocus());
        }

        /// <summary>
        /// ワイルドカード検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更されたワイルドカード検索条件</param>
        public void ModefyCriteria(WildcardItemViewModel modifiedItem)
        {
            FileSearchCriteriaManager.RemoveCriteria(modifiedItem.OriginalWildcardCriteria, FileSearchOption.Wildcard);
            FileSearchCriteriaManager.AddCriteria(modifiedItem.Criteria, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
        }

        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れます。
        /// </summary>
        public void LeaveListBoxCriteria()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                ModefyCriteria(listItem);
            }
            // ワイルドカード検索条件の新規欄を反映する
            IsCriteriaConditionCorrent(SearchCriteriaText);
        }

        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        /// <param name="pattern">チェックするワイルドカード文字列</param>
        /// <param name="originalPattern">(必要ならば)元のワイルドカード文字列</param>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "")
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(pattern))
            {
                SearchErrorStatus = WildcardSearchErrorStatus.Empty;
                return false;
            }

            // 既に登録されているかをチェックする
            if (pattern != originalPattern)
            {
                foreach (var item in WildcardItems)
                {
                    if (item.Criteria == pattern)
                    {
                        SearchErrorStatus = WildcardSearchErrorStatus.AlreadyRegistered;
                        return false;
                    }
                }
            }

            // アスタリスクの数をチェックする
            var filenameWithoutExtentionAsteriskCount = Path.GetFileNameWithoutExtension(pattern).Count(c => c == '*');
            var filenameExtentionAsteriskCount = Path.GetExtension(pattern).Count(c => c == '*');
            if (filenameWithoutExtentionAsteriskCount > 1 || filenameExtentionAsteriskCount > 1)
            {
                SearchErrorStatus = WildcardSearchErrorStatus.TooManyAsterisk;
                return false;
            }

            // Windowsファイルシステムで禁止されてる文字列が含まれているかチェックする
            char[] invalidChars = ['\\', '/', ':', '"', '<', '>', '|'];
            foreach (var invalidChar in invalidChars)
            {
                if (pattern.Contains(invalidChar))
                {
                    SearchErrorStatus = WildcardSearchErrorStatus.NotAllowedCharacter;
                    return false;
                }
            }

            // ワイルドカード検索条件文字列に問題はなかった
            SearchErrorStatus = WildcardSearchErrorStatus.None;
            return true;
        }
    }
}
