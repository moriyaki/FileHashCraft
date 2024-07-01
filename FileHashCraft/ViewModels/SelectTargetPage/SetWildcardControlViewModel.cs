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
        /// 検索条件コレクション
        /// </summary>
        ObservableCollection<BaseCriteriaItemViewModel> CriteriaItems { get; set; }
        /// <summary>
        /// 選択された検索条件コレクション
        /// </summary>
        ObservableCollection<BaseCriteriaItemViewModel> SelectedItems { get; set; }
        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        WildcardSearchErrorStatus SearchErrorStatus { get; }
        /// <summary>
        /// 検索条件を追加します。
        /// </summary>
        void AddCriteria();
        /// <summary>
        /// リストボックスの検索条件から離れます。
        /// </summary>
        void LeaveListBoxCriteria();
        /// <summary>
        /// リストボックスの検索条件から強制的に離れます。
        /// </summary>
        void LeaveListBoxCriteriaForce();
        /// <summary>
        /// 検索条件が正しいかを検査します。
        /// </summary>
        bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "");

        // 開発用
        string SearchCriteriaText { get; set; }
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
        private WildcardSearchErrorStatus _searchErrorStatus = WildcardSearchErrorStatus.None;
        public WildcardSearchErrorStatus SearchErrorStatus
        {
            get => _searchErrorStatus;
            set
            {
                SetProperty(ref _searchErrorStatus, value);
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
        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public SetWildcardControlViewModel()
        {
            throw new NotImplementedException(nameof(SetWildcardControlViewModel));
        }

        public SetWildcardControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IHelpWindowViewModel helpWindowViewModel,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(messenger, settingsService, helpWindowViewModel, pageSelectTargetViewModelMain)
        {
            // ヘルプ画面を開きます
            HelpOpenCommand = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _helpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // リストボックスの選択状態が変わった時の処理をします。
            _messenger.Register<IsSelectedWildcardChangedMessage>(this, (_, m) =>
            {
                if (m.IsSelected)
                {
                    SelectedItems.Add(m.SelectedItem);
                }
                else
                {
                    SelectedItems.Remove(m.SelectedItem);
                }
                ModifyCriteriaCommand.NotifyCanExecuteChanged();
                RemoveCriteriaCommand.NotifyCanExecuteChanged();
            });
            // リストボックスアイテムが編集された時のエラーチェックをします。
            _messenger.Register<SelectedChangedWildcardCriteriaRequestMessage>(this, (_, m) =>
                m.Reply(IsCriteriaConditionCorrent(m.WildcardCriteria, m.OriginalWildcardCriteria)));
        }
        #endregion コンストラクタ

        /// <summary>
        /// ワイルドカード検索条件を追加します。
        /// </summary>
        public override void AddCriteria()
        {
            var newWildcard = new WildcardCriteriaItemViewModel(_messenger, _settingsService)
            {
                Criteria = SearchCriteriaText,
            };
            CriteriaItems.Add(newWildcard);
            FileSearchCriteriaManager.AddCriteria(SearchCriteriaText, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
            SearchCriteriaText = string.Empty;
        }

        /// <summary>
        /// ワイルドカード検索条件を削除します。
        /// </summary>
        public override void RemoveCriteria()
        {
            foreach (var item in SelectedItems)
            {
                FileSearchCriteriaManager.RemoveCriteria(item.Criteria, FileSearchOption.Wildcard);
                _pageSelectTargetViewModelMain.SetAllTargetfilesCount();
                _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
                CriteriaItems.Remove(item);
            }
            SelectedItems.Clear();
            ModifyCriteriaCommand.NotifyCanExecuteChanged();
            RemoveCriteriaCommand.NotifyCanExecuteChanged();
            LeaveListBoxCriteria();
            _messenger.Send(new NewWildcardCriteriaFocusMessage());
        }

        /// <summary>
        /// リストボックスのワイルドカード検索条件から離れます。
        /// </summary>
        public override void LeaveListBoxCriteria()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                ModefyCriteria(listItem);
            }
            // ワイルドカード検索条件の新規欄を反映する
            IsCriteriaConditionCorrent(SearchCriteriaText);
        }

        /// <summary>
        /// リストボックスのワイルドカード検索条件から強制的に離れます。
        /// </summary>
        public override void LeaveListBoxCriteriaForce()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                listItem.Criteria = listItem.OriginalCriteria;
                _messenger.Send(new NewWildcardCriteriaFocusMessage());
                IsCriteriaConditionCorrent(SearchCriteriaText);
            }
        }
        /// <summary>
        /// ワイルドカード検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更されたワイルドカード検索条件</param>
        public override void ModefyCriteria(BaseCriteriaItemViewModel modifiedItem)
        {
            FileSearchCriteriaManager.RemoveCriteria(modifiedItem.OriginalCriteria, FileSearchOption.Wildcard);
            FileSearchCriteriaManager.AddCriteria(modifiedItem.Criteria, FileSearchOption.Wildcard);
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
        }

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
