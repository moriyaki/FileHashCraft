using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// SeteExtentionUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SetExtentionUserControl : UserControl
    {
        public SetExtentionUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISetExtentionControlViewModel>();
        }
    }
}