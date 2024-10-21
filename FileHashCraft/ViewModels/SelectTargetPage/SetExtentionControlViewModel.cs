using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    #region インターフェース
    public interface ISetExtentionControlViewModel
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
        void AddExtention(string extention);
        /// <summary>
        /// 拡張子のコレクションをクリアします。
        /// </summary>
        void ClearExtentions();
        /// <summary>
        /// 拡張子グループチェックボックスに連動して拡張子チェックボックスをチェックします。
        /// </summary>
        void ChangeCheckBoxGroup(bool changedCheck, IEnumerable<string> extentionCollention);
        /// <summary>
        /// 言語が変わった場合に備えて、拡張子グループを再設定します。
        /// </summary>
        public void RefreshExtentionLanguage();
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

    public class SetExtentionControlViewModel : BaseViewModel, ISetExtentionControlViewModel
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
        /// ファイル拡張子のチェックボックスラベルが選択された時の処理をします
        /// </summary>
        public RelayCommand<object> ExtentionGroupCheckBoxClickedCommand { get; set; }
        /// <summary>
        /// ファイル拡張子のチェックボックスラベルが選択解除された時の処理をします
        /// </summary>
        public RelayCommand<object> ExtentionCheckBoxClickedCommand { get; set; }
        #endregion バインディング

        #region コンストラクタ
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly IExtentionManager _extentionManager;
        private readonly IFileSearchCriteriaManager _fileSearchCriteriaManager;
        private readonly IShowTargetInfoUserControlViewModel _pageSelectTargetViewModelMain;

        public SetExtentionControlViewModel(
            IMessenger messenger,
            ISettingsService settingsService,
            IExtentionManager extentionManager,
            IScannedFilesManager scannedFilesManager,
            IFileSearchCriteriaManager fileSearchCriteriaManager,
            IShowTargetInfoUserControlViewModel pageSelectTargetViewModelMain
        ) : base(messenger, settingsService)
        {
            _scannedFilesManager = scannedFilesManager;
            _extentionManager = extentionManager;
            _fileSearchCriteriaManager = fileSearchCriteriaManager;
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
                if (parameter is BaseExtensionOrTypeCheckBox checkBoxViewModel)
                {
                    App.Current?.Dispatcher?.Invoke(() =>
                        checkBoxViewModel.IsChecked = !checkBoxViewModel.IsChecked);
                }
            });

            // 拡張子がチェックされたらグループも変更する
            _messenger.Register<ExtentionChechReflectToGroupMessage>(this, (_, m) =>
            {
                CheckExtentionReflectToGroup(m.Name);
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
            });

            // 拡張子がチェック解除されたらグループも変更する
            _messenger.Register<ExtentionUnchechReflectToGroupMessage>(this, (_, m) =>
            {
                UncheckExtentionReflectToGroup(m.Name);
                _pageSelectTargetViewModelMain.SetTargetCountChanged();
            });

            // 拡張子グループのチェック変更に連動して拡張子チェックボックス変更
            _messenger.Register<ExtentionGroupCheckedMessage>(this, (_, m) =>
                ChangeCheckBoxGroup(m.IsChecked, m.ExtentionCollection));
        }
        #endregion コンストラクタ

        #region 拡張子絞り込みの処理
        /// <summary>
        /// ファイルの拡張子グループをリストボックスに追加します。
        /// </summary>
        public void AddFileTypes()
        {
            var movies = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            movies.Initialize(FileGroupType.Movies);
            var pictures = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            pictures.Initialize(FileGroupType.Pictures);
            var musics = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            musics.Initialize(FileGroupType.Musics);
            var documents = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            documents.Initialize(FileGroupType.Documents);
            var archives = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            archives.Initialize(FileGroupType.Archives);
            var applications = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            applications.Initialize(FileGroupType.Applications);
            var sources = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            sources.Initialize(FileGroupType.SourceCodes);
            var registrations = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            registrations.Initialize(FileGroupType.Registrations);
            var others = new ExtentionGroupCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
            others.Initialize();

            App.Current?.Dispatcher.Invoke(() =>
            {
                // 「動画」「画像」「サウンド」「ドキュメント」「圧縮」ファイルは標準でチェックを付ける
                if (movies.ExtentionCount > 0) { ExtentionsGroupCollection.Add(movies); movies.IsChecked = true; }
                if (pictures.ExtentionCount > 0) { ExtentionsGroupCollection.Add(pictures); pictures.IsChecked = true; }
                if (musics.ExtentionCount > 0) { ExtentionsGroupCollection.Add(musics); musics.IsChecked = true; }
                if (documents.ExtentionCount > 0) { ExtentionsGroupCollection.Add(documents); documents.IsChecked = true; }
                if (archives.ExtentionCount > 0) { ExtentionsGroupCollection.Add(archives); archives.IsChecked = true; }
                if (applications.ExtentionCount > 0) { ExtentionsGroupCollection.Add(applications); }
                if (sources.ExtentionCount > 0) { ExtentionsGroupCollection.Add(sources); }
                if (registrations.ExtentionCount > 0) { ExtentionsGroupCollection.Add(registrations); }
                if (others.ExtentionCount > 0) { ExtentionsGroupCollection.Add(others); }
            });
        }
        /// <summary>
        /// 拡張子をリストボックスに追加します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void AddExtention(string extention)
        {
            var extentionManager = Ioc.Default.GetService<IExtentionManager>() ?? throw new NullReferenceException(nameof(IExtentionManager));
            if (extentionManager.GetExtentionsCount(extention) > 0)
            {
                var item = new ExtensionCheckBoxViewModel(_messenger, _settingsService, _extentionManager, _fileSearchCriteriaManager);
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
            var extensionsToUpdate = ExtentionCollection.Where(e => extentionSet.Contains(e.Name)).ToHashSet();

            // UIスレッドでの更新を一回のInvokeでまとめて行う
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var extension in extensionsToUpdate)
                {
                    extension.IsCheckedForce = changedCheck;
                }
            });

            // 拡張子条件の追加・削除処理
            foreach (var extension in extensionCollection)
            {
                if (changedCheck)
                {
                    _fileSearchCriteriaManager.AddCriteria(extension, FileSearchOption.Extention);
                }
                else
                {
                    _fileSearchCriteriaManager.RemoveCriteria(extension, FileSearchOption.Extention);
                }
            }

            // 拡張子数の変更通知
            _pageSelectTargetViewModelMain.ChangeSelectedToListBox();
            _pageSelectTargetViewModelMain.SetTargetCountChanged();
        }

        /// <summary>
        /// 言語が変わった場合に備えて、拡張子グループを再設定します。
        /// </summary>
        public void RefreshExtentionLanguage()
        {
            App.Current?.Dispatcher?.InvokeAsync(() => {
                ExtentionsGroupCollection.Clear();
                AddFileTypes();
            });
        }
        #endregion 拡張子絞り込みの処理

        #region 拡張子チェックボックスの管理
        /// <summary>
        /// 拡張子チェックボックスにチェックされた時に拡張子グループに反映します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        public void CheckExtentionReflectToGroup(string extention)
        {
            var fileGroupType = _extentionManager.GetFileGroupFromExtention(extention);
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
                // 1つでもチェックされてない拡張子があればnullのまま戻る
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
            var fileGroupType = _extentionManager.GetFileGroupFromExtention(extention);
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
                // 1つでもチェックされてない拡張子があればnullのまま戻る
                if (item != null && item.IsChecked == true) { return; }
            }
            // 全てチェック解除されていたらチェック状態を null → false
            App.Current.Dispatcher.Invoke(() => group.IsCheckedForce = false);
        }
        #endregion 拡張子チェックボックスの管理
    }
}
