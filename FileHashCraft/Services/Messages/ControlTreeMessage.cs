using System.Windows;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.Services.Messages
{
    public class TreeViewIsCheckBoxVisible : RequestMessage<Visibility>;

    /// <summary>
    /// ディレクトリノードを展開マネージャに追加するメッセージ
    /// </summary>
    public class AddToExpandDirectoryManagerMessage
    {
        public DirectoryTreeViewItemModel Child { get; set; }
        public AddToExpandDirectoryManagerMessage() { throw new NotImplementedException(nameof(AddToExpandDirectoryManagerMessage)); }
        public AddToExpandDirectoryManagerMessage(DirectoryTreeViewItemModel child)
        {
            Child = child;
        }
    }

    /// <summary>
    /// ディレクトリノードを展開マネージャから削除するメッセージ
    /// </summary>
    public class RemoveFromExpandDirectoryManagerMessage
    {
        public DirectoryTreeViewItemModel Child { get; set; }
        public RemoveFromExpandDirectoryManagerMessage() { throw new NotImplementedException(nameof(RemoveFromExpandDirectoryManagerMessage)); }
        public RemoveFromExpandDirectoryManagerMessage(DirectoryTreeViewItemModel child)
        {
            Child = child;
        }
    }
}
