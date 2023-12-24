using System.Reflection.Metadata.Ecma335;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.ViewModels.ExplorerPage
{
    public interface IExpandedDirectoryManager
    {
        public List<string> Directories { get; }
        public bool IsExpandedDirectory(string path);
        public void AddDirectory(string path);
        public void RemoveDirectory(string path);
    }

    // TODO : 削除するパスの特殊フォルダは除外
    public class ExpandedDirectoryManager : IExpandedDirectoryManager
    {
        /// <summary>
        /// 登録したディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> Directories {
            get
            {
                var allList = new List<string>();
                allList.AddRange(_specialDirectoriesRoot);
                allList.AddRange(_specialSubDirectories);
                allList.AddRange(_normalDirectories);
                return allList;
            }
        }

        /// <summary>
        /// 登録したディレクトリのリスト
        /// </summary>
        private readonly List<string> _normalDirectories = [];

        /// <summary>
        /// 登録した特殊ディレクトリのリスト
        /// </summary>
        private readonly List<string> _specialSubDirectories = [] ;

        /// <summary>
        /// /特殊ディレクトリのリスト
        /// </summary>
        private readonly List<string> _specialDirectoriesRoot = [];

        private readonly IFileSystemInformationManager FileSystemInfoManager;

        public ExpandedDirectoryManager(IFileSystemInformationManager fileSystemInfoManager)
        {
            FileSystemInfoManager = fileSystemInfoManager;
            foreach (var rootInfo in FileSystemInfoManager.SpecialFolderScan())
            {
                _specialDirectoriesRoot.Add(rootInfo.FullPath);
            }
        }

        public bool IsExpandedDirectory(string path)
        {
            return HasDirectory(path) || HasSpecialSubDirectory(path) || IsSpecialDirectory(path);
        }

        /// <summary>
        /// 特殊ディレクトリかどうか
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        private bool IsSpecialDirectory(string fullPath) => _specialDirectoriesRoot.Any(dir => dir == fullPath);

        /// <summary>
        /// 特殊ディレクトリかどうか
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        private bool IsSpecialSubDirectory(string fullPath) => _specialDirectoriesRoot.Any(root => fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));



        /// <summary>
        /// 特殊ディレクトリに含まれているかどうか
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        private bool HasSpecialSubDirectory(string fullPath) => _specialSubDirectories.Any(dir => dir == fullPath);


        /// <summary>
        /// 特殊ディレクトリ以外で、ディレクトリがTreeViewで展開されているかどうか
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>TreeViewで展開されているかどうか</returns>
        public bool HasDirectory(string fullPath) => _normalDirectories.Where(dir => dir == fullPath).Any();

        /// <summary>
        /// 指定したパスを管理対象に追加する。
        /// </summary>
        /// <param name="fullPath">追加するパス</param>
        public void AddDirectory(string fullPath)
        {
            // 特殊ディレクトリそのものであれば登録しない
            if (IsSpecialDirectory(fullPath)) { return; }

            // 特殊ディレクトリの配下時の処理
            if (IsSpecialSubDirectory(fullPath) && !HasSpecialSubDirectory(fullPath)) 
            { 
                _specialSubDirectories.Add(fullPath);
                return;
            } 

            if (!HasDirectory(fullPath))
            {
                _normalDirectories.Add(fullPath);
                return;
            }
        }

        /// <summary>
        /// 指定したパスを管理対象から外す。
        /// </summary>
        /// <param name="fullPath">削除するパス</param>
        public void RemoveDirectory(string fullPath)
        {
            if (IsSpecialDirectory(fullPath)) { return; }

            // 特殊ディレクトリの配下時の処理
            if (IsSpecialSubDirectory(fullPath))
            {
                _specialSubDirectories.Remove(fullPath);
                return;
            }

            if (HasDirectory(fullPath))
            {
                _normalDirectories.Remove(fullPath);
                return;
            }
        }
    }
}
