using System.Diagnostics;
using System.IO;
using FileHashCraft.Properties;

namespace FileHashCraft.Models
{
    /// <summary>
    /// ファイル拡張子を扱うクラス
    /// </summary>
    #region インターフェース
    public interface IExtentionHelper
    {
        /// <summary>
        /// ファイルを拡張子の辞書に登録します。
        /// </summary>
        /// <param name="fileFullPath"></param>
        public void AddFile(string fileFullPath);
        /// <summary>
        /// 拡張子のコレクションを取得します。
        /// </summary>
        public IEnumerable<string> GetExtentions();
        /// <summary>
        /// 拡張子の種類を対象に、拡張子を取得します。
        /// </summary>
        public IEnumerable<string> GetGroupExtentions(FileGroupType fileGroupType);
        /// <summary>
        /// 拡張子を持つファイル数を取得します。
        /// </summary>
        public int GetExtentionsCount(string extention);
        /// <summary>
        /// 拡張子グループのファイル数を取得します。
        /// </summary>
        public int GetGroupExtentionCount(FileGroupType fileGroupType);
        /// <summary>
        /// 拡張子のコレクションを全削除します。
        /// </summary>
        public void Clear();
    }
    #endregion インターフェース
    public class ExtentionHelper : IExtentionHelper
    {
        #region 拡張子の管理
        /// <summary>
        /// 拡張子をキーとしたファイルのリストを持つ辞書
        /// </summary>
        private readonly Dictionary<string, int> _extentionDic = [];

        /// <summary>
        /// フルパスから拡張子リストに登録します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        public void AddFile(string fileFullPath)
        {
            var extention = Path.GetExtension(fileFullPath).ToLower();
            if (extention == null) { return; }

            if (_extentionDic.TryGetValue(extention, out int value))
            {
                _extentionDic[extention] = ++value;
            }
            else
            {
                _extentionDic[extention] = 1;
            }
        }

        /// <summary>
        /// 拡張子のコレクションを取得します。
        /// </summary>
        /// <returns>拡張子コレクション</returns>
        public IEnumerable<string> GetExtentions()
        {
            return _extentionDic.Keys.OrderBy(key => key);
        }

        /// <summary>
        /// 拡張子の種類を対象に、拡張子を取得します。
        /// </summary>
        /// <param name="fileGroupType">拡張子の種類</param>
        /// <returns>拡張子コレクション</returns>
        public IEnumerable<string> GetGroupExtentions(FileGroupType fileGroupType)
        {
            if (fileGroupType == FileGroupType.Others)
            {
                var excludedGroups = new[]
                {
                    FileGroupType.Movies,
                    FileGroupType.Pictures,
                    FileGroupType.Sounds,
                    FileGroupType.Documents,
                    FileGroupType.Applications,
                    FileGroupType.Archives,
                    FileGroupType.SourceCodes,
                    FileGroupType.Registrations
                };

                foreach (var extension in _extentionDic.Keys.OrderBy(key => key))
                {
                    if (excludedGroups.Any(group => FileTypeHelper.GetFileGroupExtention(group).Contains(extension))) { continue; }
                    yield return extension;
                }
            }
            else
            {
                foreach (var extention in FileTypeHelper.GetFileGroupExtention(fileGroupType))
                {
                    yield return extention;
                }
            }
        }

        /// <summary>
        /// 拡張子を持つファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <returns>ファイル数</returns>
        public int GetExtentionsCount(string extention)
        {
            extention = extention.ToLower();
            return _extentionDic.TryGetValue(extention, out int value) ? value : 0;
        }

        /// <summary>
        /// 拡張子グループのファイル数を取得します。
        /// </summary>
        /// <param name="fileGroupType">拡張子グループ</param>
        /// <returns>ファイル数</returns>
        public int GetGroupExtentionCount(FileGroupType fileGroupType)
        {
            var groupCount = 0;
            foreach (var extention in GetGroupExtentions(fileGroupType))
            {
                groupCount += GetExtentionsCount(extention);
            }
            return groupCount;
        }

        /// <summary>
        /// 拡張子コレクションを全削除します
        /// </summary>
        public void Clear()
        {
            _extentionDic.Clear();
        }
        #endregion 拡張子の管理
    }

