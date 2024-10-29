using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.DupSelectAndDelete
{
    public interface IDupFilesManager
    {
        /// <summary>
        /// 重複ファイルがあるディレクトリを取得する
        /// </summary>
        HashSet<string> GetDirectories();

        /// <summary>
        /// ディレクトリ内のハッシュ重複ファイルを取得する
        /// </summary>
        /// <param name="directory"></param>
        void GetDuplicateFiles(string directory);

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

        #region コンストラクタ

        public DupFilesManager()
        { throw new NotImplementedException(nameof(DupFilesManager)); }

        private readonly IMessenger _messenger;

        public DupFilesManager(
            IMessenger messenger
        )
        {
            _messenger = messenger;
        }

        #endregion コンストラクタ

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
        /// ディレクトリ内のハッシュ重複ファイルを取得する
        /// </summary>
        /// <param name="directory"></param>
        public void GetDuplicateFiles(string directory)
        {
            _messenger.Send(new DuplicateFilesMessage(directory, _dicectoryFiles[directory]));
        }

        #endregion 取得メソッド

        /// <summary>
        /// 重複ファイルがあるファイルを一括で追加する
        /// </summary>
        /// <param name="files">重複ファイルHashSet</param>
        public void AddDuplicateFiles(HashSet<HashFile> files)
        {
            // ハッシュ毎に格納
            foreach (var hashFile in files)
            {
                if (!_dupHashFiles.TryGetValue(hashFile.FileHash, out HashSet<HashFile>? value))
                {
                    value = [];
                    _dupHashFiles[hashFile.FileHash] = value;
                }
                value.Add(hashFile);
            }

            // ディレクトリ毎に格納
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