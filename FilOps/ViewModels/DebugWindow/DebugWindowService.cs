using System.Windows;

namespace FilOps.ViewModels.DebugWindow
{
    public interface IDebugWindowService
    {
        public void ShowDebugWindow();
        public void CloseDebugWindow();
    }

    public class DebugWindowService : IDebugWindowService
    {
        private readonly Window _debugWindow;

        public DebugWindowService()
        {
            throw new NotImplementedException();
        }

        public DebugWindowService(Window debugWindow)
        {
            _debugWindow = debugWindow;
            
        }
        public void ShowDebugWindow()
        {
            _debugWindow.Show();
        }

        public void CloseDebugWindow()
        {
            _debugWindow.Close();
        }
    }
}

