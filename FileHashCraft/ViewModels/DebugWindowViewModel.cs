/*  DebugWindowViewModel.cs

    デバッグウィンドウの ViewModel を提供します。
 */
using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IDebugWindowViewModel
    {
        double Top { get; set; }
        double Left { get; set; }
    }
    #endregion インターフェース
    public partial class DebugWindowViewModel : BaseViewModel, IDebugWindowViewModel
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
        [ObservableProperty]
        private double _Top = 450d;

        /// <summary>
        /// 左端の位置設定
        /// </summary>
        [ObservableProperty]
        public double _Left = 400d;

        /// <summary>
        /// ウィンドウの幅
        /// </summary>
        [ObservableProperty]
        private double _Width = 400d;

        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        [ObservableProperty]
        private double _Height = 800d;

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
        [ObservableProperty]
        private string _DebugText = "ポーリング待機中";

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
            IMessenger messenger,
            ISettingsService settingsService,
            IDirectoriesManager directoriesManager
        ) : base(messenger, settingsService)
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
