using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FilOps.ViewModels
{
    public interface ISettingsPageViewModel
    {

    }

    /// <summary>
    /// 言語の選択用クラス
    /// </summary>
    public class Language
    {
        public string Lang { get; }
        public string Name { get; }

        public Language() { throw new NotImplementedException(); }

        public Language(string lang, string name)
        {
            Lang = lang;
            Name = name;
        }
    }

    public class SettingsPageViewModel : ObservableObject, ISettingsPageViewModel
    {
        /// <summary>
        ///  選択できる言語
        /// </summary>
        public ObservableCollection<Language> Languages { get; private set; }

        /// <summary>
        /// 選択されている言語
        /// </summary>
        private string _SelectedLanguage = string.Empty;
        public string SelectedLanguage
        {
            get => _SelectedLanguage;
            set
            {
                ResourceService.Current.ChangeCulture(value);
                OnPropertyChanged("Resources");
                SetProperty(ref _SelectedLanguage, value);
            }
        }

        /// <summary>
        /// エクスプローラー風画面にページに移動
        /// </summary>
        public DelegateCommand ToExplorer { get; set; }

        public SettingsPageViewModel()
        {
            Languages =
            [
                new Language("en-US", "English"),
                new Language("ja-JP", "日本語"),
            ];
            SelectedLanguage = CultureInfo.CurrentCulture.Name;

            ToExplorer = new DelegateCommand(() => { WeakReferenceMessenger.Default.Send(new ToExplorerPage()); });
        }
    }
}
