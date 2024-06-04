namespace FileHashCraft.Services.Messages
{
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
    public class ExtentionCheckChangedToListBox
    {
        public string Name { get; set; } = string.Empty;
        public bool IsChecked;
        public ExtentionCheckChangedToListBox() { throw new NotImplementedException(); }
        public ExtentionCheckChangedToListBox(string name, bool isChecked)
        {
            Name = name;
            IsChecked = isChecked;
        }
    }

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
}
