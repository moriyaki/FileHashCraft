/*  PageSelectTargetViewModel.PartialRegularExpression.cs

    正規表現による検索画面を提供するタブの ViewModel を提供します。
 */
using System.Windows.Media;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public partial class PageSelectTargetViewModel
    {
        /// <summary>
        /// 入力された正規表現検索条件
        /// </summary>
        private string _RegularExpressionCritieria = string.Empty;
        public string RegularExpressionCritieria
        {
            get => _RegularExpressionCritieria;
            set => SetProperty(ref  _RegularExpressionCritieria, value);
        }

        /// <summary>
        /// 入力された正規表現条件にエラーがあれば色を変える
        /// </summary>
        private Brush _RegularExpressionErrorStatus = Brushes.White;
        public Brush RegularExpressionErrorStatus
        {
            get => _RegularExpressionErrorStatus;
            set => SetProperty(ref _RegularExpressionErrorStatus, value);
        }

        /// <summary>
        /// /入力された正規表現条件のエラーメッセージ
        /// </summary>
        private string _RegularExpressionErrorString = string.Empty;
        public string RegularExpressionErrorString
        {
            get => _RegularExpressionErrorString;
            set => SetProperty(ref _RegularExpressionErrorString, value);
        }
    }
}
