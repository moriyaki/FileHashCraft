using CommunityToolkit.Mvvm.Messaging.Messages;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.Services.Messages
{
    public class FileScanFinished;

    #region ファイルスキャンメッセージ
    /// <summary>
    /// スキャンした全ディレクトリ数に加算するメッセージ
    /// </summary>
    public class AddScannedDirectoriesCountMessage
    {
        public int DirectoriesCount { get; set; } = 0;
        public AddScannedDirectoriesCountMessage() { throw new NotImplementedException(nameof(AddScannedDirectoriesCountMessage)); }
        public AddScannedDirectoriesCountMessage(int directoryCount)
        {
            DirectoriesCount = directoryCount;
        }
    }

    /// <summary>
    /// ファイルスキャンが完了したディレクトリ数に加算するメッセージ
    /// </summary>
    public class AddFilesScannedDirectoriesCountMessage;

    /// <summary>
    /// ハッシュ取得対象となる総対象ファイル数にファイル数を設定するメッセージ
    /// </summary>
    public class SetAllTargetfilesCountMessge;

    /// <summary>
    /// 拡張子をリストボックスに追加するメッセージ
    /// </summary>
    public class AddExtentionMessage
    {
        public string Extention { get; set; } = string.Empty;
        public AddExtentionMessage() { throw new NotImplementedException(nameof(AddExtentionMessage)); }
        public AddExtentionMessage(string extention)
        {
            Extention = extention;
        }
    }

    /// <summary>
    /// ファイルの拡張子グループをリストボックスに追加するメッセージ
    /// </summary>
    public class AddFileTypesMessage;

    #endregion ファイルスキャンメッセージ

    #region 拡張子チェックボックスメッセージ
    /// <summary>
    /// 拡張子がチェックボックスされたら拡張子グループ変更するメッセージ
    /// </summary>
    public class ExtentionCheckReflectToGroupMessage
    {
        public string Name { get; set; } = string.Empty;
        public ExtentionCheckReflectToGroupMessage() { throw new NotImplementedException(nameof(ExtentionCheckReflectToGroupMessage)); }
        public ExtentionCheckReflectToGroupMessage(string name)
        {
            Name = name;
        }
    }
    /// <summary>
    /// 拡張子がチェックボックス解除されたら拡張子グループ変更するメッセージ
    /// </summary>
    public class ExtentionUncheckReflectToGroupMessage
    {
        public string Name { get; set; } = string.Empty;
        public ExtentionUncheckReflectToGroupMessage() { throw new NotImplementedException(nameof(ExtentionUncheckReflectToGroupMessage)); }
        public ExtentionUncheckReflectToGroupMessage(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// チェック状態をカレントディレクトリリストボックスに反映するメッセージ
    /// </summary>
    public class ExtentionCheckChangedToListBoxMessage;

    /// <summary>
    /// 拡張子グループのチェック状態に連動して拡張子のチェック状態を変更するメッセージ
    /// </summary>
    public class ExtentionGroupCheckedMessage
    {
        public bool IsChecked { get; set; }
        public IEnumerable<string> ExtentionCollection;
        public ExtentionGroupCheckedMessage() { throw new NotImplementedException(nameof(ExtentionGroupCheckedMessage)); }
        public ExtentionGroupCheckedMessage(bool isChecked, IEnumerable<string> extentionCollection)
        {
            IsChecked = isChecked;
            ExtentionCollection = extentionCollection;
        }
    }

    public class ChangeSelectedCountMessage;
    #endregion 拡張子チェックボックスメッセージ

    #region ワイルドカード/正規表現共通メッセージ
    /// <summary>
    /// 編集モード/閲覧モードの変更を通知するメッセージ
    /// </summary>
    public class IsEditModeChangedMessage;
    #endregion ワイルドカード/正規表現共通メッセージ

    #region ワイルドカードメッセージ
    /// <summary>
    /// ワイルドカード一覧のテキストボックスにフォーカスを移すメッセージ
    /// </summary>
    public class ListBoxSeletedWildcardTextBoxFocusMessage;

    /// <summary>
    /// ワイルドカード検索条件入力テキストボックスにフォーカスを戻すメッセージ
    /// </summary>
    public class NewWildcardCriteriaFocusMessage;

    /// <summary>
    /// 選択されたワイルドカードアイテムの状態が変わったことを知らせるメッセージ
    /// </summary>
    public class IsSelectedWildcardChangedMessage
    {
        public bool IsSelected { get; set; }
        public WildcardCriteriaItemViewModel SelectedItem { get; set; }
        public IsSelectedWildcardChangedMessage() { throw new NotSupportedException(nameof(IsSelectedWildcardChangedMessage)); }
        public IsSelectedWildcardChangedMessage(bool isSelected, WildcardCriteriaItemViewModel selectedItem)
        {
            IsSelected = isSelected;
            SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// リストボックスのワイルドカード検索条件編集の内容通知メッセージ
    /// </summary>
    public class SelectedChangedWildcardCriteriaRequestMessage : RequestMessage<bool>
    {
        public string WildcardCriteria { get; set; } = string.Empty;
        public string OriginalWildcardCriteria { get; set; } = string.Empty;
        public SelectedChangedWildcardCriteriaRequestMessage() { throw new NotImplementedException(nameof(SelectedChangedWildcardCriteriaRequestMessage)); }
        public SelectedChangedWildcardCriteriaRequestMessage(string wildcardCriteria)
        {
            WildcardCriteria = wildcardCriteria;
        }
        public SelectedChangedWildcardCriteriaRequestMessage(string wildcardCriteria, string oldWildcardCriteria)
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
    public class ListBoxSeletedRegexTextBoxFocusMessage;

    /// <summary>
    /// 正規表現検索条件入力テキストボックスにフォーカスを戻すメッセージ
    /// </summary>
    public class NewRegexCriteriaFocusMessage;

    /// <summary>
    /// 選択された正規表現アイテムの状態が変わったことを知らせるメッセージ
    /// </summary>
    public class IsSelectedRegexChangedMessage
    {
        public bool IsSelected { get; set; }
        public RegexCriteriaItemViewModel SelectedItem { get; set; }
        public IsSelectedRegexChangedMessage() { throw new NotSupportedException(nameof(IsSelectedRegexChangedMessage)); }
        public IsSelectedRegexChangedMessage(bool isSelected, RegexCriteriaItemViewModel selectedItem)
        {
            IsSelected = isSelected;
            SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// リストボックスの正規表現検索条件編集の内容通知メッセージ
    /// </summary>
    public class SelectedChangedRegexCriteriaRequestMessage : RequestMessage<bool>
    {
        public string RegexCriteria { get; set; } = string.Empty;
        public string OriginalRegexCriteria { get; set; } = string.Empty;
        public SelectedChangedRegexCriteriaRequestMessage() { throw new NotImplementedException(nameof(SelectedChangedRegexCriteriaRequestMessage)); }
        public SelectedChangedRegexCriteriaRequestMessage(string wildcardCriteria)
        {
            RegexCriteria = wildcardCriteria;
        }
        public SelectedChangedRegexCriteriaRequestMessage(string regexCriteria, string oldRegexCriteria)
        {
            RegexCriteria = regexCriteria;
            OriginalRegexCriteria = oldRegexCriteria;
        }
    }
    #endregion 正規表現メッセージ
}
