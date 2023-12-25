using System.IO;
using FilOps.Models;

namespace FilOps.ViewModels.ExplorerPage
{
    // ディレクトリスキャンを避けるため、この状態遷移はViewModelで行い、結果だけをここに伝える
    #region インターフェース
    public interface ICheckedDirectoryManager
    {
        public List<string> DirectoriesWithSubdirectories { get; }
        public List<string> DirectoriesOnly { get; }

        /// <summary>
        /// ディレクトリがチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectory(string fullPath);

        /// <summary>
        /// ディレクトリがサブディレクトリを含む形でチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectoriesWithSubdirectories(string fullPath);

        /// <summary>
        /// ディレクトリが単独でチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectoriesOnly(string fullPath);

        /// <summary>
        /// 子ディレクトリを含むディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryWithSubdirectories(string fullPath);

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryOnly(string fullPath);

        /// <summary>
        /// 自分自身のディレクトリの登録解除
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectory(string fullPath);
    }
    #endregion インターフェース

    public class CheckedDirectoryManager : ICheckedDirectoryManager
    {
        /// <summary>
        /// 登録した、サブディレクトリを含むディレクトリのリスト
        /// </summary>
        private readonly List<string> _directoriesWithSubdirectories = [];

        /// <summary>
        /// 登録した、サブディレクトリを含むディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> DirectoriesWithSubdirectories { get => _directoriesWithSubdirectories; }

        /// <summary>
        /// 登録した、サブディレクトリを含まないディレクトリのリスト
        /// </summary>
        private readonly List<string> _directoriesOnly = [];
        /// <summary>
        /// 登録した、サブディレクトリを含まないディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> DirectoriesOnly { get => _directoriesOnly; }

        /// <summary>
        /// ディレクトリがチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectory(string fullPath)
        {
            if (_directoriesOnly.Any(dir => dir == fullPath)) return true;
            if (_directoriesWithSubdirectories.Contains(fullPath)) return true;
            return false;
        }

        /// <summary>
        /// ディレクトリがサブディレクトリを含む形でチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectoriesWithSubdirectoriesRoot(string fullPath)
        {
            return _directoriesWithSubdirectories.Any((dir => dir == fullPath));
        }

        /// <summary>
        /// ディレクトリがサブディレクトリを含む形でチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectoriesWithSubdirectories(string fullPath)
        {
            foreach (var dir in _directoriesWithSubdirectories)
            {
                if (fullPath.StartsWith(dir)) return true;
            }
            return false;
        }

        /// <summary>
        /// ディレクトリが単独でチェックされているかどうか
        /// </summary>
        /// <param name="fullPath">確認するディレクトリのフルパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectoriesOnly(string fullPath)
        {
            return _directoriesOnly.Any(dir => dir == fullPath);
        }

        /// <summary>
        /// 子ディレクトリを含むディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryWithSubdirectories(string fullPath)
        {
            if (!FileSystemInformationManager.FileItemScan(fullPath, false).Any()) { return; }

            // 既存のディレクトリが含まれている場合は追加しない
            foreach (var existDirWithSub in _directoriesWithSubdirectories)
            {
                if (fullPath.StartsWith(existDirWithSub, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            // 新しいディレクトリが既存のディレクトリを含んでいる場合は削除する
            _directoriesWithSubdirectories.RemoveAll(foundDirWithSub =>
                foundDirWithSub.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase));

            // 単独監視に含まれている場合は削除する
            var existDir = _directoriesOnly.Find(dir => dir == fullPath);
            if (existDir != null)
            {
                _directoriesOnly.Remove(existDir);
            }

            _directoriesWithSubdirectories.Add(fullPath);
        }

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryOnly(string fullPath)
        {
            if (!FileSystemInformationManager.FileItemScan(fullPath, false).Any()) { return; }
            try
            {
                var hasDirectory = Directory.EnumerateFileSystemEntries(fullPath).Any();
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException ||
                    ex is IOException)
                {
                    return;
                }
            }

            if (!_directoriesOnly.Any(existingDirectory => existingDirectory == fullPath))
            {
                _directoriesOnly.Add(fullPath);
            }
        }

        /// <summary>
        /// ディレクトリの登録解除
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectory(string fullPath)
        {
            // 単独チェックならそのまま削除
            if (HasDirectoriesOnly(fullPath))
            {
                _directoriesOnly.Remove(fullPath);
                return;
            }

            // サブディレクトリ含むディレクトリそのものの場合
            if (HasDirectoriesWithSubdirectoriesRoot(fullPath))
            {
                _directoriesWithSubdirectories.Remove(fullPath);
                return;
            }

            // サブディレクトリ監視の子ディレクトリ処理

            // 親ディレクトリの取得
            var parentPath = Path.GetDirectoryName(fullPath);
            if (parentPath == null) { return; }

            // 親ディレクトリを単独監視に
            _directoriesWithSubdirectories.Remove(parentPath);
            _directoriesOnly.Add(parentPath);

            // 子ディレクトリを、解除されたディレクトリを除きサブディレクトリ含む監視
            foreach (var dir in FileSystemInformationManager.FileItemScan(parentPath, false))
            {
                if (dir.FullPath != fullPath)
                {
                    _directoriesWithSubdirectories.Add(dir.FullPath);
                }
            }
        }
    }
}
