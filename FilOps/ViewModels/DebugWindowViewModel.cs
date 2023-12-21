using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels
{
    public interface IDebugWindowViewModel
    {
    }
    public class DebugWindowViewModel : ObservableObject, IDebugWindowViewModel
    {
        /// <summary>
        /// ポーリング中の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Polling(object? sender, EventArgs e)
        {
            var list = ExplorerPageViewModel.ExpandDirManager.Directories;
            list.Sort();
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

        #region バインディング
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

        public string PollingStatus
        {
            get => IsPolling ?  "ポーリング終了" : "ポーリング開始";
        }

        /// <summary>
        /// デバッグテキスト
        /// </summary>
        private string _DebugText = "aaa";
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

        public DebugWindowViewModel()
        {
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
                    }
                    else
                    {
                        timer.Start();
                        IsPolling = true;
                    }
                }
            );

            ExplorerPageViewModel = App.Current.Services.GetService<IExplorerPageViewModel>() ?? throw new NullReferenceException(nameof(IExplorerPageViewModel));
        }
        private readonly IExplorerPageViewModel ExplorerPageViewModel;
    }
}
