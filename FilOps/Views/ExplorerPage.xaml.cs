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
using FilOps.ViewModels;

namespace FilOps.Views
{
    /// <summary>
    /// ExplorerPage.xaml の相互作用ロジック
    /// </summary>
    public partial class ExplorerPage : Page
    {
        public ExplorerPage()
        {
            InitializeComponent();
            viewModel = new ExplorerPageViewModel();
            DataContext = viewModel;
        }

        private readonly ExplorerPageViewModel viewModel;
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                if (viewModel is not null)
                {
                    if (e.Delta > 0)
                    {
                        viewModel.FontSize += 1;
                    }
                    else
                    {
                        viewModel.FontSize -= 1;
                    }
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
