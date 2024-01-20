using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.Views
{
    /// <summary>
    /// TargetFilterWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TargetFilterWindow : Window
    {
        public TargetFilterWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ITargetFilterWindowViewModel>();
        }

        private static readonly object lockObject = new();
        private static TargetFilterWindow? instance;
        public static TargetFilterWindow GetInstance()
        {
            lock (lockObject)
            {
                instance ??= new TargetFilterWindow();
                return instance;
            }
        }
        public static void ShowWindow()
        {
            GetInstance().Show();
        }

        public static void CloseWindow()
        {
            GetInstance().Close();
        }
    }
}
