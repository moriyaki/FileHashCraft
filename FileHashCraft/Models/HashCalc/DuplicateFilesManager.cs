using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashCraft.Models.HashCalc
{
    public interface IDuplicateFilesManager
    {
        /// <summary>
        /// 同一ファイル
        /// </summary>
        HashSet<HashFile> SameFiles { get; }

        void AddSameFile(HashFile file);

        void AddSameFiles(IEnumerable<HashFile> files);
    }
    public class DuplicateFilesManager : IDuplicateFilesManager
    {
        public HashSet<HashFile> SameFiles { get; set; } = [];

        /// <summary>
        /// 同一ファイルの管理への追加
        /// </summary>
        /// <param name="file">追加するファイル</param>
        public void AddSameFile(HashFile file)
        {
            SameFiles.Add(file);
        }

        /// <summary>
        /// 同一ファイルへの管理への一括追加
        /// </summary>
        /// <param name="files">追加するファイルコレクション</param>
        public void AddSameFiles(IEnumerable<HashFile> files)
        {
            SameFiles.UnionWith(files);
        }
    }
}
