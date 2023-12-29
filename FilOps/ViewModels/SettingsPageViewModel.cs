using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace FilOps.ViewModels
{
    public interface ISettingsPageViewModel
    {

    }
    public class SettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        public DelegateCommand ToExplorer { get; set; }
            
            
        public SettingsPageViewModel()
        {
            ToExplorer = new DelegateCommand(() => { WeakReferenceMessenger.Default.Send(new ToExplorerPage()); });
        }
        

    }
}
