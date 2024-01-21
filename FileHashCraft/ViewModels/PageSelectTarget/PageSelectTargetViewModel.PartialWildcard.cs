using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
