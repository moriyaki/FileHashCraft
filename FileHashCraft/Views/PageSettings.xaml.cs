using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels;

namespace FileHashCraft.Views
{
    /// <summary>
    /// PageSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSettings : Page
    {
        public PageSettings()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IPageSettingsViewModel>();
        }

        /// <summary>
        /// Ctrl + マウスホイールで拡大縮小を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                var mainViewVM = Ioc.Default.GetService<IMainWindowViewModel>();
                if (mainViewVM != null)
                {
                    if (e.Delta > 0) { mainViewVM.FontSizePlus(); }
                    else { mainViewVM.FontSizeMinus(); }
                }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
}
