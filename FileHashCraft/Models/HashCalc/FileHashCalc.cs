using System.IO;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.HashCalc
{
    #region インターフェース
    public interface IFileHashCalc
    {
        Task ProcessGetHashFilesAsync(Dictionary<string, HashSet<HashFile>> filesDictionary);

        Dictionary<string, HashSet<HashFile>> GetHashDriveFiles();
    }
    #endregion インターフェース

    public class FileHashCalc : IFileHashCalc
    {
        public FileHashCalc()
        { throw new NotImplementedException(nameof(FileHashCalc)); }

        private readonly IMessenger _Messanger;
        private readonly IScannedFilesManager _ScannedFilesManager;
        private readonly ISettingsService _SettingsService;

        public FileHashCalc(
            IMessenger messenger,
            IScannedFilesManager scannedFilesManager,
            ISettingsService settingsService
        )
        {
            _Messanger = messenger;
            _ScannedFilesManager = scannedFilesManager;
            _SettingsService = settingsService;
        }

        private int BufferSize { get; } = 1048576;

        /// <summary>
        /// ハッシュを取得するファイルをドライブ毎に辞書に振り分けます。
        /// </summary>
        /// <returns>ドライブ毎に辞書に振り分けられたファイルリスト</returns>
        public Dictionary<string, HashSet<HashFile>> GetHashDriveFiles()
        {
            var duplicateSizeFiles = _ScannedFilesManager.GetAllCriteriaFileName(_SettingsService.IsHiddenFileInclude, _SettingsService.IsReadOnlyFileInclude)
                .GroupBy(f => f.FileSize)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            var filesDictionary = new Dictionary<string, HashSet<HashFile>>();
            foreach (var file in duplicateSizeFiles)
            {
                var drive = Path.GetPathRoot(file.FileFullPath) ?? "";
                if (!filesDictionary.TryGetValue(drive, out HashSet<HashFile>? value))
                {
                    value = [];
                    filesDictionary[drive] = value;
                }

                value.Add(file);
            }
            var drives = filesDictionary.Keys.ToHashSet();
            _Messanger.Send(new CalcingDriveMessage(drives));
            return filesDictionary;
        }

        /// <summary>
        /// 同じファイルサイズの全てのファイルハッシュを計算します。
        /// </summary>
        public async Task ProcessGetHashFilesAsync(Dictionary<string, HashSet<HashFile>> filesDictionary)
        {
            var semaphone = new SemaphoreSlim(filesDictionary.Count);

            var tasks = filesDictionary.Select(async drive =>
            {
                await semaphone.WaitAsync();
                var filePath = string.Empty;
                try
                {
                    foreach (var file in drive.Value)
                    {
                        if (!string.IsNullOrEmpty(file.FileHash))
                        {
                            switch (_SettingsService.HashAlgorithm)
                            {
                                case "SHA-256":
                                    if (file.HashAlgorithm == FileHashAlgorithm.SHA256) continue;
                                    break;

                                case "SHA-384":
                                    if (file.HashAlgorithm == FileHashAlgorithm.SHA384) continue;
                                    break;

                                case "SHA-512":
                                    if (file.HashAlgorithm == FileHashAlgorithm.SHA512) continue;
                                    break;
                            }
                        }

                        using HashAlgorithm hashAlgorithm = _SettingsService.HashAlgorithm switch
                        {
                            "SHA-256" => SHA256.Create(),
                            "SHA-384" => SHA384.Create(),
                            "SHA-512" => SHA512.Create(),
                            _ => SHA256.Create(),
                        };

                        _Messanger.Send(new StartCalcingFileMessage(file.FileFullPath));
                        var hash = await CalculateHashFileAsync(file.FileFullPath, hashAlgorithm);
                        if (string.IsNullOrEmpty(hash)) { continue; }

                        file.HashAlgorithm = _SettingsService.HashAlgorithm switch
                        {
                            "SHA-256" => FileHashAlgorithm.SHA256,
                            "SHA-384" => FileHashAlgorithm.SHA384,
                            "SHA-512" => FileHashAlgorithm.SHA512,
                            _ => throw new NotImplementedException(nameof(ProcessGetHashFilesAsync)),
                        };

                        file.FileHash = hash;
                        filePath = file.FileFullPath;
                    }
                }
                finally
                {
                    _Messanger.Send(new EndCalcingFileMessage(filePath));
                    semaphone.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);
            _Messanger.Send(new FinishedCalcingFileMessage());
        }

        /// <summary>
        /// ファイルのハッシュを計算します。
        /// </summary>
        /// <param name="filePath">ファイルのフルパス</param>
        /// <param name="hashAlgorithm">ハッシュアルゴリズム</param>
        /// <returns>ファイルのハッシュ</returns>
        private async Task<string> CalculateHashFileAsync(string filePath, HashAlgorithm hashAlgorithm)
        {
            try
            {
                await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);

                byte[] hashValue = await hashAlgorithm.ComputeHashAsync(fileStream);
                if (hashAlgorithm.Hash != null)
                {
                    return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException e)
            {
                DebugManager.ErrorWrite($"Access Exception: {e.Message}");
            }
            return string.Empty;
        }
    }
}