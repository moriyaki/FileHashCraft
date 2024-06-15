using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.Views
{
    /// <summary>
    /// SeteExtentionUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SeteExtentionUserControl : UserControl
    {
        public SeteExtentionUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISetExtentionControlViewModel>();
        }
    }
}
