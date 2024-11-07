using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.DuplicateSelectPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DupDirsFilesTreeViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DupDirsFilesTreeControl : UserControl
    {
        public DupDirsFilesTreeControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDupDirsFilesTreeViewModel>();
        }
    }
}