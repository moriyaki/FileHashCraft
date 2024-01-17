using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.Modules;
using FileHashCraft.ViewModels.PageSelectTargetFile;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IPageSelectTargetFileViewModel
    {
        /// <summary>
        /// 他ページから移動してきた時の初期化処理をします。
        /// </summary>
        public void Initialize();
        /// <summary>
        /// ハッシュアルゴリズム
        /// </summary>
        public string SelectedHashAlgorithm { get; set; }
        /// <summary>
        /// 検索ステータスを変更します。
        /// </summary>
        public void ChangeHashScanStatus(FileScanStatus status);
        /// <summary>
        /// 全ディレクトリ数に加算します。
        /// </summary>
        public void AddScannedDirectoriesCount(int count = 1);
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        public void AddFilesScannedDirectoriesCount(int count = 1);
        /// <summary>
        /// 総対象ファイル数に加算します。
        /// </summary>
        public void AddAllTargetFiles(int allTargetFiles);
        /// <summary>
        /// ファイルの種類をリストボックスに追加します
        /// </summary>
        public void AddFileTypes();
        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        public void AddExtentions(string extention);
        /// <summary>
        /// 拡張子のリストをクリアします。
        /// </summary>
        public void ClearExtentions();
        /// <summary>
        /// 拡張子毎のファイル数が変更された時の処理をします。
        /// </summary>
        public void ChangeExtentionCount(int extentionCount);
        /// <summary>
        /// 拡張子グループのチェックボックス状態が変更されたら、拡張子にも反映します。
        /// </summary>
        public void ChangeCheckBoxGroupChanged(bool changedCheck, List<string> extentionList);
    }
    #endregion インターフェース

    #region ハッシュ計算するファイルの取得状況
    public enum FileScanStatus
    {
        DirectoriesScanning,
        FilesScanning,
        DataWriting,
        Finished,
    }
    #endregion ハッシュ計算するファイルの取得状況

    public class PageSelectTargetFileViewModel : ObservableObject, IPageSelectTargetFileViewModel
    {
        #region バインディング
        /// <summary>
        /// ファイルスキャン状況
        /// </summary>
        private FileScanStatus _Status = FileScanStatus.Finished;
        public FileScanStatus Status
        {
            get => _Status;
            set
            {
                _Status = value;
                switch (value)
                {
                    case FileScanStatus.DirectoriesScanning:
                        StatusColor = Brushes.Pink;
                        StatusMessage = $"{Resources.LabelDirectoryScanning} {CountScannedDirectories}";
                        break;
                    case FileScanStatus.FilesScanning:
                        StatusColor = Brushes.Yellow;
                        StatusMessage = $"{Resources.LabelDirectoryCount} ({CountHashFilesDirectories} / {CountScannedDirectories})";
                        break;
                    case FileScanStatus.DataWriting:
                        StatusMessage = Resources.LabelSettings;
                        StatusColor = Brushes.Cyan;
                        break;
                    case FileScanStatus.Finished:
                        StatusColor = Brushes.LightGreen;
                        StatusMessage = Resources.LabelFinished;
                        break;
                    default: // 異常
                        StatusColor = Brushes.Red;
                        break;
                }
            }
        }

        /// <summary>
        /// ファイルスキャン状況に合わせた背景色
        /// </summary>
        private Brush _StatusColor = Brushes.LightGreen;
        public Brush StatusColor
        {
            get => _StatusColor;
            set => SetProperty(ref _StatusColor, value);
        }

        /// <summary>
        /// ファイルスキャン状況に合わせた文字列
        /// </summary>
        private string _StatusMessage = string.Empty;
        public string StatusMessage
        {
            get => _StatusMessage;
            set => SetProperty(ref _StatusMessage, value);
        }

        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountScannedDirectories = 0;
        public int CountScannedDirectories
        {
            get => _CountScannedDirectories;
            set
            {
                SetProperty(ref _CountScannedDirectories, value);
                Status = FileScanStatus.DirectoriesScanning;
            }
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountHashFilesDirectories = 0;
        public int CountHashFilesDirectories
        {
            get => _CountHashFilesDirectories;
            set
            {
                SetProperty(ref _CountHashFilesDirectories, value);
                Status = FileScanStatus.FilesScanning;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        private int _CountAllTargetFilesGetHash = 0;
        public int CountAllTargetFilesGetHash
        {
            get => _CountAllTargetFilesGetHash;
            set
            {
                SetProperty(ref _CountAllTargetFilesGetHash, value);
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        private int _CountFilteredGetHash = 0;
        public int CountFilteredGetHash
        {
            get => _CountFilteredGetHash;
            set => SetProperty(ref _CountFilteredGetHash, value);
        }

        /// <summary>
        /// ファイルの種類による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; } = [];

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtensionOrTypeCheckBoxBase> ExtentionCollection { get; set; } = [];

        /// <summary>
        /// フォントの設定
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
        /// フォントサイズの設定
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
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } =
        [
            new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
            new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
            new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
        ];
        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        public string SelectedHashAlgorithm
        {
            get => _MainWindowViewModel.HashAlgorithm;
            set
            {
                _MainWindowViewModel.HashAlgorithm = value;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
            }
        }
        /// <summary>
        /// フィルタするファイル
        /// </summary>
        private string _FilterTextBox = string.Empty;
        public string FilterTextBox
        {
            get => _FilterTextBox;
            set
            {
                SetProperty(ref _FilterTextBox, value);
            }
        }
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 設定画面を開きます。
        /// </summary>
        public DelegateCommand SettingsOpen { get; set; }

        /// <summary>
        /// デバッグウィンドウを開きます。
        /// </summary>
        public DelegateCommand DebugOpen { get; set; }

        /// <summary>
        /// エクスプローラー画面に戻ります。
        /// </summary>
        public DelegateCommand ToPageExplorer { get; set; }

        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        public DelegateCommand ToPageHashCalcing { get; set; }
        #endregion コマンド

        #region コンストラクタと初期処理
        private readonly IControDirectoryTreeViewlViewModel _ControDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly ISpecialFolderAndRootDrives _SpecialFolderAndRootDrives;
        private IScanHashFilesClass? _ScanHashFilesClass;
        private readonly IMainWindowViewModel _MainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetFileViewModel(
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            ICheckedDirectoryManager checkedDirectoryManager,
            ISpecialFolderAndRootDrives specialFolderAndRootDrives,
            IMainWindowViewModel mainWindowViewModel)
        {
            _ControDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
            _SpecialFolderAndRootDrives = specialFolderAndRootDrives;
            _MainWindowViewModel = mainWindowViewModel;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new DelegateCommand(() =>
            {
                IsExecuting = true;
                WeakReferenceMessenger.Default.Send(new ToPageSetting(ReturnPageEnum.PageTargetFileSelect));
            });
            // デバッグウィンドウを開くコマンド
            DebugOpen = new DelegateCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });

            // カレントハッシュ計算アルゴリズムを保存
            SelectedHashAlgorithm = _MainWindowViewModel.HashAlgorithm;

            // エクスプローラー風画面に移動するコマンド
            ToPageExplorer = new DelegateCommand(() =>
            {
                CTS?.Cancel();
                WeakReferenceMessenger.Default.Send(new ToPageExplorer());
            });
            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new DelegateCommand(
                () => WeakReferenceMessenger.Default.Send(new ToPageHashCalcing()),
                () => false // チェックボックスにチェックが付いていたらtrueにする所
            );

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);
        }

        public CancellationTokenSource? CTS;
        public CancellationToken cancellationToken;

        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize()
        {
            var currentAlgorithm = _MainWindowViewModel.HashAlgorithm;

            HashAlgorithms.Clear();
            HashAlgorithms =
            [
                new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(HashAlgorithmHelper.GetHashAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

            // ハッシュ計算アルゴリズムを再設定
            SelectedHashAlgorithm = currentAlgorithm;
            OnPropertyChanged(nameof(HashAlgorithms));
            Status = _Status;

            if (IsExecuting)
            {
                IsExecuting = false;
                return;
            }

            // 値の初期化
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                CountScannedDirectories = 0;
                CountHashFilesDirectories = 0;
                CountAllTargetFilesGetHash = 0;
                CountFilteredGetHash = 0;
                ExtentionsGroupCollection.Clear();
                ExtentionCollection.Clear();
            });
            // ツリービューのアイテムを初期化する
            _ControDirectoryTreeViewlViewModel.ClearRoot();
            _ControDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            foreach (var root in _CheckedDirectoryManager.NestedDirectories)
            {
                var fi = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            foreach (var root in _CheckedDirectoryManager.NonNestedDirectories)
            {
                var fi = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                var node = _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
                node.HasChildren = false;
                node.Children.Clear();
            }

            // スキャンするディレクトリの追加
            _ScanHashFilesClass = Ioc.Default.GetService<IScanHashFilesClass>();
            CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;
            _ScanHashFilesClass?.ScanHashFiles(HashAlgorithmHelper.GetHashAlgorithmType(SelectedHashAlgorithm), cancellationToken);
        }
        #endregion コンストラクタと初期処理

        #region ファイル数の管理処理
        /// <summary>
        /// 検索のステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public void ChangeHashScanStatus(FileScanStatus status)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => Status = status);
        }
        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountScannedDirectories += directoriesCount);
        }
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddFilesScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountHashFilesDirectories += directoriesCount);
        }
        /// <summary>
        /// ハッシュ取得対象となる全てのファイル数を設定します。
        /// </summary>
        public void AddAllTargetFiles(int targetFilesCount)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountAllTargetFilesGetHash += targetFilesCount);
        }
        #endregion ファイル数の管理処理

        #region ファイル絞り込みの処理
        /// <summary>
        /// ファイルの種類をリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(FileGroupType.Sounds);
            var documents = new ExtentionGroupCheckBoxViewModel(FileGroupType.Documents);
            var applications = new ExtentionGroupCheckBoxViewModel(FileGroupType.Applications);
            var archives = new ExtentionGroupCheckBoxViewModel(FileGroupType.Archives);
            var sources = new ExtentionGroupCheckBoxViewModel(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel();

            App.Current?.Dispatcher.Invoke(() =>
            {
                ExtentionsGroupCollection.Add(movies);
                ExtentionsGroupCollection.Add(pictures);
                ExtentionsGroupCollection.Add(musics);
                ExtentionsGroupCollection.Add(documents);
                ExtentionsGroupCollection.Add(applications);
                ExtentionsGroupCollection.Add(archives);
                ExtentionsGroupCollection.Add(sources);
                ExtentionsGroupCollection.Add(registrations);
                ExtentionsGroupCollection.Add(others);
            });
        }

        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
         public void AddExtentions(string extention)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                if (FileExtentionManager.Instance.GetExtentionsCount(extention) > 0)
                {
                    var item = new ExtensionCheckBox(extention);
                    ExtentionCollection.Add(item);
                }
            });
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => ExtentionCollection.Clear());
        }
        /// <summary>
        /// 拡張子チェックボックスにより、スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        /// <param name="extentionCount"></param>
        public void ChangeExtentionCount(int extentionCount)
        {
            App.Current?.Dispatcher?.Invoke(() => CountFilteredGetHash += extentionCount);
        }
        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックする
        /// </summary>
        /// <param name="changedCheck">チェックされたか外されたか</param>
        /// <param name="extentionList">拡張子のリストコレクション</param>
        public void ChangeCheckBoxGroupChanged(bool changedCheck, List<string> extentionList)
        {
            foreach (var extension in ExtentionCollection)
            {
                var foundItem = extentionList.Find(e => e == extension.ExtentionOrGroup);
                if (foundItem != null)
                {
                    extension.IsChecked = changedCheck;
                }
            }
        }
        #endregion ファイル絞り込みの処理
    }
}
