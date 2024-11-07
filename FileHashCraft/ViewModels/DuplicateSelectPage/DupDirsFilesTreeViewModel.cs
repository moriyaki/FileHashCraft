using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.HashCalc;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    #region インターフェース

    public interface IDupDirsFilesTreeViewModel
    {
        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        double DupDirsFilesTreeViewWidth { get; set; }
    }

    #endregion インターフェース

    public class DupDirsFilesTreeViewModel : BaseViewModel, IDupDirsFilesTreeViewModel
    {
        #region バインディング

        public ObservableCollection<IDupTreeItem> TreeRoot { get; set; } = [];

        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        private double _DupDirsFilesTreeViewWidth;

        public double DupDirsFilesTreeViewWidth
        {
            get => _DupDirsFilesTreeViewWidth;
            set
            {
                if (_DupDirsFilesTreeViewWidth == value) return;
                SetProperty(ref _DupDirsFilesTreeViewWidth, value);
                _SettingsService.DupDirsFilesTreeViewWidth = value;
            }
        }

        #endregion バインディング

        #region コンストラクタ

        private readonly IFileManager _FileManager;
        private readonly IDuplicateFilesManager _DuplicateFilesManager;

        public DupDirsFilesTreeViewModel()
        { throw new NotImplementedException(nameof(DupDirsFilesTreeViewModel)); }

        public DupDirsFilesTreeViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileManager fileManager,
            IDuplicateFilesManager duplicateFilesManager
            ) : base(messenger, settingsService)
        {
            _FileManager = fileManager;
            _DuplicateFilesManager = duplicateFilesManager;

            // 重複ファイルを含むディレクトリ受信
            _Messanger.Register<DuplicateFilesMessage>(this, (_, m) =>
            {
                // ツリービューのクリア
                if (TreeRoot.Any()) { TreeRoot.Clear(); }
                _Messanger.Send(new DuplicateLinkClearMessage());

                // 親ディレクトリの設定
                var parent = new DupTreeItem(m.Directory, false);
                TreeRoot.Add(parent);
                TreeRoot[0].IsSelected = true;
                TreeRoot[0].IsExpanded = true;

                // 各ファイルのアイテム設定
                foreach (var file in fileManager.EnumerateFiles(m.Directory))
                {
                    var hashFile = m.HashFiles.FirstOrDefault(f => f.FileFullPath == file);
                    var child = hashFile != null
                        ? new DupTreeItem(m.Directory, hashFile)
                        : new DupTreeItem(m.Directory, file);
                    parent.Children.Add(child);
                }

                // ハッシュを持つファイルをDuplicateFilesManagerに渡す
                var hashes = new HashSet<string>();
                foreach (var hashFile in m.HashFiles)
                {
                    hashes.Add(hashFile.FileHash);
                }
                _DuplicateFilesManager.GetDuplicateLinkFiles(hashes, m.Directory);
            });

            DupDirsFilesTreeViewWidth = settingsService.DupDirsFilesTreeViewWidth;
        }

        #endregion コンストラクタ
    }
}