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
        public void AllTargetFiles();
        /// <summary>
        /// 既にハッシュ獲得しているファイル数に加算します。
        /// </summary>
        public void AlreadyGetHashCount();
        /// <summary>
        /// まだハッシュ獲得していないファイル数に加算します。
        /// </summary>
        public void RequireGetHashCount();
        /// <summary>
        /// ファイルの種類をリストボックスに追加します
        /// </summary>
        public void AddFileTypes(FileHashAlgorithm fileHashAlgorithm);
        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        public void AddExtentions(string extention, FileHashAlgorithm fileHashAlgorithm);
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
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        public bool IsZeroSizeFileDelete
        {
            get => _mainWindowViewModel.IsZeroSizeFileDelete;
            set
            {
                if (_mainWindowViewModel.IsZeroSizeFileDelete == value) return;
                _mainWindowViewModel.IsZeroSizeFileDelete = value;
                OnPropertyChanged(nameof(IsZeroSizeFileDelete));
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        public bool IsEmptyDirectoryDelete
        {
            get => _mainWindowViewModel.IsEmptyDirectoryDelete;
            set
            {
                if (_mainWindowViewModel.IsEmptyDirectoryDelete == value) return;
                _mainWindowViewModel.IsEmptyDirectoryDelete = value;
                OnPropertyChanged(nameof(IsEmptyDirectoryDelete));
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
        /// 全ディレクトリ数
        /// </summary>
        private int _ScannedDirectoriesCount = 0;
        public int CountScannedDirectories
        {
            get => _ScannedDirectoriesCount;
            set
            {
                SetProperty(ref _ScannedDirectoriesCount, value);
                Status = FileScanStatus.DirectoriesScanning;
            }
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数
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
        /// ハッシュを既に取得しているファイル数
        /// </summary>
        private int _CountAlreadyGetHash = 0;
        public int CountAlreadyGetHash
        {
            get => _CountAlreadyGetHash;
            set => SetProperty(ref _CountAlreadyGetHash, value);
        }

        /// <summary>
        /// まだハッシュを獲得していないファイル数
        /// </summary>
        private int _CountRequireGetHash = 0;
        public int CountRequireGetHash
        {
            get => _CountRequireGetHash;
            set => SetProperty(ref _CountRequireGetHash, value);
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
        private readonly ObservableCollection<ExtentionGroupCheckBoxViewModel> _ExtentionsGroupCollection = [];
        public ReadOnlyObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection
        {
            get => new(_ExtentionsGroupCollection);
        }

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        private readonly ObservableCollection<ExtensionOrTypeCheckBoxBase> _ExtentionCollection = [];
        public ReadOnlyObservableCollection<ExtensionOrTypeCheckBoxBase> ExtentionCollection
        {
            get => new(_ExtentionCollection);
        }

        /// <summary>
        /// フォントの設定
        /// </summary>
        public FontFamily UsingFont
        {
            get => _mainWindowViewModel.UsingFont;
            set
            {
                _mainWindowViewModel.UsingFont = value;
                OnPropertyChanged(nameof(UsingFont));
            }
        }

        /// <summary>
        /// フォントサイズの設定
        /// </summary>
        public double FontSize
        {
            get => _mainWindowViewModel.FontSize;
            set
            {
                _mainWindowViewModel.FontSize = value;
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
            get => _mainWindowViewModel.HashAlgorithm;
            set
            {
                _mainWindowViewModel.HashAlgorithm = value;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
                CountAlreadyGetHash = FileHashInfoManager.FileHashInstance.GetHashAlgorithmsAllCount(HashAlgorithmHelper.GetHashAlgorithmType(value));
                CountRequireGetHash = CountAllTargetFilesGetHash - CountAlreadyGetHash;
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
        ///  0 サイズのファイルを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsZeroSizeFIleDeleteClicked { get; set; }
        /// <summary>
        /// 空のフォルダを削除するかどうかのテキストがクリックされた時のコマンド
        /// </summary>
        public DelegateCommand IsEmptyDirectoryDeleteClicked { get; set; }
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
        private readonly IControDirectoryTreeViewlViewModel _controDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        private readonly ISpecialFolderAndRootDrives _specialFolderAndRootDrives;
        private readonly IMainWindowViewModel _mainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetFileViewModel(
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            ICheckedDirectoryManager checkedDirectoryManager,
            ISpecialFolderAndRootDrives specialFolderAndRootDrives,
            IMainWindowViewModel mainWindowViewModel)
        {
            _controDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _checkedDirectoryManager = checkedDirectoryManager;
            _specialFolderAndRootDrives = specialFolderAndRootDrives;
            _mainWindowViewModel = mainWindowViewModel;

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

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new DelegateCommand(() => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new DelegateCommand(() => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // カレントハッシュ計算アルゴリズムを保存
            SelectedHashAlgorithm = _mainWindowViewModel.HashAlgorithm;

            // エクスプローラー風画面に移動するコマンド
            ToPageExplorer = new DelegateCommand(() =>
                WeakReferenceMessenger.Default.Send(new ToPageExplorer()));

            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new DelegateCommand(
                () => WeakReferenceMessenger.Default.Send(new ToPageHashCalcing()),
                () => CountRequireGetHash > 0
            );

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);
        }

        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize()
        {
            var currentAlgorithm = _mainWindowViewModel.HashAlgorithm;

            HashAlgorithms.Clear();
            HashAlgorithms =
            [
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
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
            CountScannedDirectories = 0;
            CountHashFilesDirectories = 0;
            CountAllTargetFilesGetHash = 0;
            CountAlreadyGetHash = 0;
            CountRequireGetHash = 0;
            CountFilteredGetHash = 0;

            // ツリービューのアイテムを初期化する
            _controDirectoryTreeViewlViewModel.ClearRoot();
            _controDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            foreach (var root in _checkedDirectoryManager.NestedDirectories)
            {
                var fi = _specialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            foreach (var root in _checkedDirectoryManager.NonNestedDirectories)
            {
                var fi = _specialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                var node = _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
                node.HasChildren = false;
                node.Children.Clear();
            }

            // スキャンするディレクトリの追加
            var scanHashFilesClass = Ioc.Default.GetService<IScanHashFilesClass>();
            scanHashFilesClass?.ScanHashFiles(HashAlgorithmHelper.GetHashAlgorithmType(SelectedHashAlgorithm));
        }
        #endregion コンストラクタと初期処理

        #region ファイル数の管理処理
        /// <summary>
        /// 検索のステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public void ChangeHashScanStatus(FileScanStatus status)
        {
            App.Current?.Dispatcher?.Invoke(() => Status = status);
        }
        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.Invoke(() => CountScannedDirectories += directoriesCount);
        }
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddFilesScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.Invoke(() => CountHashFilesDirectories += directoriesCount);
        }
        /// <summary>
        /// ハッシュ取得対象となる全てのファイル数を設定します。
        /// </summary>
        public void AllTargetFiles()
        {
            App.Current?.Dispatcher?.Invoke(() => CountAllTargetFilesGetHash = FileHashInfoManager.FileHashInstance.AllCount);
        }
        /// <summary>
        /// 既にハッシュを獲得しているファイル数に加算します。
        /// </summary>
        public void AlreadyGetHashCount()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                CountAlreadyGetHash = HashAlgorithmHelper.GetHashAlgorithmType(SelectedHashAlgorithm) switch
                {
                    FileHashAlgorithm.SHA256 => FileHashInfoManager.FileHashInstance.StatusSHA256.Count,
                    FileHashAlgorithm.SHA384 => FileHashInfoManager.FileHashInstance.StatusSHA384.Count,
                    FileHashAlgorithm.SHA512 => FileHashInfoManager.FileHashInstance.StatusSHA512.Count,
                    _ => throw new NotImplementedException(),
                };
            });
        }
        /// <summary>
        /// ハッシュの獲得が必要なファイル数に加算します。
        /// </summary>
        public void RequireGetHashCount()
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                CountRequireGetHash = HashAlgorithmHelper.GetHashAlgorithmType(SelectedHashAlgorithm) switch
                {
                    FileHashAlgorithm.SHA256 => FileHashInfoManager.FileHashInstance.AllCount - FileHashInfoManager.FileHashInstance.StatusSHA256.Count,
                    FileHashAlgorithm.SHA384 => FileHashInfoManager.FileHashInstance.AllCount - FileHashInfoManager.FileHashInstance.StatusSHA384.Count,
                    FileHashAlgorithm.SHA512 => FileHashInfoManager.FileHashInstance.AllCount - FileHashInfoManager.FileHashInstance.StatusSHA512.Count,
                    _ => throw new NotImplementedException(),
                };
            });
        }
        #endregion ファイル数の管理処理

        #region ファイル絞り込みの処理
        /// <summary>
        /// ファイルの種類をリストボックスに追加します。
        /// </summary>
        /// <param name="fileHashAlgorithm">利用するハッシュアルゴリズム</param>
        public void AddFileTypes(FileHashAlgorithm fileHashAlgorithm)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                var movies = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Movies);
                _ExtentionsGroupCollection.Add(movies);
                var pictures = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Pictures);
                _ExtentionsGroupCollection.Add(pictures);
                var musics = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Sounds);
                _ExtentionsGroupCollection.Add(musics);
                var documents = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Documents);
                _ExtentionsGroupCollection.Add(documents);
                var applications = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Applications);
                _ExtentionsGroupCollection.Add(applications);
                var archives = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.Archives);
                _ExtentionsGroupCollection.Add(archives);
                var sources = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm, FileGroupType.SourceCodes);
                _ExtentionsGroupCollection.Add(sources);
                var others = new ExtentionGroupCheckBoxViewModel(fileHashAlgorithm);
                _ExtentionsGroupCollection.Add(others);
            });
        }

        /// <summary>
        /// 拡張子をリストボックス追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <param name="fileHashAlgorithm">利用するハッシュアルゴリズム</param>
        public void AddExtentions(string extention, FileHashAlgorithm fileHashAlgorithm)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                if (FileHashInfoManager.FileHashInstance.GetExtentionsCount(extention, fileHashAlgorithm) > 0)
                {
                    var item = new ExtensionCheckBox(extention, fileHashAlgorithm);
                    _ExtentionCollection.Add(item);
                }
            });
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => _ExtentionCollection.Clear());
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
