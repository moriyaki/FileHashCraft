using System.Diagnostics;
using System.Threading;
using System.Windows;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;
using static FileHashCraft.Models.FileHashManager;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType, CancellationToken cancellationToken);
        public void ScanExtention(FileHashAlgorithm hashAlgorithm);
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly IPageSelectTargetFileViewModel _PageSelectTargetFileViewModel;
        private readonly ICheckedDirectoryManager _CheckedDirectoryManager;
        public ScanHashFilesClass() { throw new NotImplementedException(); }
        public ScanHashFilesClass(
            IPageSelectTargetFileViewModel pageSelectTargetFileViewModel,
            ICheckedDirectoryManager checkedDirectoryManager)
        {
            _PageSelectTargetFileViewModel = pageSelectTargetFileViewModel;
            _CheckedDirectoryManager = checkedDirectoryManager;
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType, CancellationToken cancellationToken)
        {
            // XML からファイルを読み込む
            Instance.LoadHashXML();
            List<string> directoriesList = [];

            try
            {
                // ディレクトリのスキャン
                _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
                directoriesList = await DirectoriesScan(cancellationToken);

                // ファイルのスキャン
                _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);
                await Task.Run(() => DirectoryFilesScan(cancellationToken), cancellationToken);

            }
            catch (OperationCanceledException)
            {
                return;
            }

            // XML 書き込みの表示に切り替える
            _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.DataWriting);

            // XML にファイルを書き込む
            Instance.SaveHashXML();
            _PageSelectTargetFileViewModel.ClearExtentions();
            ScanExtention(HashAlgorithmType);

            // スキャン終了の表示に切り替える
            _PageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }

        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        private async Task<List<string>> DirectoriesScan(CancellationToken cancellationToken)
        {
            var directoriesList = new List<string>();
            // 各ドライブに対してタスクを回す
            await DirectorySearch(_CheckedDirectoryManager.NestedDirectories, cancellationToken);
            await DirectorySearch(_CheckedDirectoryManager.NonNestedDirectories, cancellationToken);
            _PageSelectTargetFileViewModel.AddScannedDirectoriesCount(_CheckedDirectoryManager.NonNestedDirectories.Count);
            return directoriesList;
        }

        /// <summary>
        /// ハッシュを取得する、またはしているディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="cancellationToken">キャンセリングトークン</param>
        private void DirectoryFilesScan(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var directoryFullPath in _directoriesList)
                {
                    Instance.ScanFiles(directoryFullPath);

                    _PageSelectTargetFileViewModel.AddFilesScannedDirectoriesCount();
                    _PageSelectTargetFileViewModel.AllTargetFiles();

                    _PageSelectTargetFileViewModel.AlreadyGetHashCount();
                    _PageSelectTargetFileViewModel.RequireGetHashCount();

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) {
                return;
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        /// <param name="hashAlgorithm">対象となるハッシュアルゴリズム</param>
        public void ScanExtention(FileHashAlgorithm hashAlgorithm)
        {
            foreach (var extention in Instance.GetExtensions(hashAlgorithm))
            {
                _PageSelectTargetFileViewModel.AddExtentions(extention, hashAlgorithm);
            }
            _PageSelectTargetFileViewModel.AddFileTypes(hashAlgorithm);
        }

        #region ディレクトリを検索する
        private readonly List<string> _directoriesList = [];

        private async Task DirectorySearch(List<string> rootDirectories, CancellationToken cancellationToken)
        {
            var directoriesList = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    foreach (var rootDirectory in rootDirectories)
                    {
                        _directoriesList.AddRange(GetDirectories(rootDirectory, cancellationToken));
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException) { return; }
        }

        private List<string> GetDirectories(string rootDirectory, CancellationToken cancellationToken)
        {
            List<string> result = [];
            Stack<string> paths = [];
            try
            {
                paths.Push(rootDirectory);
                while (paths.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string currentDirectory = paths.Pop();
                    result.Add(currentDirectory);
                    _PageSelectTargetFileViewModel.AddScannedDirectoriesCount();

                    foreach (var subDir in FileManager.Instance.EnumerateDirectories(currentDirectory))
                    {
                        paths.Push(subDir);
                    }
                }
                return result;
            }
            catch (OperationCanceledException) { return result; }
        }

        #endregion ディレクトリを検索する
    }
}
