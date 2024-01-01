using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FileHashCraft.Models
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
    #region WindowsAPIで使うenumとstruct
    /// <summary>
    /// SHGetFIleInfoの第4引数で指定する、取得する情報のパラメータです。
    /// </summary>
    [Flags]
    public enum SHGFI : uint
    {
        SHGFI_LARGEICON = 0x000000000,  // SHGFI_ICONの設定必要、大きいアイコン(32x32)取得
        SHGFI_SMALLICON = 0x000000001,  // SHGFI_ICONの設定必要、小さいアイコン(16x16)取得
        SHGFI_OPENICON = 0x000000002,   // SHGFI_ICONとSHGFI_SYSICONINDEXの設定必要、開いた状態のアイコンを取得
        SHGFI_SHELLICONSIZE = 0x000000004,  // SHGFI_ICONの設定必要、シェルサイズのアイコン取得
        SHGFI_PIDL = 0x000000008,   // パスではなく ITEMIDLIST でデータを取得する
        SHGFI_USEFILEATTRIBUTES = 0x000000010,  // ファイルにアクセスしない事を示し、属性を取得する
        SHGFI_ADDOVERLAYS = 0x000000020,// 適切なオーバーレイをアイコンに適用(SHGFI_ICON必要)
        SHGFI_OVERLAYINDEX = 0x000000040,   // Ver5.0より SHGFI_ICONの設定必要、オーバーレイアイコンのインデックスを取得 psfiのiIcon上位8ビット
        SHGFI_ICON = 0x000000100,   // ファイルを表すアイコンのハンドルと、システムイメージリストのアイコンインデックスをpsfiのhIconとiIconに
        SHGFI_DISPLAYNAME = 0x000000200,// ファイルの表示名をpsfiのszDisplayNameに
        SHGFI_TYPENAME = 0x000000400,   // ファイルの種類を説明する文字列をpsfiのszTypeNameに
        SHGFI_ATTRIBUTES = 0x000000800, // アイテムの属性をpsfiのdwAttributesに
        SHGFI_ICONLOCATION = 0x000001000,   // アイコンが含まれるファイルパスをpsfiのszDisplayNameに、インデックスをpsfiのiIconに
        SHGFI_EXETYPE = 0x000002000,// 実行可能ファイルの場合、そのタイプを戻り値に(他フラグと併用不可)
        SHGFI_SYSICONINDEX = 0x000004000,   // システムイメージリストアイコンのインデックス取得、psfiのiIconにコピーされたアイコンのみ有効
        SHGFI_LINKOVERLAY = 0x000008000,// SHGFI_ICONの設定必要、リンクを示す絵を加える
        SHGFI_SELECTED = 0x000010000,   // SHGFI_ICONの設定必要、システムのハイライト色とブレンドさせる
        SHGFI_ATTR_SPECIFIED = 0x000020000, // SHGFI_ATTRIBUTES変更を示し、psfiのdwAttributesに必要な属性が含まれる(SHGFI_ICON不可)
    }

    /// <summary>
    /// SIGDN 列挙型は、SHGetNameFromIDList 関数で使用される表示名の取得方法を指定します。
    /// </summary>
    public enum SIGDN : uint
    {
        SIGDN_NORMALDISPLAY = 0x00000000,
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
        SIGDN_FILESYSPATH = 0x80058000,
        SIGDN_URL = 0x80068000,
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
        SIGDN_PARENTRELATIVE = 0x80080001
    }

    /// <summary>
    /// SHGetFileInfoの第3引数で、情報を受け取る構造体です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        public fixed ushort szDisplayName[260];
        public fixed ushort szTypeName[80];
    };
    # endregion WindowsAPIで使うenumとstruct

    public static partial class WindowsAPI
    {
        #region WindowsAPIへのLibraryImport
        // パスからpidlを取得します。
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHParseDisplayName(
            string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut
        );

        // pidlからフォルダ表示名を取得します。
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHGetNameFromIDList(
            IntPtr pidl, SIGDN sigdnName, out string ppszName);

        // ファイルシステム内のオブジェクト(ファイル、フォルダ、ディレクトリ、ドライブルートなど)に関する情報を取得します。
        [LibraryImport("shell32.dll", EntryPoint = "SHGetFileInfoW", StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribs, ref SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        // 指定されたアイコンハンドルを解放します。
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool DestroyIcon(IntPtr handle);

        private static readonly Dictionary<KnownFolder, string> _specialFolder = [];

        // 特殊フォルダのパスを取得する
        [LibraryImport("shell32", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHGetKnownFolderPath(
            Guid rfid, uint dwFlags, nint hToken, out string @return);
        #endregion WindowsAPIへのLibraryImport]

        #region WindowsAPIを使うメソッド
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
        /// <summary>
        /// ディレクトリやファイルのアイコンとファイル種類を取得します。
        /// </summary>
        /// <param name="path">情報を取得したいディレクトリやファイルのフルパス</param>
        /// <param name="is_original">オリジナルのアイコンを取得するかどうか</param>
        /// <returns>取得したファイルのアイコンとファイル種類</returns>
        /// <exception cref="Exception">ファイル情報の取得に失敗したときに発生</exception>
        private static (BitmapSource?, string) GetResourceContent(string path, bool is_original = false)
        {
            // SHFILEINFO 構造体のインスタンスを作成
            SHFILEINFO shinfo = new();

            // SHGetFileInfo 関数の戻り値を格納する変数
            IntPtr shFileInfoResult;

            // オリジナルのアイコンを取得するかどうかでフラグを設定
            if (is_original)
            {
                shFileInfoResult = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_TYPENAME);
            }
            else
            {
                shFileInfoResult = SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI.SHGFI_ICON | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);
            }

            // SHGetFileInfo 関数が失敗した場合の例外処理
            if (shFileInfoResult == IntPtr.Zero || shinfo.hIcon == IntPtr.Zero)
            {
                int lastError = Marshal.GetLastWin32Error();
                LogManager.DebugLog($"SHGetFileInfo Failed with error code {lastError}", LogLevel.Error);
                return (null, string.Empty);
            }

            // アイコンを BitmapSource に変換
            BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // アイコンハンドルを解放
            DestroyIcon(shinfo.hIcon);

            // 取得したアイコンとファイル種類を返す
            unsafe
            {
                return (icon, Utf16StringMarshaller.ConvertToManaged(shinfo.szTypeName) ?? String.Empty);
            }
        }
        #endregion WindowsAPIを使うメソッド

        #region ファイルのアイコンと種類をキャッシュしながら管理する
        /// <summary>
        /// キャッシュに指定されたキーが登録されていない場合、指定されたファイルの情報を取得して登録します。
        /// </summary>
        /// <param name="key">利用するキー</param>
        /// <param name="path">ファイルのフルパス</param>
        private static void AddDirectoryCache(string key, string path)
        {
            // キャッシュに指定されたキーが登録されていない場合の処理
            if (!FileIconCache.ContainsKey(key) || !FileTypeCache.ContainsKey(key))
            {
                // 指定されたファイルの情報を取得
                var (icon, file_type) = GetResourceContent(path, true);

                // 取得した情報をキャッシュに登録
                if (icon != null)
                {
                    FileIconCache[key] = icon;
                    FileTypeCache[key] = file_type;
                }
            }
        }

        /// <summary>
        /// ファイルのアイコンとファイル種類をキャッシュに格納し、キャッシュのキーを取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>キャッシュに利用するキー</returns>
        private static string ReadIconAndTypeToCache(string path)
        {
            // 特殊なアイコンが必要な拡張子のリスト
            var specialIconExtensions = new List<string> { ".exe", ".lnk", ".ico" };

            // ファイルの拡張子を取得
            var fileExtension = Path.GetExtension(path);

            // ディレクトリまたは特殊なアイコンが必要な拡張子の場合
            if (Directory.Exists(path) || specialIconExtensions.Any(x => x == fileExtension))
            {
                // 個別のアイコンである可能性があるもの
                AddDirectoryCache(path, path);
                return path;
            }
            else
            {
                // ファイルで同じアイコンの可能性が高いもの
                AddDirectoryCache(fileExtension, path);
                return fileExtension;
            }
        }

        // ファイルのアイコンと種類のキャッシュ
        private static readonly Dictionary<string, BitmapSource> FileIconCache = [];
        private static readonly Dictionary<string, string> FileTypeCache = [];
        #endregion ファイルのアイコンと種類をキャッシュしながら管理する

        #region 外部からのアクセスメソッド
        /// <summary>
        /// ファイルのアイコンを取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>アイコン</returns>
        public static BitmapSource? GetIcon(string path)
        {
            if (Path.GetExtension(path) == ".tmp") return null;
            if (path.Length > 3)
            {
                var key = ReadIconAndTypeToCache(path);
                return FileIconCache[key];
            }
            else
            {
                var (icon, _) = GetResourceContent(path, true);
                return icon;
            }
        }

        /// <summary>
        /// ファイルの種類を取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>ファイルの種類</returns>
        public static string GetType(string path)
        {
            if (Path.GetExtension(path) == ".tmp") return string.Empty;
            if (path.Length > 3)
            {
                var key = ReadIconAndTypeToCache(path);
                return FileTypeCache[key];
            }
            else
            {
                var (_, file_type) = GetResourceContent(path, true);
                return file_type;
            }
        }

        /// <summary>
        /// ファイルパスからファイルの表示名を取得します。
        /// </summary>
        /// もし取得に失敗した場合は元のファイルパスを返します。
        /// <returns>ファイルの表示名</returns>
        public static string GetDisplayName(string path)
        {
            try
            {
                Marshal.ThrowExceptionForHR(SHParseDisplayName(path, IntPtr.Zero, out var pidl, 0, out _));
                Marshal.ThrowExceptionForHR(SHGetNameFromIDList(pidl, SIGDN.SIGDN_NORMALDISPLAY, out var display_name));
                return display_name;
            }
            catch
            {
                return path;
            }
        }
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
        #endregion 外部からのアクセスメソッド
    }
}
