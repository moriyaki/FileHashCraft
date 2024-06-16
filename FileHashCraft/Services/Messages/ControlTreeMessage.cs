using System.Windows;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.Services.Messages
{
    public class TreeViewIsCheckBoxVisible : RequestMessage<Visibility>;

    /// <summary>
    /// ディレクトリノードを展開マネージャに追加するメッセージ
    /// </summary>
    public class AddToExpandDirectoryManager
    {
        public DirectoryTreeViewItemModel Child { get; set; }
        public AddToExpandDirectoryManager() { throw new NotImplementedException(nameof(AddToExpandDirectoryManager)); }
        public AddToExpandDirectoryManager(DirectoryTreeViewItemModel child)
        {
            Child = child;
        }
    }

    /// <summary>
    /// ディレクトリノードを展開マネージャから削除するメッセージ
    /// </summary>
    public class RemoveFromExpandDirectoryManager
    {
        public DirectoryTreeViewItemModel Child { get; set; }
        public RemoveFromExpandDirectoryManager() { throw new NotImplementedException(nameof(RemoveFromExpandDirectoryManager)); }
        public RemoveFromExpandDirectoryManager(DirectoryTreeViewItemModel child)
        {
            Child = child;
        }
    }
}
