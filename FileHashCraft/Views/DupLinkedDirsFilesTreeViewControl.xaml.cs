using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.DuplicateSelectPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DupLinkedDirsFilesTreeViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DupLinkedDirsFilesTreeViewControl : UserControl
    {
        public DupLinkedDirsFilesTreeViewControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDupLinkedDirsFilesTreeViewModel>();
        }
    }
}