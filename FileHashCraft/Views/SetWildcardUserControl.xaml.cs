using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// SetWildcardUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SetWildcardUserControl : UserControl
    {
        public SetWildcardUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISetWildcardControlViewModel>();

            // ワイルドカード検索条件一覧のテキストボックスにフォーカスを当てる
            var _Messanger = Ioc.Default.GetService<IMessenger>() ?? throw new NullReferenceException(nameof(IMessenger));
            _Messanger.Register<ListBoxSeletedWildcardTextBoxFocusMessage>(this, (_, _) =>
            {
                if (WildcardSearchListBox.SelectedIndex == -1) { return; }
                if (WildcardSearchListBox.ItemContainerGenerator.ContainerFromIndex(
                    WildcardSearchListBox.SelectedIndex) is ListBoxItem firstSelectedItem)
                {
                    var textBox = FindVisualChild<TextBox>(firstSelectedItem);
                    textBox?.Focus();
                }
            });

            _Messanger.Register<NewWildcardCriteriaFocusMessage>(this, (_, _) =>
                NewWildcardCriteria.Focus());
        }

        /// <summary>
        /// 親オブジェクトに属する T 型のオブジェクトを取得する
        /// </summary>
        /// <typeparam name="T">取得するオブジェクト型</typeparam>
        /// <param name="parent">親オブジェクト</param>
        /// <returns>子オブジェクト</returns>
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
        /// ワイルドカードの新規作成でEnterが押されたら、リストボックスに追加します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Input.KeyEventArgs</param>
        private void NewWildcardCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _pageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
                var _selectTargetPageViewModel = Ioc.Default.GetService<ISelectTargetPageViewModel>() ?? throw new NullReferenceException(nameof(ISelectTargetPageViewModel));
                if (_pageSelectTargetViewModelWildcard.SearchErrorStatus == WildcardSearchErrorStatus.None)
                {
                    if (_selectTargetPageViewModel.Status == FileScanStatus.Finished)
                    {
                        _pageSelectTargetViewModelWildcard.AddCriteria();
                    }
                }
            }
        }

        /// <summary>
        /// リストボックスのアイテムが無い場所をクリックされたら、編集テキストボックスを閲覧モードにします。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void ListBoxWildcardCriteria_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox wildcardSearchListBox && e.OriginalSource is ScrollViewer)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
                if (_PageSelectTargetViewModelWildcard.SelectedItems.Count > 0)
                {
                    foreach (var item in _PageSelectTargetViewModelWildcard.CriteriaItems)
                    {
                        item.IsEditMode = false;
                        item.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// 検索条件一覧リストボックスでF2キーが推されたら、編集モードに移行します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItemWildcardCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
                if (_PageSelectTargetViewModelWildcard.SelectedItems.Count > 0)
                {
                    _PageSelectTargetViewModelWildcard.SelectedItems[0].IsEditMode = true;
                }
            }
        }

        /// <summary>
        /// 検索条件リストボックスの編集でEnterが押されたら、リストボックスに追加します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxWildcardCriterias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
                _PageSelectTargetViewModelWildcard.LeaveListBoxCriteria();
            }
            if (e.Key == Key.Escape)
            {
                var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
                _PageSelectTargetViewModelWildcard.LeaveListBoxCriteriaForce();
            }
        }

        /// <summary>
        /// ワイルドカード編集のテキストボックスがフォーカスを失ったら閲覧モードにします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.RoutedEventArgs</param>
        /// <exception cref="NullReferenceException"></exception>
        private void ListBoxWildcardCriterias_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var _PageSelectTargetViewModelWildcard = Ioc.Default.GetService<ISetWildcardControlViewModel>() ?? throw new NullReferenceException(nameof(ISetWildcardControlViewModel));
            if (_PageSelectTargetViewModelWildcard.SelectedItems.Count > 0)
            {
                _PageSelectTargetViewModelWildcard.SelectedItems[0].IsEditMode = false;
            }
        }

        private void ListBoxWildcardCriterias_LostFocus(object sender, MouseButtonEventArgs e)
        {
            NewWildcardCriteria.Focus();
        }
    }
}