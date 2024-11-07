using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    public interface IDupLinkedDirsFilesTreeViewModel;

    public class DupLinkedDirsFilesTreeViewModel : BaseViewModel, IDupLinkedDirsFilesTreeViewModel
    {
        #region バインディング

        public ObservableCollection<IDupTreeItem> TreeRoot { get; set; } = [];

        #endregion バインディング

        #region コンストラクタ

        private readonly IFileManager _FileManager;

        public DupLinkedDirsFilesTreeViewModel()
        { throw new NotImplementedException(nameof(DupLinkedDirsFilesTreeViewModel)); }

        public DupLinkedDirsFilesTreeViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileManager fileManager
            ) : base(messenger, settingsService)
        {
            _FileManager = fileManager;

            _Messanger.Register<DuplicateLinkClearMessage>(this, (_, _) =>
            {
                if (TreeRoot.Any()) { TreeRoot.Clear(); }
            });

            // 重複ファイルを含むディレクトリ受信
            _Messanger.Register<DuplicateLinkFilesMessage>(this, (_, m) =>
            {
                // 親ディレクトリの設定
                var parent = new DupTreeItem(m.Directory, true);
                TreeRoot.Add(parent);

                // 各ファイルのアイテム設定
                foreach (var file in fileManager.EnumerateFiles(m.Directory))
                {
                    var hashFile = m.HashFiles.FirstOrDefault(f => f.FileFullPath == file);
                    var child = hashFile != null
                        ? new DupTreeItem(m.Directory, hashFile)
                        : new DupTreeItem(m.Directory, file);
                    parent.Children.Add(child);
                }
            });
        }

        #endregion コンストラクタ
    }
}