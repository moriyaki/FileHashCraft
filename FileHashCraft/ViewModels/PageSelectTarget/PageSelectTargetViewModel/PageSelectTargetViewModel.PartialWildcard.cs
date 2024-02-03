/*  PageSelectTargetViewModel.PartialWildcard.cs

    ワイルドカードによる検索画面を提供するタブの ViewModel を提供します。
 */

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public partial class PageSelectTargetViewModel
    {
        /// <summary>
        /// ワイルドカード検索条件文字列
        /// </summary>
        private string _WildcardCritieria = string.Empty;
        public string WildcardCritieria
        {
            get => _WildcardCritieria;
            set => SetProperty(ref _WildcardCritieria, value);
        }
    }
}
