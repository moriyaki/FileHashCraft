﻿using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;

namespace FilOps.Models
{

    #region ディレクトリとファイル情報
    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileInformation
    {
        /// <summary>
        /// ディレクトリのフルパス
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// ドライブが準備できているかどうか
        /// </summary>
        public bool IsReady { get; set; } = false;

        /// <summary>
        /// ファイルがディレクトリかどうか
        /// </summary>
        public bool IsDirectory { get; set; } = false;

        /// <summary>
        /// 子ディレクトリが存在するかどうか
        /// </summary>
        public bool HasChildren { get; set; } = false;

        /// <summary>
        /// ファイルの更新日時
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// ファイルのサイズ
        /// </summary>
        public long FileSize { get; set; } = 0;

    }
    #endregion ディレクトリとファイル情報

    public class FileSystemManager
    {
        #region Singleton
        private static readonly FileSystemManager _instance = new();
        public static FileSystemManager Instance => _instance;
        private FileSystemManager() { }
        #endregion Singleton

        #region ディレクトリとファイルのスキャン関連
        /// <summary>
        /// 指定されたディレクトリに子ディレクトリが存在するかどうかを判定します。
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>子ディレクトリが存在する場合は true、それ以外は false</returns>
        private static bool HasChildrenDirectories(string full_path)
        {
            try
            {
                var HasDirectory = Directory.EnumerateDirectories(full_path).Any();
                if (HasDirectory)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException ||
                    ex is DirectoryNotFoundException ||
                    ex is IOException)
                {
                    return false;
                }
                throw;
            }
            return false;
        }

        /// <summary>
        /// 特殊フォルダをスキャンして情報を取得します。
        /// </summary>
        /// <returns>特殊フォルダの情報のコレクション</returns>
        public IEnumerable<FileInformation> SpecialFolderScan()
        {
            IEnumerable<string> special_folder_path =
            [
                WindowsAPI.GetPath(KnownFolder.Objects3D),
                WindowsAPI.GetPath(KnownFolder.Downloads),
                WindowsAPI.GetPath(KnownFolder.Desktop),
                WindowsAPI.GetPath(KnownFolder.Documents),
                WindowsAPI.GetPath(KnownFolder.Pictures),
                WindowsAPI.GetPath(KnownFolder.Videos),
                WindowsAPI.GetPath(KnownFolder.Music),
                WindowsAPI.GetPath(KnownFolder.User),
            ];

            foreach (var folder in special_folder_path)
            {
                yield return GetFileInformationFromDirectorPath(folder);
            }
        }

        /// <summary>
        /// ルートドライブをスキャンして情報を取得します。
        /// </summary>
        /// <returns>ルートドライブのフォルダ情報のコレクション</returns>
        public static IEnumerable<FileInformation> DriveScan()
        {
            foreach (var dir in DriveInfo.GetDrives())
            {
                FileInformation item;
                item = GetFileInformationFromDirectorPath(dir.Name, dir.IsReady);
                yield return item;
            }
        }

        /// <summary>
        /// 指定したディレクトリ内のフォルダとファイルをスキャンします
        /// </summary>
        /// <param name="path">スキャンするディレクトリのパス</param>
        /// <returns>ファイル情報のコレクション</returns>
        public static IEnumerable<FileInformation> FileItemScan(string path, bool isFilesInclude)
        {
            IEnumerable<string> folders;
            try
            {
                folders = Directory.EnumerateDirectories(path);
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is IOException)
                {
                    yield break;
                }
                throw;
            }


            foreach (var folder in folders)
            {
                FileAttributes attributes = File.GetAttributes(folder);
                if ((attributes & FileAttributes.System) != FileAttributes.System)
                {
                    yield return GetFileInformationFromDirectorPath(folder);
                }
            }
            if (isFilesInclude)
            {
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    try
                    {
                        using FileStream fs = File.OpenRead(file);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (IOException) { }

                    yield return GetFileInformationFromDirectorPath(file);
                }
            }
        }

        #endregion ディレクトリとファイルのスキャン関連

        /// <summary>
        /// 指定されたディレクトリのパスから、FileInformationを生成します。
        /// </summary>
        /// <param name="path">FileInformationを生成するファイルのフルパス</param>
        /// <param name="isReady">ドライブが準備されているかどうか。既定値は true です。</param>
        /// <returns>指定されたディレクトリのFileInformation</returns>
        /// <remarks>
        /// メソッドの動作には、指定されたディレクトリが存在することが前提とされています。
        /// </remarks>
        public static FileInformation GetFileInformationFromDirectorPath(string path, bool isReady = true)
        {
            FileInformation? item;
            if (Directory.Exists(path) || !isReady)
            {
                var fileInfo = new FileInfo(path);
                item = new FileInformation
                {
                    FullPath = fileInfo.FullName,
                    IsDirectory = true,
                    IsReady = isReady,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    HasChildren = HasChildrenDirectories(fileInfo.FullName),
                };
                return item;
            }
            else
            {
                var fileInfo = new FileInfo(path);
                item = new FileInformation
                {
                    FullPath = fileInfo.FullName,
                    IsDirectory = false,
                    IsReady = isReady,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    HasChildren = false,
                };
                return item;
            }
        }
    }
}