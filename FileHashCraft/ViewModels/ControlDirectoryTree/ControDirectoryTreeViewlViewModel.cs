using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.FileSystemWatch;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IControDirectoryTreeViewlViewModel
    {
        /// <summary>
        /// ツリービューのルートコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewModel> TreeRoot { get; }
        /// <summary>
        /// チェックボックスを表示するか否か
        /// </summary>
        public void SetIsCheckBoxVisible(bool isVisible);
        /// <summary>
        /// チェックボックスが表示されるか否か
        /// </summary>
        public Visibility IsCheckBoxVisible { get; }
        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        public string CurrentFullPath { get; set; }
        /// <summary>
        /// ルートにアイテムを追加します。
        /// </summary>
        public DirectoryTreeViewModel AddRoot(FileItemInformation item, bool findSpecial);
        /// <summary>
        /// ルートアイテムをクリアします。
        /// </summary>
        public void ClearRoot();
        /// <summary>
        /// ディレクトリノードを展開マネージャに追加します。
        /// </summary>
        public void AddDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node);
        /// <summary>
        /// ディレクトリノードを展開マネージャから削除します。
        /// </summary>
        public void RemoveDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node);
        /// <summary>
        /// チェックマネージャからツリービューのチェックボックス状態を確認します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager();
    }
    #endregion インターフェース
    public partial class ControDirectoryTreeViewlViewModel : ObservableObject, IControDirectoryTreeViewlViewModel
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

        /// <summary>
        /// フォントの取得と設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _MainWindowViewModel.UsingFont;
            set
            {
                _MainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの取得と設定
        /// </summary>
        public double FontSize
        {
            get => _MainWindowViewModel.FontSize;
            set
            {
                _MainWindowViewModel.FontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
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
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly IExpandedDirectoryManager _ExpandedDirectoryManager;
        public readonly ISpecialFolderAndRootDrives _SpecialFolderAndRootDrives;
        public readonly IWindowsAPI _WindowsAPI;
        private readonly IMainWindowViewModel _MainWindowViewModel;

        /// <summary>
        /// 引数なしで生成はさせない
        /// </summary>
        /// <exception cref="NotImplementedException">DIコンテナを利用するように</exception>
        public ControDirectoryTreeViewlViewModel()
        {
            throw new NotImplementedException();
        }

        // 通常コンストラクタ
        public ControDirectoryTreeViewlViewModel(
            IDrivesFileSystemWatcherService drivesFileSystemWatcherService,
            ICheckedDirectoryManager checkedDirectoryManager,
            IExpandedDirectoryManager expandDirectoryManager,
            ISpecialFolderAndRootDrives specialFolderAndRootDrives,
            IWindowsAPI windowsAPI,
            IMainWindowViewModel mainViewModel)
        {
            _DrivesFileSystemWatcherService = drivesFileSystemWatcherService;
            _CheckedDirectoryManager = checkedDirectoryManager;
            _ExpandedDirectoryManager = expandDirectoryManager;
            _SpecialFolderAndRootDrives = specialFolderAndRootDrives;
            _WindowsAPI = windowsAPI;
            _MainWindowViewModel = mainViewModel;

            // カレントディレクトリの変更メッセージ
            WeakReferenceMessenger.Default.Register<CurrentChangeMessage>(this, (_, message) =>
            {
                if (CurrentFullPath == message.CurrentFullPath) return;
                CurrentFullPath = message.CurrentFullPath;
                FolderSelectedChanged(CurrentFullPath);
            });

            // フォントの変更メッセージ
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) => FontSize = message.FontSize);

            foreach (var root in _SpecialFolderAndRootDrives.ScanDrives())
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
        {
            IsCheckBoxVisible = isVisible ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged(nameof(IsCheckBoxVisible));
        }

        /// <summary>
        /// ルートノードにアイテムを追加します。
        /// </summary>
        /// <param name="item">追加する FileItemInformation</param>
        /// <returns>追加されたノード</returns>
        public DirectoryTreeViewModel AddRoot(FileItemInformation item, bool findSpecial)
        {
            var currentNode = new DirectoryTreeViewModel(item);
            TreeRoot.Add(currentNode);
            // 内部キックをしない場合そのまま終了
            if (!findSpecial) return currentNode;

            // 展開ディレクトリに追加
            _ExpandedDirectoryManager.AddDirectory(item.FullPath);
            /* ルートドライブが追加された時、特殊フォルダは追加されている
              * 特殊フォルダがルートドライブに含まれているなら、内部的に Kick して展開しておく
              * そうすることで、特殊フォルダのチェックに対してドライブ下のディレクトリにも反映される
              */
            // ルートドライブではない場合終了
            if (item.FullPath.Length != 3) { return currentNode; }

            // 追加されたノードのフルパスで始まるノードを検索する
            var driveNode = TreeRoot.Where(root => root.FullPath.StartsWith(currentNode.FullPath));
            if (driveNode == null) { return currentNode; }

            foreach (var drive in driveNode)
            {
                // 各追加されたノードで始まるパスを分解する
                var dirs = GetDirectoryNames(drive.FullPath);
                //if (node == null) continue;
                var childNode = currentNode;

                // ドライブルートを除外する
                dirs.RemoveAt(0);
                foreach (var dir in dirs)
                {
                    if (childNode == null) break;

                    // サブディレクトリを内部キックし、サブディレクトリからノードを取得する
                    childNode.KickChild();
                    childNode = childNode.Children.FirstOrDefault(c => c.FullPath == dir);
                }
            }
            return currentNode;
        }

        /// <summary>
        /// ツリーノードのアイテムをクリアする
        /// </summary>
        public void ClearRoot()
        {
            TreeRoot.Clear();
        }
        #endregion 初期処理

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
            foreach (var directory in GetDirectoryNames(changedPath))
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

        /// <summary>
        /// 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得します。
        /// </summary>
        /// <param name="path">コレクションを取得するディレクトリ</param>
        /// <returns>親ディレクトリからのコレクション</returns>
        public static IList<string> GetDirectoryNames(string path)
        {
            var list = new List<string>();
            string? parent = path;
            list.Add(path);
            while ((parent = Path.GetDirectoryName(parent)) != null)
            {
                list.Add(parent);
            }
            list.Reverse();
            return list;
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
            if (!node.IsExpanded) return;

            foreach (var child in node.Children)
            {
                AddDirectoryToExpandedDirectoryManager(child);
            }
        }

        /// <summary>
        /// TreeViewItem が展開された時に展開解除マネージャに通知します。
        /// </summary>
        /// <param name="node">展開解除されたノード</param>
        public void RemoveDirectoryToExpandedDirectoryManager(DirectoryTreeViewModel node)
        {
            _ExpandedDirectoryManager.RemoveDirectory(node.FullPath);
            if (!node.HasChildren) { return; }

            foreach (var child in node.Children)
            {
                RemoveDirectoryToExpandedDirectoryManager(child);
            }
        }
        #endregion 展開マネージャへの追加削除処理

        #region チェックマネージャからチェック状態を反映
        /// <summary>
        /// チェックマネージャの情報に基づき、チェック状態を変更します。
        /// </summary>
        public void CheckStatusChangeFromCheckManager()
        {
            // サブディレクトリを含む管理をしているディレクトリを巡回する
            foreach (var fullPath in _CheckedDirectoryManager.NestedDirectories)
            {
                CheckStatusChange(fullPath, true);
            }
            // サブディレクトリを含まない管理をしているディレクトリを巡回する
            foreach (var fullPath in _CheckedDirectoryManager.NonNestedDirectories)
            {
                CheckStatusChange(fullPath, null);
            }
        }

        /// <summary>
        /// ディレクトリのフルパスから、チェック状態を変更する
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="isChecked"></param>
        /// <returns>成功の可否</returns>
        private bool CheckStatusChange(string fullPath, bool? isChecked)
        {
            // 親ディレクトリから順に、現在のディレクトリまでのコレクションを取得
            var dirs = GetDirectoryNames(fullPath);

            // ドライブノードを取得する
            DirectoryTreeViewModel? node = TreeRoot.FirstOrDefault(r => r.FullPath == dirs[0]);
            if (node == null) return false;
            node.KickChild();
            node.IsExpanded = true;

            if (node.FullPath == fullPath)
            {
                if (node.FullPath == fullPath)
                {
                    if (isChecked == true || isChecked == false)
                    {
                        node.IsChecked = isChecked;
                    }
                    else
                    {
                        node.IsCheckedForSync = null;
                    }
                    return true;
                }
            }

            // リストからドライブノードを除去する
            dirs.RemoveAt(0);
            foreach (var dir in dirs)
            {
                node = node.Children.FirstOrDefault(c => c.FullPath == dir);
                if (node == null) return false;

                node.KickChild();
                if (node.FullPath == fullPath)
                {
                    if (isChecked == true || isChecked == false)
                    {
                        node.IsChecked = isChecked;
                    }
                    else
                    {
                        node.IsCheckedForSync = null;
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion チェックマネージャからチェック状態を反映
    }
}
