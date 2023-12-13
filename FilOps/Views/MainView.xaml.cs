using System.Windows;
using System.Windows.Input;
using FilOps.ViewModels;

namespace FilOps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private MainViewModel? _ViewModel;
        public MainViewModel? ViewModel
        {
            get => _ViewModel;
            set
            {
                DataContext = value;
                _ViewModel = value;
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                if (ViewModel != null)
                {
                    if (e.Delta > 0)
                    {
                        ViewModel.FontSize += 1;
                    }
                    else
                    {
                        ViewModel.FontSize -= 1;
                    }
                }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
}