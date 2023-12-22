using System.Windows;
using FilOps.ViewModels.DebugWindow;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.Views
{
    /// <summary>
    /// DebugWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<IDebugWindowViewModel>();
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
