using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FilOps.Views;

namespace FilOps.ViewModels.DebugWindow
{
    public interface IWindowService
    {
        public void ShowDebugWindow();
        public void CloseDebugWindow();
    }

    public class WindowService : IWindowService
    {

        public void ShowDebugWindow()
        {
            DebugWindow.ShowWindow();
        }

        public void CloseDebugWindow()
        {
            DebugWindow.CloseWindow();
        }
    }
}

