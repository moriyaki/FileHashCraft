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
        string WildcardCriteria { get; set; }
    }

    public class WildcardItemViewModel : BaseViewModel, IWildcardItemViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        private string _WildcardCriteria = string.Empty;
        private string _WildcardCriteriaBackup = string.Empty;
        private bool _WildcardCriteriaConditionCorrent;
        public string WildcardCriteria
        {
            get => _WildcardCriteria;
            set
            {
                SetProperty(ref _WildcardCriteria, value);
                _WildcardCriteriaConditionCorrent = WeakReferenceMessenger.Default.Send(new WildcardSelectedCriteria(value));
            }
        }

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
        /// 表示モードかどうか
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
                    WeakReferenceMessenger.Default.Send(new WildcardSeletedTextBoxFocus());
                }
                else
                {
                    RestoreWildcardCriteria();
                }
                _WildcardCriteriaBackup = WildcardCriteria;
                OnPropertyChanged(nameof(ItemBackgroudColor));
                OnPropertyChanged(nameof(BorderTickness));
                WeakReferenceMessenger.Default.Send(new IsEditModeChanged());
                WeakReferenceMessenger.Default.Send(new WildcardSelectedCriteria(WildcardCriteria));
            }
        }

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

        public RelayCommand ListBoxTextWildcardCriteriaClicked { get; set; }
        #endregion バインディング

        #region コンストラクタ
        public WildcardItemViewModel(
            ISettingsService settingsService
        ) : base(settingsService)
        {
            ListBoxTextWildcardCriteriaClicked = new RelayCommand(() =>
            {
                if (IsSelected)
                {
                    IsEditMode = true;
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new WildcardCriteriaFocus());
                    IsSelected = true;
                }
            });
        }
        #endregion コンストラクタ

        private void RestoreWildcardCriteria()
        {
            // 間違っているワイルドカード検索条件は元に戻す
            if (!_WildcardCriteriaConditionCorrent)
            {
                WildcardCriteria = _WildcardCriteriaBackup;
            }
            WeakReferenceMessenger.Default.Send(new WildcardCriteriaFocus());
        }
    }
}
