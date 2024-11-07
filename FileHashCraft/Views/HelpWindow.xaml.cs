using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels;

namespace FileHashCraft.Views
{
    /// <summary>
    /// HelpWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IHelpWindowViewModel>();
        }
    }
}