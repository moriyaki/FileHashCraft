using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FileHashCraft.Services;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    public abstract class BaseCriteriaItemViewModel : BaseViewModel
    {
        #region バインディング
        /// <summary>
        /// ワイルドカード検索条件
        /// </summary>
        protected string _Criteria = string.Empty;
        public abstract string Criteria { get; set; }

        public string OriginalCriteria { get; set; } = string.Empty;

        /// <summary>
        /// ワイルドカード検索条件が正当かどうか
        /// </summary>
        protected bool _CriteriaConditionCorrent;

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
        protected bool _IsEditMode = false;
        public abstract bool IsEditMode { get; set; }

        /// <summary>
        /// 該当アイテムが選択されているかのプロパティです。
        /// </summary>
        protected bool _IsSelected = false;
        public abstract bool IsSelected { get; set; }
        #endregion バインディング

        private BaseCriteriaItemViewModel() { throw new NotImplementedException("BaseCriteriaItemViewModel"); }
        protected BaseCriteriaItemViewModel(
             ISettingsService settingsService
        ) : base(settingsService)
        {
        }

        protected abstract void RestoreCriteria();
    }
}
