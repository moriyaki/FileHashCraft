using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.HashCalcingPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// PageHashCalcing.xaml の相互作用ロジック
    /// </summary>
    public partial class HashCalcingPage : Page
    {
        public HashCalcingPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IHashCalcingPageViewModel>();
        }

        /// <summary>
        /// Ctrl + マウスホイールで拡大縮小を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                var settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));
                {
                    if (e.Delta > 0) { settingsService.FontSizePlus(); }
                    else { settingsService.FontSizeMinus(); }
                    e.Handled = true;
                }
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
}