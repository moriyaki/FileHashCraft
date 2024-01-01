using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels;

namespace FileHashCraft.Views
{
    /// <summary>
    /// PageTargetFileSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class PageTargetFileSetting : Page
    {
        public PageTargetFileSetting()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IPageTargetFileSelectViewModel>();
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
                if (mainViewVM is not null)
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
