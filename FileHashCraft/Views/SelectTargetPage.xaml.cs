﻿using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Services;
using FileHashCraft.ViewModels.SelectTargetPage;

namespace FileHashCraft.Views
{
    /// <summary>
    /// PageTargetFileSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectTargetPage : Page
    {
        public SelectTargetPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<ISelectTargetPageViewModel>();
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
                var settingsService = Ioc.Default.GetService<ISettingsService>() ?? throw new NullReferenceException(nameof(ISettingsService));
                if (e.Delta > 0) { settingsService.FontSizePlus(); }
                else { settingsService.FontSizeMinus(); }
                e.Handled = true;
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        /// <summary>
        /// ツリービューのスプリッタが移動された時、TreeViewの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_TreeDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            HashTargetTreeView.Width += e.HorizontalChange;
        }

        /// <summary>
        /// リストボックスのスプリッタが移動された時、ListBoxの横幅を設定します。
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Controls.Primitives.DragDeltaEventArgs</param>
        private void GridSplitter_ListDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            FileListBox.Width += e.HorizontalChange;
        }
    }
}