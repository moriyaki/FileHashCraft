using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public abstract class BaseCriteriaViewModel : BaseViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        public virtual ObservableCollection<CriteriaItemViewModel> CriteriaItems { get; set; }

        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        public virtual ObservableCollection<CriteriaItemViewModel> SelectedItems { get; set; }

        /// <summary>
        /// 検索条件エラー出力の背景色
        /// </summary>
        private Brush _SearchErrorBackground = Brushes.Transparent;
        public Brush SearchErrorBackground
        {
            get => _SearchErrorBackground;
            set => SetProperty(ref _SearchErrorBackground, value);
        }

        /// <summary>
        /// 検索条件エラー出力
        /// </summary>
        private string _SearchCriteriaErrorOutput = string.Empty;
        public string SearchCriteriaErrorOutput
        {
            get => _SearchCriteriaErrorOutput;
            set => SetProperty(ref _SearchCriteriaErrorOutput, value);
        }

        /// <summary>
        /// リストボックスで検索条件編集中の時は入力不可の色にする
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

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public abstract RelayCommand HelpOpenCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        protected readonly IHelpWindowViewModel _helpWindowViewModel;
        protected readonly IShowTargetInfoUserControlViewModel _pageSelectTargetViewModelMain;
        protected BaseCriteriaViewModel() { throw new NotImplementedException("BaseCriteriaViewModel"); }

        protected BaseCriteriaViewModel(
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(settingsService)
        {
            _helpWindowViewModel = helpWindowViewModel;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;

            // コレクションのインスタンスを生成します。
            CriteriaItems = [];
            SelectedItems = [];

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
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public void AddCriteria()
        {
            var newWildcard = new CriteriaItemViewModel(_settingsService)
            {
                Criteria = SearchCriteriaText,
            };
            CriteriaItems.Add(newWildcard);
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
                CriteriaItems.Remove(item);
            }
            SelectedItems.Clear();
            ModifyCriteriaCommand.NotifyCanExecuteChanged();
            LeaveListBoxCriteria();
            WeakReferenceMessenger.Default.Send(new NewWildcardCriteriaFocus());
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
        /// リストボックスのワイルドカード検索条件から強制的に離れます。
        /// </summary>
        public void LeaveListBoxCriteriaForce()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                listItem.Criteria = listItem.OriginalCriteria;
                WeakReferenceMessenger.Default.Send(new NewWildcardCriteriaFocus());
                IsCriteriaConditionCorrent(SearchCriteriaText);
            }
        }
        /// <summary>
        /// ワイルドカード検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更されたワイルドカード検索条件</param>
        public void ModefyCriteria(CriteriaItemViewModel modifiedItem)
        {
            FileSearchCriteriaManager.RemoveCriteria(modifiedItem.OriginalCriteria, FileSearchOption.Wildcard);
            FileSearchCriteriaManager.AddCriteria(modifiedItem.Criteria, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
        }
        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// 注：継承して使います。
        /// </summary>
        /// <param name="pattern">チェックするワイルドカード文字列</param>
        /// <param name="originalPattern">(必要ならば)元のワイルドカード文字列</param>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public virtual bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "")
        {
            return false;
        }
    }
}
