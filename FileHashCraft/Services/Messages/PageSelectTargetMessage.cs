using CommunityToolkit.Mvvm.Messaging.Messages;
using FileHashCraft.ViewModels.PageSelectTarget;

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
        public ExtentionChechReflectToGroup() { throw new NotImplementedException(); }
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
        public ExtentionUnchechReflectToGroup() { throw new NotImplementedException(); }
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
        public ExtentionGroupChecked() { throw new NotImplementedException(); }
        public ExtentionGroupChecked(bool isChecked, IEnumerable<string> extentionCollection)
        {
            IsChecked = isChecked;
            ExtentionCollection = extentionCollection;
        }
    }
    #endregion 拡張子チェックボックスメッセージ

    #region ワイルドカードメッセージ
    /// <summary>
    /// 編集モード/閲覧モードの変更を通知するメッセージ
    /// </summary>
    public class IsEditModeChanged;

    /// <summary>
    /// ワイルドカード一覧のテキストボックスにフォーカスを移すメッセージ
    /// </summary>
    public class ListBoxSeletedTextBoxFocus;

    /// <summary>
    /// ワイルドカード検索条件入力テキストボックスにフォーカスを戻すメッセージ
    /// </summary>
    public class NewCriteriaFocus;

    /// <summary>
    /// 選択されたアイテムの状態が変わったことを知らせるメッセージ
    /// </summary>
    public class IsSelectedChanged
    {
        public bool IsSelected { get; set; }
        public WildcardItemViewModel SelectedItem { get; set; }
        public IsSelectedChanged() { throw new NotSupportedException(); }
        public IsSelectedChanged(bool isSelected, WildcardItemViewModel selectedItem)
        {
            IsSelected = isSelected;
            SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// リストボックスのワイルドカード検索条件編集の内容通知メッセージ
    /// </summary>
    public class SelectedChangedCriteria : RequestMessage<bool>
    {
        public string WildcardCriteria { get; set; } = string.Empty;
        public string OriginalWildcardCriteria { get; set; } = string.Empty;
        public SelectedChangedCriteria() { throw new NotImplementedException(); }
        public SelectedChangedCriteria(string wildcardCriteria)
        {
            WildcardCriteria = wildcardCriteria;
        }
        public SelectedChangedCriteria(string wildcardCriteria, string oldWildcardCriteria)
        {
            WildcardCriteria = wildcardCriteria;
            OriginalWildcardCriteria = oldWildcardCriteria;
        }
    }
    #endregion ワイルドカードメッセージ
}
