using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHashCraft.Models;

namespace FileHashCraft.Services.Messages
{
    /// <summary>
    /// 重複ファイルを送信するメッセージ
    /// </summary>
    public class DuplicateFilesMessage
    {
        public string Directory { get; } = string.Empty;
        public HashSet<HashFile> HashFiles { get; }
        public DuplicateFilesMessage() { throw new NotImplementedException(nameof(DuplicateFilesMessage)); }
        public DuplicateFilesMessage(string directory, HashSet<HashFile> hashFiles)
        {
            Directory = directory;
            HashFiles = hashFiles;
        }
    }
}
