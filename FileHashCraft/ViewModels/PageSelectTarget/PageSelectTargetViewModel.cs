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
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using System.Xml.Linq;
using FileHashCraft.Services.Messages;

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
        /// PageSelectTargetViewModelの拡張子ViewModel
        /// </summary>
        public IPageSelectTargetViewModelExtention ViewModelExtention { get; }
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
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        public int CountFilteredGetHash { get; }
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
        /// PageSelectTargetViewModelの拡張子ViewModel
        /// </summary>
        public IPageSelectTargetViewModelExtention ViewModelExtention { get; }

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
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        public int CountFilteredGetHash { get => ViewModelMain.CountFilteredGetHash; }
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
        #endregion バインディング

        #region コンストラクタ
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;
        private readonly ITreeManager _directoryTreeManager;
        private readonly IControDirectoryTreeViewlModel _controDirectoryTreeViewlViewModel;
        private bool IsExecuting = false;

        public PageSelectTargetViewModel(
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain,
            IPageSelectTargetViewModelExtention pageSelectTargetViewModelExtention,
            IMessageServices messageServices,
            ISettingsService settingsService,
            ITreeManager directoryTreeManager,
            IControDirectoryTreeViewlModel controDirectoryTreeViewlViewModel
        )
        {
            ViewModelMain = pageSelectTargetViewModelMain;
            ViewModelExtention = pageSelectTargetViewModelExtention;
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
                ViewModelMain.ChangeCurrentPath(_controDirectoryTreeViewlViewModel.TreeRoot[0].FullPath);
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
                ViewModelExtention.ExtentionsGroupCollection.Clear();
                ViewModelExtention.AddFileTypes();
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
                ViewModelExtention.ExtentionCollection.Clear();
                ViewModelExtention.ExtentionsGroupCollection.Clear();
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

    }
}
