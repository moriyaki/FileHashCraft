using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.ViewModels.DuplicateSelectPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// DuplicateSelectPage.xaml の相互作用ロジック
    /// </summary>
    public partial class DuplicateSelectPage : Page
    {
        public DuplicateSelectPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IDuplicateSelectPageViewModel>();
        }

        /// <summary>
        /// 同一ファイルのフォルダスプリッタが移動された時、TreeViewの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_DuplicateListBoxDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            /*
            var explorerTree = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new NullReferenceException(nameof(IControDirectoryTreeViewlModel));
            explorerTree.TreeWidth = DuplicateHasFolder.ActualWidth;
            */
        }
        /// <summary>
        /// リストボックスのスプリッタが移動された時、ListBoxの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_DuplicateTreeViewDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            /*
            var _PageSelectTargetViewModel = Ioc.Default.GetService<ISelectTargetPageViewModel>() ?? throw new NullReferenceException(nameof(ISelectTargetPageViewModel));
            _PageSelectTargetViewModel.ListWidth = DuplicateFolder.ActualWidth + e.HorizontalChange;
            */
        }
    }
}
