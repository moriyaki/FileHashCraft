using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.DependencyInjection;
using FilOps.ViewModels;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.Views
{
    /// <summary>
    /// SettingsPage.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISettingsPageViewModel>();
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
                var mainViewVM = Ioc.Default.GetService<IMainViewModel>();
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
