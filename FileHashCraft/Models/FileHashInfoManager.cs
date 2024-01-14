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
        #region シングルトンと必須パブリック変数宣言
        public static FileHashInfoManager FileHashInstance { get; } = new();
        private FileHashInfoManager() { }

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// ファイルの総数
        /// </summary>
        public int AllCount = 0;
        /// <summary>
        /// ファイルハッシュのアルゴリズム毎のハッシュを持つファイル
        /// </summary>
        public readonly FileAlgorithmStatus StatusSHA256 = new(FileHashAlgorithm.SHA256);
        public readonly FileAlgorithmStatus StatusSHA384 = new(FileHashAlgorithm.SHA384);
        public readonly FileAlgorithmStatus StatusSHA512 = new(FileHashAlgorithm.SHA512);
        /// <summary>
        /// ファイルハッシュのアルゴリズムによる、拡張子毎にハッシュを持たないファイル
        /// </summary>
        public readonly NotHaveHashExtentions NotHaveHashSHA256 = new(FileHashAlgorithm.SHA256);
        public readonly NotHaveHashExtentions NotHaveHashSHA384 = new(FileHashAlgorithm.SHA384);
        public readonly NotHaveHashExtentions NotHaveHashSHA512 = new(FileHashAlgorithm.SHA512);

        /// <summary>
        /// 全ファイルハッシュリストのデータ
        /// </summary>
        private List<HashDir> _fileHashDirList = new(10000);
        #endregion シングルトンと必須パブリック変数宣言

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

        #region ファイルハッシュ取得状況管理クラス
        /// <summary>
        /// ファイルハッシュ取得状況管理クラス
        /// </summary>
        public class FileAlgorithmStatus
        {
            /// <summary>
            /// 引数無しは許容しない
            /// </summary>
            /// <exception cref="NotImplementedException">引数無しは許容しない</exception>
            public FileAlgorithmStatus()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// アルゴリズムを引数としたコンストラクタ
            /// </summary>
            /// <param name="hashAlgorithm">ファイルのハッシュアルゴリズム</param>
            public FileAlgorithmStatus(FileHashAlgorithm hashAlgorithm)
            {
                HashAlgorithm = hashAlgorithm;
            }

            /// <summary>
            /// ファイルのハッシュアルゴリズム
            /// </summary>
            public FileHashAlgorithm HashAlgorithm { get; }

            /// <summary>
            /// このクラスのアルゴリズムのハッシュを持つフルパスファイル名リストコレクション
            /// </summary>
            private readonly List<string> _hashHaveFullPathList = new(10000);

            /// <summary>
            /// ハッシュを持つファイル数を取得します。
            /// </summary>
            public int Count
            {
                get => _hashHaveFullPathList.Count;
            }

            /// <summary>
            /// ハッシュを持つファイルのフルパスを登録します。
            /// </summary>
            /// <param name="fileFullPth"></param>
            public void AddHashFilePath(string fileFullPth)
            {
                _hashHaveFullPathList.Add(fileFullPth);
                if (_hashHaveFullPathList.Count % 10000 == 0)
                {
                    _hashHaveFullPathList.Capacity += 10000;
                }
            }
        }
        #endregion ファイルハッシュ取得状況管理クラス

        #region 拡張子毎のハッシュを持たないファイルの管理クラス
        public class NotHaveHashExtentions
        {
            /// <summary>
            /// 引数無しは許容しない
            /// </summary>
            /// <exception cref="NotImplementedException">引数無しは許容しない</exception>
            public NotHaveHashExtentions()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// アルゴリズムを引数としたコンストラクタ
            /// </summary>
            /// <param name="hashAlgorithm">ファイルのハッシュアルゴリズム</param>
            public NotHaveHashExtentions(FileHashAlgorithm hashAlgorithm)
            {
                HashAlgorithm = hashAlgorithm;
            }

            /// <summary>
            /// ファイルのハッシュアルゴリズム
            /// </summary>
            public FileHashAlgorithm HashAlgorithm { get; }

            /// <summary>
            /// ファイルハッシュを持たない拡張子をキーとしたフルパスリスト辞書
            /// </summary>
            private readonly Dictionary<string, List<string>> _notHaveHashFile = [];

            /// <summary>
            /// 拡張子毎のハッシュを持たないファイルの数を取得します。
            /// </summary>
            /// <param name="extention"></param>
            /// <returns></returns>
            public int Count(string extention)
            {
                var value = new List<string>();
                if (!_notHaveHashFile.TryGetValue(extention, out value))
                {
                    _notHaveHashFile[extention] = [];
                    return 0;
                }
                return _notHaveHashFile[extention].Count;
            }

            /// <summary>
            /// ファイルハッシュを持たないファイルを登録します。
            /// </summary>
            /// <param name="extention">ハッシュを持たないファイルの拡張子</param>
            /// <param name="fileFullPath">ハッシュを持たないファイルのフルパス</param>
            public void Add(string extention, string fileFullPath)
            {
                var value = new List<string>();
                if (!_notHaveHashFile.TryGetValue(extention, out value))
                {
                    _notHaveHashFile[extention] = [];
                }
                _notHaveHashFile[extention].Add(fileFullPath);
            }

            /// <summary>
            /// ファイルハッシュを持たないファイルがある拡張子リストを取得します。
            /// </summary>
            /// <returns>ファイルハッシュを持たないファイルがある拡張子コレクション</returns>
            public IEnumerable<string> GetExtentions()
            {
                return _notHaveHashFile.Keys.OrderBy(keys => keys);
            }
        }
        #endregion 拡張子毎のハッシュを持たないファイルの管理クラス

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
        /// ファイルを追加変更削除し、ハッシュ取得状況を取得します。
        /// </summary>
        /// <param name="directoryFullPath">走査するディレクトリのフルパス</param>
        /// <returns>走査するディレクトリの情報</returns>
        public void ScanFiles(string directoryFullPath)
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

            // ストレージに存在するファイル毎に処理
            foreach (var fullPath in storageFiles)
            {
                var fileInfo = new FileInfo(fullPath);
                if (xmlFiles?.Contains(fullPath) == true)
                {
                    AllCount++;

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
                            StatusSHA256.AddHashFilePath(fullPath);
                        }
                        if (!string.IsNullOrEmpty(existedFile.SHA384))
                        {
                            StatusSHA384.AddHashFilePath(fullPath);
                        }
                        if (!string.IsNullOrEmpty(existedFile.SHA512))
                        {
                            StatusSHA512.AddHashFilePath(fullPath);
                        }
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
        }

        /// <summary>
        /// 拡張子をキーに、未取得ハッシュのファイルフルパスを登録します。
        /// </summary>
        /// <param name="directoryFullPath">ディレクトリのフルパス</param>
        public void ScanFileExtentions(string directoryFullPath)
        {
            // ディレクトリのファイルを取得する
            var xmlItem = _fileHashDirList.Find(f => f.FullPath == directoryFullPath);
            if (xmlItem == null) { return; }

            foreach (var file in xmlItem.Files)
            {
                // フルパスを生成し、そこから拡張子を取得する
                var fileFullPath = Path.Combine(directoryFullPath, file.Name);
                var extention = Path.GetExtension(fileFullPath).ToLowerInvariant();

                // 既にXML内に存在していたら更新チェック
                var existedFile = xmlItem?.Files.Find(x => x.Name == Path.GetFileName(fileFullPath));

                // フルパスファイル名がハッシュを持たず、リストに追加されてなければ追加する
                if (string.IsNullOrEmpty(existedFile?.SHA256))
                {
                    NotHaveHashSHA256.Add(extention, fileFullPath);
                }
                if (string.IsNullOrEmpty(existedFile?.SHA384))
                {
                    NotHaveHashSHA384.Add(extention, fileFullPath);
                }
                if (string.IsNullOrEmpty(existedFile?.SHA512))
                {
                    NotHaveHashSHA512.Add(extention, fileFullPath);
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
                FileHashAlgorithm.SHA256 => StatusSHA256.Count,
                FileHashAlgorithm.SHA384 => StatusSHA384.Count,
                FileHashAlgorithm.SHA512 => StatusSHA512.Count,
                _ => throw new ArgumentException("Invalid hash algorithm."),
            };
        }

        /// <summary>
        /// ファイルハッシュを持たないファイルがある拡張子を取得します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetExtensions(FileHashAlgorithm hashAlgorithms)
        {
            return hashAlgorithms switch
            {
                FileHashAlgorithm.SHA256 => NotHaveHashSHA256.GetExtentions(),
                FileHashAlgorithm.SHA384 => NotHaveHashSHA384.GetExtentions(),
                FileHashAlgorithm.SHA512 => NotHaveHashSHA512.GetExtentions(),
                _ => throw new ArgumentException("Invalid hash algorithm."),
            };
        }

        /// <summary>
        /// ファイルハッシュを持たないファイルを拡張子毎に総数を取得します。
        /// </summary>
        /// <returns></returns>
        public int GetExtentionsCount(string extention, FileHashAlgorithm hashAlgorithm)
        {
            return hashAlgorithm switch
            {
                FileHashAlgorithm.SHA256 => NotHaveHashSHA256.Count(extention),
                FileHashAlgorithm.SHA384 => NotHaveHashSHA384.Count(extention),
                FileHashAlgorithm.SHA512 => NotHaveHashSHA512.Count(extention),
                _ => throw new ArgumentException("Hash Not Implemented."),
            };
        }
    }
}
