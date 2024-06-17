using CommunityToolkit.Mvvm.Messaging.Messages;
using FileHashCraft.ViewModels.PageSelectTarget;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.Services.Messages
{
    public class FileScanFinished;

    #region 拡張子チェックボックスメッセージ
    /// <summary>
    /// 拡張子チェックボックスがチェックされたら拡張子グループ変更するメッセージ
    /// </summary>
    public class ExtentionChechReflectToGroup
    {
        public string Name { get; set; } = string.Empty;
        public ExtentionChechReflectToGroup() { throw new NotImplementedException(nameof(ExtentionChechReflectToGroup)); }
        public ExtentionChechReflectToGroup(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 拡張子チェックボックスがチェック解除されたら拡張子グループ変更するメッセージ
    /// </summary>
    public class ExtentionUnchechReflectToGroup
    {
        public string Name { get; set; } = string.Empty;
        public ExtentionUnchechReflectToGroup() { throw new NotImplementedException(nameof(ExtentionUnchechReflectToGroup)); }
        public ExtentionUnchechReflectToGroup(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// チェック状態をカレントディレクトリリストボックスに反映するメッセージ
    /// </summary>
    public class ExtentionCheckChangedToListBox;

    /// <summary>
    /// 拡張子グループのチェック状態に連動して拡張子のチェック状態を変更するメッセージ
    /// </summary>
    public class ExtentionGroupChecked
    {
        public bool IsChecked { get; set; }
        public IEnumerable<string> ExtentionCollection;
        public ExtentionGroupChecked() { throw new NotImplementedException(nameof(ExtentionGroupChecked)); }
        public ExtentionGroupChecked(bool isChecked, IEnumerable<string> extentionCollection)
        {
            IsChecked = isChecked;
            ExtentionCollection = extentionCollection;
        }
    }
    #endregion 拡張子チェックボックスメッセージ

    #region ワイルドカード/正規表現共通メッセージ
    /// <summary>
    /// 編集モード/閲覧モードの変更を通知するメッセージ
    /// </summary>
    public class IsEditModeChanged;
    #endregion ワイルドカード/正規表現共通メッセージ

    #region ワイルドカードメッセージ
    /// <summary>
    /// ワイルドカード一覧のテキストボックスにフォーカスを移すメッセージ
    /// </summary>
    public class ListBoxSeletedWildcardTextBoxFocus;

    /// <summary>
    /// ワイルドカード検索条件入力テキストボックスにフォーカスを戻すメッセージ
    /// </summary>
    public class NewWildcardCriteriaFocus;

    /// <summary>
    /// 選択されたワイルドカードアイテムの状態が変わったことを知らせるメッセージ
    /// </summary>
    public class IsSelectedWildcardChanged
    {
        public bool IsSelected { get; set; }
        public WildcardCriteriaItemViewModel SelectedItem { get; set; }
        public IsSelectedWildcardChanged() { throw new NotSupportedException(nameof(IsSelectedWildcardChanged)); }
        public IsSelectedWildcardChanged(bool isSelected, WildcardCriteriaItemViewModel selectedItem)
        {
            IsSelected = isSelected;
            SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// リストボックスのワイルドカード検索条件編集の内容通知メッセージ
    /// </summary>
    public class SelectedChangedWildcardCriteria : RequestMessage<bool>
    {
        public string WildcardCriteria { get; set; } = string.Empty;
        public string OriginalWildcardCriteria { get; set; } = string.Empty;
        public SelectedChangedWildcardCriteria() { throw new NotImplementedException(nameof(SelectedChangedWildcardCriteria)); }
        public SelectedChangedWildcardCriteria(string wildcardCriteria)
        {
            WildcardCriteria = wildcardCriteria;
        }
        public SelectedChangedWildcardCriteria(string wildcardCriteria, string oldWildcardCriteria)
        {
            WildcardCriteria = wildcardCriteria;
            OriginalWildcardCriteria = oldWildcardCriteria;
        }
    }
    #endregion ワイルドカードメッセージ

    #region 正規表現メッセージ
    /// <summary>
    /// 正規表現一覧のテキストボックスにフォーカスを移すメッセージ
    /// </summary>
    public class ListBoxSeletedRegexTextBoxFocus;

    /// <summary>
    /// 正規表現検索条件入力テキストボックスにフォーカスを戻すメッセージ
    /// </summary>
    public class NewRegexCriteriaFocus;

    /// <summary>
    /// 選択された正規表現アイテムの状態が変わったことを知らせるメッセージ
    /// </summary>
    public class IsSelectedRegexChanged
    {
        public bool IsSelected { get; set; }
        public RegexCriteriaItemViewModel SelectedItem { get; set; }
        public IsSelectedRegexChanged() { throw new NotSupportedException(nameof(IsSelectedRegexChanged)); }
        public IsSelectedRegexChanged(bool isSelected, RegexCriteriaItemViewModel selectedItem)
        {
            IsSelected = isSelected;
            SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// リストボックスの正規表現検索条件編集の内容通知メッセージ
    /// </summary>
    public class SelectedChangedRegexCriteria : RequestMessage<bool>
    {
        public string RegexCriteria { get; set; } = string.Empty;
        public string OriginalRegexCriteria { get; set; } = string.Empty;
        public SelectedChangedRegexCriteria() { throw new NotImplementedException(nameof(SelectedChangedRegexCriteria)); }
        public SelectedChangedRegexCriteria(string wildcardCriteria)
        {
            RegexCriteria = wildcardCriteria;
        }
        public SelectedChangedRegexCriteria(string regexCriteria, string oldRegexCriteria)
        {
            RegexCriteria = regexCriteria;
            OriginalRegexCriteria = oldRegexCriteria;
        }
    }
    #endregion 正規表現メッセージ
}
