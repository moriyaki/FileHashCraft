using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface ICriteriaItemViewModel
    {
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        string Criteria { get; set; }
    }

    public class CriteriaItemViewModel : BaseCriteriaItemViewModel, ICriteriaItemViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        public override string Criteria
        {
            get => _Criteria;
            set
            {
                _CriteriaConditionCorrent = WeakReferenceMessenger.Default.Send(new SelectedChangedCriteria(value, OriginalCriteria));
                SetProperty(ref _Criteria, value);
            }
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        public override bool IsEditMode
        {
            get => _IsEditMode;
            set
            {
                if (_IsEditMode == value) { return; }
                SetProperty(ref _IsEditMode, value);
                if (value)
                {
                    // 表示モードになったら、オリジナルを保存して編集モードに入ります。
                    OriginalCriteria = Criteria;
                    WeakReferenceMessenger.Default.Send(new ListBoxSeletedWildcardTextBoxFocus());
                }
                else
                {
                    // エラーが出ているなら、オリジナルの検索条件に戻します。
                    RestoreCriteria();
                }
                OnPropertyChanged(nameof(ItemBackgroudColor));
                OnPropertyChanged(nameof(BorderTickness));
                WeakReferenceMessenger.Default.Send(new IsEditModeChanged());
                WeakReferenceMessenger.Default.Send(new SelectedChangedCriteria(Criteria, OriginalCriteria));
            }
        }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        public override bool IsSelected
        {
            get => _IsSelected;
            set
            {
                SetProperty(ref _IsSelected, value);
                if (!value)
                {
                    RestoreCriteria();
                }
                WeakReferenceMessenger.Default.Send(new IsSelectedChanged(value, this));
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
        public CriteriaItemViewModel(
            ISettingsService settingsService
        ) : base(settingsService)
        {
            ListBoxItemTextBoxWildcardCriteriaClicked = new RelayCommand(() =>
            {
                IsEditMode = true;
                if (!IsSelected)
                {
                    // 選択状態が外れたら、新規入力画面にキャレットを当てます。
                    WeakReferenceMessenger.Default.Send(new NewWildcardCriteriaFocus());
                }
            });
        }
        #endregion コンストラクタ

        protected override void RestoreCriteria()
        {
            // 間違っているワイルドカード検索条件は元に戻します。
            if (!_CriteriaConditionCorrent)
            {
                Criteria = OriginalCriteria;
            }
        }
    }
}
