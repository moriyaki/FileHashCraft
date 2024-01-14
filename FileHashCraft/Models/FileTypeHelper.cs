namespace FileHashCraft.Models
{
    public enum FileGroupType
    {
        Movies,
        Pictures,
        Sounds,
        Documents,
        Applications,
        Archives,
        SourceCodes,
        Others,
    }

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
                _ => [],
            };
        }

        /// <summary>
        /// 主な動画ファイル
        /// </summary>
        private readonly List<string> MovieFiles =
        [
            ".mp4",
            ".avi",
            ".mkv",
            ".mov",
            ".wmv",
            ".flv",
            ".webm",
            ".mpeg",
            ".mpg",
            ".3gp",
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
        ];
        /// <summary>
        /// 主なサウンドファイル
        /// </summary>
        private readonly List<string> MusicFiles =
        [
            ".mp3",
            ".flac",
            ".aac",
            ".ogg",
            ".m4a",
            ".wma",
            ".aiff",
            ".cue",
            ".wav",
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
            ".rpm",
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
            ".rs",
        ];
    }
}
