using FileHashCraft.Services;

namespace FileHashCraft.ViewModels
{
    public interface IPageHashCalcingViewModel;
    public class PageHashCalcingViewModel : BaseViewModel, IPageHashCalcingViewModel
    {
        public PageHashCalcingViewModel() : base() { }
        public PageHashCalcingViewModel(ISettingsService settingsService) : base(settingsService) { }
    }
}
