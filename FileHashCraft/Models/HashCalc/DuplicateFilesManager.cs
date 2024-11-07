using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.HashCalc
{
    #region インターフェース
    public interface IDuplicateFilesManager
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
        /// ハッシュが同一のディレクトリを一括で送信する
        /// </summary>
        void GetDuplicateLinkFiles(HashSet<string> hashes, string currentDirectory);

        /// <summary>
        /// 重複ファイルがあるファイルを一括で追加する
        /// </summary>
        void AddDuplicateFiles(HashSet<HashFile> files);
    }
    #endregion インターフェース

    public class DuplicateFilesManager : IDuplicateFilesManager
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

        public DuplicateFilesManager()
        { throw new NotImplementedException(nameof(DuplicateFilesManager)); }

        private readonly IMessenger _Messanger;

        public DuplicateFilesManager(
            IMessenger messenger
        )
        {
            _Messanger = messenger;
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
        /// ディレクトリ内のハッシュ重複ファイルを一括で送信する
        /// </summary>
        /// <param name="directory">重複ファイルのあるディレクトリ</param>
        public void GetDuplicateFiles(string directory)
        {
            _Messanger.Send(new DuplicateFilesMessage(directory, _dicectoryFiles[directory]));
        }

        /// <summary>
        /// ハッシュが同一のディレクトリを一括で送信する
        /// </summary>
        /// <param name="hashes">重複ファイルのハッシュのコレクション</param>
        /// <param name="currentDirectory">カレントディレクトリ</param>
        public void GetDuplicateLinkFiles(HashSet<string> hashes, string currentDirectory)
        {
            // 同一ハッシュを持つディレクトリ一覧を取得する
            var directories = hashes
                .SelectMany(hash => _dupHashFiles[hash])
                .Select(file => Path.GetDirectoryName(file.FileFullPath) ?? string.Empty)
                .ToHashSet();

            // ディレクトリ一覧からファイル一覧を取得して返す
            foreach (var directory in directories)
            {
                _Messanger.Send(new DuplicateLinkFilesMessage(directory, _dicectoryFiles[directory]));
            }
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