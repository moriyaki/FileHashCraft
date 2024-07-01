using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using FileHashCraft.Models.FileScan;
using FileHashCraft.Services;
using FileHashCraft.Services.Messages;

namespace FileHashCraft.Models.HashCalc
{
    public interface IFileHashCalc
    {
        Task ProcessGetHashFilesAsync();
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

        private int BufferSize { get; } = 8192;

        /// <summary>
        /// 全てのファイルハッシュを計算します。
        /// </summary>
        public async Task ProcessGetHashFilesAsync()
        {
            var drivesDic = GetHashDriveFiles();
            var semaphone = new SemaphoreSlim(drivesDic.Count);

            var tasks = drivesDic.Select(async drive =>
            {
                await semaphone.WaitAsync();
                var beforeFilePath = string.Empty;
                try
                {
                    foreach (var file in drive.Value)
                    {
                        _messenger.Send(new StartCalcingFile(beforeFilePath, file.FileFullPath));
                        await Task.Delay(5);
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
                            switch (_settingsService.HashAlgorithm)
                            {
                                case "SHA-256":
                                    file.SHA256 = hash;
                                    break;
                                case "SHA-384":
                                    file.SHA384 = hash;
                                    break;
                                case "SHA-512":
                                    file.SHA512 = hash;
                                    break;
                                default:
                                    throw new NotImplementedException(nameof(ProcessGetHashFilesAsync));
                            }
                        }
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
            /*
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
                Console.WriteLine($"Access Exception: {e.Message}");
            }
            */
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);
            return string.Empty;
        }

        /// <summary>
        /// ハッシュを取得するファイルをドライブ毎に辞書に振り分けます。
        /// </summary>
        /// <returns>ドライブ毎に辞書に振り分けられたファイルリスト</returns>
        private Dictionary<string, HashSet<HashFile>> GetHashDriveFiles()
        {
            var filesDictionary = new Dictionary<string, HashSet<HashFile>>();
            foreach (var file in _scannedFilesManager.GetAllCriteriaFileName(_settingsService.IsHiddenFileInclude, _settingsService.IsReadOnlyFileInclude))
            {
                var drive = file.FileFullPath[..2];
                if (!filesDictionary.TryGetValue(drive, out HashSet<HashFile>? value))
                {
                    value = [];
                    filesDictionary[drive] = value;
                }

                value.Add(file);
            }
            return filesDictionary;
        }
    }
}
