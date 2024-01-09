using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public class ExtentionCheckBoxViewModel : ObservableObject
    {
        public ExtentionCheckBoxViewModel() { }
        public ExtentionCheckBoxViewModel(string extention, Models.FileHashAlgorithm hashAlgorithmType)
        {
            Extention = extention;
            _extentionCount = FileHashInfoManager.FileHashInstance.GetExtentionsCount(extention, hashAlgorithmType);
        }

        private readonly int _extentionCount;

        public string ExtentionView
        {
            get
            {
                if (string.IsNullOrEmpty(Extention))
                {
                    return $"{Resources.NoHaveExtentions} ({_extentionCount})";
                }
                return $"{_Extention} ({_extentionCount})";
            }
        }

        private string _Extention = string.Empty;
        public string Extention
        {
            get => _Extention;
            set => SetProperty(ref _Extention, value);
        }

        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value)
                {
                    WeakReferenceMessenger.Default.Send(new AddExtentionCount(_extentionCount));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new RemoveExtentionCount(_extentionCount));
                }
                SetProperty(ref _IsChecked, value);
            }
        }
    }
}