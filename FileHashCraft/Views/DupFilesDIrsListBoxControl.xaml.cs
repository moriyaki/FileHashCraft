using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.DuplicateSelectPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DupFilesDIrsListBoxControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DupFilesDirsListBoxControl : UserControl
    {
        public DupFilesDirsListBoxControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDupFilesDirsListBoxControlViewModel>();
        }
    }
}
