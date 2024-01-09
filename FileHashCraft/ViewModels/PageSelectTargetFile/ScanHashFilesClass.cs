using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models;
using FileHashCraft.ViewModels.Modules;
using static FileHashCraft.Models.FileHashInfoManager;

namespace FileHashCraft.ViewModels.PageSelectTargetFile
{
    public interface IScanHashFilesClass
    {
        public Task ScanHashFiles(FileHashAlgorithm HashAlgorithmType);
    }

    public class ScanHashFilesClass : IScanHashFilesClass
    {
        #region コンストラクタと初期化
        private readonly ICheckedDirectoryManager _checkedDirectoryManager;
        public ScanHashFilesClass() { throw new NotImplementedException(); }
        public ScanHashFilesClass(
            ICheckedDirectoryManager checkedDirectoryManager)
        {
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
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.DirectoriesScanning));
            await Task.Run(DirectoriesScan).ConfigureAwait(false);

            // ファイルのスキャン
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.FilesScanning));
            await Task.Run(() => DirectoryFilesScan(HashAlgorithmType)).ConfigureAwait(false);

            // XML 書き込みの表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.XMLWriting));

            // XML にファイルを書き込む
            FileHashInstance.SaveHashXML();
            WeakReferenceMessenger.Default.Send(new ClearExtentions());
            ScanExtention(HashAlgorithmType);

            // スキャン終了の表示に切り替える
            WeakReferenceMessenger.Default.Send(new HashScanStatusChanged(FileScanStatus.Finished));
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
            WeakReferenceMessenger.Default.Send(new AddHashScanDirectories(_checkedDirectoryManager.NonNestedDirectories.Count));
        }

        /// <summary>
        /// ハッシュを取得する、またはしているディレクトリのファイルをスキャンする
        /// </summary>
        /// <param name="hashAlgorithms">利用するハッシュアルゴリズム</param>
        private void DirectoryFilesScan(FileHashAlgorithm hashAlgorithms)
        {
            foreach (var directoryFullPath in _directoriesList)
            {
                var result = FileHashInstance.ScanFiles(directoryFullPath);
                FileHashInstance.ScanFileExtentions(directoryFullPath);

                WeakReferenceMessenger.Default.Send(new AddFilesHashScanDirectories());
                WeakReferenceMessenger.Default.Send(new AddAllTargetFilesGetHash(result.AllCount));
                switch (hashAlgorithms)
                {
                    case FileHashAlgorithm.SHA256:
                        WeakReferenceMessenger.Default.Send(new AddAlreadyGetHash(result.CountSHA256));
                        WeakReferenceMessenger.Default.Send(new AddRequireGetHash(result.AllCount - result.CountSHA256));
                        break;
                    case FileHashAlgorithm.SHA384:
                        WeakReferenceMessenger.Default.Send(new AddAlreadyGetHash(result.CountSHA384));
                        WeakReferenceMessenger.Default.Send(new AddRequireGetHash(result.AllCount - result.CountSHA384));
                        break;
                    case FileHashAlgorithm.SHA512:
                        WeakReferenceMessenger.Default.Send(new AddAlreadyGetHash(result.CountSHA512));
                        WeakReferenceMessenger.Default.Send(new AddRequireGetHash(result.AllCount - result.CountSHA512));
                        break;
                    default:
                        throw new ArgumentException("Invalid hash algorithm.");
                }
            }
        }

        /// <summary>
        /// 拡張子によるファイルフィルタの設定
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        private void ScanExtention(FileHashAlgorithm hashAlgorithm)
        {
            foreach (var extention in FileHashInstance.GetExtensions(hashAlgorithm))
            {
                WeakReferenceMessenger.Default.Send(new AddExtentions(extention, hashAlgorithm));
            }
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
            WeakReferenceMessenger.Default.Send(new AddHashScanDirectories());
        }
        #endregion 再帰的にディレクトリを検索する
    }
}
