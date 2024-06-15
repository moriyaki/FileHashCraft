using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IWildcardItemViewModel
    {
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        string Criteria { get; set; }
    }

    public class WildcardItemViewModel : BaseViewModel, IWildcardItemViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        private string _Criteria = string.Empty;
        public string Criteria
        {
            get => _Criteria;
            set
            {
                _WildcardCriteriaConditionCorrent = WeakReferenceMessenger.Default.Send(new SelectedChangedCriteria(value, _OriginalWildcardCriteria));
                SetProperty(ref _Criteria, value);
            }
        }

        /// <summary>
        /// オリジナルのワイルドカード検索条件
        /// </summary>
        private string _OriginalWildcardCriteria = string.Empty;
        public string OriginalWildcardCriteria
        {
            get => _OriginalWildcardCriteria;
        }

        /// <summary>
        /// ワイルドカード検索条件が正当かどうか
        /// </summary>
        private bool _WildcardCriteriaConditionCorrent;

        /// <summary>
        /// アイテムの背景色
        /// </summary>
        public Brush ItemBackgroudColor
        {
            get =>  IsEditMode ? Brushes.White : Brushes.Transparent;
        }

        /// <summary>
        /// TextBoxの枠
        /// </summary>
        public string BorderTickness
        {
            get => IsEditMode ? "1,2,1,2" : "0";
        }

        /// <summary>
        /// 編集モードかどうか
        /// </summary>
        private bool _IsEditMode = false;
        public bool IsEditMode
        {
            get => _IsEditMode;
            set
            {
                if (_IsEditMode == value) { return; }
                SetProperty(ref _IsEditMode, value);
                if (value)
                {
                    // 表示モードになったら、オリジナルを保存して編集モードに入ります。
                    _OriginalWildcardCriteria = Criteria;
                    WeakReferenceMessenger.Default.Send(new ListBoxSeletedTextBoxFocus());
                }
                else
                {
                    // エラーが出ているなら、オリジナルの検索条件に戻します。
                    RestoreWildcardCriteria();
                }
                OnPropertyChanged(nameof(ItemBackgroudColor));
                OnPropertyChanged(nameof(BorderTickness));
                WeakReferenceMessenger.Default.Send(new IsEditModeChanged());
                WeakReferenceMessenger.Default.Send(new SelectedChangedCriteria(Criteria,_OriginalWildcardCriteria));
            }
        }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                SetProperty(ref _IsSelected, value);
                if (!value)
                {
                    RestoreWildcardCriteria();
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
        public WildcardItemViewModel(
            ISettingsService settingsService
        ) : base(settingsService)
        {
            ListBoxItemTextBoxWildcardCriteriaClicked = new RelayCommand(() =>
            {
                IsEditMode = true;
                if (!IsSelected)
                {
                    // 選択状態が外れたら、新規入力画面にキャレットを当てます。
                    WeakReferenceMessenger.Default.Send(new NewCriteriaFocus());
                }
            });
        }
        #endregion コンストラクタ

        private void RestoreWildcardCriteria()
        {
            // 間違っているワイルドカード検索条件は元に戻します。
            if (!_WildcardCriteriaConditionCorrent)
            {
                Criteria = _OriginalWildcardCriteria;
            }
        }
    }
}
