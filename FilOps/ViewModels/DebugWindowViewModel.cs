using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.ViewModels.ExplorerPage;
using FilOps.Models;

namespace FilOps.ViewModels
{
    public interface IDebugWindowViewModel
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
    public class DebugWindowViewModel : ObservableObject, IDebugWindowViewModel
    {
        enum PollingTarget
        {
            None,
            ExpandDirectoryManager,
            CheckedDirectoryManager,
        }
        /// <summary>
        /// 【ここでポーリング対象を決める】
        /// </summary>
        private readonly PollingTarget pollingTarget = PollingTarget.CheckedDirectoryManager;


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
        private double _Width = 400d;
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
        /// ICheckedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly IExpandedDirectoryManager _ExpandedDirectoryManager;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;

        /// <summary>
        /// コンストラクタ、ポーリングの設定とポーリング対象を獲得します。
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        /// <param name="expandDirManager">今はIExpandedDirectoryManager</param>
        public DebugWindowViewModel(
            IExpandedDirectoryManager expandedDirectoryManager,
            ICheckedDirectoryManager checkedDirectoryManager,
            IMainViewModel mainViewModel
            )
        {
            _ExpandedDirectoryManager = expandedDirectoryManager;
            _CheckedDirectoryManager = checkedDirectoryManager;

            Top = mainViewModel.Top;
            Left = mainViewModel.Left + mainViewModel.Width;

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
            PollingCommand.Execute(null);
        }


        /// <summary>
        /// ポーリング中の処理を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polling(object? sender, EventArgs e)
        {
            var sb = new StringBuilder();

            switch (pollingTarget)
            {
                case PollingTarget.ExpandDirectoryManager:
                    foreach (var item in _ExpandedDirectoryManager.Directories)
                    {
                        sb.AppendLine(item);
                    }
                    break;
                case PollingTarget.CheckedDirectoryManager:
                    sb.AppendLine("サブディレクトリを含む管理");
                    foreach (var item in _CheckedDirectoryManager.NestedDirectories)
                    {
                        sb.AppendLine($"\t{item}");
                    }
                    sb.AppendLine("-------------------------------");
                    sb.AppendLine("サブディレクトリを含まない管理");
                    foreach (var item in _CheckedDirectoryManager.NonNestedDirectories)
                    {
                        sb.AppendLine($"\t{item}");
                    }
                    break;
                default:
                    break;
            }

            App.Current?.Dispatcher.Invoke(() =>
            {
                DebugText = string.Empty;
                DebugText = sb.ToString();
            });
        }
    }
}
