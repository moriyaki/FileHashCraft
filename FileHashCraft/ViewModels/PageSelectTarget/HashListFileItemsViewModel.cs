/*  HashListFileItemsViewModel.cs

    ハッシュ対象を表示するリストボックスのアイテム ViewModel です。
*/
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public class HashListFileItems : ObservableObject
    {
        #region バインディング
        private string _FullPathFileName = string.Empty;
        public string FullPathFileName
        {
            get => _FullPathFileName;
            set => SetProperty(ref _FullPathFileName, value);
        }

        public string FileName
        {
            get => Path.GetFileName(_FullPathFileName);
        }

        private bool _IsHashTarget = false;
        public bool IsHashTarget
        {
            get => IsHashTarget;
            set => SetProperty(ref _IsHashTarget, value);
        }

        /// <summary>
        /// 背景色：ハッシュ検索対象になっていたら変更される
        /// </summary>
        private SolidColorBrush _HashTargetColor = new(Colors.Pink);
        public SolidColorBrush HashTargetColor
        {
            get => _HashTargetColor;
            set => SetProperty(ref _HashTargetColor, value);
        }
        #endregion バインディング

        #region コンストラクタ
        public HashListFileItems()
        {
        }
        #endregion コンストラクタ
    }
 }
