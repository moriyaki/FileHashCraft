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
    /// SetRegexUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SetRegexUserControl : UserControl
    {
        public SetRegexUserControl()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISetRegexControlViewModel>();

            // ワイルドカード検索条件一覧のテキストボックスにフォーカスを当てる
            var _messenger = Ioc.Default.GetService<IMessenger>() ?? throw new NotImplementedException(nameof(IMessenger));
            _messenger.Register<ListBoxSeletedRegexTextBoxFocusMessage>(this, (_, _) =>
            {
                if (RegexSearchListBox.SelectedIndex == -1) { return; }
                if (RegexSearchListBox.ItemContainerGenerator.ContainerFromIndex(
                    RegexSearchListBox.SelectedIndex) is ListBoxItem firstSelectedItem)
                {
                    var textBox = FindVisualChild<TextBox>(firstSelectedItem);
                    textBox?.Focus();
                }
            });

            _messenger.Register<NewRegexCriteriaFocusMessage>(this, (_, _) =>
                NewRegexCriteria.Focus());
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
        private void NewRegexCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
                var _selectTargetPageViewModel = Ioc.Default.GetService<ISelectTargetPageViewModel>() ?? throw new NullReferenceException(nameof(ISelectTargetPageViewModel));
                if (_SetRegexControlViewModel.SearchErrorStatus == RegexSearchErrorStatus.None)
                {
                    if (_selectTargetPageViewModel.Status == FileScanStatus.Finished)
                    {
                        _SetRegexControlViewModel.AddCriteria();
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
        private void ListBoxRegexCriteria_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox RegexSearchListBox && e.OriginalSource is ScrollViewer)
            {
                var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
                if (_SetRegexControlViewModel.SelectedItems.Count > 0)
                {
                    foreach (var item in _SetRegexControlViewModel.CriteriaItems)
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
        private void ListBoxItemRegexCriteria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
                if (_SetRegexControlViewModel.SelectedItems.Count > 0)
                {
                    _SetRegexControlViewModel.SelectedItems[0].IsEditMode = true;
                }
            }
        }

        /// <summary>
        /// 検索条件リストボックスの編集でEnterが押されたら、リストボックスに追加します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxRegexCriterias_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
                _SetRegexControlViewModel.LeaveListBoxCriteria();
            }
            if (e.Key == Key.Escape)
            {
                var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
                _SetRegexControlViewModel.LeaveListBoxCriteriaForce();
            }
        }

        /// <summary>
        /// ワイルドカード編集のテキストボックスがフォーカスを失ったら閲覧モードにします。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.RoutedEventArgs</param>
        /// <exception cref="NullReferenceException"></exception>
        private void ListBoxRegexCriterias_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var _SetRegexControlViewModel = Ioc.Default.GetService<ISetRegexControlViewModel>() ?? throw new NullReferenceException(nameof(ISetRegexControlViewModel));
            if (_SetRegexControlViewModel.SelectedItems.Count > 0)
            {
                _SetRegexControlViewModel.SelectedItems[0].IsEditMode = false;
            }
        }

        private void ListBoxRegexCriterias_LostFocus(object sender, MouseButtonEventArgs e)
        {
            NewRegexCriteria.Focus();
        }
    }
}
