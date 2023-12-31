using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DebugWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDebugWindowViewModel>();
        }

        private static readonly object lockObject = new();
        private static DebugWindow? instance;
        public static DebugWindow GetInstance()
        {
            lock (lockObject)
            {
                instance ??= new DebugWindow();
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
