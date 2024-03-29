﻿/*  ScanHashFiles.cs

    ファイルを全スキャンする処理を実装するクラスです。
    ScanFiles だけを利用します。

 */
using CommunityToolkit.Mvvm.DependencyInjection;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.ControlDirectoryTree;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.ViewModels.PageSelectTarget
{
    public interface IScanHashFiles
    {
        public Task ScanFiles(CancellationToken cancellation);
    }

    public class ScanHashFiles : IScanHashFiles
    {
        #region コンストラクタと初期化
        private readonly ISearchConditionsManager _SearchManager;
        private readonly IExtentionManager _ExtentionManager;
        private readonly IPageSelectTargetViewModel _PageSelectTargetViewModel;
        private readonly ITreeManager _DirectoryTreeManager;
        public ScanHashFiles() { throw new NotImplementedException(); }
        public ScanHashFiles(
            ISearchConditionsManager searchManager,
            IExtentionManager extentionManager,
            IPageSelectTargetViewModel pageSelectTargetViewModel,
            ITreeManager directoryTreeManager)
        {
            _SearchManager = searchManager;
            _ExtentionManager = extentionManager;
            _PageSelectTargetViewModel = pageSelectTargetViewModel;
            _DirectoryTreeManager = directoryTreeManager;
        }

        private readonly List<string> _oldDirectoriesList = [];
        #endregion コンストラクタと初期化

        #region メイン処理
        /// <summary>
        /// スキャンするファイルを検出します。
        /// </summary>
        public async Task ScanFiles(CancellationToken cancellation)
        {
            // クリアしないとキャンセルから戻ってきた時、ファイル数がおかしくなる
            _oldDirectoriesList.Clear();
            _oldDirectoriesList.AddRange(_directoriesList);
            _directoriesList.Clear();
            try
            {
                /*
                var sw = new Stopwatch();
                sw.Start();
                */
                // ディレクトリのスキャン
                _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                await DirectoriesScan(cancellation);

                // ファイルのスキャン
                _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => DirectoryFilesScan(cancellation), cancellation);
                /*
                sw.Stop();
                DebugManager.InfoWrite($"GetFileSecurity Version : {sw.ElapsedMilliseconds}ms.", true);
                */
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _PageSelectTargetViewModel.ClearExtentions();
            ScanExtention(cancellation);

            // スキャン終了の表示に切り替える
            _PageSelectTargetViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }
        #endregion メイン処理

        #region ハッシュを取得するファイルのスキャン処理
        /// <summary>
        /// ハッシュを取得するディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private async Task DirectoryFilesScan(CancellationToken cancellation)
        {
            ISearchFileManager searchFileManager = Ioc.Default.GetService<ISearchFileManager>() ?? throw new InvalidOperationException($"{nameof(IExtentionManager)} dependency not resolved.");
            var semaphore = new SemaphoreSlim(5);

            int fileCount = 0;
            foreach (var directoryFullPath in _directoriesList)
            {
                try
                {
                    await semaphore.WaitAsync(cancellation);
                    // ファイルを保持する
                    fileCount = 0;
                    foreach (var fileFullPath in FileManager.EnumerateFiles(directoryFullPath))
                    {
                        searchFileManager.AddFile(fileFullPath);
                        fileCount++;
                    }

                    _PageSelectTargetViewModel.AddFilesScannedDirectoriesCount();
                    _PageSelectTargetViewModel.AddAllTargetFiles(fileCount);

                    if (cancellation.IsCancellationRequested) { return; }
                }
                finally { semaphore.Release(); }
            }
            var removedDirectory = _oldDirectoriesList.Except(_directoriesList).ToList();
            foreach (var directoryFullPath in removedDirectory)
            {
                searchFileManager.RemoveDirectory(directoryFullPath);
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private void ScanExtention(CancellationToken cancellation)
        {
            foreach (var extention in _ExtentionManager.GetExtentions())
            {
                _PageSelectTargetViewModel.AddExtentions(extention);
                if (cancellation.IsCancellationRequested) { return; }
            }
            // 拡張子を集める処理
            _PageSelectTargetViewModel.AddFileTypes();
        }
        #endregion ハッシュを取得するファイルのスキャン処理

        #region ディレクトリを検索する
        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        /// <param name="cancellation">キャンセリングトークン</param>
        private async Task DirectoriesScan(CancellationToken cancellation)
        {
            // 各ドライブに対してタスクを回す
            await DirectorySearch(_DirectoryTreeManager.NestedDirectories, cancellation);
            _PageSelectTargetViewModel.AddScannedDirectoriesCount(_DirectoryTreeManager.NonNestedDirectories.Count);
        }

        /// <summary>
        /// 全ディレクトリのリスト
        /// </summary>
        private readonly List<string> _directoriesList = [];

        /// <summary>
        /// ディレクトリを全て検索します
        /// </summary>
        /// <param name="rootDirectories">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        private async Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellation)
        {
            var directoriesList = new List<string>();

            await Task.Run(() =>
            {
                foreach (var rootDirectory in rootDirectories)
                {
                    _directoriesList.AddRange(GetDirectories(rootDirectory, cancellation));
                    if (cancellation.IsCancellationRequested) { return; }
                }
            }, cancellation);
        }

        /// <summary>
        /// ディレクトリ内部をを検索して取得します。
        /// </summary>
        /// <param name="rootDirectory">検索するディレクトリのルート</param>
        /// <param name="cancellation">キャンセリングトークン</param>
        /// <returns></returns>
        private List<string> GetDirectories(string rootDirectory, CancellationToken cancellation)
        {
            List<string> result = [];
            Stack<string> paths = [];

            paths.Push(rootDirectory);
            while (paths.Count > 0)
            {
                string currentDirectory = paths.Pop();
                result.Add(currentDirectory);
                _PageSelectTargetViewModel.AddScannedDirectoriesCount();

                foreach (var subDir in FileManager.EnumerateDirectories(currentDirectory))
                {
                    paths.Push(subDir);
                    if (cancellation.IsCancellationRequested) { return []; }
                }
            }
            return result;
        }
        #endregion ディレクトリを検索する
    }
}
