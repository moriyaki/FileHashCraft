/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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
    }
    #endregion インターフェース

    public partial class PageSelectTargetViewModel : ObservableObject, IPageSelectTargetViewModel
    {
        #region バインディング
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

        /// <summary>
        /// ワイルドカードの条件を追加します。
        /// </summary>
        public DelegateCommand AddWildcard { get; set; }
        /// <summary>
        /// ワイルドカードの条件を削除します。
        /// </summary>
        public DelegateCommand RemoveWildcard { get; set; }

        /// <summary>
        /// //正規表現の条件を追加します。
        /// </summary>
        public DelegateCommand AddRegularExpression { get; set; }
        /// <summary>
        /// 正規表現の条件を削除します。
        /// </summary>
        public DelegateCommand RemoveRegularExpression { get; set; }
        #endregion コマンド

        #region コンストラクタ
        private readonly IExtentionHelper _ExtentionManager;
        private readonly ISearchManager _SearchManager;
        private readonly IControDirectoryTreeViewlViewModel _ControDirectoryTreeViewlViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        private readonly ISpecialFolderAndRootDrives _SpecialFolderAndRootDrives;
        private readonly IMainWindowViewModel _MainWindowViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetViewModel(
            IExtentionHelper extentionManager,
            ISearchManager searchManager,
            IControDirectoryTreeViewlViewModel directoryTreeViewControlViewModel,
            ICheckedDirectoryManager checkedDirectoryManager,
            ISpecialFolderAndRootDrives specialFolderAndRootDrives,
            IMainWindowViewModel mainWindowViewModel)
        {
            _ExtentionManager = extentionManager;
            _SearchManager = searchManager;
            _ControDirectoryTreeViewlViewModel = directoryTreeViewControlViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
            _SpecialFolderAndRootDrives = specialFolderAndRootDrives;
            _MainWindowViewModel = mainWindowViewModel;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new DelegateCommand(() =>
            {
                IsExecuting = true;
                WeakReferenceMessenger.Default.Send(new ToPageSetting(ReturnPageEnum.PageTargetSelect));
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
                () => CountFilteredGetHash > 0
            );
            // 正規表現の条件を追加するコマンド
            AddWildcard = new DelegateCommand(
                () => MessageBox.Show("ワイルドカードの追加：未実装"));

            // 正規表現の条件を削除するコマンド
            RemoveWildcard = new DelegateCommand(
                () => MessageBox.Show("ワイルドカードの削除：未実装"));

            // 正規表現の条件を追加するコマンド
            AddRegularExpression = new DelegateCommand(
                () => MessageBox.Show("正規表現の追加：未実装"));

            // 正規表現の条件を削除するコマンド
            RemoveRegularExpression = new DelegateCommand(
                () => MessageBox.Show("正規表現の削除：未実装"));

            // 読み取り専用ファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsReadOnlyFileIncludeClicked = new DelegateCommand(() => IsReadOnlyFileInclude = !IsReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsHiddenFileIncludeClicked = new DelegateCommand(() => IsHiddenFileInclude = !IsHiddenFileInclude);

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new DelegateCommand(() => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new DelegateCommand(() => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

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
                _ExtentionManager.Clear();
            });

            var _ScanHashFilesClass = Ioc.Default.GetService<IScanHashFiles>() ?? throw new InvalidOperationException($"{nameof(IScanHashFiles)} dependency not resolved."); CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;

            // 移動ボタンの利用状況を設定
            Status = FileScanStatus.None;
            ToPageHashCalcing.RaiseCanExecuteChanged();

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

            foreach (var root in _CheckedDirectoryManager.NestedDirectories)
            {
                var fi = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            foreach (var root in _CheckedDirectoryManager.NonNestedDirectories)
            {
                var fi = _SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                var node = _ControDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }
        #endregion 初期処理

    }
}
