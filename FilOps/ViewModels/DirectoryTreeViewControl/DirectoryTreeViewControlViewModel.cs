using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using FilOps.ViewModels.DebugWindow;
using FilOps.ViewModels.ExplorerPage;

namespace FilOps.ViewModels.DirectoryTreeViewControl
{
    public interface IDirectoryTreeViewControlViewModel
    {
        public void SetIsCheckBoxVisible(bool isVisible);
        public void AddRoot(FileItemInformation item);
    }

    public class DirectoryTreeViewControlViewModel : ObservableObject, IDirectoryTreeViewControlViewModel
    {
        #region バインディング
        /// <summary>
        /// TreeView にバインドするコレクション
        /// </summary>
        public ObservableCollection<DirectoryTreeViewModel> TreeRoot { get; set; } = [];

        /// <summary>
        /// チェックボックスの表示状態の設定
        /// </summary>
        private Visibility _IsCheckBoxVisible = Visibility.Visible;
        public Visibility IsCheckBoxVisible
        {
            get => _IsCheckBoxVisible;
            private set => _IsCheckBoxVisible = value;
        }
        #endregion バインディング
        private readonly IMainViewModel _MainWindowViewModel;
        private readonly IExpandedDirectoryManager _ExpandedDirectoryManager;

        public DirectoryTreeViewControlViewModel()
        {
            throw new NotImplementedException();
        }

        public DirectoryTreeViewControlViewModel(
            IExpandedDirectoryManager expandDirManager,
            IMainViewModel mainViewModel)
        {
            _ExpandedDirectoryManager = expandDirManager;
            _MainWindowViewModel = mainViewModel;
        }

        /// <summary>
        /// チェックボックスを表示するかどうかを設定します。
        /// </summary>
        /// <param name="isVisible">表示するかどうか</param>
        public void SetIsCheckBoxVisible(bool isVisible)
            => IsCheckBoxVisible = isVisible ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// ルートノードにアイテムを追加します。
        /// </summary>
        /// <param name="item">追加する FileItemInformation</param>
        public void AddRoot(FileItemInformation item)
        {
            var info = new DirectoryTreeViewModel(this, item);
            TreeRoot.Add(info);
        }

    }
}
