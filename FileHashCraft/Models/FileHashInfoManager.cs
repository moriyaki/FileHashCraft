using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

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
        public static FileHashInfoManager FileHashInstance { get; } = new();
        private FileHashInfoManager() { }

        /// <summary>
        /// 全ファイルハッシュリストのデータ
        /// </summary>
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
            var gzFileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            return Path.Combine(gzFileDirectory, settingGZFile);
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

            lock (_lock)
            {
                // ディレクトリ内の、既に存在しないディレクトリの情報を検索して削除
                var storageDirectory = scanFileItems.EnumerateDirectories(fullPath);
                var xmlDirectory = _fileHashDirList
                    .Where(x => fullPath == Path.GetDirectoryName(x.FullPath))
                    .Select(d => d.FullPath).ToList();
                var deletedDirectory = xmlDirectory
                    .Except(storageDirectory).ToList();
                if (deletedDirectory.Count > 0)
                {
                    _fileHashDirList.RemoveAll(dir => deletedDirectory.Contains(dir.FullPath));
                }

                var xmlItem = _fileHashDirList.Find(f => f.FullPath == fullPath);
                if (xmlItem == null)
                {
                    // XMLにディレクトリが無かったら追加
                    xmlItem = new()
                    {
                        FullPath = fullPath,
                    };
                    xmlItem.Files.Capacity = scanFileItems.EnumerateFiles(fullPath).Count();
                    _fileHashDirList.Add(xmlItem);
                    if (_fileHashDirList.Count % 10000 == 0)
                    {
                        _fileHashDirList.Capacity += 10000;
                    }
                }
            }
        }

        /// <summary>
        /// ファイル総数とハッシュ取得済み数
        /// </summary>
        public class FileHashStatus
        {
            public int AllCount { get; set; } = 0;
            public int CountSHA256 { get; set; } = 0;
            public int CountSHA384 { get; set; } = 0;
            public int CountSHA512 { get; set; } = 0;
        }

        /// <summary>
        /// SHA256ハッシュを保持するファイルのフルパス
        /// </summary>
        private readonly List<string> _sha256DirList = new(10000);
        /// <summary>
        /// SHA384ハッシュを保持するファイルのフルパス
        /// </summary>
        private readonly List<string> _sha384DirList = new(10000);
        /// <summary>
        /// SHA512ハッシュを保持するファイルのフルパス
        /// </summary>
        private readonly List<string> _sha512DirList = new(10000);

        /// <summary>
        /// ファイルを追加変更削除し、ハッシュ取得状況を取得します。
        /// </summary>
        /// <param name="directoryFullPath">走査するディレクトリのフルパス</param>
        /// <returns>走査するディレクトリの情報</returns>
        public FileHashStatus ScanFiles(string directoryFullPath)
        {
            // フルパスが同じ物を探し、既に存在しないファイル情報を削除
            var scanFileItems = new ScanFileItems();
            var storageFiles = scanFileItems.EnumerateFiles(directoryFullPath);
            var xmlItem = _fileHashDirList.Find(f => f.FullPath == directoryFullPath);
            var xmlFiles = xmlItem?.Files.Select(x => Path.Combine(directoryFullPath, x.Name));

            var deletedFiles = xmlFiles?.Except(storageFiles).ToList();

            if (deletedFiles?.Count > 0)
            {
                xmlItem?.Files.RemoveAll(file => deletedFiles.Contains(file.Name.ToLowerInvariant()));
            }

            var result = new FileHashStatus
            {
                AllCount = storageFiles.Count(),
                CountSHA256 = 0,
                CountSHA384 = 0,
                CountSHA512 = 0
            };
            foreach (var fullPath in storageFiles)
            {
                var fileInfo = new FileInfo(fullPath);
                if (xmlFiles?.Contains(fullPath) == true)
                {
                    // 既にXML内に存在していたら更新チェック
                    var existedFile = xmlItem?.Files.Find(x => x.Name == Path.GetFileName(fullPath));
                    if (existedFile == null) continue;

                    if (existedFile.Length != fileInfo.Length || existedFile.LastModified != fileInfo.LastWriteTimeUtc)
                    {
                        // ファイルが更新されていたら情報更新
                        existedFile.Length = fileInfo.Length;
                        existedFile.LastModified = fileInfo.LastWriteTimeUtc;
                        // ハッシュは消す
                        existedFile.SHA256 = string.Empty;
                        existedFile.SHA384 = string.Empty;
                        existedFile.SHA512 = string.Empty;
                    }
                    else
                    {
                        // ハッシュが存在したら各ハッシュ存在リストに追加
                        if (!string.IsNullOrEmpty(existedFile.SHA256))
                        {
                            result.CountSHA256++;
                            _sha256DirList.Add(fullPath);
                        }
                        if (!string.IsNullOrEmpty(existedFile.SHA384))
                        {
                            result.CountSHA384++;
                            _sha384DirList.Add(fullPath);
                        }
                        if (!string.IsNullOrEmpty(existedFile.SHA512))
                        {
                            result.CountSHA512++;
                            _sha512DirList.Add(fullPath);
                        }
                        continue;
                    }
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
            return result;
        }

        /// <summary>
        /// 拡張子をキーにSHA256未取得ハッシュのファイルフルパスを取得するリスト
        /// </summary>
        private readonly Dictionary<string, List<string>> _extentionNotHaveHashSHA256 = [];
        /// <summary>
        /// 拡張子をキーにSHA256未取得ハッシュのファイルフルパスを取得するリスト
        /// </summary>
        private readonly Dictionary<string, List<string>> _extentionNotHaveHashSHA384 = [];
        /// <summary>
        /// 拡張子をキーにSHA256未取得ハッシュのファイルフルパスを取得するリスト
        /// </summary>
        private readonly Dictionary<string, List<string>> _extentionNotHaveHashSHA512 = [];
        /// <summary>
        /// 拡張子をキーに、未取得ハッシュのファイルフルパスを登録する
        /// </summary>
        /// <param name="directoryFullPath">ディレクトリのフルパス</param>
        public void ScanFileExtentions(string directoryFullPath)
        {
            // 拡張子をキーに辞書登録する
            var value = new List<string>();

            // ディレクトリのファイルを取得する
            var xmlItem = _fileHashDirList.Find(f => f.FullPath == directoryFullPath);
            if (xmlItem == null) { return; }

            foreach (var file in xmlItem.Files)
            {
                // フルパスを生成し、そこから拡張子を取得する
                var fileFullPath = Path.Combine(directoryFullPath, file.Name);
                var extention = Path.GetExtension(fileFullPath).ToLowerInvariant();

                // リストが存在しないなら作成する
                if (!_extentionNotHaveHashSHA256.TryGetValue(extention, out value))
                {
                    _extentionNotHaveHashSHA256[extention] = [];
                }
                if (!_extentionNotHaveHashSHA384.TryGetValue(extention, out value))
                {
                    _extentionNotHaveHashSHA384[extention] = [];
                }
                if (!_extentionNotHaveHashSHA512.TryGetValue(extention, out value))
                {
                    _extentionNotHaveHashSHA512[extention] = [];
                }

                // フルパスファイル名がリストに追加されてなければ追加する
                if (!_extentionNotHaveHashSHA256[extention].Contains(fileFullPath))
                {
                    _extentionNotHaveHashSHA256[extention].Add(fileFullPath);
                }
                if (!_extentionNotHaveHashSHA384[extention].Contains(fileFullPath))
                {
                    _extentionNotHaveHashSHA384[extention].Add(fileFullPath);
                }
                if (!_extentionNotHaveHashSHA512[extention].Contains(fileFullPath))
                {
                    _extentionNotHaveHashSHA512[extention].Add(fileFullPath);
                }
            }
        }

        /// <summary>
        /// ハッシュアルゴリズム毎のハッシュを獲得しているファイル数を取得します。
        /// </summary>
        /// <param name="algorithmType">ハッシュアルゴリズムの種類</param>
        /// <returns>ハッシュを獲得しているファイル数</returns>
        /// <exception cref="ArgumentException">適切ではないハッシュアルゴリズムを指定</exception>
        public int GetHashAlgorithmsAllCount(FileHashAlgorithm algorithmType)
        {
            return algorithmType switch
            {
                FileHashAlgorithm.SHA256 => _sha256DirList.Count,
                FileHashAlgorithm.SHA384 => _sha384DirList.Count,
                FileHashAlgorithm.SHA512 => _sha512DirList.Count,
                _ => throw new ArgumentException("Invalid hash algorithm."),
            };
        }

        /// <summary>
        /// 検索したファイルの拡張子を取得します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExtensions(FileHashAlgorithm hashAlgorithms)
        {
            return hashAlgorithms switch
            {
                FileHashAlgorithm.SHA256 => _extentionNotHaveHashSHA256.Keys.OrderBy(key => key),
                FileHashAlgorithm.SHA384 => _extentionNotHaveHashSHA384.Keys.OrderBy(key => key),
                FileHashAlgorithm.SHA512 => _extentionNotHaveHashSHA384.Keys.OrderBy(key => key),
                _ => throw new ArgumentException("Invalid hash algorithm."),
            };
        }

        /// <summary>
        /// 検索したファイルの拡張子がある、未取得ファイル数を取得します。
        /// </summary>
        /// <returns></returns>
        public int GetExtentionsCount(string extention, FileHashAlgorithm hashAlgorithm)
        {
            return hashAlgorithm switch
            {
                FileHashAlgorithm.SHA256 => _extentionNotHaveHashSHA256[extention].Count,
                FileHashAlgorithm.SHA384 => _extentionNotHaveHashSHA384[extention].Count,
                FileHashAlgorithm.SHA512 => _extentionNotHaveHashSHA512[extention].Count,
                _ => throw new ArgumentException("Hash Not Implemented."),
            };
        }
    }
}
