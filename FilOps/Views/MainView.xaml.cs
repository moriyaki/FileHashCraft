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
            _mainVM = new MainViewModel();
            DataContext = _mainVM;
        }

        private readonly MainViewModel _mainVM;

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                if (_mainVM != null)
                {
                    if (e.Delta > 0)
                    {
                        _mainVM.FontSize += 1;
                    }
                    else
                    {
                        _mainVM.FontSize -= 1;
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