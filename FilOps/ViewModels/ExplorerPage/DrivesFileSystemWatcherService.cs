using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using FilOps.Models;
using Microsoft.Extensions.DependencyInjection;
using static FilOps.ViewModels.ExplorerPage.DrivesFileSystemWatcherService;

namespace FilOps.ViewModels.ExplorerPage
{
    public interface IDrivesFileSystemWatcherService
    {
    }

    public class DrivesFileSystemWatcherService : IDrivesFileSystemWatcherService
    {
    }
}
