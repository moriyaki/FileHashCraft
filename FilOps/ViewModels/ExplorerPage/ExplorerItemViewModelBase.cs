using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels.ExplorerPage
{
    public class ExplorerItemViewModelBase : ObservableObject, IComparable<ExplorerItemViewModelBase>
    {
        public ExplorerItemViewModelBase() 
        {
            throw new NotImplementedException();
        }

        public ExplorerItemViewModelBase(IExplorerPageViewModel explorerVM)
        {
            ExplorerVM = explorerVM;
        }

        protected readonly IExplorerPageViewModel ExplorerVM;

        public ExplorerItemViewModelBase(IExplorerPageViewModel explorerPageVM, FileItemInformation f)
        {
            ExplorerVM = explorerPageVM;

            FullPath = f.FullPath;
            IsReady = f.IsReady;
            IsRemovable = f.IsRemovable;
            IsDirectory = f.IsDirectory;
            HasChildren = f.HasChildren;
        }

        /// <summary>
        /// ソートのための比較関数
        /// </summary>
        /// <param name="other">ExplorerListItemViewModel?</param>
        /// <returns><bool/returns>
        /// <exception cref="NotImplementedException"></exception>
        public int CompareTo(ExplorerItemViewModelBase? other)
        {
            return Name.CompareTo(other?.Name);
        }

        /// <summary>
        /// ファイルの表示名
        /// </summary>
        private string _Name = string.Empty;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        /// <summary>
        /// ファイル実体名
        /// </summary>
        public string FileName
        {
            get => Path.GetFileName(FullPath);
        }

        /// <summary>
        /// ファイルまたはフォルダのフルパス
        /// </summary>
        private string _FullPath = string.Empty;
        public string FullPath
        {
            get => _FullPath;
            set
            {
                SetProperty(ref _FullPath, value);
                Name = WindowsAPI.GetDisplayName(FullPath);

                App.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    Icon = WindowsAPI.GetIcon(FullPath);
                    FileType = WindowsAPI.GetType(FullPath);
                }));
            }
        }

        /// <summary>
        /// ファイルのアイコン
        /// </summary>
        private BitmapSource? _Icon = null;
        public BitmapSource? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }

        /// <summary>
        /// ファイルの種類
        /// </summary>
        private string _FileType = string.Empty;
        public string FileType
        {
            get => _FileType;
            set => SetProperty(ref _FileType, value);
        }


        /// <summary>
        /// ディレクトリがディレクトリを持つかどうか
        /// </summary>
        protected bool _HasChildren = false;
        public virtual bool HasChildren
        {
            get => _HasChildren;
            set => SetProperty(ref _HasChildren, value);
        }

        /// <summary>
        /// ディレクトリのドライブが準備されているかどうか
        /// </summary>
        protected bool _IsReady = false;
        public virtual bool IsReady
        {
            get => _IsReady;
            set => SetProperty(ref _IsReady, value);
        }

        /// <summary>
        /// ディレクトリのドライブが着脱可能か
        /// </summary>
        private bool _IsRemovable = false;
        public bool IsRemovable
        {
            get => _IsRemovable;
            set => SetProperty(ref _IsRemovable, value);
        }

        /// <summary>
        /// ディレクトリかどうか
        /// </summary>
        private bool _IsDirectory = false;
        public bool IsDirectory
        {
            get => _IsDirectory;
            set => SetProperty(ref _IsDirectory, value);
        }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public static double FontSize
        {
            get
            {
                var explorerVM = App.Current.Services.GetService<IExplorerPageViewModel>();
                return explorerVM?.FontSize ?? SystemFonts.MessageFontSize;
            }
        }
    }
}
