/*  DebugWindowViewModel.cs

    デバッグウィンドウの ViewModel を提供します。
 */
using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.ControlDirectoryTree;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IDebugWindowViewModel
    {
        double Top { get; set; }
        double Left { get; set; }
    }
    #endregion インターフェース
    public class DebugWindowViewModel : BaseViewModel, IDebugWindowViewModel
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
        private double _top = 450d;
        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        public double _left = 400d;
        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        private double _width = 400d;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        private double _height = 800d;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        /// <summary>
        /// ポーリング処理中か否か
        /// </summary>
        private bool _isPolling = false;
        public bool IsPolling
        {
            get => _isPolling;
            set
            {
                SetProperty(ref _isPolling, value);
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
        private string _debugText = "ポーリング待機中";
        public string DebugText
        {
            get => _debugText;
            set => SetProperty(ref _debugText, value);
        }

        /// <summary>
        /// ポーリング用タイマー
        /// </summary>
        private readonly DispatcherTimer timer;
        public RelayCommand PollingCommand { get; set; }
        #endregion バインディング

        #region コンストラクタと初期化
        /// <summary>
        /// ICheckedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly IDirectoriesManager _directoriesManager;
        /// <summary>
        /// コンストラクタ、ポーリングの設定とポーリング対象を獲得します。
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        public DebugWindowViewModel(
            ISettingsService settingsService,
            IDirectoriesManager directoriesManager
        ) : base(settingsService)
        {
            _directoriesManager = directoriesManager;

            Top = _settingsService.Top;
            Left = _settingsService.Left + _settingsService.Width;
            Width = _settingsService.Width / 2;
            Height = _settingsService.Height;

            timer = new DispatcherTimer();
            timer.Tick += Polling;
            timer.Interval = TimeSpan.FromMilliseconds(200);
            PollingCommand = new RelayCommand(
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
        #endregion コンストラクタと初期化

        #region ポーリング処理
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
                    foreach (var item in _directoriesManager.Directories)
                    {
                        sb.AppendLine(item);
                    }
                    break;
                case PollingTarget.CheckedDirectoryManager:
                    sb.AppendLine("サブディレクトリを含まない管理");
                    foreach (var item in _directoriesManager.NonNestedDirectories)
                    {
                        sb.Append('\t').AppendLine(item);
                    }
                    sb.AppendLine("-------------------------------");
                    sb.AppendLine("サブディレクトリを含む管理");
                    foreach (var item in _directoriesManager.NestedDirectories)
                    {
                        sb.Append('\t').AppendLine(item);
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
        #endregion ポーリング処理
    }
}
