using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.Views
{
    /// <summary>
    /// ShowTargetInfoUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ShowTargetInfoUserControl : UserControl
    {
        public ShowTargetInfoUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IShowTargetInfoUserControlViewModel>();
        }
    }
}
