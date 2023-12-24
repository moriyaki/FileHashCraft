using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilOps.ViewModels.ExplorerPage
{
     // ディレクトリスキャンを避けるため、この状態遷移はViewModelで行い、結果だけをここに伝える
    public interface ICheckedDirectoryManager
    {
        public List<string> DirectoriesWithSubdirectories { get; }
        public List<string> DirectoriesOnly { get; }

        /// <summary>
        /// 子ディレクトリを含むディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(string fullPath);

        /// <summary>
        /// 子ディレクトリを含むディレクトリの解除
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectoryWithSubdirectoriesToCheckedDirectoryManager(string fullPath);

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryOnlyToCheckedDirectoryManager(string fullPath);

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectoryOnlyToCheckedDirectoryManager(string fullPath);
    }

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
        /// 子ディレクトリを含むディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryWithSubdirectoriesToCheckedDirectoryManager(string fullPath)
        {
            // 既存のディレクトリが含まれている場合は追加しない
            foreach (string existingDirectory in _directoriesWithSubdirectories)
            {
                if (fullPath.StartsWith(existingDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            // 新しいディレクトリが既存のディレクトリを含んでいる場合は削除する
            _directoriesWithSubdirectories.RemoveAll(existingDirectory =>
                existingDirectory.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase));

            _directoriesWithSubdirectories.Add(fullPath);
        }

        /// <summary>
        /// 子ディレクトリを含むディレクトリの解除
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectoryWithSubdirectoriesToCheckedDirectoryManager(string fullPath)
        {
            if (_directoriesWithSubdirectories.Any(existingDirectory => existingDirectory == fullPath))
            {
                _directoriesWithSubdirectories.Remove(fullPath);
            }
        }

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">登録するディレクトリのフルパス</param>
        public void AddDirectoryOnlyToCheckedDirectoryManager(string fullPath)
        {
            if (!_directoriesOnly.Any(existingDirectory => existingDirectory == fullPath))
            {
                _directoriesOnly.Add(fullPath);
            }
        }

        /// <summary>
        /// 子ディレクトリを含まない、自分自身のみディレクトリの登録
        /// </summary>
        /// <param name="fullPath">解除するディレクトリのフルパス</param>
        public void RemoveDirectoryOnlyToCheckedDirectoryManager(string fullPath)
        {
            if (_directoriesOnly.Any(existingDirectory => existingDirectory == fullPath))
            {
                _directoriesOnly.Remove(fullPath);
            }
        }
    }
}
