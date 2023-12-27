using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using FilOps.ViewModels.DirectoryTreeViewControl;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.ViewModels.DebugWindow
{
    public interface IDebugWindowViewModel
    {
        public double Top {  get; set; }
        public double Left { get; set; }
        
        public void Cancel();
    }
    public class DebugWindowViewModel : ObservableObject, IDebugWindowViewModel
    {
        #region バインディング
        
        /// <summary>
        /// 画面の上端設定
        /// </summary>
        private double _Top = 450d;
        public double Top
        {
            get => _Top;
            set => SetProperty(ref _Top, value);
        }

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        public double _Left = 400d;
        public double Left
        {
            get => _Left;
            set => SetProperty(ref _Left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private  double _Width = 400d;
        public double Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        /// <summary>
        /// ポーリング処理中か否か
        /// </summary>
        private bool _IsPolling = false;
        public bool IsPolling
        {
            get => _IsPolling;
            set
            {
                SetProperty(ref _IsPolling, value);
                OnPropertyChanged(nameof(PollingStatus));
            }
        }

        /// <summary>
        /// ポーリング開始/終了のボタン文字列
        /// </summary>
        public string PollingStatus
        {
            get => IsPolling ? "ポーリング終了" : "ポーリング開始";
        }

        /// <summary>
        /// デバッグテキスト
        /// </summary>
        private string _DebugText = "ポーリング待機中";
        public string DebugText
        {
            get => _DebugText;
            set => SetProperty(ref _DebugText, value);
        }

        /// <summary>
        /// ポーリング用タイマー
        /// </summary>
        //private readonly DispatcherTimer timer;
        public DelegateCommand PollingCommand { get; set; }
        #endregion バインディング

        /// <summary>
        /// アプリケーション終了時に必要な後処理をします。
        /// </summary>
        public void Cancel()
        {
            if (_IsPolling)
            {
                IsPolling = false;
                PollingCommand.Execute(null);
            }
        }

        /// <summary>
        /// ICheckedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly ICheckedDirectoryManager DebugClass;

        private readonly IFileSystemInformationManager _FileSystemInformationManager;
        private readonly IDirectoryTreeViewControlViewModel _DirectoryTreeViewControlViewModel;

        /// <summary>
        /// コンストラクタ、ポーリングの設定とポーリング対象を獲得します。
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        /// <param name="expandDirManager">今はIExpandedDirectoryManager</param>
        public DebugWindowViewModel(
            IMainViewModel mainViewModel,
            IFileSystemInformationManager fileSystemInformationManager,
            ICheckedDirectoryManager debugClass,
            IDirectoryTreeViewControlViewModel directoryTreeViewControlViewModel
            )
        {
            _DirectoryTreeViewControlViewModel = directoryTreeViewControlViewModel;
            _FileSystemInformationManager = fileSystemInformationManager;
            DebugClass = debugClass;
            PollingCommand = new DelegateCommand(() => { });

            this.Top = mainViewModel.Top;
            this.Left = mainViewModel.Left + mainViewModel.Width;

            // TreeView用
            foreach (var rootInfo in _FileSystemInformationManager.SpecialFolderScan())
            {
                _DirectoryTreeViewControlViewModel.AddRoot(rootInfo);
            }
            foreach (var rootInfo in FileSystemInformationManager.DriveScan())
            {
                _DirectoryTreeViewControlViewModel.AddRoot(rootInfo);
            }




                /*
                timer = new DispatcherTimer();
                timer.Tick += new EventHandler(Polling);
                timer.Interval = TimeSpan.FromMilliseconds(200);
                PollingCommand = new DelegateCommand(
                    () =>
                    {
                        if (IsPolling)
                        {
                            timer.Stop();
                            IsPolling = false;
                            DebugText = string.Empty;
                        }
                        else
                        {
                            timer.Start();
                            IsPolling = true;
                        }
                    }
                );
                //PollingCommand.Execute(null);
            }

            /// <summary>
            /// ポーリング中の処理を行います。
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Polling(object? sender, EventArgs e)
            {
                /*
                var OnlyList = DebugClass.DirectoriesOnly;
                var SubList = DebugClass.DirectoriesWithSubdirectories;
                */

                App.Current?.Dispatcher.Invoke(() =>
            {
                DebugText = string.Empty;

                var sb = new StringBuilder();
                sb.Append(DebugClass.ToString());
                /*
                foreach (var dir in OnlyList)
                {
                    sb.Append(dir);
                    sb.Append(Environment.NewLine);
                }
                sb.Append("-------------------以下Sub");
                sb.Append(Environment.NewLine);
                foreach (var dir in SubList)
                {
                    sb.Append(dir);
                    sb.Append(Environment.NewLine);
                }
                */
                DebugText = sb.ToString();
            });
        }

    }
}
