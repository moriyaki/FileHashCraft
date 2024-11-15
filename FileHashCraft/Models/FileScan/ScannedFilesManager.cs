﻿using System.IO;
using System.Text.RegularExpressions;

namespace FileHashCraft.Models.FileScan
{
    #region インターフェース
    public interface IScannedFilesManager
    {
        /// <summary>
        /// 全管理対象ファイルディクショナリに追加します。
        /// </summary>
        void AddFile(string fileFullPath, FileHashAlgorithm hashAlgorithm = FileHashAlgorithm.SHA256, string fileHash = "");

        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        HashSet<HashFile> AllFiles { get; }

        /// <summary>
        /// 属性条件に合致する全てのファイル数を取得します。
        /// </summary>
        int GetAllFilesCount(bool includeHidden, bool includeReadOnly);

        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        int GetAllCriteriaFilesCount(bool includeHidden, bool includeReadOnly);

        /// <summary>
        /// 検索条件に合致するファイルを取得する
        /// </summary>
        HashSet<HashFile> GetAllCriteriaFileName(bool includeHidden, bool includeReadOnly);

        /// <summary>
        /// ファイルが検索条件に合致しているかを取得します。
        /// </summary>
        bool IsCriteriaFile(string fileFullPath, bool includeHidden, bool includeReadOnly);
    }
    #endregion インターフェース

    public class ScannedFilesManager : IScannedFilesManager
    {
        public ScannedFilesManager()
        { throw new NotImplementedException(nameof(ScannedFilesManager)); }

        private readonly IFileSearchCriteriaManager _FileSearchCriteriaManager;

        public ScannedFilesManager(
            IFileSearchCriteriaManager fileSearchCriteriaManager
        )
        {
            _FileSearchCriteriaManager = fileSearchCriteriaManager;
        }

        /// <summary>
        /// ディレクトリをキーとした全てのファイルを持つファイルの辞書
        /// </summary>
        public HashSet<HashFile> AllFiles { get; } = [];

        /// <summary>
        /// ファイルを追加します
        /// </summary>
        /// <param name="fileFullPath">追加するファイルのフルパス</param>
        /// <param name="hashAlgorithm">ファイルハッシュのアルゴリズム</param>
        /// <param name="fileHash">ファイルのハッシュ</param>
        public void AddFile(string fileFullPath, FileHashAlgorithm hashAlgorithm = FileHashAlgorithm.SHA256, string fileHash = "")
        {
            var fileInfo = new FileInfo(fileFullPath);
            var existingFIle = AllFiles.FirstOrDefault(f => f.FileFullPath == fileFullPath);
            if (existingFIle != null)
            {
                AllFiles.Remove(existingFIle);
            }

            var hashFile = new HashFile(fileFullPath, hashAlgorithm, fileHash);
            AllFiles.Add(hashFile);
        }

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private readonly object lockObject = new();

