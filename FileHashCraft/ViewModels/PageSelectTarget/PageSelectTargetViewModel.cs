/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModel
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
        public void ExtentionCountChanged();
        /// <summary>
        /// 拡張子グループのチェックボックス状態が変更されたら、拡張子にも反映します。
        /// </summary>
        public void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention);
        /// <summary>
        /// リストボックスの幅
        /// </summary>
        public double ListWidth { get; set; }
        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムの一覧です。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; }
    }
    #endregion インターフェース

    public partial class PageSelectTargetViewModel : ObservableObject, IPageSelectTargetViewModel
    {
        #region バインディング
        /// <summary>
        /// ファイルスキャン状況
        /// </summary>
        private FileScanStatus _Status = FileScanStatus.None;
        public FileScanStatus Status
        {
            get => _Status;
            set
            {
                _Status = value;
                switch (value)
                {
                    case FileScanStatus.None:
                        StatusColor = Brushes.Pink;
                        break;
                    case FileScanStatus.DirectoriesScanning:
                        StatusColor = Brushes.Pink;
                        StatusMessage = $"{Resources.LabelDirectoryScanning} {CountScannedDirectories}";
                        break;
                    case FileScanStatus.FilesScanning:
                        StatusColor = Brushes.Yellow;
                        StatusMessage = $"{Resources.LabelDirectoryCount} ({CountHashFilesDirectories} / {CountScannedDirectories})";
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
        /// ツリー横幅の設定
        /// </summary>
        public double TreeWidth
        {
            get => _MainWindowViewModel.TreeWidth;
            set
            {
                _MainWindowViewModel.TreeWidth = value;
                OnPropertyChanged(nameof(TreeWidth));
            }
        }
        /// <summary>
        /// リストボックスの幅を設定します
        /// </summary>
        public double ListWidth
        {
            get => _MainWindowViewModel.ListWidth;
            set
            {
                _MainWindowViewModel.ListWidth = value;
                OnPropertyChanged(nameof(ListWidth));
            }
        }
        /// <summary>
        /// ハッシュ計算アルゴリズムの一覧
        /// </summary>
        public ObservableCollection<HashAlgorithm> HashAlgorithms { get; set; } =
        [
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
            new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
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

        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムの一覧です。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; set; } = [];
        #endregion バインディング

        #region コマンド
        /// <summary>
        /// 設定画面を開きます。
        /// </summary>
        public RelayCommand SettingsOpen { get; set; }

        /// <summary>
        /// デバッグウィンドウを開きます。
        /// </summary>
        public RelayCommand DebugOpen { get; set; }

        /// <summary>
        /// エクスプローラー画面に戻ります。
        /// </summary>
        public RelayCommand ToPageExplorer { get; set; }

        /// <summary>
        /// ハッシュ計算画面に移動します。
        /// </summary>
        public RelayCommand ToPageHashCalcing { get; set; }

        /// <summary>
        /// ワイルドカードの条件を追加します。
        /// </summary>
        public RelayCommand AddWildcard { get; set; }
        /// <summary>
        /// ワイルドカードの条件を削除します。
        /// </summary>
        public RelayCommand RemoveWildcard { get; set; }

        /// <summary>
        /// //正規表現の条件を追加します。
        /// </summary>
        public RelayCommand AddRegularExpression { get; set; }
        /// <summary>
        /// 正規表現の条件を削除します。
        /// </summary>
        public RelayCommand RemoveRegularExpression { get; set; }

        /// <summary>
        /// ファイル種類のチェックボックスが選択された時の処理をします
        /// </summary>
        public RelayCommand<object> ExtentionGroupCheckBoxClickedCommand { get; set; }
        /// <summary>
        /// ファイル種類のチェックボックスが選択された時の処理をします
        /// </summary>
        public RelayCommand<object> ExtentionCheckBoxClickedCommand { get; set; }
        #endregion コマンド

        #region コンストラクタ
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly IControDirectoryTreeViewlViewModel _ControDirectoryTreeViewlViewModel;
        private readonly IMainWindowViewModel _MainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetViewModel(
            ICheckedDirectoryManager checkedDirectoryManager,
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            IMainWindowViewModel mainWindowViewModel)
        {
            _ControDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
            _MainWindowViewModel = mainWindowViewModel;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
            {
                IsExecuting = true;
                WeakReferenceMessenger.Default.Send(new ToPageSetting(ReturnPageEnum.PageTargetSelect));
            });
            // デバッグウィンドウを開くコマンド
            DebugOpen = new RelayCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });
            // カレントハッシュ計算アルゴリズムを保存
            SelectedHashAlgorithm = _MainWindowViewModel.HashAlgorithm;

            // エクスプローラー風画面に移動するコマンド
            ToPageExplorer = new RelayCommand(() =>
            {
                CTS?.Cancel();
                WeakReferenceMessenger.Default.Send(new ToPageExplorer());
            });
            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new RelayCommand(
                () => WeakReferenceMessenger.Default.Send(new ToPageHashCalcing()),
                () => CountFilteredGetHash > 0
            );
            // 正規表現の条件を追加するコマンド
            AddWildcard = new RelayCommand(
                () => MessageBox.Show("ワイルドカードの追加：未実装"));

            // 正規表現の条件を削除するコマンド
            RemoveWildcard = new RelayCommand(
                () => MessageBox.Show("ワイルドカードの削除：未実装"));

            // 正規表現の条件を追加するコマンド
            AddRegularExpression = new RelayCommand(
                () => MessageBox.Show("正規表現の追加：未実装"));

            // 正規表現の条件を削除するコマンド
            RemoveRegularExpression = new RelayCommand(
                () => MessageBox.Show("正規表現の削除：未実装"));

            ExtentionGroupCheckBoxClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is ExtentionGroupCheckBoxViewModel checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.InvokeAsync(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });
            ExtentionCheckBoxClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is ExtensionOrTypeCheckBoxBase checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.InvokeAsync(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });
            // 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsReadOnlyFileIncludeClicked = new RelayCommand(() => IsReadOnlyFileInclude = !IsReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsHiddenFileIncludeClicked = new RelayCommand(() => IsHiddenFileInclude = !IsHiddenFileInclude);

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new RelayCommand(() => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new RelayCommand(() => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // メインウィンドウからのツリービュー幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<TreeWidthChanged>(this, (_, message) => TreeWidth = message.TreeWidth);

            // メインウィンドウからのリストボックス幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<ListWidthChanged>(this, (_, message) => ListWidth = message.ListWidth);

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);

            // カレントディレクトリが変更されたメッセージ受診
            WeakReferenceMessenger.Default.Register<CurrentChangeMessage>(this, (_, message) =>
                ChangeCurrentPath(message.CurrentFullPath));
        }

        #endregion コンストラクタ

        #region 初期処理
        public CancellationTokenSource? CTS;
        public CancellationToken cancellationToken;

        private readonly List<string> NestedDirectories = [];
        private readonly List<string> NonNestedDirectories = [];

        /// <summary>
        /// 初期設定をします。
        /// </summary>
        public void Initialize()
        {
            // 言語変更に伴う対策
            LanguageChangedMeasures();

            // 設定画面から戻ってきた場合、処理を終了する
            if (IsExecuting)
            {
                IsExecuting = false;
                return;
            }

            // ツリービューのアイテムを初期化する
            InitializeTreeView();
            try
            {
                ChangeCurrentPath(_ControDirectoryTreeViewlViewModel.TreeRoot[0].FullPath);
                _ControDirectoryTreeViewlViewModel.TreeRoot[0].IsSelected = true;
            }
            catch { }
            // 既にファイル検索がされていて、ディレクトリ選択設定が変わっていなければ終了
            if (Status == FileScanStatus.Finished
             && _CheckedDirectoryManager.NestedDirectories.OrderBy(x => x).SequenceEqual(NestedDirectories.OrderBy(x => x))
             && _CheckedDirectoryManager.NonNestedDirectories.OrderBy(x => x).SequenceEqual(NonNestedDirectories.OrderBy(x => x)))
            {
                return;
            }

            // 現在のディレクトリ選択設定を保存する
            NestedDirectories.Clear();
            NestedDirectories.AddRange(_CheckedDirectoryManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_CheckedDirectoryManager.NonNestedDirectories);

            // 状況が変わっているので、必要な値の初期化をする
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                CountScannedDirectories = 0;
                CountHashFilesDirectories = 0;
                CountAllTargetFilesGetHash = 0;
                CountFilteredGetHash = 0;
                ExtentionCollection.Clear();
                ExtentionsGroupCollection.Clear();
            });

            var _ScanHashFilesClass = Ioc.Default.GetService<IScanHashFiles>() ?? throw new InvalidOperationException($"{nameof(IScanHashFiles)} dependency not resolved."); CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;

            // 移動ボタンの利用状況を設定
            Status = FileScanStatus.None;
            ToPageHashCalcing.NotifyCanExecuteChanged();

            // スキャンするディレクトリの追加
            _ScanHashFilesClass?.ScanFiles(cancellationToken);
        }

        /// <summary>
        /// 表示言語の変更に伴う対策をします。
        /// </summary>
        private void LanguageChangedMeasures()
        {
            var currentAlgorithm = _MainWindowViewModel.HashAlgorithm;

            HashAlgorithms.Clear();
            HashAlgorithms =
            [
                new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA256), Resources.HashAlgorithm_SHA256),
                new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA384), Resources.HashAlgorithm_SHA384),
                new(HashAlgorithmHelper.GetAlgorithmName(FileHashAlgorithm.SHA512), Resources.HashAlgorithm_SHA512),
            ];

            // ハッシュ計算アルゴリズムを再設定
            SelectedHashAlgorithm = currentAlgorithm;
            OnPropertyChanged(nameof(HashAlgorithms));
            Status = _Status;

            // 言語が変わった場合に備えて、拡張子グループを再設定
            App.Current?.Dispatcher?.InvokeAsync(() => {
                ExtentionsGroupCollection.Clear();
                AddFileTypes();
            });
        }

        /// <summary>
        /// ツリービューの初期化をします。
        /// </summary>
        private void InitializeTreeView()
        {
            _ControDirectoryTreeViewlViewModel.ClearRoot();
            _ControDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            var specialFolderAndRootDrives = Ioc.Default.GetService<ISpecialFolderAndRootDrives>() ?? throw new NullReferenceException(nameof(ISpecialFolderAndRootDrives));

            foreach (var root in _CheckedDirectoryManager.NestedDirectories)
            {
                var fi = specialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            foreach (var root in _CheckedDirectoryManager.NonNestedDirectories)
            {
                var fi = specialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                var node = _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }
        #endregion 初期処理

        private void ChangeCurrentPath(string currentFullPath)
        {
            HashFileListItems.Clear();
            var fileManager = Ioc.Default.GetService<IFileManager>() ?? throw new NullReferenceException(nameof(IFileManager));
            foreach (var file in fileManager.EnumerateFiles(currentFullPath))
            {
                var item = new HashListFileItems
                {
                    FullPathFileName = file,
                    IsHashTarget = false
                };
                HashFileListItems.Add(item);
            }
        }
    }
}
