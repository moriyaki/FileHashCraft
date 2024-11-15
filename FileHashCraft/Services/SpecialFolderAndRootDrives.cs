﻿/*  SpecialFolderAndRootDrives.cs

    特殊フォルダとルートドライブを取得するクラスです。
 */

using System.IO;
using FileHashCraft.ViewModels.Modules;

namespace FileHashCraft.Services
{
    #region ディレクトリとファイル情報

    /// <summary>
    /// ファイル情報
    /// </summary>
    public class FileItemInformation
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
        /// ドライブが着脱可能かどうか
        /// </summary>
        public bool IsRemovable { get; set; } = false;

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

    public static class SpecialFolderAndRootDrives
    {
        #region フルパスからFileItemInformationを取得

        /// <summary>
        /// 指定されたディレクトリのパスから、FileInformationを生成します。
        /// </summary>
        /// <param name="fullPath">FileInformationを生成するファイルのフルパス</param>
        /// <returns>指定されたディレクトリのFileInformation</returns>
        /// <remarks>
        /// メソッドの動作には、指定されたディレクトリが存在することが前提とされています。
        /// </remarks>
        public static FileItemInformation GetFileInformationFromDirectorPath(string fullPath)
        {
            var driveLetter = fullPath[0].ToString() ?? string.Empty;
            var driveInfo = new DriveInfo(driveLetter);
            var isRemovable = driveInfo.DriveType == DriveType.Removable;
            var isReady = driveInfo.IsReady;

            if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                return new FileItemInformation
                {
                    FullPath = fileInfo.FullName,
                    IsDirectory = false,
                    IsRemovable = isRemovable,
                    IsReady = isReady,
                    LastModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    HasChildren = false,
                };
            }
            else
            {
                var dirInfo = new DirectoryInfo(fullPath);
                return new FileItemInformation
                {
                    FullPath = dirInfo.FullName,
                    IsDirectory = true,
                    IsRemovable = isRemovable,
                    IsReady = isReady,
                    LastModifiedDate = dirInfo.LastWriteTime,
                    HasChildren = HasChildrenDirectories(dirInfo.FullName),
                };
            }
        }

        /// <summary>
        /// 指定されたディレクトリに子ディレクトリが存在するかどうかを判定します。
        /// </summary>
        /// <param name="fullPath">ディレクトリのフルパス</param>
        /// <returns>子ディレクトリが存在する場合は true、それ以外は false</returns>
        public static bool HasChildrenDirectories(string fullPath)
        {
            try
            {
                var HasDirectory = Directory.EnumerateDirectories(fullPath).Any();
                if (HasDirectory) { return true; }
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

        #endregion フルパスからFileItemInformationを取得

        /// <summary>
        /// 特殊フォルダをスキャンして情報を取得します。
        /// </summary>
        /// <returns>特殊フォルダの情報のコレクション</returns>
        public static IEnumerable<FileItemInformation> ScanSpecialFolders()
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
                var fi = GetFileInformationFromDirectorPath(folder);
                if (fi != null) yield return fi;
            }
        }

        /// <summary>
        /// ルートドライブをスキャンして情報を取得します。
        /// </summary>
        /// <returns>ルートドライブのフォルダ情報のコレクション</returns>
        public static IEnumerable<FileItemInformation> ScanDrives()
        {
            foreach (var dir in DriveInfo.GetDrives())
            {
                var item = GetFileInformationFromDirectorPath(dir.Name);
                yield return item;
            }
        }
    }
}