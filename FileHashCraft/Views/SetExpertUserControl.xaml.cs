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
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.Views
{
    /// <summary>
    /// SetExpertUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SetExpertUserControl : UserControl
    {
        public SetExpertUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISetExpertControlViewModel>();
        }
    }
}