    #region ファイル種類
    /// <summary>
    /// ファイルの種類の列挙
    /// </summary>
    public enum FileGroupType
    {
        Movies,
        Pictures,
        Sounds,
        Documents,
        Applications,
        Archives,
        SourceCodes,
        Registrations,
        Others,
    }
    #endregion ファイル種類

    /// <summary>
    /// ファイルタイプから表示名や拡張子を取得するクラス
    /// </summary>
    public static class FileTypeHelper
    {
        #region ファイル種類の管理
        /// <summary>
        /// ファイルタイプから表示名を取得します。
        /// </summary>
        /// <param name="type">ファイルタイプ</param>
        /// <returns>ファイルタイプの表示名</returns>
        public static string GetFileGroupName(FileGroupType type)
        {
            return type switch
            {
                FileGroupType.Movies => Resources.LabelMovies,
                FileGroupType.Pictures => Resources.LabelPictures,
                FileGroupType.Sounds => Resources.LabelSounds,
                FileGroupType.Documents => Resources.LabelDocuments,
                FileGroupType.Applications => Resources.LabelApplications,
                FileGroupType.Archives => Resources.LabelArchives,
                FileGroupType.SourceCodes => Resources.LabelSourceCodes,
                FileGroupType.Registrations => Resources.LabelRegistrations,
                _ => Resources.LabelOtherFiles,
            };
        }

        /// <summary>
        /// ファイルタイプから、該当する拡張子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本的にファイルタイプに該当する拡張子、ただしOthersだけは除外する拡張子</returns>
        public static List<string> GetFileGroupExtention(FileGroupType type)
        {
            return type switch
            {
                FileGroupType.Movies => MovieFiles,
                FileGroupType.Pictures => PictureFiles,
                FileGroupType.Sounds => MusicFiles,
                FileGroupType.Documents => DocumentFiles,
                FileGroupType.Applications => ApplicationFiles,
                FileGroupType.Archives => ArchiveFiles,
                FileGroupType.SourceCodes => SourceCodeFiles,
                FileGroupType.Registrations => RegistrationFiles,
                _ => [],
            };
        }
        #endregion ファイル種類の管理

