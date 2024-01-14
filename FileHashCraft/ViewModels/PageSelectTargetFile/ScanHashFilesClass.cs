using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;
using static FileHashCraft.Models.FileHashInfoManager;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType);
        public void ScanExtention(FileHashAlgorithm hashAlgorithm);
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly IPageSelectTargetFileViewModel _pageSelectTargetFileViewModel;
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        public ScanHashFilesClass() { throw new NotImplementedException(); }
        public ScanHashFilesClass(
            IPageSelectTargetFileViewModel pageSelectTargetFileViewModel,
            ICheckedDirectoryManager checkedDirectoryManager)
        {
            _pageSelectTargetFileViewModel = pageSelectTargetFileViewModel;
            _checkedDirectoryManager = checkedDirectoryManager;
        }
        #endregion コンストラクタと初期化

        /// <summary>
        /// スキャンするディレクトリを追加します。
        /// </summary>
        public async Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType)
        {
            // XML からファイルを読み込む
            FileHashInstance.LoadHashXML();

            // ディレクトリのスキャン
            _pageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.DirectoriesScanning);
            await Task.Run(DirectoriesScan).ConfigureAwait(false);

            // ファイルのスキャン
            _pageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.FilesScanning);
            await Task.Run(() => DirectoryFilesScan(HashAlgorithmType)).ConfigureAwait(false);

            // XML 書き込みの表示に切り替える
            _pageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.DataWriting);

            // XML にファイルを書き込む
            FileHashInstance.SaveHashXML();
            _pageSelectTargetFileViewModel.ClearExtentions();
            ScanExtention(HashAlgorithmType);

            // スキャン終了の表示に切り替える
            _pageSelectTargetFileViewModel.ChangeHashScanStatus(FileScanStatus.Finished);
        }

        /// <summary>
        /// ハッシュを取得するディレクトリをスキャンする
        /// </summary>
        private void DirectoriesScan()
        {
            // 各ドライブに対してタスクを回す
            RecursivelyDirectorySearch(_checkedDirectoryManager.NestedDirectories);
            foreach (var directory in _checkedDirectoryManager.NonNestedDirectories)
            {
                ScanDirectory(directory);
            }
            _pageSelectTargetFileViewModel.AddScannedDirectoriesCount(_checkedDirectoryManager.NonNestedDirectories.Count);
        }

        /// <summary>
        /// ハッシュを取得する、またはしているディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="hashAlgorithms">利用するハッシュアルゴリズム</param>
        private void DirectoryFilesScan(FileHashAlgorithm hashAlgorithms)
        {
            foreach (var directoryFullPath in _directoriesList)
            {
                FileHashInstance.ScanFiles(directoryFullPath);
                FileHashInstance.ScanFileExtentions(directoryFullPath);

                _pageSelectTargetFileViewModel.AddFilesScannedDirectoriesCount();
                _pageSelectTargetFileViewModel.AllTargetFiles();
                switch (hashAlgorithms)
                {
                    case FileHashAlgorithm.SHA256:
                        _pageSelectTargetFileViewModel.AlreadyGetHashCount();
                        _pageSelectTargetFileViewModel.RequireGetHashCount();
                        break;
                    case FileHashAlgorithm.SHA384:
                        _pageSelectTargetFileViewModel.AlreadyGetHashCount();
                        _pageSelectTargetFileViewModel.RequireGetHashCount();
                        break;
                    case FileHashAlgorithm.SHA512:
                        _pageSelectTargetFileViewModel.AlreadyGetHashCount();
                        _pageSelectTargetFileViewModel.RequireGetHashCount();
                        break;
                    default:
                        throw new ArgumentException("Invalid hash algorithm.");
                }
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        /// <param name="hashAlgorithm">対象となるハッシュアルゴリズム</param>
        public void ScanExtention(FileHashAlgorithm hashAlgorithm)
        {
            foreach (var extention in FileHashInstance.GetExtensions(hashAlgorithm))
            {
                _pageSelectTargetFileViewModel.AddExtentions(extention, hashAlgorithm);
            }
            _pageSelectTargetFileViewModel.AddFileTypes(hashAlgorithm);
        }

        #region 再帰的にディレクトリを検索する
        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object lockObject = new();
        /// <summary>
        /// ディレクトリリスト
        /// </summary>
        private readonly List<string> _directoriesList = new(500);

        /// <summary>
        /// 並列処理でディレクトリを検索して、スキャン処理に渡します。
        /// </summary>
        private void RecursivelyDirectorySearch(List<string> rootDirectory)
        {
            Parallel.ForEach(rootDirectory, RecursivelyRetrieveDirectories);
        }

        /// <summary>
        /// ディレクトリをスキャンしてXMLに反映させて、子のディレクトリに再帰処理をします。
        /// </summary>
        /// <param name="fullPath">スキャンするディレクトリ</param>
        private void RecursivelyRetrieveDirectories(string fullPath)
        {
            if (!_directoriesList.Contains(fullPath))
            {
                ScanDirectory(fullPath);
            }

            var fileInfoManager = new ScanFileItems();
            foreach (var directory in fileInfoManager.EnumerateDirectories(fullPath))
            {
                RecursivelyRetrieveDirectories(directory);
            }
        }

        /// <summary>
        /// ディレクトリ情報をXMLファイルに反映させます。
        /// </summary>
        /// <param name="fullPath">反映させるディレクトリのフルパス</param>
        private void ScanDirectory(string fullPath)
        {
            FileHashInfoManager.FileHashInstance.ScanDirectory(fullPath);
            lock(lockObject)
            {
                _directoriesList.Add(fullPath);
                if (_directoriesList.Count % 500 == 0)
                {
                    _directoriesList.Capacity += 500;
                }
            }
            _pageSelectTargetFileViewModel.AddScannedDirectoriesCount();
        }
        #endregion 再帰的にディレクトリを検索する
    }
}
