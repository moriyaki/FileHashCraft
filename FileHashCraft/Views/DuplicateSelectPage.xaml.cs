using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Services;
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
        /// Ctrl + マウスホイールで拡大縮小を行います。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                var settingsService = Ioc.Default.GetService<ISettingsService>();
                if (settingsService is not null)
                {
                    if (e.Delta > 0) { settingsService.FontSizePlus(); }
                    else { settingsService.FontSizeMinus(); }
                }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        /// <summary>
        /// 同一ファイルのフォルダスプリッタが移動された時、TreeViewの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_DupFilesDirsListBoxDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            /*
            var explorerTree = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new NullReferenceException(nameof(IControDirectoryTreeViewlModel));
            explorerTree.TreeWidth = DuplicateHasFolder.ActualWidth;
            */
            var _DuplicateSelectPageViewModel = Ioc.Default.GetService<IDuplicateSelectPageViewModel>() ?? throw new NullReferenceException(nameof(IDuplicateSelectPageViewModel));
        }
        /// <summary>
        /// リストボックスのスプリッタが移動された時、ListBoxの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_DupDirsFilesTreeViewDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            /*
            var _PageSelectTargetViewModel = Ioc.Default.GetService<ISelectTargetPageViewModel>() ?? throw new NullReferenceException(nameof(ISelectTargetPageViewModel));
            _PageSelectTargetViewModel.ListWidth = DuplicateFolder.ActualWidth + e.HorizontalChange;
            */
            var _DuplicateSelectPageViewModel = Ioc.Default.GetService<IDuplicateSelectPageViewModel>() ?? throw new NullReferenceException(nameof(IDuplicateSelectPageViewModel));
        }
    }
}
