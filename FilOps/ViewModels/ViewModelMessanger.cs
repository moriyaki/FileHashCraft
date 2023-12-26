using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilOps.ViewModels
{
    public class CurrentChangeMessage
    {
        public string CurrentFullPath { get; } = string.Empty;
        public CurrentChangeMessage() { }
        public CurrentChangeMessage(string currentFullPath)
        {
            CurrentFullPath = currentFullPath;
        }
    }

    public class FontChanged
    {
        public double FontSize { get; } = SystemFonts.MessageFontSize;

        public FontChanged() { }

        public FontChanged(double fontSize)
        {
            FontSize = fontSize;
        }
    }
}
