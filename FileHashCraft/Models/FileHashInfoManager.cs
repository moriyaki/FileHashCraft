using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace FileHashCraft.Models
{
    #region XMLシリアライズ／デシリアライズするクラス
    /// <summary>
    /// フルパスとファイルリストを持つ
    /// </summary>
    public class HashDir
    {
        public string FullPath { get; set; } = string.Empty;
        public List<HashFile> Files { get; set; } = [];
    }

    /// <summary>
    /// ファイル情報を持つ
    /// </summary>
    public class HashFile
    {
        /// <summary>
        /// ファイル名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// ファイルの最終更新日
        /// </summary>
        private DateTime _LastModiried;
        public DateTime LastModified
        {
            get => _LastModiried;
            set => _LastModiried = value.ToUniversalTime();
        }
        /// <summary>
        /// SHA-256ハッシュ
        /// </summary>
        public string SHA256 { get; set; } = string.Empty;
　       /// <summary>
        /// SHA-384ハッシュ
        /// </summary>
        public string SHA384 { get; set; } = string.Empty;
        /// <summary>
        /// SHA-512ハッシュ
        /// </summary>
        public string SHA512 { get; set; } = string.Empty;
    }
    #endregion XMLシリアライズ／デシリアライズするクラス
    public sealed class FileHashInfoManager
    {
        public static FileHashInfoManager Instance { get; } = new();
        private FileHashInfoManager() { }

        private List<HashDir> _fileHashDirList = new(10000);

        #region ハッシュ情報XMLへの読み書き
        const string appName = "FileHashCraft";

        private static string GetXMLFileName()
        {
            var settingXMLFile = $"{appName}.xml";
            string xmlFileDirectory = string.Empty;
#if DEBUG
            xmlFileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
#else
            xmlFileDirectory = Path.GetTempPath();
#endif
            return Path.Combine(xmlFileDirectory, settingXMLFile);
        }

        private static string GetGZFileName()
        {
            var settingGZFile = $"{appName}.gz";
            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            return Path.Combine(localAppDataPath, settingGZFile);
        }

        public void SetAllDirectoriesCount(int directoriesCount)
        {
            _fileHashDirList.Capacity = directoriesCount;
        }

        /// <summary>
        /// FileHashCraftが保持しているファイルからハッシュデータを読込します。
        /// </summary>
        public void LoadHashXML()
        {
            if (Path.Exists(GetGZFileName()))
            {
                // ファイルを展開する
                using FileStream compressedFileStream = new(GetGZFileName(), FileMode.Open, FileAccess.Read);
                using FileStream decompressedFileStream = new(GetXMLFileName(), FileMode.Create, FileAccess.Write);
                using GZipStream gzipStream = new(compressedFileStream, CompressionMode.Decompress);
                // gzipで圧縮されたファイルを展開して書き込み
                gzipStream.CopyTo(decompressedFileStream);
            }

            if (Path.Exists(GetXMLFileName()))
            {
                XmlSerializer serializer = new(typeof(List<HashDir>));

                // XML ファイルを読み込む
                using TextReader reader = new StreamReader(GetXMLFileName());
                var _fileHashList = (List<HashDir>?)serializer.Deserialize(reader);
                if (_fileHashList == null) return;
                _fileHashDirList = _fileHashList;
            }
        }

        /// <summary>
        /// ハッシュデータをFileHashCraftが保持しているファイルに書込します。
        /// </summary>
        public void SaveHashXML()
        {
            try
            {
                XmlSerializer serializer = new(typeof(List<HashDir>));

                XmlWriterSettings settings = new()
                {
#if DEBUG
                    Indent = true,
                    IndentChars = " ",
                    NewLineHandling = NewLineHandling.Replace,
#else
                    Indent = false,   // インデントを無効にする
                    NewLineHandling = NewLineHandling.None  // 新しい行の処理を無効にする
#endif
                };

                // XML ファイルに書き込む
                using TextWriter writer = new StreamWriter(GetXMLFileName());
                using XmlWriter xmlWriter = XmlWriter.Create(writer, settings);
                serializer.Serialize(xmlWriter, _fileHashDirList);
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"ハッシュ情報の書き込みにエラーが発生しました: {ex.Message}");
            }

            try
            {
                // ファイルを圧縮する
                using FileStream originalFileStream = new(GetXMLFileName(), FileMode.Open);
                using FileStream compressedFileStream = new(GetGZFileName(), FileMode.Create);
                using GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress);
                originalFileStream.CopyTo(compressionStream);
            }
            catch (Exception ex)
            {
                DebugManager.ExceptionWrite($"ハッシュ情報の圧縮中にエラーが発生しました: {ex.Message}");
            }
        }
        #endregion ハッシュ情報XMLへの読み書き

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// ディレクトリとファイルを追加変更削除します。
        /// </summary>
        /// <param name="fullPath">追加するディレクトリのフルパス</param>
        public void ScanDirectory(string fullPath)
        {
            var scanFileItems = new ScanFileItems();

            lock(_lock)
            {
                // ディレクトリ内の、既に存在しないディレクトリの情報を検索して削除
                var storageDirectory = scanFileItems.EnumerateDirectories(fullPath);
                var xmlDirectoryCollection = _fileHashDirList.Where(x => x.FullPath.StartsWith(fullPath));
                var xmlPathCollection = xmlDirectoryCollection.Select(d => d.FullPath).ToList();
                var deletedDirectory = xmlPathCollection.Except(storageDirectory).ToList();
                if (deletedDirectory.Count > 0)
                {
                    _fileHashDirList.RemoveAll(dir => deletedDirectory.Contains(dir.FullPath));
                }
            }

            // フルパスが同じ物を探し、既に存在しないファイル情報を削除
            var storageFiles = scanFileItems.EnumerateFiles(fullPath);

            var xmlItem = _fileHashDirList.Find(f => f.FullPath == fullPath);
            var xmlFiles = xmlItem?.Files.Select(x => x.Name);
            var deletedFiles = xmlFiles?.Except(storageFiles).ToList();
            if (deletedFiles?.Count > 0)
            {
                xmlItem?.Files.RemoveAll(file => deletedFiles.Contains(file.Name.ToLowerInvariant()));
            }

            // ファイル情報の追加更新処理
            if (xmlItem == null)
            {
                // XMLにディレクトリが無かったら追加
                xmlItem = new()
                {
                    FullPath = fullPath,
                };
                xmlItem.Files.Capacity = storageFiles.Count();
                _fileHashDirList.Add(xmlItem);
                if (_fileHashDirList.Count % 10000 == 0)
                {
                    _fileHashDirList.Capacity += 10000;
                }
            }

            foreach (var file in storageFiles)
            {
                var fileInfo = new FileInfo(file);
                if (xmlFiles?.Contains(file) == true)
                {
                    // 既にXML内に存在していたら更新チェック
                    var existedFile = xmlItem?.Files.Find(x => x.Name == file);
                    if (existedFile != null && (existedFile.Length != fileInfo.Length || existedFile.LastModified != fileInfo.LastWriteTimeUtc))
                    {
                        // ファイルが更新されていたら情報更新
                        existedFile.Length = fileInfo.Length;
                        existedFile.LastModified = fileInfo.LastWriteTimeUtc;
                        // ハッシュは消す
                        existedFile.SHA256 = string.Empty;
                        existedFile.SHA384 = string.Empty;
                        existedFile.SHA512 = string.Empty;
                    }
                    continue;
                }
                else
                {
                    // 存在していなかったら追加
                    xmlItem?.Files.Add(new()
                    {
                        Name = fileInfo.Name,
                        Length = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime,
                    });
                }
            }
        }
    }
}
