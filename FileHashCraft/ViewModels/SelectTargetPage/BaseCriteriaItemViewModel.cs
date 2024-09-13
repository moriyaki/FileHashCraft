using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public abstract class BaseCriteriaItemViewModel : BaseViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        protected string _criteria = string.Empty;
        public abstract string Criteria { get; set; }

        public string OriginalCriteria { get; set; } = string.Empty;

        /// <summary>
        /// ワイルドカード検索条件が正当かどうか
        /// </summary>
        protected bool _criteriaConditionCorrent;

        /// <summary>
        /// アイテムの背景色
        /// </summary>
        public Brush ItemBackgroudColor
        {
            get => IsEditMode ? Brushes.White : Brushes.Transparent;
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
        protected bool _isEditMode = false;
        public abstract bool IsEditMode { get; set; }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        protected bool _isSelected = false;
        public abstract bool IsSelected { get; set; }
        #endregion バインディング

        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        private BaseCriteriaItemViewModel() { throw new NotImplementedException(nameof(BaseCriteriaItemViewModel)); }
        protected BaseCriteriaItemViewModel(
            IMessenger messenger,
            ISettingsService settingsService
        ) : base(messenger, settingsService)
        {
        }

        /// <summary>
        /// 間違っているワイルドカード検索条件は元に戻します。
        /// </summary>
        protected void RestoreCriteria()
        {
            // 
            if (!_criteriaConditionCorrent)
            {
                Criteria = OriginalCriteria;
            }
        }
    }
}
