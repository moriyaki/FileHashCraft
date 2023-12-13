using System.Runtime.InteropServices;

namespace FilOps.Models.WindowsAPI
{
    /// <summary>
    /// 特殊フォルダの列挙子
    /// </summary>
    public enum KnownFolder
    {
        Objects3D,
        Downloads,
        Desktop,
        Documents,
        Pictures,
        Videos,
        Music,
        User,
    }

    public static partial class WindowsSpecialFolder
    {
        /// <summary>
        /// KnownFolderをキーにGUIDを取得する辞書
        /// </summary>
        private static readonly Dictionary<KnownFolder, Guid> _guids = new()
        {
            [KnownFolder.Objects3D] = new Guid("{31C0DD25-9439-4F12-BF41-7FF4EDA38722}"),
            [KnownFolder.Downloads] = new Guid("374DE290-123F-4565-9164-39C4925E467B"),
            [KnownFolder.Desktop] = new Guid("{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}"),
            [KnownFolder.Documents] = new Guid("{FDD39AD0-238F-46AF-ADB4-6C85480369C7}"),
            [KnownFolder.Pictures] = new Guid("{33E28130-4E1E-4676-835A-98395C3BC3BB}"),
            [KnownFolder.Videos] = new Guid("{18989B1D-99B5-455B-841C-AB7C74E4DDFC}"),
            [KnownFolder.Music] = new Guid("{4BD8D571-6D19-48D3-BE97-422220080E43}"),
            [KnownFolder.User] = new Guid("\t{5E6C858F-0E22-4760-9AFE-EA3317B67173}"),

        };

        private static readonly Dictionary<KnownFolder, string> _specialFolder = [];

        // 特殊フォルダのパスを取得する
        [LibraryImport("shell32", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHGetKnownFolderPath(
            Guid rfid, uint dwFlags, nint hToken, out string @return);

        /// <summary>
        /// 指定された特殊フォルダのパスを取得します。
        /// </summary>
        /// <param name="knownFolder">取得したい特殊フォルダ</param>
        /// <returns>特殊フォルダのパス</returns>
        public static string GetPath(KnownFolder knownFolder)
        {
            // 指定された特殊フォルダのパスを取得
            SHGetKnownFolderPath(_guids[knownFolder], 0, 0, out var path);

            // 取得したパスを特殊フォルダのキャッシュに保存
            _specialFolder[knownFolder] = path;

            // 取得した特殊フォルダのパスを返す
            return path;
        }

        /// <summary>
        /// 指定されたパスが特殊フォルダかどうかを調査します。
        /// </summary>
        /// <param name="path">調査するフルパス</param>
        /// <returns>特殊フォルダである場合は true、それ以外の場合は false</returns>
        public static bool IsSpecialFolder(string path)
        {
            // キャッシュされた特殊フォルダのパスと一致するか確認
            return _specialFolder.ContainsValue(path);
        }
    }
}
