﻿using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Resources;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.SelectTargetPage
{
    #region インターフェース

    public interface ISetRegexControlViewModel
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
        RegexSearchErrorStatus SearchErrorStatus { get; }

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
    }

    #endregion インターフェース

    #region 正規表現エラー

    public enum RegexSearchErrorStatus
    {
        None,
        Empty,
        AlreadyRegistered,
        AlternationHasComment,
        AlternationHasMalformedCondition,
        AlternationHasMalformedReference,
        AlternationHasNamedCapture,
        AlternationHasTooManyConditions,
        AlternationHasUndefinedReference,
        CaptureGroupNameInvalid,
        CaptureGroupOfZero,
        ExclusionGroupNotLast,
        InsufficientClosingParentheses,
        InsufficientOpeningParentheses,
        InsufficientOrInvalidHexDigits,
        InvalidGroupingConstruct,
        InvalidUnicodePropertyEscape,
        MalformedNamedReference,
        MalformedUnicodePropertyEscape,
        MissingControlCharacter,
        NestedQuantifiersNotParenthesized,
        QuantifierAfterNothing,
        QuantifierOrCaptureGroupOutOfRange,
        ReversedCharacterRange,
        ReversedQuantifierRange,
        ShorthandClassInCharacterRange,
        UndefinedNamedReference,
        UndefinedNumberedReference,
        UnescapedEndingBackslash,
        Unknown,
        UnrecognizedControlCharacter,
        UnrecognizedEscape,
        UnrecognizedUnicodeProperty,
        UnterminatedBracket,
        UnterminatedComment,
    }

    #endregion 正規表現エラー

    public class SetRegexControlViewModel : BaseCriteriaViewModel, ISetRegexControlViewModel
    {
        #region バインディング

        /// <summary>
        /// ボタン「変更」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ButtonModify { get => ResourceService.GetString("ButtonModify"); }

        /// <summary>
        /// ボタン「削除」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ButtonRemove { get => ResourceService.GetString("ButtonRemove"); }

        /// <summary>
        /// ボタン「追加」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string ButtonAdd { get => ResourceService.GetString("ButtonAdd"); }

        /// <summary>
        /// ボタン「正規条件での検索条件」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string LabelRegex_Criteria { get => ResourceService.GetString("LabelRegex_Criteria"); }

        /// <summary>
        /// ボタン「正規表現のエラー」表示の取得
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static")] 
        public string LabelRegex_Error { get => ResourceService.GetString("LabelRegex_Error"); }

        /// <summary>
        /// 検索条件入力のステータス
        /// </summary>
        private RegexSearchErrorStatus _SearchErrorStatus = RegexSearchErrorStatus.None;

        public RegexSearchErrorStatus SearchErrorStatus
        {
            get => _SearchErrorStatus;
            set
            {
                SetProperty(ref _SearchErrorStatus, value);
                if (value == RegexSearchErrorStatus.None)
                {
                    SearchErrorBackground = Brushes.Transparent;
                    SearchCriteriaErrorOutput = string.Empty;
                    return;
                }
                SearchErrorBackground = Brushes.Red;
                switch (value)
                {
                    case RegexSearchErrorStatus.Empty:
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelWildcardError_Empty")}";
                        break;

                    case RegexSearchErrorStatus.AlreadyRegistered:
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelWildcardError_AlreadyRegistered}")}";
                        break;

                    case RegexSearchErrorStatus.AlternationHasMalformedCondition:
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_AlternationHasMalformedCondition}")}";
                        break;

                    case RegexSearchErrorStatus.AlternationHasMalformedReference:   // "(x)(?(3x|y)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_AlternationHasMalformedReference}")}";
                        break;

                    case RegexSearchErrorStatus.AlternationHasNamedCapture:         // "(?(?<x>)true|false)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_AlternationHasNamedCapture}")}";
                        break;

                    case RegexSearchErrorStatus.AlternationHasTooManyConditions:    // "(?(foo)a|b|c)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_AlternationHasTooManyConditions}")}";
                        break;

                    case RegexSearchErrorStatus.AlternationHasUndefinedReference:   // "(?(1))"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_AlternationHasUndefinedReference}")}";
                        break;

                    case RegexSearchErrorStatus.CaptureGroupNameInvalid:            // "(?'x)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_CaptureGroupNameInvalid}")}";
                        break;

                    case RegexSearchErrorStatus.CaptureGroupOfZero:                 // "(?'0'foo)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_CaptureGroupOfZero}")}";
                        break;

                    case RegexSearchErrorStatus.ExclusionGroupNotLast:              // "[a-z-[xy]A]"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_ExclusionGroupNotLast}")}";
                        break;

                    case RegexSearchErrorStatus.InsufficientClosingParentheses:     // "(((foo))"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_InsufficientClosingParentheses}")}";
                        break;

                    case RegexSearchErrorStatus.InsufficientOpeningParentheses:     // "((foo)))"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_InsufficientOpeningParentheses}")}";
                        break;

                    case RegexSearchErrorStatus.InsufficientOrInvalidHexDigits:     // @"\xr"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_InsufficientOrInvalidHexDigits}")}";
                        break;

                    case RegexSearchErrorStatus.InvalidGroupingConstruct:           // "(?"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_InvalidGroupingConstruct}")}";
                        break;

                    case RegexSearchErrorStatus.InvalidUnicodePropertyEscape:       // @"\p{ L}"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_InvalidUnicodePropertyEscape}")}";
                        break;

                    case RegexSearchErrorStatus.MalformedNamedReference:            // @"\k<"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_MalformedNamedReference}")}";
                        break;

                    case RegexSearchErrorStatus.MalformedUnicodePropertyEscape:     // @"\p {L}"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_MalformedUnicodePropertyEscape}")}";
                        break;

                    case RegexSearchErrorStatus.MissingControlCharacter:            // @"\c"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_MissingControlCharacter}")}";
                        break;

                    case RegexSearchErrorStatus.NestedQuantifiersNotParenthesized:  // "abc**"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_NestedQuantifiersNotParenthesized}")}";
                        break;

                    case RegexSearchErrorStatus.QuantifierAfterNothing:             // "((*foo)bar)"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_QuantifierAfterNothing}")}";
                        break;

                    case RegexSearchErrorStatus.QuantifierOrCaptureGroupOutOfRange: // "x{234567899988}"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_QuantifierOrCaptureGroupOutOfRange}")}";
                        break;

                    case RegexSearchErrorStatus.ReversedCharacterRange:             // "[z-a]"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_ReversedCharacterRange}")}";
                        break;

                    case RegexSearchErrorStatus.ReversedQuantifierRange:            // "abc{3,0}"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_ReversedQuantifierRange}")}";
                        break;

                    case RegexSearchErrorStatus.ShorthandClassInCharacterRange:     // @"[a-\w]"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_ShorthandClassInCharacterRange}")}";
                        break;

                    case RegexSearchErrorStatus.UndefinedNamedReference:            // @"\k<x>"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UndefinedNamedReference}")}";
                        break;

                    case RegexSearchErrorStatus.UndefinedNumberedReference:         // @"(x)\2"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UndefinedNumberedReference}")}";
                        break;

                    case RegexSearchErrorStatus.UnescapedEndingBackslash:           // @"foo\"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnescapedEndingBackslash}")}";
                        break;

                    case RegexSearchErrorStatus.Unknown:
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_Unknown}")}";
                        break;

                    case RegexSearchErrorStatus.UnrecognizedControlCharacter:       // @"\c!"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnrecognizedControlCharacter}")}";
                        break;

                    case RegexSearchErrorStatus.UnrecognizedEscape:                 // @"\C"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnrecognizedEscape}")}";
                        break;

                    case RegexSearchErrorStatus.UnrecognizedUnicodeProperty:        // @"\p{Lll}"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnrecognizedUnicodeProperty}")}";
                        break;

                    case RegexSearchErrorStatus.UnterminatedBracket:                //  "[a-b"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnterminatedBracket}")}";
                        break;

                    case RegexSearchErrorStatus.UnterminatedComment:                // "(?#comment .*"
                        SearchCriteriaErrorOutput = $"{ResourceService.GetString("LabelRegexError_UnterminatedComment}")}";
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
        public SetRegexControlViewModel()
        {
            throw new NotImplementedException(nameof(SetRegexControlViewModel));
        }

        public SetRegexControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileSearchCriteriaManager fileSearchCriteriaManager,
            IHelpWindowViewModel helpWindowViewModel
        ) : base(messenger, settingsService, fileSearchCriteriaManager, helpWindowViewModel)
        {
            // ヘルプ画面を開きます
            HelpOpenCommand = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _HelpWindowViewModel.Initialize(HelpPage.Wildcard);
            });

            // リストボックスの選択状態が変わった時の処理をします。
            messenger.Register<IsSelectedRegexChangedMessage>(this, (_, m) =>
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
            messenger.Register<SelectedChangedRegexCriteriaRequestMessage>(this, (_, m) =>
                m.Reply(IsCriteriaConditionCorrent(m.RegexCriteria, m.OriginalRegexCriteria)));
        }

        #endregion コンストラクタ

        /// <summary>
        /// 正規表現検索条件を追加します。
        /// </summary>
        public override void AddCriteria()
        {
            var newRegex = new RegexCriteriaItemViewModel(_Messanger, _SettingsService)
            {
                Criteria = SearchCriteriaText,
            };
            CriteriaItems.Add(newRegex);
            _FileSearchCriteriaManager.AddCriteria(SearchCriteriaText, FileSearchOption.Regex);
            _Messanger.Send(new ChangeSelectedCountMessage());
            SearchCriteriaText = string.Empty;
        }

        /// <summary>
        /// 正規表現検索条件を削除します。
        /// </summary>
        public override void RemoveCriteria()
        {
            foreach (var item in SelectedItems)
            {
                _FileSearchCriteriaManager.RemoveCriteria(item.Criteria, FileSearchOption.Regex);
                //_selectTargetPageViewModel.SetAllTargetfilesCount();
                _Messanger.Send(new ChangeSelectedCountMessage());
                CriteriaItems.Remove(item);
            }
            SelectedItems.Clear();
            ModifyCriteriaCommand.NotifyCanExecuteChanged();
            RemoveCriteriaCommand.NotifyCanExecuteChanged();
            LeaveListBoxCriteria();
            _Messanger.Send(new NewRegexCriteriaFocusMessage());
        }

        /// <summary>
        /// リストボックスの正規表現検索条件から離れます。
        /// </summary>
        public override void LeaveListBoxCriteria()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                ModefyCriteria(listItem);
            }
            // 正規表現検索条件の新規欄を反映する
            IsCriteriaConditionCorrent(SearchCriteriaText);
        }

        /// <summary>
        /// リストボックスの正規表現検索条件から強制的に離れます。
        /// </summary>
        public override void LeaveListBoxCriteriaForce()
        {
            var listItem = SelectedItems.FirstOrDefault(c => c.IsEditMode);
            if (listItem != null)
            {
                listItem.IsEditMode = false;
                listItem.Criteria = listItem.OriginalCriteria;
                _Messanger.Send(new NewRegexCriteriaFocusMessage());
                IsCriteriaConditionCorrent(SearchCriteriaText);
            }
        }

        /// <summary>
        /// 正規表現検索条件を変更します。
        /// </summary>
        /// <param name="modifiedItem">変更された正規表現検索条件</param>
        public override void ModefyCriteria(BaseCriteriaItemViewModel modifiedItem)
        {
            _FileSearchCriteriaManager.RemoveCriteria(modifiedItem.OriginalCriteria, FileSearchOption.Wildcard);
            _FileSearchCriteriaManager.AddCriteria(modifiedItem.Criteria, FileSearchOption.Regex);
            _Messanger.Send(new ChangeSelectedCountMessage());
        }

        #region 正規表現チェック

        /// <summary>
        /// 正規表現文字列が正しいかを検査します。
        /// </summary>
        /// <param name="pattern">チェックする正規表現文字列</param>
        /// <param name="originalPattern">(必要ならば)元の正規表現文字列</param>
        /// <returns>正規表現文字列が正当かどうか</returns>
        public override bool IsCriteriaConditionCorrent(string pattern, string originalPattern = "")
        {
            // 空欄かチェックする
            if (string.IsNullOrEmpty(pattern))
            {
                SearchErrorStatus = RegexSearchErrorStatus.Empty;
                return false;
            }

            // 既に登録されているかをチェックする
            if (pattern != originalPattern)
            {
                foreach (var item in CriteriaItems)
                {
                    if (item.Criteria == pattern)
                    {
                        SearchErrorStatus = RegexSearchErrorStatus.AlreadyRegistered;
                        return false;
                    }
                }
            }

            // 正規表現のチェック処理
            try
            {
                var regex = new Regex(pattern);
            }
            catch (RegexParseException regException)
            {
                switch (regException.Error)
                {
                    case RegexParseError.AlternationHasComment:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasComment;
                        return false;

                    case RegexParseError.AlternationHasMalformedCondition:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasMalformedCondition;
                        return false;

                    case RegexParseError.AlternationHasMalformedReference:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasMalformedReference;
                        return false;

                    case RegexParseError.AlternationHasNamedCapture:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasNamedCapture;
                        return false;

                    case RegexParseError.AlternationHasTooManyConditions:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasTooManyConditions;
                        return false;

                    case RegexParseError.AlternationHasUndefinedReference:
                        SearchErrorStatus = RegexSearchErrorStatus.AlternationHasUndefinedReference;
                        return false;

                    case RegexParseError.CaptureGroupNameInvalid:
                        SearchErrorStatus = RegexSearchErrorStatus.CaptureGroupNameInvalid;
                        return false;

                    case RegexParseError.CaptureGroupOfZero:
                        SearchErrorStatus = RegexSearchErrorStatus.CaptureGroupOfZero;
                        return false;

                    case RegexParseError.ExclusionGroupNotLast:
                        SearchErrorStatus = RegexSearchErrorStatus.ExclusionGroupNotLast;
                        return false;

                    case RegexParseError.InsufficientClosingParentheses:
                        SearchErrorStatus = RegexSearchErrorStatus.InsufficientClosingParentheses;
                        return false;

                    case RegexParseError.InsufficientOpeningParentheses:
                        SearchErrorStatus = RegexSearchErrorStatus.InsufficientOpeningParentheses;
                        return false;

                    case RegexParseError.InsufficientOrInvalidHexDigits:
                        SearchErrorStatus = RegexSearchErrorStatus.InsufficientOrInvalidHexDigits;
                        return false;

                    case RegexParseError.InvalidGroupingConstruct:
                        SearchErrorStatus = RegexSearchErrorStatus.InvalidGroupingConstruct;
                        return false;

                    case RegexParseError.InvalidUnicodePropertyEscape:
                        SearchErrorStatus = RegexSearchErrorStatus.InvalidUnicodePropertyEscape;
                        return false;

                    case RegexParseError.MalformedNamedReference:
                        SearchErrorStatus = RegexSearchErrorStatus.MalformedNamedReference;
                        return false;

                    case RegexParseError.MalformedUnicodePropertyEscape:
                        SearchErrorStatus = RegexSearchErrorStatus.MalformedUnicodePropertyEscape;
                        return false;

                    case RegexParseError.NestedQuantifiersNotParenthesized:
                        SearchErrorStatus = RegexSearchErrorStatus.NestedQuantifiersNotParenthesized;
                        return false;

                    case RegexParseError.MissingControlCharacter:
                        SearchErrorStatus = RegexSearchErrorStatus.MissingControlCharacter;
                        return false;

                    case RegexParseError.QuantifierAfterNothing:
                        SearchErrorStatus = RegexSearchErrorStatus.QuantifierAfterNothing;
                        return false;

                    case RegexParseError.QuantifierOrCaptureGroupOutOfRange:
                        SearchErrorStatus = RegexSearchErrorStatus.QuantifierOrCaptureGroupOutOfRange;
                        return false;

                    case RegexParseError.ReversedCharacterRange:
                        SearchErrorStatus = RegexSearchErrorStatus.ReversedCharacterRange;
                        return false;

                    case RegexParseError.ReversedQuantifierRange:
                        SearchErrorStatus = RegexSearchErrorStatus.ReversedQuantifierRange;
                        return false;

                    case RegexParseError.ShorthandClassInCharacterRange:
                        SearchErrorStatus = RegexSearchErrorStatus.ShorthandClassInCharacterRange;
                        return false;

                    case RegexParseError.UndefinedNamedReference:
                        SearchErrorStatus = RegexSearchErrorStatus.UndefinedNamedReference;
                        return false;

                    case RegexParseError.UndefinedNumberedReference:
                        SearchErrorStatus = RegexSearchErrorStatus.UndefinedNumberedReference;
                        return false;

                    case RegexParseError.UnescapedEndingBackslash:
                        SearchErrorStatus = RegexSearchErrorStatus.UnescapedEndingBackslash;
                        return false;

                    case RegexParseError.Unknown:
                        SearchErrorStatus = RegexSearchErrorStatus.Unknown;
                        return false;

                    case RegexParseError.UnrecognizedControlCharacter:
                        SearchErrorStatus = RegexSearchErrorStatus.UnrecognizedControlCharacter;
                        return false;

                    case RegexParseError.UnrecognizedEscape:
                        SearchErrorStatus = RegexSearchErrorStatus.UnrecognizedEscape;
                        return false;

                    case RegexParseError.UnrecognizedUnicodeProperty:
                        SearchErrorStatus = RegexSearchErrorStatus.UnrecognizedUnicodeProperty;
                        return false;

                    case RegexParseError.UnterminatedBracket:
                        SearchErrorStatus = RegexSearchErrorStatus.UnterminatedBracket;
                        return false;

                    case RegexParseError.UnterminatedComment:
                        SearchErrorStatus = RegexSearchErrorStatus.UnterminatedComment;
                        return false;
                }
            }
            // 正規表現検索条件文字列に問題はなかった
            SearchErrorStatus = RegexSearchErrorStatus.None;
            return true;
        }

        #endregion 正規表現チェック
    }
}