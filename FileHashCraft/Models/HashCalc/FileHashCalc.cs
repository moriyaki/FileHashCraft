using System.IO;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Models.Helpers;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.HashCalc
{
    public interface IFileHashCalc
    {
        Task ProcessGetHashFilesAsync(Dictionary<string, HashSet<HashFile>> filesDictionary);
        Dictionary<string, HashSet<HashFile>> GetHashDriveFiles();
    }
    public class FileHashCalc : IFileHashCalc
    {
        public FileHashCalc() { throw new NotImplementedException(nameof(FileHashCalc)); }

        private readonly IMessenger _messenger;
        private readonly IScannedFilesManager _scannedFilesManager;
        private readonly ISettingsService _settingsService;
        public FileHashCalc(
            IMessenger messenger,
            IScannedFilesManager scannedFilesManager,
            ISettingsService settingsService
        )
        {
            _messenger = messenger;
            _scannedFilesManager = scannedFilesManager;
            _settingsService = settingsService;
        }

        private int BufferSize { get; } = 1048576;

        /// <summary>
        /// ハッシュを取得するファイルをドライブ毎に辞書に振り分けます。
        /// </summary>
        /// <returns>ドライブ毎に辞書に振り分けられたファイルリスト</returns>
        public Dictionary<string, HashSet<HashFile>> GetHashDriveFiles()
        {
            var duplicateSizeFiles = _scannedFilesManager.GetAllCriteriaFileName(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude)
                .GroupBy(f => f.FileSize)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            var filesDictionary = new Dictionary<string, HashSet<HashFile>>();
            foreach (var file in duplicateSizeFiles)
            {
                DebugManager.InfoWrite($"{file.FileFullPath},{file.FileSize}");
                var drive = Path.GetPathRoot(file.FileFullPath) ?? "";
                if (!filesDictionary.TryGetValue(drive, out HashSet<HashFile>? value))
                {
                    value = [];
                    filesDictionary[drive] = value;
                }

                value.Add(file);
            }
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
                var beforeFilePath = string.Empty;
                try
                {
                    foreach (var file in drive.Value)
                    {
                        _messenger.Send(new StartCalcingFile(beforeFilePath, file.FileFullPath));
                        using HashAlgorithm hashAlgorithm = _settingsService.HashAlgorithm switch
                        {
                            "SHA-512" => SHA512.Create(),
                            "SHA-384" => SHA384.Create(),
                            "SHA-256" => SHA256.Create(),
                            _ => SHA256.Create(),
                        };

                        var hash = await CalculateHashFileAsync(file.FileFullPath, hashAlgorithm);
                        if (!string.IsNullOrEmpty(hash))
                        {
                            file.HashAlgorithm = _settingsService.HashAlgorithm switch
                            {
                                "SHA-256" => FileHashAlgorithm.SHA256,
                                "SHA-384" => FileHashAlgorithm.SHA384,
                                "SHA-512" => FileHashAlgorithm.SHA512,
                                _ => throw new NotImplementedException(nameof(ProcessGetHashFilesAsync)),
                            };
                        }
                        file.FileHash = hash;
                        beforeFilePath = file.FileFullPath;
                    }
                }
                finally
                {
                    _messenger.Send(new EndCalcingFile(beforeFilePath));
                    semaphone.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);
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