        /// <summary>
        /// 主な動画ファイル
        /// </summary>
        #region 主な動画ファイル
        private static readonly List<string> MovieFiles =
        [
            ".avi",
            ".divx",
            ".mpg",
            ".mpeg",
            ".mpe",
            ".m1v",
            ".m2v",
            ".mp2v",
            ".pva",
            ".evo",
            ".m2p",
            ".sfd",
            ".tp",
            ".trp",
            ".m2t",
            ".m2ts",
            ".mts",
            ".rec",
            ".ssif",
            ".vob",
            ".ifo",
            ".mkv",
            ".mk3d",
            ".webm",
            ".mp4",
            ".m4v",
            ".mp4v",
            ".hdmov",
            ".ismv",
            ".mov",
            ".3gp",
            ".3gpp",
            ".3ga",
            ".3g2",
            ".3gp2",
            ".flv",
            ".f4v",
            ".ogm",
            ".ogv",
            ".rm",
            ".ram",
            ".rmm",
            ".rmvb",
            ".wmv",
            ".wmp",
            ".wm",
            ".asf",
            ".smk",
            ".bik",
            ".fli",
            ".flc",
            ".flic",
            ".roq",
            ".dsm",
            ".dsv",
            ".dsa",
            ".dss",
            ".y4v",
            ".h264",
            ".264",
            ".vc1",
            ".h265",
            ".hm10",
            ".hevc",
            ".obu",
            ".amv",
            ".wtv",
            ".dvr-ms",
            ".mxf",
            ".ivf",
            ".nut",
            ".swf",
        ];
        #endregion 主な動画ファイル
        /// <summary>
        /// 主な画像ファイル
        /// </summary>
        #region 主な画像ファイル
        private static readonly List<string> PictureFiles =
        [
            ".jpeg",
            ".jpg",
            ".gif",
            ".bmp",
            ".tiff",
            ".tif",
            ".raw",
            ".svg",
            ".webp",
            ".odg",
            ".png",
            ".ani",
            ".dib",
            ".emf",
            ".eps",
            ".ps",
            ".heic",
            ".ico",
            ".jfif",
            ".jpe",
            ".pcx",
            ".dcx",
            ".pgm",
            ".ppm",
            ".psd",
            ".psb",
            ".rle",
            ".wmf",
            ".xbm",
            ".xpm",
        ];
        #endregion 主な画像ファイル
        /// <summary>
        /// 主なサウンドファイル
        /// </summary>
        #region 主なサウンドファイル
        private static readonly List<string> MusicFiles =
        [
            ".ac3",
            ".eac3",
            ".dts",
            ".dtshd",
            ".dtsma",
            ".aif",
            ".aifc",
            ".aiff",
            ".alac",
            ".amr",
            ".awb",
            ".ape",
            ".apl",
            ".au",
            ".snd",
            ".cda",
            ".aob",
            ".dsf",
            ".flac",
            ".m4a",
            ".m4b",
            ".aac",
            ".mid",
            ".midi",
            ".rmi",
            ".mka",
            ".weba",
            ".mlp",
            ".mp3",
            ".mpa",
            ".mp2",
            ".m1a",
            ".m2a",
            ".mpc",
            ".ofr",
            ".ofs",
            ".ogg",
            ".oga",
            ".ra",
            ".tak",
            ".tta",
            ".wav",
            ".w64",
            ".wma",
            ".wv",
            ".opus",
            ".spx",
            ".asx",
            ".m3u",
            ".m3u8",
            ".pls",
            ".mpcpl",
            ".xspf",
            ".cue",
            ".fpl",
        ];
        #endregion 主なサウンドファイル
        /// <summary>
        /// 主なドキュメントファイル
        /// </summary>
        #region 主なドキュメントファイル
        private static readonly List<string> DocumentFiles =
        [
            ".doc",
            ".docx",
            ".pdf",
            ".xls",
            ".xlsx",
            ".ppt",
            ".pptx",
            ".txt",
            ".html",
            ".htm",
            ".csv",
            ".odt",
            ".ods",
            ".odp",
            ".pages",
            ".tex",
            ".md",
            ".markdown",
            ".yaml",
        ];
        #endregion 主なドキュメントファイル
        /// <summary>
        /// 主なアプリケーションファイル
        /// </summary>
        #region 主なアプリケーションファイル
        private static readonly List<string> ApplicationFiles =
        [
            ".exe",
            ".ocx",
            ".app",
            ".apk",
            ".deb",
            ".dmg",
            ".rpm",
            ".msi",
            ".jar",
            ".bat",
            ".sh",
            ".ps1",
        ];
        #endregion 主なアプリケーションファイル
        /// <summary>
        /// 主な圧縮ファイル
        /// </summary>
        #region 主な圧縮ファイル
        private static readonly List<string> ArchiveFiles =
        [
            ".7z",
            ".zip",
            ".rar",
            ".cab",
            ".iso",
            ".xz",
            ".txz",
            ".lzma",
            ".tar",
            ".cpio",
            ".bz2",
            ".bzip2",
            ".tbz2",
            ".tbz",
            ".gz",
            ".gzip",
            ".tgz",
            ".tpz",
            ".z",
            ".taz",
            ".lzh",
            ".lha",
            ".arj",
        ];
        #endregion 主な圧縮ファイル
        /// <summary>
        /// 主なソースコードファイル
        /// </summary>
        #region 主なソースコードファイル
        private static readonly List<string> SourceCodeFiles =
        [
            ".c",
            ".cpp",
            ".cc",
            ".cxx",
            ".h",
            ".java",
            ".py",
            ".js",
            ".css",
            ".php",
            ".rb",
            ".swift",
            ".go",
            ".ts",
            ".sql",
            ".json",
            ".xml",
            ".cs",
            ".sln",
            ".vcproj",
            ".rs",
            ".vbs",
        ];
        #endregion 主なソースコードファイル
        /// <summary>
        /// 主な登録ファイル
        /// </summary>
        #region 主な登録ファイル
        private static readonly List<string> RegistrationFiles =
        [
            ".reg",
            ".inf",
            ".regtrans-ms",
            ".pol",
        ];
        #endregion 主な登録ファイル
    }
}
