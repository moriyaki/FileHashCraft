using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface ISetWildcardControlViewModel
    {
        /// <summary>
        /// ワイルドカード検索条件コレクション
        /// </summary>
        ObservableCollection<CriteriaItemViewModel> CriteriaItems { get; set; }
        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        ObservableCollection<CriteriaItemViewModel> SelectedItems { get; set; }
        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        WildcardSearchErrorStatus SearchErrorStatus { get; }
        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        void AddCriteria();
        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れます。
        /// </summary>
        void LeaveListBoxCriteria();
        /// <summary>
        /// リストボックスのワイルドカード検索条件から強制的に離れます。
        /// </summary>
        void LeaveListBoxCriteriaForce();
        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "");
    }
    #endregion インターフェース

    public enum WildcardSearchErrorStatus
    {
        None,
        Empty,
        AlreadyRegistered,
        TooManyAsterisk,
        NotAllowedCharacter,
    }

    public class SetWildcardControlViewModel : BaseCriteriaViewModel, ISetWildcardControlViewModel
    {
        #region バインディング
        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        private WildcardSearchErrorStatus _SearchErrorStatus = WildcardSearchErrorStatus.None;
        public WildcardSearchErrorStatus SearchErrorStatus
        {
            get => _SearchErrorStatus;
            set
            {
                SetProperty(ref _SearchErrorStatus, value);
                switch (value)
                {
                    case WildcardSearchErrorStatus.None:
                        SearchErrorBackground = Brushes.Transparent;
                        SearchCriteriaErrorOutput = string.Empty;
                        break;
                    case WildcardSearchErrorStatus.Empty:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_Empty}";
                        break;
                    case WildcardSearchErrorStatus.AlreadyRegistered:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_AlreadyRegistered}";
                        break;
                    case WildcardSearchErrorStatus.TooManyAsterisk:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_TooManyAsterisk}";
                        break;
                    case WildcardSearchErrorStatus.NotAllowedCharacter:
                        SearchErrorBackground = Brushes.Red;
                        SearchCriteriaErrorOutput = $"{Resources.LabelWildcardError_NotAllowedCharacter}";
                        break;
                }
            }
        }

        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public override RelayCommand HelpOpenCommand { get; set; }

        #endregion バインディング

        #region コンストラクタ
        public SetWildcardControlViewModel()
        {
            throw new NotImplementedException("PageSelectTargetViewModelWildcard");
        }

        public SetWildcardControlViewModel(
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(settingsService, helpWindowViewModel, pageSelectTargetViewModelMain)
        {
            // ヘルプ画面を開きます
            HelpOpenCommand = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // リストボックスアイテムが編集された時のエラーチェックをします。
            WeakReferenceMessenger.Default.Register<SelectedChangedCriteria>(this, (_, m) =>
                m.Reply(IsCriteriaConditionCorrent(m.WildcardCriteria, m.OriginalWildcardCriteria)));
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード文字列が正しいかを検査します。
        /// </summary>
        /// <param name="pattern">チェックするワイルドカード文字列</param>
        /// <param name="originalPattern">(必要ならば)元のワイルドカード文字列</param>
        /// <returns>ワイルドカード文字列が正当かどうか</returns>
        public override bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "")
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(pattern))
            {
                SearchErrorStatus = WildcardSearchErrorStatus.Empty;
                return false;
            }

            // 既に登録されているかをチェックする
            if (pattern != originalPattern)
            {
                foreach (var item in CriteriaItems)
                {
                    if (item.Criteria == pattern)
                    {
                        SearchErrorStatus = WildcardSearchErrorStatus.AlreadyRegistered;
                        return false;
                    }
                }
            }

            // アスタリスクの数をチェックする
            var filenameWithoutExtentionAsteriskCount = Path.GetFileNameWithoutExtension(pattern).Count(c => c == '*');
            var filenameExtentionAsteriskCount = Path.GetExtension(pattern).Count(c => c == '*');
            if (filenameWithoutExtentionAsteriskCount > 1 || filenameExtentionAsteriskCount > 1)
            {
                SearchErrorStatus = WildcardSearchErrorStatus.TooManyAsterisk;
                return false;
            }

            // Windowsファイルシステムで禁止されてる文字列が含まれているかチェックする
            char[] invalidChars = ['\\', '/', ':', '"', '<', '>', '|'];
            foreach (var invalidChar in invalidChars)
            {
                if (pattern.Contains(invalidChar))
                {
                    SearchErrorStatus = WildcardSearchErrorStatus.NotAllowedCharacter;
                    return false;
                }
            }

            // ワイルドカード検索条件文字列に問題はなかった
            SearchErrorStatus = WildcardSearchErrorStatus.None;
            return true;
        }
    }
}
