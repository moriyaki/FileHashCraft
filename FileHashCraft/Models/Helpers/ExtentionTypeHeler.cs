/*  ExtentionTypeHeler.cs

    拡張子グループから拡張子を取得するヘルパークラスです。
 */
using FileHashCraft.Properties;

namespace FileHashCraft.Models.Helpers
{
    #region 拡張子グループ
    /// <summary>
    /// 拡張子グループの列挙
    /// </summary>
    public enum FileGroupType
    {
        Movies,
        Pictures,
        Musics,
        Documents,
        Applications,
        Archives,
        SourceCodes,
        Registrations,
        Others,
    }
    #endregion ファイル種類

    /// <summary>
    /// 拡張子グループから表示名や拡張子を取得するクラス
    /// </summary>
    public static class ExtentionTypeHelper
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
                FileGroupType.Movies => Resources.LabelExtentionFiles_Movies,
                FileGroupType.Pictures => Resources.LabelExtentionFiles_Pictures,
                FileGroupType.Musics => Resources.LabelExtentionFiles_Sounds,
                FileGroupType.Documents => Resources.LabelExtentionFiles_Documents,
                FileGroupType.Applications => Resources.LabelExtentionFiles_Applications,
                FileGroupType.Archives => Resources.LabelExtentionFiles_Archives,
                FileGroupType.SourceCodes => Resources.LabelExtentionFiles_SourceCodes,
                FileGroupType.Registrations => Resources.LabelExtentionFiles_Registrations,
                _ => Resources.LabelExtentionFiles_OtherFiles,
            };
        }

        /// <summary>
        /// 拡張子グループから、該当する拡張子を取得します。
        /// </summary>
        /// <param name="type"></param>
        /// <returns>基本的にファイルタイプに該当する拡張子、ただしOthersだけは空</returns>
        public static HashSet<string> GetFileGroupExtention(FileGroupType type)
        {
            return type switch
            {
                FileGroupType.Movies => MovieFiles,
                FileGroupType.Pictures => PictureFiles,
                FileGroupType.Musics => MusicFiles,
                FileGroupType.Documents => DocumentFiles,
                FileGroupType.Applications => ApplicationFiles,
                FileGroupType.Archives => ArchiveFiles,
                FileGroupType.SourceCodes => SourceCodeFiles,
                FileGroupType.Registrations => RegistrationFiles,
                _ => [],
            };
        }

        /// <summary>
        /// 拡張子がどの拡張子グループに属しているかを取得します。
        /// </summary>
        /// <param name="extention">拡張子</param>
        /// <returns>拡張子グループ</returns>
        public static FileGroupType GetFileGroupFromExtention(string extention)
        {
            if (MovieFiles.Contains(extention)) { return FileGroupType.Movies; }
            if (PictureFiles.Contains(extention)) { return FileGroupType.Pictures; }
            if (MusicFiles.Contains(extention)) { return FileGroupType.Musics; }
            if (DocumentFiles.Contains(extention)) { return FileGroupType.Documents; }
            if (ApplicationFiles.Contains(extention)) { return FileGroupType.Applications; }
            if (ArchiveFiles.Contains(extention)) { return FileGroupType.Archives; }
            if (SourceCodeFiles.Contains(extention)) { return FileGroupType.SourceCodes; }
            if (RegistrationFiles.Contains(extention)) { return FileGroupType.Registrations; }
            return FileGroupType.Others;
        }
        #endregion ファイル種類の管理
        /// <summary>
        /// 主な動画ファイル
        /// </summary>
        #region 主な動画ファイル
        private static readonly HashSet<string> MovieFiles =
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
        private static readonly HashSet<string> PictureFiles =
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
        #region 主な音楽ファイル
        private static readonly HashSet<string> MusicFiles =
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
        private static readonly HashSet<string> DocumentFiles =
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
        private static readonly HashSet<string> ApplicationFiles =
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
        private static readonly HashSet<string> ArchiveFiles =
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
        private static readonly HashSet<string> SourceCodeFiles =
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
        private static readonly HashSet<string> RegistrationFiles =
        [
            ".reg",
            ".inf",
            ".regtrans-ms",
            ".pol",
        ];
        #endregion 主な登録ファイル
    }
}
