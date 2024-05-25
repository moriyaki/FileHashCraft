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
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Properties;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModel
    {
        /// <summary>
        /// PageSelectTargetViewModelのメインViewModel
        /// </summary>
        public IPageSelectTargetViewModelMain ViewModelMain { get; }
        /// <summary>
        /// 他ページから移動してきた時の初期化処理をします。
        /// </summary>
        public void Initialize();
        /// <summary>
        /// リストボックスの幅
        /// </summary>
        public double ListWidth { get; set; }
        /// <summary>
        /// ハッシュアルゴリズム
        /// </summary>
        public string SelectedHashAlgorithm { get; set; }

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
        public Task ExtentionCountChanged();
        /// <summary>
        /// 拡張子グループのチェックボックス状態が変更されたら、拡張子にも反映します。
        /// </summary>
        public Task ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention);
        /// <summary>
        /// ハッシュ取得対象のファイルリストアイテムの一覧です。
        /// </summary>
        public ObservableCollection<HashListFileItems> HashFileListItems { get; }
        /// <summary>
        /// 拡張子チェックボックスにチェックされたので拡張子グループに反映します。
        /// </summary>
        public Task CheckExtentionReflectToGroup(string extention);
        /// <summary>
        /// 拡張子チェックボックスがチェック解除されたので拡張子グループに反映します。
        /// </summary>
        public Task UncheckExtentionReflectToGroup(string extention);
        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        public Task ChangeExtension(string extention, bool IsTarget);
        /// <summary>
        /// 全管理対象ファイルを追加します。
        /// </summary>
        public void AddFileToAllFiles(string fileFullPath);
    }
    #endregion インターフェース

    public class PageSelectTargetViewModel : ObservableObject, IPageSelectTargetViewModel
    {
        #region バインディング
        /// <summary>
        /// PageSelectTargetViewModelのメインViewModel
        /// </summary>
        public IPageSelectTargetViewModelMain ViewModelMain { get; }
        /// <summary>
        /// ファイルの拡張子グループによる絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; } = [];

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtensionOrTypeCheckBoxBase> ExtentionCollection { get; set; } = [];

        /// <summary>
        /// フィルタするファイル
        /// </summary>
        private string _FilterTextBox = string.Empty;
        public string FilterTextBox
        {
            get => _FilterTextBox;
            set => SetProperty(ref _FilterTextBox, value);
        }
        /// <summary>
        /// ツリービュー横幅の設定
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
        // スキャンした全ファイルの管理
        private readonly IScannedFilesManager _allFilesManager;
        // 拡張子の管理
        private readonly IExtentionManager _extentionManager;
        // メッセージング
        private readonly IMessageServices _messageServices;
        // 設定画面
        private readonly ISettingsService _settingsService;
        // エクスプローラー風画面のツリービュー
        private readonly ITreeManager _directoryTreeManager;
        // ツリービューコントロール
        private readonly IControDirectoryTreeViewlModel _controDirectoryTreeViewlViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetViewModel(
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain,
            IScannedFilesManager allFilesManager,
            IExtentionManager extentionManager,
            IMessageServices messageServices,
            ISettingsService settingsService,
            ITreeManager directoryTreeManager,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel
        )
        {
            ViewModelMain = pageSelectTargetViewModelMain;
            _allFilesManager = allFilesManager;
            _extentionManager = extentionManager;
            _messageServices = messageServices;
            _settingsService = settingsService;
            _directoryTreeManager = directoryTreeManager;
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

            // ワイルドカードの条件を追加するコマンド
            AddWildcard = new RelayCommand(
                () => MessageBox.Show("ワイルドカードの追加：未実装"));

            // ワイルドカードの条件を削除するコマンド
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
            if (ViewModelMain.Status == FileScanStatus.Finished
             && _directoryTreeManager.NestedDirectories.OrderBy(x => x).SequenceEqual(NestedDirectories.OrderBy(x => x))
             && _directoryTreeManager.NonNestedDirectories.OrderBy(x => x).SequenceEqual(NonNestedDirectories.OrderBy(x => x)))
            {
                return;
            }

            // 現在のディレクトリ選択設定を保存する
            SaveCurrentDirectorySelectionSettings();

            // 状況が変わっているので、必要な値の初期化をする
            ResetInitializedValues();

            // ファイルスキャンの開始
            StartFileScan();
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
            //Status = _Status;

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

            // 該当以下のディレクトリを含むディレクトリのパスをツリービューに追加する。
            foreach (var root in _directoryTreeManager.NestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
            // 該当ディレクトリのみを単独でツリービューに追加する。
            foreach (var root in _directoryTreeManager.NonNestedDirectories)
            {
                var fi = SpecialFolderAndRootDrives.GetFileInformationFromDirectorPath(root);
                fi.HasChildren = false;
                _controDirectoryTreeViewlViewModel.AddRoot(fi, false);
            }
        }

        /// <summary>
        /// 現在のディレクトリ選択設定を保存します。
        /// </summary>
        private void SaveCurrentDirectorySelectionSettings()
        {
            NestedDirectories.Clear();
            NestedDirectories.AddRange(_directoryTreeManager.NestedDirectories);
            NonNestedDirectories.Clear();
            NonNestedDirectories.AddRange(_directoryTreeManager.NonNestedDirectories);
        }

        /// <summary>
        /// 必要な値の初期化をします。
        /// </summary>
        private void ResetInitializedValues()
        {
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                ViewModelMain.CountScannedDirectories = 0;
                ViewModelMain.CountHashFilesDirectories = 0;
                ViewModelMain.CountAllTargetFilesGetHash = 0;
                ViewModelMain.CountFilteredGetHash = 0;
                ExtentionCollection.Clear();
                ExtentionsGroupCollection.Clear();
            });
        }

        /// <summary>
        /// チェックされたディレクトリのファイルスキャンをします。
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void StartFileScan()
        {
            var scanHashFilesClass = Ioc.Default.GetService<IScanHashFiles>() ?? throw new InvalidOperationException($"{nameof(IScanHashFiles)} dependency not resolved.");
            CTS = new CancellationTokenSource();
            cancellationToken = CTS.Token;

            // 移動ボタンの利用状況を設定
            ViewModelMain.Status = FileScanStatus.None;
            ViewModelMain.ToPageHashCalcing.NotifyCanExecuteChanged();

            // スキャンするディレクトリの追加
            scanHashFilesClass.ScanFiles(cancellationToken);
        }
        #endregion 初期処理

        #region 拡張子チェックボックスの管理
        /// <summary>
        /// 拡張子チェックボックスにチェックされた時に拡張子グループに反映します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public async Task CheckExtentionReflectToGroup(string extention)
        {
            var fileGroupType = ExtentionTypeHelper.GetFileGroupFromExtention(extention);
            var group = ExtentionsGroupCollection.FirstOrDefault(g => g.FileType == fileGroupType);
            if (group == null) return;

            // グループの拡張子に一つもチェックが入ってなければ null にする
            if (group.IsChecked == false)
            {
                await App.Current.Dispatcher.InvokeAsync(() => group.IsCheckedForce = null);
            }

            // グループの拡張子が全てチェックされているか調べる
            foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
            {
                var item = ExtentionCollection.FirstOrDefault(i => i.ExtentionOrGroup == groupExtention);
                // 1つでもチェックされてない拡張子があればそのまま戻る
                if (item != null && item.IsChecked == false) { return; }
            }
            // 全てチェックされていたらチェック状態を null → true
            await App.Current.Dispatcher.InvokeAsync(() => group.IsCheckedForce = true);
        }

        /// <summary>
        /// 拡張子チェックボックスのチェックが解除された時に拡張子グループに反映します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public async Task UncheckExtentionReflectToGroup(string extention)
        {
            var fileGroupType = ExtentionTypeHelper.GetFileGroupFromExtention(extention);
            var group = ExtentionsGroupCollection.FirstOrDefault(g => g.FileType == fileGroupType);
            if (group == null) return;

            // グループの拡張子に全てチェックが入っていれば null にする
            if (group.IsChecked == true)
            {
                await App.Current.Dispatcher.InvokeAsync(() => group.IsCheckedForce = null);
            }

            // グループの拡張子が全てチェック解除されているか調べる
            foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
            {
                var item = ExtentionCollection.FirstOrDefault(i => i.ExtentionOrGroup == groupExtention);
                // 1つでもチェックされてない拡張子があればそのまま戻る
                if (item != null && item.IsChecked == true) { return; }
            }
            // 全てチェック解除されていたらチェック状態を null → false
            await App.Current.Dispatcher.InvokeAsync(() => group.IsCheckedForce = false);
        }
        #endregion 拡張子チェックボックスの管理

        #region ファイル絞り込みの処理
        /// <summary>
        /// ファイルの拡張子グループをリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(FileGroupType.Musics);
            var documents = new ExtentionGroupCheckBoxViewModel(FileGroupType.Documents);
            var applications = new ExtentionGroupCheckBoxViewModel(FileGroupType.Applications);
            var archives = new ExtentionGroupCheckBoxViewModel(FileGroupType.Archives);
            var sources = new ExtentionGroupCheckBoxViewModel(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel();

            App.Current?.Dispatcher.Invoke(() =>
            {
                if (movies.ExtentionCount > 0) { ExtentionsGroupCollection.Add(movies); }
                if (pictures.ExtentionCount > 0) { ExtentionsGroupCollection.Add(pictures); }
                if (musics.ExtentionCount > 0) { ExtentionsGroupCollection.Add(musics); }
                if (documents.ExtentionCount > 0) { ExtentionsGroupCollection.Add(documents); }
                if (applications.ExtentionCount > 0) { ExtentionsGroupCollection.Add(applications); }
                if (archives.ExtentionCount > 0) { ExtentionsGroupCollection.Add(archives); }
                if (sources.ExtentionCount > 0) { ExtentionsGroupCollection.Add(sources); }
                if (registrations.ExtentionCount > 0) { ExtentionsGroupCollection.Add(registrations); }
                if (others.ExtentionCount > 0) { ExtentionsGroupCollection.Add(others); }
            });
        }

        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void AddExtentions(string extention)
        {
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new NullReferenceException(nameof(IExtentionManager));
            if (extentionManager.GetExtentionsCount(extention) > 0)
            {
                var item = new ExtensionCheckBox(extention);
                App.Current?.Dispatcher.Invoke(() => ExtentionCollection.Add(item));
            }
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => ExtentionCollection.Clear());
            _extentionManager.ClearExtentions();
        }
        /// <summary>
        /// 拡張子チェックボックスにより、スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public async Task ExtentionCountChanged()
        {
            await App.Current.Dispatcher.InvokeAsync(() => ViewModelMain.CountFilteredGetHash = _allFilesManager.AllConditionFiles.Count);
        }

        /// <summary>
        /// ChangeCheckBoxGroupで使うロックオブジェクト
        /// </summary>
        private readonly object _changedLock = new();

        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        /// <param name="changedCheck">チェックされたか外されたか</param>
        /// <param name="extentionCollention">拡張子のリストコレクション</param>
        public async Task ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention)
        {
            var changedCollection = ExtentionCollection.Where(e => extentionCollention.Contains(e.ExtentionOrGroup));

            foreach (var extension in changedCollection)
            {
                await App.Current.Dispatcher.InvokeAsync(() => extension.IsCheckedForce = changedCheck);
                await Task.Run(async () =>
                {
                    foreach (var file in _allFilesManager.AllFiles.Values.Where(c => string.Equals(Path.GetExtension(c.FileFullPath), extension.ExtentionOrGroup, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (changedCheck)
                        {
                            lock (_changedLock)
                            {
                                _allFilesManager.AllConditionFiles.Add(file);
                            }
                        }
                        else
                        {
                            lock (_changedLock)
                            {
                                _allFilesManager.AllConditionFiles.Remove(file);
                            }
                        }
                    }
                    await ChangeExtension(extension.ExtentionOrGroup, changedCheck);
                });
                await ExtentionCountChanged();
            }
        }
        #endregion ファイル絞り込みの処理

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

                foreach (var file in FileManager.EnumerateFiles(currentFullPath))
                {
                    var item = new HashListFileItems
                    {
                        FileFullPath = file,
                        IsHashTarget = _allFilesManager.AllConditionFiles.Any(f => f.FileFullPath == file)
                    };
                    HashFileListItems.Add(item);
                }
            });
        }

        /// <summary>
        /// 拡張子の検索条件が変更された時の処理です。
        /// </summary>
        public async Task ChangeExtension(string extention, bool IsTarget)
        {
            foreach (var item in HashFileListItems)
            {
                var fileExtention = Path.GetExtension(item.FileFullPath);
                if (string.Equals(fileExtention, extention, StringComparison.OrdinalIgnoreCase))
                {
                    await App.Current.Dispatcher.InvokeAsync(() => item.IsHashTarget = IsTarget);
                }
            }
        }

        /// <summary>
        /// 全管理対象ファイルを追加します。
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        public void AddFileToAllFiles(string fileFullPath)
        {
            // 全管理対象ファイルをModelに追加する
            _allFilesManager.AddFileToAllFiles(fileFullPath);
            // 拡張子ヘルパーに拡張子を登録する(カウントもする)
            _extentionManager.AddFile(fileFullPath);
        }
    }
}
