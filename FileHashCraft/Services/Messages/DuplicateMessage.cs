using FileHashCraft.Models;

namespace FileHashCraft.Services.Messages
{
    /// <summary>
    /// 重複があるファイルを送信するメッセージ
    /// </summary>
    public class DuplicateFilesMessage
    {
        public string Directory { get; } = string.Empty;
        public HashSet<HashFile> HashFiles { get; }

        public DuplicateFilesMessage()
        { throw new NotImplementedException(nameof(DuplicateFilesMessage)); }

        public DuplicateFilesMessage(string directory, HashSet<HashFile> hashFiles)
        {
            Directory = directory;
            HashFiles = hashFiles;
        }
    }

    /// <summary>
    /// 重複リンクファイルのツリービュークリアメッセージ
    /// </summary>
    public class DuplicateLinkClearMessage;

    /// <summary>
    /// 重複ファイルのリンクファイルを送信するメッセージ
    /// </summary>
    public class DuplicateLinkFilesMessage
    {
        public string Directory { get; } = string.Empty;
        public HashSet<HashFile> HashFiles { get; }

        public DuplicateLinkFilesMessage()
        { throw new NotImplementedException(nameof(DuplicateLinkFilesMessage)); }

        public DuplicateLinkFilesMessage(string directory, HashSet<HashFile> hashFiles)
        {
            Directory = directory;
            HashFiles = hashFiles;
        }
    }
}