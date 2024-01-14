using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.Properties;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public class ExtentionCheckBoxViewModel : ObservableObject
    {
        private readonly IPageSelectTargetFileViewModel _pageSelectTargetFileViewModel;
        private readonly IScanHashFilesClass _scanHashFilesClass;

        public ExtentionCheckBoxViewModel(string extention, FileHashAlgorithm fileHashAlgorithm)
        {
            _pageSelectTargetFileViewModel = Ioc.Default.GetService<IPageSelectTargetFileViewModel>() ?? throw new InvalidOperationException($"{nameof(IPageSelectTargetFileViewModel)} dependency not resolved.");
            _scanHashFilesClass = Ioc.Default.GetService<IScanHashFilesClass>() ?? throw new InvalidOperationException($"{nameof(IScanHashFilesClass)} dependency not resolved.");
            Extention = extention;
            CurrentHashAlgorithm = fileHashAlgorithm;
        }

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

        /// <summary>
        /// 拡張子を持つファイル数
        /// </summary>
        private int _extentionCount = 0;

        /// <summary>
        /// 現在のハッシュアルゴリズム
        /// </summary>
        private FileHashAlgorithm _CurrentHashAlgorithm = FileHashAlgorithm.SHA256;
        public FileHashAlgorithm CurrentHashAlgorithm
        {
            get => _CurrentHashAlgorithm;
            private set
            {
                if (_CurrentHashAlgorithm != value)
                {
                    _CurrentHashAlgorithm = value;
                }
            }
        }

        /// <summary>
        /// 拡張子
        /// </summary>
        private string _Extention = string.Empty;
        public string Extention
        {
            get => _Extention;
            set
            {
                SetProperty(ref _Extention, value);
                _extentionCount = FileHashInfoManager.FileHashInstance.GetExtentionsCount(value, CurrentHashAlgorithm);
            }
        }

        /// <summary>
        /// チェックボックスのチェック状態変更
        /// </summary>
        private bool _IsChecked = false;
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (value)
                {
                    _pageSelectTargetFileViewModel.ChangeExtentionCount(_extentionCount);
                }
                else
                {
                    _pageSelectTargetFileViewModel.ChangeExtentionCount(-_extentionCount);
                }
                SetProperty(ref _IsChecked, value);
            }
        }
    }
}