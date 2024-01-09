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
        public void Initialize();
    }
    #endregion インターフェース

    #region ハッシュ計算するファイルの取得状況
    public enum FileScanStatus
    {
        DirectoriesScanning,
        FilesScanning,
        XMLWriting,
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
                    case FileScanStatus.XMLWriting:
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
        /// ハッシュスキャン対象のファイルスキャン完了ディレクトリ数
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
        /// ハッシュ取得が必要なファイル数
        /// </summary>
        private int _CountRequireGetHash = 0;
        public int CountRequireGetHash
        {
            get => _CountRequireGetHash;
            set => SetProperty(ref _CountRequireGetHash, value);
        }

        /// <summary>
        /// フィルタ済みのハッシュ取得が必要なファイル数
        /// </summary>
        private int _CountFilteredGetHash = 0;
        public int CountFilteredGetHash
        {
            get => _CountFilteredGetHash;
            set => SetProperty(ref _CountFilteredGetHash, value);
        }

        /// <summary>
        /// 拡張子検索フィルタ用コンボボックス
        /// </summary>
        public ObservableCollection<ExtentionCheckBoxViewModel> ExtentionCollection { get; set; } = [];

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
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(HashAlgorithmHelper.GetHashAlgorithmName(Models.FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
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
        private string _FilterTextBox = "aaa";
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
        /// 設定画面を開く
        /// </summary>
        public DelegateCommand SettingsOpen { get; set; }

        /// <summary>
        /// デバッグウィンドウを開く
        /// </summary>
        public DelegateCommand DebugOpen { get; set; }

        /// <summary>
        /// エクスプローラー画面に戻る
        /// </summary>
        public DelegateCommand ToPageExplorer { get; set; }

        /// <summary>
        /// ハッシュ計算画面に移動する
        /// </summary>
        public DelegateCommand ToPageHashCalcing { get; set; }
        #endregion コマンド

        #region コンストラクタと初期処理
        private readonly IControDirectoryTreeViewlViewModel _controDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        private readonly ISpecialFolderAndRootDrives _specialFolderAndRootDrives;
        private readonly IMainWindowViewModel _mainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetFileViewModel() { throw new NotImplementedException(); }

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
            ToPageHashCalcing = new DelegateCommand(() =>
                WeakReferenceMessenger.Default.Send(new ToPageHashCalcing()));

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);

            // ステータスの変更メッセージ受信
            WeakReferenceMessenger.Default.Register<HashScanStatusChanged>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() => Status = message.Status));

            // 全ディレクトリ数の変更メッセージ受信
            WeakReferenceMessenger.Default.Register<AddHashScanDirectories>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountScannedDirectories += message.AddScannedDirectories));

            // ハッシュスキャン対象のファイルスキャン完了ディレクトリ数増加メッセージ
            WeakReferenceMessenger.Default.Register<AddFilesHashScanDirectories>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountHashFilesDirectories += message.HashDirectoriesCount));

            // ハッシュスキャン対象のファイル数追加メッセージ
            WeakReferenceMessenger.Default.Register<AddAllTargetFilesGetHash>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountAllTargetFilesGetHash += message.HashFileCount));

            // ハッシュを既に取得しているファイル数増加メッセージ
            WeakReferenceMessenger.Default.Register<AddAlreadyGetHash>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountAlreadyGetHash += message.AlreadyGetHashCount));

            // ハッシュ取得が必要なファイル数増加メッセージ
            WeakReferenceMessenger.Default.Register<AddRequireGetHash>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountRequireGetHash += message.RequireHashCount));

            // ハッシュフィルタに利用する拡張子追加メッセージ
            WeakReferenceMessenger.Default.Register<AddExtentions>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    ExtentionCollection.Add(new ExtentionCheckBoxViewModel(
                        message.Extention, message.HashAlgorithm))));

            // ハッシュフィルタに利用する拡張子全削除メッセージ
            WeakReferenceMessenger.Default.Register<ClearExtentions>(this, (_, _) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    ExtentionCollection.Clear()));

            // 拡張子フィルタによるファイル数増加メッセージ
            WeakReferenceMessenger.Default.Register<AddExtentionCount>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountFilteredGetHash += message.ExtentionCount));

            // 拡張子フィルタによるファイル数減少メッセージ
            WeakReferenceMessenger.Default.Register<RemoveExtentionCount>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountFilteredGetHash -= message.ExtentionCount));
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
    }
}
