using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
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
        /// 検索対象の一覧を閲覧する画面を開きます。
        /// </summary>
        public DelegateCommand TargetFilterWindowOpen { get; set; }
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
            TargetFilterWindowOpen = new DelegateCommand(() =>
            {
                var targetFilterWIndow = new Views.TargetFilterWindow();
                targetFilterWIndow.Show();
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

            // メインウィンドウからのフォント変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontChanged>(this, (_, message) =>
                UsingFont = message.UsingFont);

            // メインウィンドウからのフォントサイズ変更メッセージ受信
            WeakReferenceMessenger.Default.Register<FontSizeChanged>(this, (_, message) =>
                FontSize = message.FontSize);
        }
        #endregion コンストラクタ
    }
}
