using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IWildcardCriteriaItemViewModel
    {
        /// <summary>
        /// 検索条件
        /// </summary>
        string Criteria { get; set; }
    }
    #endregion インターフェース

    public class WildcardCriteriaItemViewModel : BaseCriteriaItemViewModel, IWildcardCriteriaItemViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        public override string Criteria
        {
            get => _criteria;
            set
            {
                _criteriaConditionCorrent = _messenger.Send(new SelectedChangedWildcardCriteriaRequestMessage(value, OriginalCriteria));
                SetProperty(ref _criteria, value);
            }
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public override bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode == value) { return; }
                SetProperty(ref _isEditMode, value);
                if (value)
                {
                    // 表示モードになったら、オリジナルを保存して編集モードに入ります。
                    OriginalCriteria = Criteria;
                    _messenger.Send(new ListBoxSeletedWildcardTextBoxFocusMessage());
                }
                else
                {
                    // エラーが出ているなら、オリジナルの検索条件に戻します。
                    RestoreCriteria();
                }
                OnPropertyChanged(nameof(ItemBackgroudColor));
                OnPropertyChanged(nameof(BorderTickness));
                _messenger.Send(new IsEditModeChangedMessage());
                _messenger.Send(new SelectedChangedWildcardCriteriaRequestMessage(Criteria, OriginalCriteria));
            }
        }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                if (!value)
                {
                    RestoreCriteria();
                }
                _messenger.Send(new IsSelectedWildcardChangedMessage(value, this));
            }
        }

        /// <summary>
        /// リストボックスアイテムのテキストボックスがクリックされた時の処理です。
        /// </summary>
        public RelayCommand ListBoxItemTextBoxWildcardCriteriaClicked { get; set; }
        #endregion バインディング

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ：リストボックスでアイテムがクリックされたかのイベント処理をします。
        /// </summary>
        /// <param name="settingsService"></param>
        public WildcardCriteriaItemViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        ) : base(messenger, settingsService)
        {
            ListBoxItemTextBoxWildcardCriteriaClicked = new RelayCommand(() =>
            {
                IsEditMode = true;
                if (!IsSelected)
                {
                    // 選択状態が外れたら、新規入力画面にキャレットを当てます。
                    _messenger.Send(new NewWildcardCriteriaFocusMessage());
                }
            });
        }
        #endregion コンストラクタ
    }
}
