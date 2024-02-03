/*  PageSelectTargetViewModel.cs

    ハッシュを取得する検索条件ウィンドウの ViewModel を提供します。
    PartialNormal, PartialWildcard, PartialRegularExpression, PartialExpert に分割されています。
 */
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

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
        /// ファイル拡張子グループをリストボックスに追加します
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
        /// <summary>
        /// 拡張子チェックボックスにチェックされたので拡張子グループに反映します。
        /// </summary>
        public void CheckExtentionReflectToGroup(string extention);
        /// <summary>
        /// 拡張子チェックボックスがチェック解除されたので拡張子グループに反映します。
        /// </summary>
        public void UncheckExtentionReflectToGroup(string extention);
        /// <summary>
        /// ファイルの検索条件が変更されたのを反映します。
        /// </summary>
        public void ChangeCondition();
        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; }
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; }
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        public void AddFileToAllFiles(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "");
        /// <summary>
        /// ディレクトリをファイルディクショナリから削除します。
        /// </summary>
        public void RemoveDirectoryFromAllFiles(string directoryFullPath);
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; }
        /// <summary>
        /// 検索条件コレクションに追加します。
        /// </summary>
        public void AddCondition(SearchConditionType type, string contidionString);
        /// <summary>
        /// 検索条件コレクションから削除します。
        /// </summary>
        public void RemoveCondition(SearchConditionType type, string contidionString);
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
        /// ツリー横幅の設定
        /// </summary>
        private double _TreeWidth;
        public double TreeWidth
        {
            get => _TreeWidth;
            set
            {
                if (value == _TreeWidth) { return; }

                SetProperty(ref _TreeWidth, value);
                _messageServices.SendTreeWidth(value);
            }
        }
        /// <summary>
        /// リストボックスの幅を設定します
        /// </summary>
        private double _ListWidth;
        public double ListWidth
        {
            get => _ListWidth;
            set
            {
                if (value == _ListWidth) { return; }

                SetProperty(ref _ListWidth, value);
                _messageServices.SendListWidth(value);
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
        private string _SelectedHashAlgorithm;
        public string SelectedHashAlgorithm
        {
            get => _SelectedHashAlgorithm;
            set
            {
                if (value == _SelectedHashAlgorithm) return;

                SetProperty(ref _SelectedHashAlgorithm, value);
                _messageServices.SendHashAlogrithm(value);
            }
        }
        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムの一覧です。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; set; } = [];

        /// <summary>
        /// 検索条件に合致するファイルを保持するリスト
        /// </summary>
        public HashSet<HashFile> AllConditionFiles { get; } = [];
        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public Dictionary<string, HashFile> AllFiles { get; } = [];
        /// <summary>
        /// 検索条件をキーとしたファイルを保持するリスト
        /// </summary>
        public Dictionary<SearchCondition, HashSet<HashFile>> ConditionFiles { get; } = [];
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
        /// ファイル拡張子のチェックボックスが選択された時の拡張子グループチェックボックス処理をします
        /// </summary>
        public RelayCommand<object> ExtentionGroupCheckBoxClickedCommand { get; set; }
        /// <summary>
        /// ファイル拡張子のチェックボックスが選択解除された時のグループチェックボックス処理をします
        /// </summary>
        public RelayCommand<object> ExtentionCheckBoxClickedCommand { get; set; }
        #endregion コマンド

        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        private readonly ITreeManager _directoryTreeManager;
        private readonly IExtentionManager _extentionManager;
        private readonly IControDirectoryTreeViewlModel _controDirectoryTreeViewlViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetViewModel(
            IMessageServices messageServices,
            ISettingsService settingsService,
            ITreeManager directoryTreeManager,
            IExtentionManager extentionManager,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel
            )
        {
            _messageServices = messageServices;
            _settingsService = settingsService;
            _directoryTreeManager = directoryTreeManager;
            _extentionManager = extentionManager;
            _controDirectoryTreeViewlViewModel = controDirectoryTreeViewlViewModel;

            // カレントハッシュ計算アルゴリズムを保存
            SelectedHashAlgorithm = _settingsService.HashAlgorithm;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(() =>
            {
                IsExecuting = true;
                _messageServices.SendToSettingsPage(ReturnPageEnum.PageTargetSelect);
            });
            // デバッグウィンドウを開くコマンド
            DebugOpen = new RelayCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });
            // エクスプローラー風画面に移動するコマンド
            ToPageExplorer = new RelayCommand(() =>
            {
                CTS?.Cancel();
                _messageServices.SendToExplorerPage();
            });
            // ハッシュ計算画面に移動するコマンド
            ToPageHashCalcing = new RelayCommand(
                () => _messageServices.SendToHashCalcingPage(),
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
            IsReadOnlyFileIncludeClicked = new RelayCommand(()
                => IsReadOnlyFileInclude = !IsReadOnlyFileInclude);

            // 隠しファイルを利用するかどうかがクリックされた時、チェック状態を切り替えるコマンド
            IsHiddenFileIncludeClicked = new RelayCommand(()
                => IsHiddenFileInclude = !IsHiddenFileInclude);

            //  0 サイズのファイルを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsZeroSizeFIleDeleteClicked = new RelayCommand(()
                => IsZeroSizeFileDelete = !IsZeroSizeFileDelete);

            // 空のフォルダを削除するかどうかのテキストがクリックされた時、チェック状態を切り替えるコマンド
            IsEmptyDirectoryDeleteClicked = new RelayCommand(()
                => IsEmptyDirectoryDelete = !IsEmptyDirectoryDelete);

            // ツリービュー幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<TreeWidthChanged>(this, (_, m)
                => TreeWidth = m.TreeWidth);

            // リストボックス幅変更メッセージ受信
            WeakReferenceMessenger.Default.Register<ListWidthChanged>(this, (_, m)
                => ListWidth = m.ListWidth);

            // フォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentFontFamilyChanged>(this, (_, m)
                => CurrentFontFamily = m.CurrentFontFamily);

            // フォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, m)
                => FontSize = m.FontSize);

            // カレントディレクトリが変更されたメッセージ受信
            WeakReferenceMessenger.Default.Register<CurrentDirectoryChanged>(this, (_, m)
                => ChangeCurrentPath(m.CurrentFullPath));

            _TreeWidth = _settingsService.TreeWidth;
            _ListWidth = _settingsService.ListWidth;
            _SelectedHashAlgorithm = _settingsService.HashAlgorithm;

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;
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
                ChangeCurrentPath(_controDirectoryTreeViewlViewModel.TreeRoot[0].FullPath);
                _controDirectoryTreeViewlViewModel.TreeRoot[0].IsSelected = true;
            }
            catch { }
            // 既にファイル検索がされていて、ディレクトリ選択設定が変わっていなければ終了
            if (Status == FileScanStatus.Finished
             && _directoryTreeManager.NestedDirectories.OrderBy(x => x).SequenceEqual(NestedDirectories.OrderBy(x => x))
             && _directoryTreeManager.NonNestedDirectories.OrderBy(x => x).SequenceEqual(NonNestedDirectories.OrderBy(x => x)))
            {
                return;
            }

            // 現在のディレクトリ選択設定を保存する
            NestedDirectories.Clear();
            NestedDirectories.AddRange(_directoryTreeManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_directoryTreeManager.NonNestedDirectories);

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
            var currentAlgorithm = _settingsService.HashAlgorithm;

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
            _controDirectoryTreeViewlViewModel.ClearRoot();
            _controDirectoryTreeViewlViewModel.SetIsCheckBoxVisible(false);

            foreach (var root in _directoryTreeManager.NestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            foreach (var root in _directoryTreeManager.NonNestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }
        #endregion 初期処理

        #region ツリービュー選択処理
        /// <summary>
        /// ツリービューの選択ディレクトリが変更された時の処理です。
        /// </summary>
        /// <param name="currentFullPath">カレントディレクトリ</param>
        /// <exception cref="NullReferenceException">IFileManagerが取得できなかった時の例外</exception>
        private void ChangeCurrentPath(string currentFullPath)
        {
            App.Current?.Dispatcher?.Invoke(() =>
            {
                HashFileListItems.Clear();
                var files = FileManager.EnumerateFiles(currentFullPath);

                foreach (var file in files)
                {
                    var item = new HashListFileItems
                    {
                        FileFullPath = file,
                        IsHashTarget = AllConditionFiles.Any(f => f.FileFullPath == file)
                    };
                    HashFileListItems.Add(item);
                }
            });
        }

        private readonly object _lock = new();

        /// <summary>
        /// 検索条件が変更された時の処理です。
        /// </summary>
        public void ChangeCondition()
        {
            ChangeCurrentPath(_controDirectoryTreeViewlViewModel.CurrentFullPath);
        }
        #endregion ツリービュー選択処理

        #region ファイルの追加とディレクトリの削除
        /// <summary>
        /// ファイルを追加します
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        /// <param name="hashSHA256">SHA256のハッシュ</param>
        /// <param name="hashSHA384">SHA384のハッシュ</param>
        /// <param name="hashSHA512">SHA512のハッシュ</param>
        public void AddFileToAllFiles(string fileFullPath, string hashSHA256 = "", string hashSHA384 = "", string hashSHA512 = "")
        {
            var fileInfo = new FileInfo(fileFullPath);
            if (AllFiles.TryGetValue(fileFullPath, out HashFile? value))
            {
                // 同一日付とサイズなら追加しない
                if (fileInfo.LastWriteTime == value.LastWriteTime && fileInfo.Length == value.Length) { return; }

                // 既にハッシュを持っているなら設定する
                if (!string.IsNullOrEmpty(value.SHA256) && string.IsNullOrEmpty(hashSHA256)) { hashSHA256 = value.SHA256; }
                if (!string.IsNullOrEmpty(value.SHA384) && string.IsNullOrEmpty(hashSHA384)) { hashSHA384 = value.SHA384; }
                if (!string.IsNullOrEmpty(value.SHA512) && string.IsNullOrEmpty(hashSHA512)) { hashSHA512 = value.SHA512; }

                // データが異なるか、ハッシュ更新されていれば昔のデータを削除する
                AllFiles.Remove(fileFullPath);
            }
            // 新しいデータなら追加する(日付と更新日はコンストラクタで設定される)
            var hashFile = new HashFile(fileFullPath, hashSHA256, hashSHA384, hashSHA512);
            AllFiles.Add(fileFullPath, hashFile);

            // 拡張子ヘルパーに拡張子を登録する(カウントもする)
            _extentionManager.AddFile(fileFullPath);
        }

        /// <summary>
        /// ディレクトリを削除します
        /// </summary>
        /// <param name="directoryFullPath">削除するディレクトリのフルパス</param>
        public void RemoveDirectoryFromAllFiles(string directoryFullPath)
        {
            foreach (var fileToRemove in AllFiles.Keys.Where(d => Path.GetDirectoryName(d) == directoryFullPath).ToList())
            {
                AllFiles.Remove(fileToRemove);
            }
        }
        #endregion ファイルの追加とディレクトリの削除
    }
}
