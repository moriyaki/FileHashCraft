/*  ControDirectoryTreeViewlViewModel.cs

    ディレクトリツリービューの ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.FileSystemWatcherServices;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.ControlDirectoryTree;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IControDirectoryTreeViewlModel
    {
        /// <summary>
        /// ツリービューのルートコレクション
        /// </summary>
        ObservableCollection<DirectoryTreeViewItemModel> TreeRoot { get; }
        /// <summary>
        /// チェックボックスを表示するか否か
        /// </summary>
        void SetIsCheckBoxVisible(bool isVisible);
        /// <summary>
        /// チェックボックスが表示されるか否かを設定します。
        /// </summary>
        Visibility IsCheckBoxVisible { get; }
        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        string CurrentFullPath { get; set; }
        /// <summary>
        /// ルートにアイテムを追加します。
        /// </summary>
        void AddRoot(FileItemInformation item, bool findSpecial);
        /// <summary>
        /// ルートアイテムをクリアします。
        /// </summary>
        void ClearRoot();
        /// <summary>
        /// ツリービューの横幅設定をします。
        /// </summary>
        double TreeWidth { get; set; }
    }
    #endregion インターフェース
    public partial class ControDirectoryTreeViewModel : BaseViewModel, IControDirectoryTreeViewlModel
    {
        #region バインディング
        /// <summary>
        /// TreeView にバインドするコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewItemModel> TreeRoot { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            private set => _IsCheckBoxVisible = value;
        }

        /// <summary>
        /// カレントディレクトリのフルパス
        /// </summary>
        private string _CurrentFullPath = string.Empty;
        public string CurrentFullPath
        {
            get => _CurrentFullPath;
            set
            {
                if (_CurrentFullPath != value)
                {
                    SetProperty(ref _CurrentFullPath, value);
                    // カレントディレクトリを TreeView の選択状態に反映
                    FolderSelectedChanged(value);
                    // カレントディレクトリ変更のメッセージ発信
                    _fileSystemServices.NotifyChangeCurrentDirectory(value);
                }
            }
        }
        /// <summary>
        /// ツリービュー横幅の設定
        /// </summary>
        private double _TreeWidth;
        public double TreeWidth
        {
            get => _TreeWidth;
            set
            {
                if (_TreeWidth == value) { return; }
                SetProperty(ref _TreeWidth, value);
                _settingsService.SendTreeWidth(value);
            }
        }
        #endregion バインディング

        #region コンストラクタと初期処理
        private readonly IFileSystemServices _fileSystemServices;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly ITreeManager _treeManager;

        /// <summary>
        /// 引数なしの直接呼び出しは許容しません。
        /// </summary>
        /// <exception cref="NotImplementedException">引数無しの直接呼び出し</exception>
        public ControDirectoryTreeViewModel() { throw new NotImplementedException(nameof(ControDirectoryTreeViewModel)); }

        // 通常コンストラクタ
        public ControDirectoryTreeViewModel(
            IFileSystemServices fileSystemService,
            ISettingsService settingsService,
            IFileWatcherService fileWatcherService,
            ITreeManager treeManager
        ) : base(settingsService)
        {
            _fileSystemServices = fileSystemService;
            _fileWatcherService = fileWatcherService;
            _treeManager = treeManager;

            // カレントディレクトリの変更メッセージ
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChangedMessage>(this, (_, message) =>
            {
                if (CurrentFullPath == message.CurrentFullPath) return;
                CurrentFullPath = message.CurrentFullPath;
                FolderSelectedChanged(CurrentFullPath);
            });

            // ツリービュー幅変更
            WeakReferenceMessenger.Default.Register<TreeWidthChangedMessage>(this, (_, m)
                => TreeWidth = m.TreeWidth);

            foreach (var root in SpecialFolderAndRootDrives.ScanDrives())
            {
                _fileWatcherService.SetRootDirectoryWatcher(root);
            }

            // ディレクトリの内容が変更された
            WeakReferenceMessenger.Default.Register<DirectoryItemDeletedMessage>(this, async (_, m)
                => await DirectoryChanged(m.DeletedFullPath));

            // ディレクトリの内容が追加された
            WeakReferenceMessenger.Default.Register<DirectoryItemCreatedMessage>(this, async (_, m)
                => await DirectoryCreated(m.CreatedFullPath));

            // ディレクトリの名前が変更された
            WeakReferenceMessenger.Default.Register<DirectoryItemRenamedMessage>(this, async(_, m)
                => await DirectoryRenamed(m.OldFullPath, m.NewFullPath));

            // リムーバブルドライブが追加または挿入された
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaInsertedMessage>(this, async (_, m)
                => await OpticalDriveMediaInserted(m.InsertedPath));

            // リムーバブルドライブがイジェクトされた
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaEjectedMessage>(this, async (_, m)
                => await OpticalDriveMediaEjected(m.EjectedPath));

            // ツリービューのチェックボックスを表示するか否か
            WeakReferenceMessenger.Default.Register<TreeViewIsCheckBoxVisible>(this, (_, m)
                => m.Reply(IsCheckBoxVisible));

            _TreeWidth = _settingsService.TreeWidth;
        }

        /// <summary>
        /// チェックボックスを表示するかどうかを設定します。
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        public void SetIsCheckBoxVisible(bool isVisible)
        {
            IsCheckBoxVisible = isVisible ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged(nameof(IsCheckBoxVisible));
        }

        /// <summary>
        /// ルートノードにアイテムを追加します。
        /// </summary>
        /// <param name="item">追加する FileItemInformation</param>
        public void AddRoot(FileItemInformation item, bool findSpecial)
        {
            var currentNode = new DirectoryTreeViewItemModel(item);
            TreeRoot.Add(currentNode);
            // 内部キックをしない場合そのまま終了
            if (!findSpecial) return;

            // 展開ディレクトリに追加
            _treeManager.AddDirectory(item.FullPath);
            /* ルートドライブが追加された時、特殊フォルダは追加されている
              * 特殊フォルダがルートドライブに含まれているなら、内部的に Kick して展開しておく
              * そうすることで、特殊フォルダのチェックに対してドライブ下のディレクトリにも反映される
              */
            // ルートドライブではない場合終了
            if (item.FullPath.Length != 3) { return; }

            // 追加されたノードのフルパスで始まるノードを検索する
            var driveNode = TreeRoot.Where(root => root.FullPath.StartsWith(currentNode.FullPath));
            if (driveNode == null) { return; }

            foreach (var drive in driveNode)
            {
                // 各追加されたノードで始まるパスを分解する
                var dirs = DirectoryNameService.GetDirectoryNames(drive.FullPath, drive.FullPath);
                //if (node == null) continue;
                var childNode = currentNode;

                foreach (var dir in dirs)
                {
                    if (childNode == null) break;

                    // サブディレクトリを内部キックし、サブディレクトリからノードを取得する
                    childNode.KickChild();
                    childNode = childNode.Children.FirstOrDefault(c => c.FullPath == dir);
                }
            }
        }

        /// <summary>
        /// ツリーノードのアイテムをクリアする
        /// </summary>
        public void ClearRoot()
        {
            TreeRoot.Clear();
        }
        #endregion コンストラクタと初期処理

        #region カレントディレクトリ移動
        /// <summary>
        /// カレントディレクトリが変更されたときの処理を行います。
        /// </summary>
        /// <param name="changedPath">変更されたカレントディレクトリのパス</param>
        public void FolderSelectedChanged(string changedPath)
        {
            // ツリールートから一部一致するルートディレクトリを特定する
            var searchNode = TreeRoot.FirstOrDefault(root => changedPath.Contains(root.FullPath));
            if (searchNode == null) { return; }

            if (searchNode.FullPath == changedPath)
            {
                // ツリーノードのトップと等しければ選択して終了
                searchNode.IsSelected = true;
                return;
            }
            // ノードを展開する
            searchNode.IsExpanded = true;

            // パス内の各ディレクトリに対して処理を実行
            var directoryNames = DirectoryNameService.GetDirectoryNames(changedPath, searchNode.FullPath);

            foreach (var directory in directoryNames)
            {
                var child = searchNode.Children.FirstOrDefault(c => c.FullPath == directory);

                // 子ディレクトリに対して処理を実行
                if (child == null) break;

                if (directory == changedPath.TrimEnd('\\'))
                {
                    // カレントディレクトリが見つかった
                    child.IsSelected = true;
                    return;
                }
                searchNode = child;
                // サブディレクトリを展開する
                searchNode.IsExpanded = true;
            }
        }
        #endregion カレントディレクトリ移動
    }
}
