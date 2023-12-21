using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels
{
    public interface IMainViewModel 
    {

    }

    public class MainViewModel : ObservableObject, IMainViewModel
    {
        public MainViewModel() 
        {
          
        }
    }
}
