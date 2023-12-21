using System.Reflection.Metadata.Ecma335;
using CommunityToolkit.Mvvm.ComponentModel;
using FilOps.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FilOps.Models
{
    public interface IDirectoryManager
    {
        public List<string> Directories { get; }
    }

    public class ExpandedDirectoryManager : IDirectoryManager
    {
        /// <summary>
        /// 登録したディレクトリのリスト
        /// </summary>
        private readonly List<string> _directories = [];

        /// <summary>
        /// 登録したディレクトリのリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<string> Directories{ get => _directories; }

        /// <summary>
        /// パスが登録されているかどうかを調べる。
        /// </summary>
        /// <param name="path">調べるパス</param>
        /// <returns>登録されているかどうか</returns>
        public bool HasDirectory(string path) => _directories.Where(dir => dir == path).Any();

        /// <summary>
        /// 指定したパスを管理対象に追加する。
        /// </summary>
        /// <param name="path">追加するパス</param>
        public void AddDirectory(string path) { if (!HasDirectory(path)) { _directories.Add(path); } }

        /// <summary>
        /// 指定したパスを管理対象から外す。
        /// </summary>
        /// <param name="path">削除するパス</param>
        public void RemoveDirectory(string path) { if (HasDirectory(path)) { _directories.Remove(path); } }

        /// <summary>
        /// 指定したパスコレクションを管理対象に追加する。
        /// </summary>
        /// <param name="pathCollection">追加するパスコレクション</param>
        public void AddDirectory(IEnumerable<string> pathCollection) => _directories.AddRange(pathCollection.Except(_directories));

        /// <summary>
        ///  指定したパスコレクションを管理対象から外す
        /// </summary>
        /// <param name="pathCollection">削除するパスコレクションを</param>
        public void RemoveDirectory(IEnumerable<string> pathCollection) => _directories.RemoveAll(remove => pathCollection.Any(path => path == remove));
    }
}
