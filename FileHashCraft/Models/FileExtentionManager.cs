using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileHashCraft.Models
{
    /// <summary>
    /// ファイル拡張子を扱うクラス
    /// </summary>
    public sealed class FileExtentionManager
    {
        #region シングルトン
        public static FileExtentionManager Instance { get; } = new();
        private FileExtentionManager() { }
        #endregion シングルトン

        /// <summary>
        /// 拡張子をキーとしたファイルのリストを持つ辞書
        /// </summary>
        private readonly Dictionary<string, List<string>> _extentionDic = [];

        /// <summary>
        /// フルパスから拡張子リストに登録します。
        /// </summary>
        /// <param name="fileFullPath">ファイルのフルパス</param>
        public void AddFile(string fileFullPath)
        {
            var extention = Path.GetExtension(fileFullPath).ToLower();
            if (extention != null)
            {
                if (!_extentionDic.TryGetValue(extention, out List<string>? value))
                {
                    value = [];
                    _extentionDic[extention] = value;
                }

                value.Add(fileFullPath);
            }
        }

        /// <summary>
        /// 拡張子を持つファイル数を取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <returnsファイル数></returns>
        public int GetExtentionsCount(string extention)
        {
            if (!_extentionDic.TryGetValue(extention.ToLower(), out List<string>? value))
            {
                value = [];
                return 0;
            }
            return _extentionDic[extention.ToLower()].Count;
        }

        /// <summary>
        /// 拡張子のコレクションを取得します。
        /// </summary>
        /// <returns>拡張子コレクション</returns>
        public IEnumerable<string> GetExtensions()
        {
            return _extentionDic.Keys.OrderBy(key => key);
        }
    }

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

    /// <summary>
    /// ファイルタイプから表示名や拡張子を取得するクラス
    /// </summary>
    public class FileTypeHelper
    {
        /// <summary>
        /// ファイルタイプから表示名を取得します。
        /// </summary>
        /// <param name="type">ファイルタイプ</param>
        /// <returns>ファイルタイプの表示名</returns>
        public static string GetFileGroupName(FileGroupType type)
        {
            return type switch
            {
                FileGroupType.Movies => "動画ファイル",
                FileGroupType.Pictures => "画像ファイル",
                FileGroupType.Sounds => "サウンドファイル",
                FileGroupType.Documents => "ドキュメントファイル",
                FileGroupType.Applications => "アプリケーションファイル",
                FileGroupType.Archives => "圧縮ファイル",
                FileGroupType.SourceCodes => "ソースコードファイル",
                FileGroupType.Registrations => "登録ファイル",
                _ => "その他のファイル",
            };
        }

        /// <summary>
        /// ファイルタイプから、該当する拡張子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本的にファイルタイプに該当する拡張子、ただしOthersだけは除外する拡張子</returns>
        public List<string> GetFileGroupExtention(FileGroupType type)
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

        /// <summary>
        /// 主な動画ファイル
        /// </summary>
        private readonly List<string> MovieFiles =
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
        /// <summary>
        /// 主な画像ファイル
        /// </summary>
        private readonly List<string> PictureFiles =
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
        /// <summary>
        /// 主なサウンドファイル
        /// </summary>
        private readonly List<string> MusicFiles =
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
        /// <summary>
        /// 主なドキュメントファイル
        /// </summary>
        private readonly List<string> DocumentFiles =
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

        /// <summary>
        /// 主なアプリケーションファイル
        /// </summary>
        private readonly List<string> ApplicationFiles =
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
        /// <summary>
        /// 主な圧縮ファイル
        /// </summary>
        private readonly List<string> ArchiveFiles =
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
        /// <summary>
        /// 主なソースコードファイル
        /// </summary>
        private readonly List<string> SourceCodeFiles =
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
        /// <summary>
        /// 主な登録ファイル
        /// </summary>
        private readonly List<string> RegistrationFiles =
        [
            ".reg",
            ".inf",
            ".regtrans-ms",
            ".pol",
        ];
    }

}