        /// <summary>
        /// ファイルが属性対象かどうかを取得します。
        /// </summary>
        /// <param name="file">HashFile型のファイル情報</param>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>属性対象かどうか</returns>
        private static bool MatchFileAttributesCriteria(HashFile file, bool includeHidden, bool includeReadOnly)
        {
            var isReadOnly = (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            var matchesReadOnlyCriteria = includeReadOnly || !isReadOnly;

            var isHidden = (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            var matchesHiddenCriteria = includeHidden || !isHidden;

            return matchesReadOnlyCriteria && matchesHiddenCriteria;
        }

        /// <summary>
        /// ワイルドカード文字列を正規表現文字列に変換します。
        /// </summary>
        /// <param name="pattern">ワイルドカード文字列</param>
        /// <returns>正規表現文字列</returns>
        private static string WildcardToRegex(string pattern)
        {
            return Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") ?? string.Empty;
        }

        /// <summary>
        /// ワイルドカードを含むファイル名をRegex型に変換します。
        /// </summary>
        /// <param name="wildcardPattern">ワイルドカードを含むファイル名</param>
        /// <returns>Regex型</returns>
        public static Regex WildcardToRegexPattern(string wildcardPattern)
        {
            if (!wildcardPattern.Contains('.'))
            {
                // ワイルドカードに拡張子がない時の処理
                var filePattern = WildcardToRegex(wildcardPattern);
                return new Regex($"^{filePattern}$", RegexOptions.IgnoreCase);
            }

            // ワイルドカードに拡張子がある時の処理
            var fileName = Path.GetFileNameWithoutExtension(wildcardPattern);
            var fileNamePattern = WildcardToRegex(fileName);

            var fileExtention = Path.GetExtension(wildcardPattern);
            if (fileExtention == ".*")
            {
                // 拡張子全ての時、例えば"file"と"file.txt"の両方をヒットさせる
                return new Regex($"^{fileNamePattern}(?:\\..+)?$", RegexOptions.IgnoreCase);
            }
            else
            {
                // 拡張子を正規表現文字列に変換し、ファイル名と連結する
                var extensionPattern = WildcardToRegex(fileExtention);
                return new Regex($"^{fileNamePattern}{extensionPattern}$", RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// 属性条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>属性条件に合致するファイル</returns>
        public int GetAllFilesCount(bool includeHidden, bool includeReadOnly)
        {
            return AllFiles.Count(c => MatchFileAttributesCriteria(c, includeHidden, includeReadOnly));
        }

        /// <summary>
        /// 検索条件に合致する全てのファイル数を取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを含むかどうか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを含むかどうか</param>
        /// <returns>検索条件合致ファイル数</returns>
        public int GetAllCriteriaFilesCount(bool includeHidden, bool includeReadOnly)
        {
            lock (lockObject)
            {
                return GetAllCriteriaFileName(includeHidden, includeReadOnly).Count;
            }
        }

        /// <summary>
        /// 検索条件に合致するファイルを取得します。
        /// </summary>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns>検索条件合致ファイル</returns>
        public HashSet<HashFile> GetAllCriteriaFileName(bool includeHidden, bool includeReadOnly)
        {
            var files = new HashSet<HashFile>();
            foreach (var criteria in _FileSearchCriteriaManager.AllCriteria)
            {
                switch (criteria.SearchOption)
                {
                    case FileSearchOption.Extention:
                        files.UnionWith(GetFilesByExtention(criteria, includeHidden, includeReadOnly));
                        break;

                    case FileSearchOption.Wildcard:
                        files.UnionWith(GetFilesByWildcard(criteria, includeHidden, includeReadOnly));
                        break;

                    case FileSearchOption.Regex:
                        files.UnionWith(GetFilesByRegex(criteria, includeHidden, includeReadOnly));
                        break;

                    default:
                        throw new NotFiniteNumberException("GetAllCriteriaFilesCount");
                }
            }
            return files;
        }

        #region ファイルの検索条件一致ファイル取得

        /// <summary>
        /// 拡張子の検索条件に合致するファイルを返す
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns>検索条件に合致するファイル</returns>
        private IEnumerable<HashFile> GetFilesByExtention(FileSearchCriteria criteria, bool includeHidden, bool includeReadOnly)
        {
            return AllFiles.Where(f =>
                 String.Equals(criteria.SearchPattern,          // 検索条件の拡張子
                 Path.GetExtension(f.FileFullPath),             // ファイルの拡張子
                 StringComparison.CurrentCultureIgnoreCase)     // 大文字小文字の無視
             && MatchFileAttributesCriteria(f, includeHidden, includeReadOnly)).Select(f => f);
        }

        /// <summary>
        /// ワイルドカードの検索条件に合致するファイルを返す
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns>検索条件に合致するファイル</returns>
        private IEnumerable<HashFile> GetFilesByWildcard(FileSearchCriteria criteria, bool includeHidden, bool includeReadOnly)
        {
            var wildcardRegex = WildcardToRegexPattern(criteria.SearchPattern);
            return AllFiles.Where(f => wildcardRegex.IsMatch(f.FileFullPath)
             && MatchFileAttributesCriteria(f, includeHidden, includeReadOnly)).Select(f => f);
        }

        /// <summary>
        /// 正規表現の検索条件に合致するファイルを返す
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns>検索条件に合致するファイル</returns>
        private IEnumerable<HashFile> GetFilesByRegex(FileSearchCriteria criteria, bool includeHidden, bool includeReadOnly)
        {
            var regex = new Regex(criteria.SearchPattern);
            return AllFiles.Where(f => regex.IsMatch(f.FileFullPath)
             && MatchFileAttributesCriteria(f, includeHidden, includeReadOnly)).Select(f => f);
        }

        #endregion ファイルの検索条件一致ファイル取得

        /// <summary>
        /// ファイルが検索条件に合致しているかを取得します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns>検索条件に合致してるかどうか</returns>
        public bool IsCriteriaFile(string fileFullPath, bool includeHidden, bool includeReadOnly)
        {
            var file = AllFiles.FirstOrDefault(c => c.FileFullPath == fileFullPath);
            if (file == null) { return false; }
            foreach (var criteria in _FileSearchCriteriaManager.AllCriteria)
            {
                switch (criteria.SearchOption)
                {
                    case FileSearchOption.Extention:
                        if (IsMatchingExtention(criteria, file, includeHidden, includeReadOnly))
                        { return true; }
                        break;

                    case FileSearchOption.Wildcard:
                        if (IsMatchingWildcard(criteria, file, includeHidden, includeReadOnly))
                        { return true; }
                        break;

                    case FileSearchOption.Regex:
                        if (IsMatchingRegex(criteria, file, includeHidden, includeReadOnly))
                        { return true; }
                        break;

                    default:
                        throw new NotFiniteNumberException("IsCriteriaFile");
                }
            }
            return false;
        }

        #region ファイルの検索条件一致チェック

        /// <summary>
        /// ファイルが拡張子の検索条件に一致しているかどうか
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="file">ファイル情報</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns></returns>
        private static bool IsMatchingExtention(FileSearchCriteria criteria, HashFile file, bool includeHidden, bool includeReadOnly)
        {
            return String.Equals(
                criteria.SearchPattern,                 // 検索条件の拡張子
                Path.GetExtension(file.FileFullPath),   // ファイルの拡張子
                StringComparison.OrdinalIgnoreCase)     // 大文字小文字の無視
             && MatchFileAttributesCriteria(file, includeHidden, includeReadOnly);
        }

        /// <summary>
        /// ファイルがワイルドカードの検索条件に一致しているかどうか
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="file">ファイル情報</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns></returns>
        private static bool IsMatchingWildcard(FileSearchCriteria criteria, HashFile file, bool includeHidden, bool includeReadOnly)
        {
            var wildcardRegex = WildcardToRegexPattern(criteria.SearchPattern);
            return wildcardRegex.IsMatch(file.FileFullPath) && MatchFileAttributesCriteria(file, includeHidden, includeReadOnly);
        }

        /// <summary>
        /// ファイルが正規表現の検索条件に一致しているかどうか
        /// </summary>
        /// <param name="criteria">検索条件</param>
        /// <param name="file">ファイル情報</param>
        /// <param name="includeHidden">隠しファイルを対象にするか</param>
        /// <param name="includeReadOnly">読み取り専用ファイルを対象にするか</param>
        /// <returns></returns>
        private static bool IsMatchingRegex(FileSearchCriteria criteria, HashFile file, bool includeHidden, bool includeReadOnly)
        {
            var regex = new Regex(criteria.SearchPattern);
            return regex.IsMatch(file.FileFullPath) && MatchFileAttributesCriteria(file, includeHidden, includeReadOnly);
        }

        #endregion ファイルの検索条件一致チェック
    }
}