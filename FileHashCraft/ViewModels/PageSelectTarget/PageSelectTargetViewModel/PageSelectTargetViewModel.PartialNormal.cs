/*  PageSelectTargetViewModel.PartialNormal.cs

    通常向けの拡張子検索画面を提供するタブの ViewModel を提供します。
 */

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.Models.Helpers;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region ハッシュ計算するファイルの取得状況
    public enum FileScanStatus
    {
        None,
        DirectoriesScanning,
        FilesScanning,
        Finished,
    }
    #endregion ハッシュ計算するファイルの取得状況

    public partial class PageSelectTargetViewModel
    {
        #region バインディング

        /// <summary>
        /// 全ディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountScannedDirectories = 0;
        public int CountScannedDirectories
        {
            get => _CountScannedDirectories;
            set
            {
                SetProperty(ref _CountScannedDirectories, value);
                Status = FileScanStatus.DirectoriesScanning;
            }
        }

        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数(StatusBar用)
        /// </summary>
        private int _CountHashFilesDirectories = 0;
        public int CountHashFilesDirectories
        {
            get => _CountHashFilesDirectories;
            set
            {
                SetProperty(ref _CountHashFilesDirectories, value);
                Status = FileScanStatus.FilesScanning;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// ハッシュを取得する全ファイル数
        /// </summary>
        private int _CountAllTargetFilesGetHash = 0;
        public int CountAllTargetFilesGetHash
        {
            get => _CountAllTargetFilesGetHash;
            set
            {
                SetProperty(ref _CountAllTargetFilesGetHash, value);
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// 絞り込みをした時の、ハッシュを獲得するファイル数
        /// </summary>
        private int _CountFilteredGetHash = 0;
        public int CountFilteredGetHash
        {
            get => _CountFilteredGetHash;
            set
            {
                SetProperty(ref _CountFilteredGetHash, value);
                ToPageHashCalcing.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        /// ファイルの拡張子グループによる絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; } = [];

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtensionOrTypeCheckBoxBase> ExtentionCollection { get; set; } = [];

        #endregion バインディング

        #region ファイル数の管理処理
        /// <summary>
        /// 検索のステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public void ChangeHashScanStatus(FileScanStatus status)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => Status = status);
        }
        /// <summary>
        /// スキャンした全ディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountScannedDirectories += directoriesCount);
        }
        /// <summary>
        /// ファイルスキャンが完了したディレクトリ数に加算します。
        /// </summary>
        /// <param name="directoriesCount">加算する値、デフォルト値は1</param>
        public void AddFilesScannedDirectoriesCount(int directoriesCount = 1)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountHashFilesDirectories += directoriesCount);
        }
        /// <summary>
        /// ハッシュ取得対象となる全てのファイル数を設定します。
        /// </summary>
        public void AddAllTargetFiles(int targetFilesCount)
        {
            App.Current?.Dispatcher?.InvokeAsync(() => CountAllTargetFilesGetHash += targetFilesCount);
        }

        /// <summary>
        /// 拡張子チェックボックスにチェックされた時に拡張子グループに反映します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void CheckExtentionReflectToGroup(string extention)
        {
            var fileGroupType = ExtentionTypeHelper.GetFileGroupFromExtention(extention);
            var group = ExtentionsGroupCollection.FirstOrDefault(g => g.FileType == fileGroupType);
            if (group == null) return;

            // グループの拡張子に一つもチェックが入ってなければ null にする
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                if (group.IsChecked == false)
                {
                    group.IsCheckedForce = null;
                }

                // グループの拡張子が全てチェックされているか調べる
                foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
                {
                    var item = ExtentionCollection.FirstOrDefault(i => i.ExtentionOrGroup == groupExtention);
                    // 1つでもチェックされてない拡張子があればそのまま戻る
                    if (item != null && item.IsChecked == false) { return; }
                }
                // 全てチェックされていたらチェック状態を null → true
                group.IsCheckedForce = true;
            });
        }

        /// <summary>
        /// 拡張子チェックボックスのチェックが解除された時に拡張子グループに反映します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void UncheckExtentionReflectToGroup(string extention)
        {
            var fileGroupType = ExtentionTypeHelper.GetFileGroupFromExtention(extention);
            var group = ExtentionsGroupCollection.FirstOrDefault(g => g.FileType == fileGroupType);
            if (group == null) return;

            // グループの拡張子に全てチェックが入っていれば null にする
            App.Current?.Dispatcher?.InvokeAsync(() =>
            {
                if (group.IsChecked == true)
                {
                    group.IsCheckedForce = null;
                }

                // グループの拡張子が全てチェック解除されているか調べる
                foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
                {
                    var item = ExtentionCollection.FirstOrDefault(i => i.ExtentionOrGroup == groupExtention);
                    // 1つでもチェックされてない拡張子があればそのまま戻る
                    if (item != null && item.IsChecked == true) { return; }
                }
                // 全てチェック解除されていたらチェック状態を null → false
                group.IsCheckedForce = false;
            });
        }

        #endregion ファイル数の管理処理

        #region ファイル絞り込みの処理
        /// <summary>
        /// ファイルの拡張子グループをリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(FileGroupType.Musics);
            var documents = new ExtentionGroupCheckBoxViewModel(FileGroupType.Documents);
            var applications = new ExtentionGroupCheckBoxViewModel(FileGroupType.Applications);
            var archives = new ExtentionGroupCheckBoxViewModel(FileGroupType.Archives);
            var sources = new ExtentionGroupCheckBoxViewModel(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel();

            App.Current?.Dispatcher.Invoke(() =>
            {
                if (movies.ExtentionCount > 0) { ExtentionsGroupCollection.Add(movies); }
                if (pictures.ExtentionCount > 0) { ExtentionsGroupCollection.Add(pictures); }
                if (musics.ExtentionCount > 0) { ExtentionsGroupCollection.Add(musics); }
                if (documents.ExtentionCount > 0) { ExtentionsGroupCollection.Add(documents); }
                if (applications.ExtentionCount > 0) { ExtentionsGroupCollection.Add(applications); }
                if (archives.ExtentionCount > 0) { ExtentionsGroupCollection.Add(archives); }
                if (sources.ExtentionCount > 0) { ExtentionsGroupCollection.Add(sources); }
                if (registrations.ExtentionCount > 0) { ExtentionsGroupCollection.Add(registrations); }
                if (others.ExtentionCount > 0) { ExtentionsGroupCollection.Add(others); }
            });
        }

        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void AddExtentions(string extention)
        {
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new NullReferenceException(nameof(IExtentionManager));
            if (extentionManager.GetExtentionsCount(extention) > 0)
            {
                var item = new ExtensionCheckBox(extention);
                App.Current?.Dispatcher.Invoke(() => ExtentionCollection.Add(item));
            }
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => ExtentionCollection.Clear());
        }
        /// <summary>
        /// 拡張子チェックボックスにより、スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public void ExtentionCountChanged()
        {
            var pageSelectTargetViewModel = Ioc.Default.GetService<IPageSelectTargetViewModel>() ?? throw new NullReferenceException(nameof(IPageSelectTargetViewModel));
            App.Current?.Dispatcher?.InvokeAsync(() => CountFilteredGetHash = pageSelectTargetViewModel.AllConditionFiles.Count);
        }

        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        /// <param name="changedCheck">チェックされたか外されたか</param>
        /// <param name="extentionCollention">拡張子のリストコレクション</param>
        public async void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention)
        {
            var changedCollection = ExtentionCollection.Where(e => extentionCollention.Contains(e.ExtentionOrGroup));

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var extension in changedCollection)
                {
                    extension.IsChecked = changedCheck;
                }
            });
            DebugManager.InfoWrite("-----------------------------------------------------------------------------------------------------------");
            foreach (var item in AllConditionFiles)
            {
                DebugManager.InfoWrite($"AllConditionFiles : {item.FileFullPath}");
            }
            DebugManager.InfoWrite("-----------------------------------------------------------------------------------------------------------");
        }
        #endregion ファイル絞り込みの処理
    }
}
