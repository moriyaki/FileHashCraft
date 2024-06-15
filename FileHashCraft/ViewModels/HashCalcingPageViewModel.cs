using FileHashCraft.Services;

namespace FileHashCraft.ViewModels
{
    public interface IPageHashCalcingViewModel;
    public class HashCalcingPageViewModel : BaseViewModel, IPageHashCalcingViewModel
    {
        public HashCalcingPageViewModel() : base() { }
        public HashCalcingPageViewModel(ISettingsService settingsService) : base(settingsService) { }
    }
}
