using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.SelectTargetPage;

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