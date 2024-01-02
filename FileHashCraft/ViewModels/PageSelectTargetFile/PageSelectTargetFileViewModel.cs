using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.PageSelectTargetFile;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IPageSelectTargetFileViewModel
    {
        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize();
    }
    #endregion インターフェース

    #region ハッシュ計算するファイルの取得状況
    public enum FileScanStatus
    {
        DirectoryScanning,
        FileScanning,
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
                switch (value)
                {
                    case FileScanStatus.DirectoryScanning:
                        StatusColor = Brushes.Red;
                        break;
                    case FileScanStatus.FileScanning:
                        StatusColor = Brushes.Yellow;
                        break;
                    case FileScanStatus.Finished:
                        StatusColor = Brushes.LightGreen;
                        StatusMessage = "完了";
                        break;
                    default: // 異常
                        StatusColor = Brushes.Gray;
                        break;
                }
                SetProperty(ref _Status, value);
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
        public int ScannedDirectoriesCount
        {
            get => _ScannedDirectoriesCount;
            set
            {
                SetProperty(ref _ScannedDirectoriesCount, value);
                StatusColor = Brushes.Red;
                StatusMessage = $"ディレクトリスキャン中 : {value} 個のディレクトリ発見";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// ハッシュを取得するファイルの、スキャンしたディレクトリ数
        /// </summary>
        private int _CountDirectoryScanned = 0;
        public int CountDirectoryScanned
        {
            get => _CountDirectoryScanned;
            set
            {
                SetProperty(ref _CountDirectoryScanned, value);
                OnPropertyChanged(nameof(CountAllFilesGetHash));
                StatusColor = Brushes.Yellow;
                StatusMessage = $"ファイルスキャン中 ({value} / {ScannedDirectoriesCount})";
            }
        }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        private int _CountAllFilesGetHash = 0;
        public int CountAllFilesGetHash
        {
            get => _CountAllFilesGetHash;
            set
            {
                SetProperty(ref _CountAllFilesGetHash, value);
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
                new("SHA-256", Resources.HashAlgorithm_SHA256),
                new("SHA-384", Resources.HashAlgorithm_SHA384),
                new("SHA-512", Resources.HashAlgorithm_SHA512),
            ];

        /// <summary>
        /// ハッシュ計算アルゴリズムの取得と設定
        /// </summary>
        public string SelectedHashAlgorithm
        {
            get => _MainWindowViewModel.HashAlgorithm;
            set
            {
                if (_MainWindowViewModel.HashAlgorithm == value) return;
                _MainWindowViewModel.HashAlgorithm = value;
                OnPropertyChanged(nameof(SelectedHashAlgorithm));
            }
        }

        /// <summary>
        ///  0 サイズのファイルを削除するかどうか
        /// </summary>
        public bool IsZeroSizeFileDelete
        {
            get => _MainWindowViewModel.IsZeroSizeFileDelete;
            set
            {
                if (_MainWindowViewModel.IsZeroSizeFileDelete == value) return;
                _MainWindowViewModel.IsZeroSizeFileDelete = value;
                OnPropertyChanged(nameof(IsZeroSizeFileDelete));
            }
        }

        /// <summary>
        /// 空のフォルダを削除するかどうか
        /// </summary>
        public bool IsEmptyDirectoryDelete
        {
            get => _MainWindowViewModel.IsEmptyDirectoryDelete;
            set
            {
                if (_MainWindowViewModel.IsEmptyDirectoryDelete == value) return;
                _MainWindowViewModel.IsEmptyDirectoryDelete = value;
                OnPropertyChanged(nameof(IsEmptyDirectoryDelete));
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
        private readonly IControDirectoryTreeViewlViewModel _ControDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly IMainWindowViewModel _MainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetFileViewModel() { throw new NotImplementedException(); }

        public PageSelectTargetFileViewModel(
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            ICheckedDirectoryManager checkedDirectoryManager,
            IMainWindowViewModel mainWindowViewModel)
        {
            _ControDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
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

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new DelegateCommand(() => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new DelegateCommand(() => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // カレントハッシュ計算アルゴリズムを保存
            SelectedHashAlgorithm = _MainWindowViewModel.HashAlgorithm;

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

            // メインウィンドウからのハッシュ計算アルゴリズム変更メッセージ受信
            WeakReferenceMessenger.Default.Register<HashAlgorithm>(this, (_, message) =>
                SelectedHashAlgorithm = message.Algorithm);

            // ステータスの変更メッセージ受信
            WeakReferenceMessenger.Default.Register<HashScanStatusChanged>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() => Status = message.Status));

            // ハッシュ計算対象ファイルが増えた時のメッセージ
            WeakReferenceMessenger.Default.Register<HashAllFilesAdded>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountAllFilesGetHash += message.HashFileCount));

            // ハッシュを取得するファイルの、スキャンしたディレクトリ数が増えた時のメッセージ
            WeakReferenceMessenger.Default.Register<HashADirectoryScannedAdded>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() =>
                    CountDirectoryScanned += message.ScannedDirectoryCount));

            WeakReferenceMessenger.Default.Register<HashScanDirectoriesAdded>(this, (_, message) =>
                App.Current?.Dispatcher?.Invoke(() => ScannedDirectoriesCount += message.AddScannedDirectories));
        }

        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize()
        {
            HashAlgorithms.Clear();
            HashAlgorithms =
                [
                    new("SHA-256", Resources.HashAlgorithm_SHA256),
                    new("SHA-384", Resources.HashAlgorithm_SHA384),
                    new("SHA-512", Resources.HashAlgorithm_SHA512),
                ];
            OnPropertyChanged(nameof(HashAlgorithms));

            // ハッシュ計算アルゴリズムを再設定
            SelectedHashAlgorithm = _MainWindowViewModel.HashAlgorithm;
            OnPropertyChanged(nameof(SelectedHashAlgorithm));

            if (IsExecuting)
            {
                IsExecuting = false;
                return;
            }

            // 値の初期化
            ScannedDirectoriesCount = 0;
            CountDirectoryScanned = 0;
            CountAllFilesGetHash = 0;
            CountAlreadyGetHash = 0;
            CountRequireGetHash = 0;
            CountFilteredGetHash = 0;

            // ツリービューのアイテムをクリアする
            _ControDirectoryTreeViewlViewModel.ClearRoot();
            _ControDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            // スキャンするディレクトリの追加
            var scanHashFilesClass = Ioc.Default.GetService<IScanHashFilesClass>();
            scanHashFilesClass?.ScanHashFiles();
        }
        #endregion コンストラクタと初期処理
    }
}
