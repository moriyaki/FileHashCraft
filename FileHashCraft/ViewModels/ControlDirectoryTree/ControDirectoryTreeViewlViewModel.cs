/*  ControDirectoryTreeViewlViewModel.cs

    ディレクトリツリービューの ViewModel を提供します。
 */
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.FileSystemWatcherServices;
using FileHashCraft.ViewModels.ControlDirectoryTree;

namespace FileHashCraft.ViewModels.DirectoryTreeViewControl
{
    #region インターフェース
    public interface IControDirectoryTreeViewlModel
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
        /// チェックボックスが表示されるか否かを設定します。
        /// </summary>
        public Visibility IsCheckBoxVisible { get; }
        /// <summary>
        /// カレントディレクトリ
        /// </summary>
        public string CurrentFullPath { get; set; }
        /// <summary>
        /// ルートにアイテムを追加します。
        /// </summary>
        public void AddRoot(FileItemInformation item, bool findSpecial);
        /// <summary>
        /// ルートアイテムをクリアします。
        /// </summary>
        public void ClearRoot();
        /// <summary>
        /// ツリービューの横幅設定をします。
        /// </summary>
        public double TreeWidth { get; set; }
    }
    #endregion インターフェース
    public partial class ControDirectoryTreeViewModel : ObservableObject, IControDirectoryTreeViewlModel
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
                    _messageServices.SendCurrentDirectoryChanged(value);
                }
            }
        }
        /// <summary>
        /// ツリー横幅の設定
        /// </summary>
        private double _TreeWidth;
        public double TreeWidth
        {
            get => _TreeWidth;
            set
            {
                if (_TreeWidth == value) { return; }
                SetProperty(ref _TreeWidth, value);
                _messageServices.SendTreeWidth(value);
            }
        }
        /// <summary>
        /// フォントの設定
        /// </summary>
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                if (_CurrentFontFamily.Source == value.Source) { return; }
                SetProperty(ref _CurrentFontFamily, value);
                _messageServices.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        private double _FontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                if (_FontSize == value) { return; }
                SetProperty(ref _FontSize, value);
                _messageServices.SendFontSize(value);
            }
        }
        #endregion バインディング

        #region コンストラクタと初期処理
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly ITreeManager _treeManager;

        /// <summary>
        /// 引数なしで生成はさせない
        /// </summary>
        /// <exception cref="NotImplementedException">DIコンテナを利用するように</exception>
        public ControDirectoryTreeViewModel()
        {
            throw new NotImplementedException();
        }

        // 通常コンストラクタ
        public ControDirectoryTreeViewModel(
            IMessageServices messageServices,
            ISettingsService settingsService,
            IFileWatcherService fileWatcherService,
            ITreeManager treeManager)
        {
            _messageServices = messageServices;
            _settingsService = settingsService;
            _fileWatcherService = fileWatcherService;
            _treeManager = treeManager;

            // カレントディレクトリの変更メッセージ
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChanged>(this, (_, message) =>
            {
                if (CurrentFullPath == message.CurrentFullPath) return;
                CurrentFullPath = message.CurrentFullPath;
                FolderSelectedChanged(CurrentFullPath);
            });

            // ツリービュー幅変更
            WeakReferenceMessenger.Default.Register<TreeWidthChanged>(this, (_, m)
                => TreeWidth = m.TreeWidth);

            // フォントサイズの変更
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m)
                => FontSize = m.FontSize);

            foreach (var root in SpecialFolderAndRootDrives.ScanDrives())
            {
                _fileWatcherService.SetRootDirectoryWatcher(root);
            }

            // ディレクトリの内容が変更された
            WeakReferenceMessenger.Default.Register<DirectoryItemDeleted>(this, async (_, m)
                => await DirectoryChanged(m.DeletedFullPath));

            // ディレクトリの内容が追加された
            WeakReferenceMessenger.Default.Register<DirectoryItemCreated>(this, async (_, m)
                => await DirectoryCreated(m.CreatedFullPath));

            // ディレクトリの名前が変更された
            WeakReferenceMessenger.Default.Register<DirectoryItemRenamed>(this, async(_, m)
                => await DirectoryRenamed(m.OldFullPath, m.NewFullPath));

            // リムーバブルドライブが追加または挿入された
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaInserted>(this, async (_, m)
                => await OpticalDriveMediaInserted(m.InsertedPath));

            // リムーバブルドライブがイジェクトされた
            WeakReferenceMessenger.Default.Register<OpticalDriveMediaEjected>(this, async (_, m)
                => await OpticalDriveMediaEjected(m.EjectedPath));

            _TreeWidth = _settingsService.TreeWidth;
            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
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
            var currentNode = new DirectoryTreeViewModel(item);
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
