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
        void AddDuplicateFiles(HashSet<HashFile> files);
    }
    public class DupFilesManager : IDupFilesManager
    {
        /// <summary>
        /// ディレクトリごとのHashFileディクショナリ
        /// </summary>
        private readonly Dictionary<string, HashSet<HashFile>> _dicectoryFiles = [];

        /// <summary>
        /// ハッシュごとのHashFileディクショナリ
        /// </summary>
        private readonly Dictionary<string, HashSet<HashFile>> _dupHashFiles = [];

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
