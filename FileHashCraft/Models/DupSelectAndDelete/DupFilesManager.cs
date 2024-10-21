using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashCraft.Models.DupSelectAndDelete
{
    public interface IDupFilesManager
    {
        /// <summary>
        /// 重複ファイルがあるディレクトリを取得する
        /// </summary>
        HashSet<string> GetDirectories();
        /// <summary>
        /// 重複ファイルのハッシュを取得する
        /// </summary>
        HashSet<string> GetHashes();
        /// <summary>
        /// 重複ファイルがあるファイルを一括で追加する
        /// </summary>
        void AddDuplicateFiles(HashSet<HashFile> files);
    }
    public class DupFilesManager : IDupFilesManager
    {
        #region メンバ
        /// <summary>
        /// ディレクトリごとのHashFileディクショナリ
        /// </summary>
        private readonly Dictionary<string, HashSet<HashFile>> _dicectoryFiles = [];

        /// <summary>
        /// ハッシュごとのHashFileディクショナリ
        /// </summary>
        private readonly Dictionary<string, HashSet<HashFile>> _dupHashFiles = [];
        #endregion メンバ

        #region 取得メソッド
        /// <summary>
        /// 重複ファイルがあるディレクトリを取得する
        /// </summary>
        /// <returns>重複ファイルがあるディレクトリHashSet</returns>
        public HashSet<string> GetDirectories()
        {
            return [.. _dicectoryFiles.Keys];
        }

        /// <summary>
        /// 重複ファイルのハッシュを取得する
        /// </summary>
        /// <returns>重複ファイルのハッシュHashSet</returns>
        public HashSet<string> GetHashes()
        {
            return [.. _dupHashFiles.Keys];
        }
        #endregion 取得メソッド

        /// <summary>
        /// 重複ファイルがあるファイルを一括で追加する
        /// </summary>
        /// <param name="files">重複ファイルHashSet</param>
        public void AddDuplicateFiles(HashSet<HashFile> files)
        {
            foreach (var hashFile in files)
            {
                if (!_dicectoryFiles.TryGetValue(hashFile.FileHash, out HashSet<HashFile>? value))
                {
                    value = [];
                    _dupHashFiles[hashFile.FileHash] = value;
                }
                value.Add(hashFile);
            }

            foreach (var directoryFile in files)
            {
                var dir = Path.GetDirectoryName(directoryFile.FileFullPath) ?? string.Empty;
                if (!_dicectoryFiles.TryGetValue(dir, out HashSet<HashFile>? value))
                {
                    value = [];
                    _dicectoryFiles[dir] = value;
                }
                value.Add(directoryFile);
            }
        }
    }
}
