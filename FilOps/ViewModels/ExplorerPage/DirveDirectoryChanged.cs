using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;


namespace FilOps.ViewModels.ExplorerPage
{
    public partial class ExplorerPageViewModel
    {

        private void DirectoryChanged(object? sender, CurrentDirectoryFileChangedEventArgs e)
        {
            Debug.WriteLine($"Directory Changed : {e.FullPath}");
        }
    }
}
