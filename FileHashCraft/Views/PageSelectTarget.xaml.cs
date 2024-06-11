using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.DirectoryTreeViewControl;
using FileHashCraft.ViewModels.PageSelectTarget;

namespace FileHashCraft.Views
{
    /// <summary>
    /// PageTargetFileSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSelectTarget : Page
    {
        public PageSelectTarget()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<IPageSelectTargetViewModel>();

            // ワイルドカード検索条件一覧のテキストボックスにフォーカスを当てる
            WeakReferenceMessenger.Default.Register<WildcardSeletedTextBoxFocus>(this, (_, _) =>
            {
                if (WildcardSearchListBox.SelectedIndex == -1) { return; }
                if (WildcardSearchListBox.ItemContainerGenerator.ContainerFromIndex(
                    WildcardSearchListBox.SelectedIndex) is ListBoxItem firstSelectedItem)
                {
                    var textBox = FindVisualChild<TextBox>(firstSelectedItem);
                    textBox?.Focus();
                }
            });

            WeakReferenceMessenger.Default.Register<WildcardCriteriaFocus>(this, (_, _) =>
                WildcardCriteria.Focus());
        }

        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T t)
                {
                    return t;
                }
                else
                {
                    if (child != null)
                    {
                        T? childOfChild = FindVisualChild<T>(child);
                        if (childOfChild != null)
                        {
                            return childOfChild;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Ctrl + マウスホイールで拡大縮小を行う
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
        /// ツリービューのスプリッタが移動された時、TreeViewの横幅を設定する
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_TreeDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var explorerTree = Ioc.Default.GetService<IControDirectoryTreeViewlModel>() ?? throw new NullReferenceException(nameof(IControDirectoryTreeViewlModel));
            explorerTree.TreeWidth = HashTargetTreeView.ActualWidth;
        }
        /// <summary>
        /// リストボックスのスプリッタが移動された時、ListBoxの横幅を設定する
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_ListDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var _PageSelectTargetViewModel = Ioc.Default.GetService<IPageSelectTargetViewModel>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModel));
            _PageSelectTargetViewModel.ListWidth = FileListBox.ActualWidth + e.HorizontalChange;
        }

        /// <summary>
        /// ワイルドカードの新規作成でEnterが押されたら、リストボックスに追加します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Input.KeyEventArgs</param>
        private void WildcardCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<IPageSelectTargetViewModelWildcard>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModelWildcard));
                if (_PageSelectTargetViewModelWildcard.WildcardSearchErrorStatus == WildcardSearchErrorStatus.None)
                {
                    _PageSelectTargetViewModelWildcard.AddWildcardCriteria();
                }
            }
        }

        /// <summary>
        /// リストボックスのアイテムが無い場所をクリックされたら、編集テキストボックスを閲覧モードにします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void WildcardSearchListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox wildcardSearchListBox && e.OriginalSource is ScrollViewer)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<IPageSelectTargetViewModelWildcard>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModelWildcard));
                if (_PageSelectTargetViewModelWildcard.SelectedItems.Count > 0)
                {
                    foreach (var item in _PageSelectTargetViewModelWildcard.WildcardItems)
                    {
                        item.IsEditMode = false;
                        item.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// ワイルドカード編集のテキストボックスがフォーカスを失ったら閲覧モードにします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.RoutedEventArgs</param>
        /// <exception cref="NullReferenceException"></exception>
        private void WildcardCollectionTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<IPageSelectTargetViewModelWildcard>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModelWildcard));
            if (_PageSelectTargetViewModelWildcard.SelectedItems.Count > 0)
            {
                _PageSelectTargetViewModelWildcard.SelectedItems[0].IsEditMode = false;
            }
        }

        /// <summary>
        /// ワイルドカード検索条件一覧の編集でEnterが押されたら、リストボックスに追加します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxTextWildcardCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<IPageSelectTargetViewModelWildcard>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModelWildcard));
                _PageSelectTargetViewModelWildcard.LeaveListBoxWildcardCriteria();
            }
        }
    }
}
