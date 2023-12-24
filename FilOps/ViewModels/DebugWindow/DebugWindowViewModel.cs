using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly DispatcherTimer timer;
        public DelegateCommand PollingCommand { get; set; }
        #endregion バインディング

        /// <summary>
        /// アプリケーション終了時に必要な後処理をここに入れる
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
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly ICheckedDirectoryManager DebugClass;

        /// <summary>
        /// コンストラクタ、ポーリングの設定とポーリング対象を獲得する
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        /// <param name="expandDirManager">今はIExpandedDirectoryManager</param>
        public DebugWindowViewModel(
            IMainViewModel mainViewModel,
            ICheckedDirectoryManager debugClass)
        {
            DebugClass = debugClass;

            this.Top = mainViewModel.Top;
            this.Left = mainViewModel.Left + mainViewModel.Width;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Polling);
            timer.Interval = TimeSpan.FromSeconds(1);
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
            PollingCommand.Execute(null);
        }

        /// <summary>
        /// ポーリング中の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polling(object? sender, EventArgs e)
        {
            List<string> list =
            [
                "Directory Only -------------------------------",
                .. DebugClass.DirectoriesOnly,
                "With SubDirectories---------------------------",
                .. DebugClass.DirectoriesWithSubdirectories,
            ];
            App.Current?.Dispatcher.Invoke(() =>
            {
                DebugText = string.Empty;

                var sb = new StringBuilder();
                foreach (var dir in list)
                {
                    sb.Append(dir);
                    sb.Append(Environment.NewLine);
                }
                DebugText = sb.ToString();
            });
        }

    }
}
