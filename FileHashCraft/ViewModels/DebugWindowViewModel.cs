/*  DebugWindowViewModel.cs

    デバッグウィンドウの ViewModel を提供します。
 */
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels
{
    #region インターフェース
    public interface IDebugWindowViewModel
    {
        double Top { get; set; }
        double Left { get; set; }
    }
    #endregion インターフェース
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
        /// ウィンドウの高さ
        /// </summary>
        private double _Height = 800d;
        public double Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
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
        /// フォントの取得と設定
        /// </summary>
        private FontFamily _CurrentFontFamily;
        public FontFamily CurrentFontFamily
        {
            get => _CurrentFontFamily;
            set
            {
                SetProperty(ref _CurrentFontFamily, value);
                _messageServices.SendCurrentFont(value);
            }
        }

        /// <summary>
        /// フォントサイズの取得と設定
        /// </summary>
        private double _FontSize;
        public double FontSize
        {
            get => _FontSize;
            set
            {
                SetProperty(ref _FontSize, value);
                _messageServices.SendFontSize(value);
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
        public RelayCommand PollingCommand { get; set; }
        #endregion バインディング

        #region コンストラクタと初期化
        /// <summary>
        /// ICheckedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        private readonly ITreeManager _directoryTreeManager;
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// コンストラクタ、ポーリングの設定とポーリング対象を獲得します。
        /// 今はIExpandedDirectoryManager、デバッグ対象により変更する
        /// </summary>
        public DebugWindowViewModel(
            ITreeManager directoryTreeManager,
            IMessageServices messageServices,
            ISettingsService settingsService
            )
        {
            _directoryTreeManager = directoryTreeManager;
            _messageServices = messageServices;
            _settingsService = settingsService;

            Top = _settingsService.Top;
            Left = _settingsService.Left + _settingsService.Width;
            Width = _settingsService.Width / 2;
            Height = _settingsService.Height;

            _CurrentFontFamily = _settingsService.CurrentFont;
            _FontSize = _settingsService.FontSize;

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
                    foreach (var item in _directoryTreeManager.Directories)
                    {
                        sb.AppendLine(item);
                    }
                    break;
                case PollingTarget.CheckedDirectoryManager:
                    sb.AppendLine("サブディレクトリを含まない管理");
                    foreach (var item in _directoryTreeManager.NonNestedDirectories)
                    {
                        sb.Append('\t').AppendLine(item);
                    }
                    sb.AppendLine("-------------------------------");
                    sb.AppendLine("サブディレクトリを含む管理");
                    foreach (var item in _directoryTreeManager.NestedDirectories)
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
