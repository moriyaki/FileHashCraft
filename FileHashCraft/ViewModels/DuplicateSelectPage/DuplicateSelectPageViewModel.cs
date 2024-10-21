using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.DuplicateSelectPage
{
    public interface IDuplicateSelectPageViewModel;
    public class DuplicateSelectPageViewModel : BaseViewModel, IDuplicateSelectPageViewModel
    {
        #region バインディング

        /// <summary>
        /// 重複ファイルとフォルダのツリービューの幅
        /// </summary>
        private double _DupDirsFilesTreeViewWidth;
        public double DupDirsFilesTreeViewWidth
        {
            get => _DupDirsFilesTreeViewWidth;
            set
            {
                if (_DupDirsFilesTreeViewWidth == value) return;
                SetProperty(ref _DupDirsFilesTreeViewWidth, value);
                _settingsService.SendDupDirsFilesTreeViewWidth(value);
            }
        }

        /// <summary>
        /// 設定画面を開きます。
        /// </summary>
        public RelayCommand SettingsOpen { get; set; }
        /// <summary>
        /// デバッグウィンドウを開きます。
        /// </summary>
        public RelayCommand DebugOpen { get; set; }
        /// <summary>
        /// ヘルプウィンドウを開きます。
        /// </summary>
        public RelayCommand HelpOpen { get; set; }
        /// <summary>
        /// ファイル選択画面に戻ります。
        /// </summary>
        public RelayCommand ToSelectTargetPage { get; set; }
        /// <summary>
        /// 削除を実行します。
        /// </summary>
        public RelayCommand DeleteCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IFileSystemServices _FileSystemServices;
        private readonly IHelpWindowViewModel _HelpWindowViewModel;

        public DuplicateSelectPageViewModel() { throw new NotImplementedException(nameof(DuplicateSelectPageViewModel)); }

        public DuplicateSelectPageViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IFileSystemServices fileSystemServices,
            IHelpWindowViewModel helpWindowViewModel
        ) : base(messenger, settingsService)
        {
            _FileSystemServices = fileSystemServices;
            _HelpWindowViewModel = helpWindowViewModel;

            _DupDirsFilesTreeViewWidth = settingsService.DupDirsFilesTreeViewWidth;

            // 設定画面ページに移動するコマンド
            SettingsOpen = new RelayCommand(()
                => _FileSystemServices.NavigateToSettingsPage(ReturnPageEnum.SelecTargettPage));
            // デバッグウィンドウを開くコマンド
            DebugOpen = new RelayCommand(() =>
            {
                var debugWindow = new Views.DebugWindow();
                debugWindow.Show();
            });
            // ヘルプウィンドウを開くコマンド
            HelpOpen = new RelayCommand(() =>
            {
                var helpWindow = new Views.HelpWindow();
                helpWindow.Show();
                _HelpWindowViewModel.Initialize(HelpPage.Index);
            });
            // 削除コマンド
            DeleteCommand = new RelayCommand(()
                => System.Windows.MessageBox.Show("まだだよ"));

            ToSelectTargetPage = new RelayCommand(() =>
                _FileSystemServices.NavigateToSelectTargetPage());


        }
        #endregion コンストラクタ
    }
}
