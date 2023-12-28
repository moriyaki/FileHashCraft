using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FilOps.Models;
using FilOps.ViewModels.ExplorerPage;
using FilOps.ViewModels.FileSystemWatch;

namespace FilOps.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IDirectoryTreeViewControlViewModel
    {
        /// <summary>
        /// チェックボックスを表示するかどうかを設定します。
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        public void SetIsCheckBoxVisible(bool isVisible);

        /// <summary>
        /// ルートノードにアイテムを追加します。
        /// </summary>
        /// <param name="item">追加する FileItemInformation</param>
        public void AddRoot(FileItemInformation item);

        /// <summary>
        /// TreeViewItem が展開された時に展開マネージャに通知します。
        /// </summary>
        /// <param name="node">展開されたノード</param>
        public void AddDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node);

        /// <summary>
        /// TreeViewItem が展開された時に展開解除マネージャに通知します。
        /// </summary>
        /// <param name="node">展開解除されたノード</param>
        public void RemoveDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node);
    }
    #endregion インターフェース

    public partial class DirectoryTreeViewControlViewModel : ObservableObject, IDirectoryTreeViewControlViewModel
    {
        #region バインディング
        /// <summary>
        /// TreeView にバインドするコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewModel> TreeRoot { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            private set => _IsCheckBoxVisible = value;
        }

        private double _FontSize = SystemFonts.MessageFontSize;
        public double FontSize
        {
            get => _FontSize;
            set => SetProperty(ref _FontSize, value);
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
                    WeakReferenceMessenger.Default.Send(new CurrentChangeMessage(value));
                }
            }
        }
        #endregion バインディング

        #region 初期処理
        private readonly IDrivesFileSystemWatcherService _DrivesFileSystemWatcherService;
        private readonly IExpandedDirectoryManager _ExpandedDirectoryManager;
        private readonly IMainViewModel _MainWindowViewModel;

        /// <summary>
        /// 引数なしで生成はさせない
        /// </summary>
        /// <exception cref="NotImplementedException">DIコンテナを利用するように</exception>
        public DirectoryTreeViewControlViewModel()
        {
            throw new NotImplementedException();
        }

        // 通常コンストラクタ
        public DirectoryTreeViewControlViewModel(
            IDrivesFileSystemWatcherService drivesFileSystemWatcherService,
            IExpandedDirectoryManager expandDirManager,
            IMainViewModel mainViewModel)
        {
            _DrivesFileSystemWatcherService = drivesFileSystemWatcherService;
            _ExpandedDirectoryManager = expandDirManager;
            _MainWindowViewModel = mainViewModel;

            // カレントディレクトリの変更メッセージ
            WeakReferenceMessenger.Default.Register<CurrentChangeMessage>(this, (recipient, message) =>
            {
                if (CurrentFullPath == message.CurrentFullPath) return;
                CurrentFullPath = message.CurrentFullPath;
                FolderSelectedChanged(CurrentFullPath);
            });

            // フォントの変更メッセージ
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (recipient, message) =>
            {
                FontSize = message.FontSize;
            });

            foreach (var root in FileSystemInformationManager.ScanDrives())
            {
                _DrivesFileSystemWatcherService.SetRootDirectoryWatcher(root);
            }
            _DrivesFileSystemWatcherService.Changed += DirectoryChanged;
            _DrivesFileSystemWatcherService.Created += DirectoryCreated;
            _DrivesFileSystemWatcherService.Renamed += DirectoryRenamed;
            _DrivesFileSystemWatcherService.OpticalDriveMediaInserted += OpticalDriveMediaInserted;
            _DrivesFileSystemWatcherService.OpticalDriveMediaEjected += EjectOpticalDriveMedia;
        }

        /// <summary>
        /// チェックボックスを表示するかどうかを設定します。
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        public void SetIsCheckBoxVisible(bool isVisible)
            => IsCheckBoxVisible = isVisible ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// ルートノードにアイテムを追加します。
        /// </summary>
        /// <param name="item">追加する FileItemInformation</param>
        public void AddRoot(FileItemInformation item)
        {
            var info = new DirectoryTreeViewModel(this, item);
            TreeRoot.Add(info);
            _ExpandedDirectoryManager.AddDirectory(item.FullPath);
        }
        #endregion 初期処理

        #region カレントディレクトリ移動
        /// <summary>
        /// カレントディレクトリが変更されたときの処理を行います。
        /// </summary>
        /// <param name="changedPath">変更されたカレントディレクトリのパス</param>
        public void FolderSelectedChanged(string changedPath)
        {
            // パスの最後がディレクトリセパレータで終わる場合は除去
            changedPath = changedPath.Length == 3 ? changedPath : changedPath.TrimEnd(Path.DirectorySeparatorChar);

            // ルートディレクトリにある場合は選択状態に設定して終了
            var selectedRoot = TreeRoot.FirstOrDefault(root => Path.Equals(root.FullPath, changedPath));
            if (selectedRoot != null) { 
                selectedRoot.IsSelected = true;
                return;
            }

            // サブディレクトリ内の場合は一部一致するルートディレクトリを特定し、ルートディレクトリを展開
            var subDirectoryRoot = TreeRoot.FirstOrDefault(root => changedPath.Contains(root.FullPath));
            if (subDirectoryRoot == null) return;

            if (subDirectoryRoot is not DirectoryTreeViewModel selectingVM) return;
            selectingVM.IsExpanded = true;

            var directories = GetDirectoryNames(changedPath).ToList();

            // パス内の各ディレクトリに対して処理を実行
            foreach (var directory in directories)
            {
                // 親ディレクトリの各子ディレクトリに対して処理を実行
                foreach (var child in selectingVM.Children)
                {
                    if (child.FullPath == directory)
                    {
                        selectingVM = child;
                        if (Path.Equals(directory, changedPath))
                        {
                            // カレントディレクトリが見つかった
                            child.IsSelected = true;
                            return;
                        }
                        else
                        {
                            // サブディレクトリを展開する
                            child.IsExpanded = true;
                        }
                    }
                }
            }
            return;
        }

        /// <summary>
        /// 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを取得するディレクトリ</param>
        /// <returns>親ディレクトリからのコレクション</returns>
        public static IEnumerable<string> GetDirectoryNames(string path)
        {
            // パスの区切り文字に関係なく分割する
            var pathSeparated = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string fullPath = string.Empty;

            foreach (var directoryName in pathSeparated)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    // ルートディレクトリの場合、区切り文字を含めて追加
                    fullPath = directoryName + Path.DirectorySeparatorChar;
                }
                else
                {
                    // パスを結合
                    fullPath = Path.Combine(fullPath, directoryName);
                }

                yield return fullPath;
            }
        }
        #endregion カレントディレクトリ移動

        #region 展開マネージャへの追加削除処理
        /// <summary>
        /// TreeViewItem が展開された時に展開マネージャに通知します。
        /// </summary>
        /// <param name="node">展開されたノード</param>
        public void AddDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node)
        {
            _ExpandedDirectoryManager.AddDirectory(node.FullPath);
            if (node.IsExpanded)
            {
                foreach (var child in node.Children)
                {
                    AddDirectoryToExpandedDirectoryManager(child);
                }
            }
        }

        /// <summary>
        /// TreeViewItem が展開された時に展開解除マネージャに通知します。
        /// </summary>
        /// <param name="node">展開解除されたノード</param>
        public void RemoveDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node)
        {
            _ExpandedDirectoryManager.RemoveDirectory(node.FullPath);
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    RemoveDirectoryToExpandedDirectoryManager(child);
                }
            }
        }
        #endregion 展開マネージャへの追加削除処理
    }
}
