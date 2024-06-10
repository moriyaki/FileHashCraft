using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface IPageSelectTargetViewModelExtention
    {
        /// <summary>
        /// ファイルの拡張子グループによる絞り込みチェックボックスを持つリストボックス
        /// </summary>
        ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; }

        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        ObservableCollection<ExtensionCheckBoxViewModel> ExtentionCollection { get; set; }
        /// <summary>
        /// ファイルの拡張子グループをリストボックスに追加します。
        /// </summary>
        void AddFileTypes();
        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        void AddExtentions(string extention);
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        void ClearExtentions();
        /// <summary>
        /// スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        void ExtentionCountChanged();

        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention);
        /// <summary>
        /// 全管理対象ファイルを追加します。
        /// </summary>
        void AddFileToAllFiles(string fileFullPath);
        /// <summary>
        /// 拡張子チェックボックスにチェックされた時に拡張子グループに反映します。
        /// </summary>
        void CheckExtentionReflectToGroup(string extention);
        /// <summary>
        /// 拡張子チェックボックスのチェックが解除された時に拡張子グループに反映します。
        /// </summary>
        void UncheckExtentionReflectToGroup(string extention);
    }
    #endregion インターフェース

    public class PageSelectTargetViewModelExtention : ObservableObject, IPageSelectTargetViewModelExtention
    {
        #region バインディング
        /// <summary>
        /// ファイルの拡張子グループによる絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtentionGroupCheckBoxViewModel> ExtentionsGroupCollection { get; set; } = [];
        /// <summary>
        /// 拡張子による絞り込みチェックボックスを持つリストボックス
        /// </summary>
        public ObservableCollection<ExtensionCheckBoxViewModel> ExtentionCollection { get; set; } = [];
        /// <summary>
        /// ファイル拡張子のチェックボックスが選択された時の拡張子グループチェックボックス処理をします
        /// </summary>
        public RelayCommand<object> ExtentionGroupCheckBoxClickedCommand { get; set; }
        /// <summary>
        /// ファイル拡張子のチェックボックスが選択解除された時のグループチェックボックス処理をします
        /// </summary>
        public RelayCommand<object> ExtentionCheckBoxClickedCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IMessageServices _messageServices;
        private readonly ISettingsService _settingssService;
        private readonly IExtentionManager _extentionManager;
        private readonly IPageSelectTargetViewModelMain _pageSelectTargetViewModelMain;

        public PageSelectTargetViewModelExtention()
        {
            throw new NotImplementedException();
        }

        public PageSelectTargetViewModelExtention(
            IScannedFilesManager scannedFilesManager,
            IMessageServices messageService,
            ISettingsService settingsService,
            IExtentionManager extentionManager,
            IPageSelectTargetViewModelMain pageSelectTargetViewModelMain
        )
        {
            _scannedFilesManager = scannedFilesManager;
            _messageServices = messageService;
            _settingssService = settingsService;
            _extentionManager = extentionManager;
            _pageSelectTargetViewModelMain = pageSelectTargetViewModelMain;

            // 拡張子グループのチェックボックスのチェック状態が変更されたときのイベント
            ExtentionGroupCheckBoxClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is ExtentionGroupCheckBoxViewModel checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.Invoke(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });
            // 拡張子のチェックボックスのチェック状態が変更されたときのイベント
            ExtentionCheckBoxClickedCommand = new RelayCommand<object>((parameter) =>
            {
                if (parameter is ExtensionOrTypeCheckBoxBase checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.Invoke(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });

            // 拡張子がチェックされたらグループも変更する
            WeakReferenceMessenger.Default.Register<ExtentionChechReflectToGroup>(this, (_, m) =>
            {
                CheckExtentionReflectToGroup(m.Name);
                ExtentionCountChanged();
            });

            // 拡張子がチェック解除されたらグループも変更する
            WeakReferenceMessenger.Default.Register<ExtentionUnchechReflectToGroup>(this, (_, m) =>
            {
                UncheckExtentionReflectToGroup(m.Name);
                ExtentionCountChanged();
            });

            // 拡張子グループのチェック変更に連動して拡張子チェックボックス変更
            WeakReferenceMessenger.Default.Register<ExtentionGroupChecked>(this, (_, m) =>
                ChangeCheckBoxGroup(m.IsChecked, m.ExtentionCollection));
        }
        #endregion コンストラクタ

        #region 拡張子絞り込みの処理
        /// <summary>
        /// ファイルの拡張子グループをリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            movies.Initialize(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            pictures.Initialize(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            musics.Initialize(FileGroupType.Musics);
            var documents = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            documents.Initialize(FileGroupType.Documents);
            var applications = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            applications.Initialize(FileGroupType.Applications);
            var archives = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            archives.Initialize(FileGroupType.Archives);
            var sources = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            sources.Initialize(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            registrations.Initialize(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
            others.Initialize();

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
                var item = new ExtensionCheckBoxViewModel(_messageServices, _settingssService, _extentionManager);
                item.Initialize(extention);
                App.Current?.Dispatcher.Invoke(() => ExtentionCollection.Add(item));
            }
        }
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        public void ClearExtentions()
        {
            App.Current?.Dispatcher?.Invoke(() => ExtentionCollection.Clear());
            _extentionManager.ClearExtentions();
        }

        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        /// <param name="changedCheck">チェックされたか外されたか</param>
        /// <param name="extensionCollection">拡張子のリストコレクション</param>
        public void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extensionCollection)
        {
            // 拡張子のリストをハッシュセットに変換して、検索を高速化
            var extentionSet = new HashSet<string>(extensionCollection);

            // 更新対象の拡張子をまとめて変更
            var extensionsToUpdate = ExtentionCollection.Where(e => extentionSet.Contains(e.Name)).ToList();

            // UIスレッドでの更新を一回のInvokeでまとめて行う
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var extension in extensionsToUpdate)
                {
                    extension.IsCheckedForce = changedCheck;
                }
            });

            // 拡張子の追加・削除処理
            foreach (var extension in extensionCollection)
            {
                if (changedCheck)
                {
                    FileSearchCriteriaManager.AddCriteriaExtention(extension);
                }
                else
                {
                    FileSearchCriteriaManager.RemoveCriteriaExtention(extension);
                }
            }

            foreach (var extension in extensionsToUpdate)
            {
                _pageSelectTargetViewModelMain.ChangeExtensionToListBox(extension.Name, changedCheck);
            }

            // 拡張子数の変更通知
            ExtentionCountChanged();
        }

        /// <summary>
        /// スキャンするファイル数が増減した時の処理をします。
        /// </summary>
        public void ExtentionCountChanged()
        {
            App.Current.Dispatcher.Invoke(() =>
            _pageSelectTargetViewModelMain.CountFilteredGetHash = _scannedFilesManager.GetAllCriteriaFilesCount(_settingssService.IsHiddenFileInclude, _settingssService.IsReadOnlyFileInclude));
        }

        /// <summary>
        /// 全管理対象ファイルを追加します。
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        public void AddFileToAllFiles(string fileFullPath)
        {
            // 全管理対象ファイルをModelに追加する
            _scannedFilesManager.AddFile(fileFullPath);
            // 拡張子ヘルパーに拡張子を登録する(カウントもする)
            _extentionManager.AddFile(fileFullPath);
        }
        #endregion 拡張子絞り込みの処理

        #region 拡張子チェックボックスの管理
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
            if (group.IsChecked == false)
            {
                App.Current.Dispatcher.Invoke(() => group.IsCheckedForce = null);
            }

            // グループの拡張子が全てチェックされているか調べる
            foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
            {
                var item = ExtentionCollection.FirstOrDefault(i => i.Name == groupExtention);
                // 1つでもチェックされてない拡張子があればそのまま戻る
                if (item != null && item.IsChecked == false) { return; }
            }
            // 全てチェックされていたらチェック状態を null → true
            App.Current.Dispatcher.Invoke(() => group.IsCheckedForce = true);
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
            if (group.IsChecked == true)
            {
                App.Current.Dispatcher.Invoke(() => group.IsCheckedForce = null);
            }

            // グループの拡張子が全てチェック解除されているか調べる
            foreach (var groupExtention in _extentionManager.GetGroupExtentions(fileGroupType))
            {
                var item = ExtentionCollection.FirstOrDefault(i => i.Name == groupExtention);
                // 1つでもチェックされてない拡張子があればそのまま戻る
                if (item != null && item.IsChecked == true) { return; }
            }
            // 全てチェック解除されていたらチェック状態を null → false
            App.Current.Dispatcher.Invoke(() => group.IsCheckedForce = false);
        }
        #endregion 拡張子チェックボックスの管理
    }
}
