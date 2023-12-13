using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FilOps.Models.WindowsAPI
{
    /// <summary>
    /// SHGetFIleInfoの第4引数で指定する、取得する情報のパラメータです。
    /// </summary>
    public enum SHGFI : uint
    {
        SHGFI_ADDOVERLAYS = 0x000000020,// 適切なオーバーレイをアイコンに適用(SHGFI_ICON必要)
        SHGFI_ATTR_SPECIFIED = 0x000020000, // SHGFI_ATTRIBUTES変更を示し、psfiのdwAttributesに必要な属性が含まれる(SHGFI_ICON不可)
        SHGFI_ATTRIBUTES = 0x000000800, // アイテムの属性をpsfiのdwAttributesに
        SHGFI_DISPLAYNAME = 0x000000200,// ファイルの表示名をpsfiのszDisplayNameに
        SHGFI_EXETYPE = 0x000002000,// 実行可能ファイルの場合、そのタイプを戻り値に(他フラグと併用不可)
        SHGFI_ICON = 0x000000100,   // ファイルを表すアイコンのハンドルと、システムイメージリストのアイコンインデックスをpsfiのhIconとiIconに
        SHGFI_ICONLOCATION = 0x000001000,   // アイコンが含まれるファイルパスをpsfiのszDisplayNameに、インデックスをpsfiのiIconに
        SHGFI_LARGEICON = 0x000000000,  // SHGFI_ICONの設定必要、大きいアイコン(32x32)取得
        SHGFI_LINKOVERLAY = 0x000008000,// SHGFI_ICONの設定必要、リンクを示す絵を加える
        SHGFI_OPENICON = 0x000000002,   // SHGFI_ICONとSHGFI_SYSICONINDEXの設定必要、開いた状態のアイコンを取得
        SHGFI_OVERLAYINDEX = 0x000000040,   // Ver5.0より SHGFI_ICONの設定必要、オーバーレイアイコンのインデックスを取得 psfiのiIcon上位8ビット
        SHGFI_PIDL = 0x000000008,   // パスではなく ITEMIDLIST でデータを取得する
        SHGFI_SELECTED = 0x000010000,   // SHGFI_ICONの設定必要、システムのハイライト色とブレンドさせる
        SHGFI_SHELLICONSIZE = 0x000000004,  // SHGFI_ICONの設定必要、シェルサイズのアイコン取得
        SHGFI_SMALLICON = 0x000000001,  // SHGFI_ICONの設定必要、小さいアイコン(16x16)取得
        SHGFI_SYSICONINDEX = 0x000004000,   // システムイメージリストアイコンのインデックス取得、psfiのiIconにコピーされたアイコンのみ有効
        SHGFI_TYPENAME = 0x000000400,   // ファイルの種類を説明する文字列をpsfiのszTypeNameに
        SHGFI_USEFILEATTRIBUTES = 0x000000010,  // ファイルにアクセスしない事を示し、属性を取得する
    }

    /// <summary>
    /// SHGetFileInfoの第3引数で、情報を受け取る構造体です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    public static class WindowsFileSystem
    {
        // LibraryImport にしようとしたらSHFILEINFOでエラーになる
#pragma warning disable SYSLIB1054
        // ファイルシステム内のオブジェクト(ファイル、フォルダ、ディレクトリ、ドライブルートなど)に関する情報を取得します。
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttribs, ref SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        // 指定されたアイコンハンドルを解放します。
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);
#pragma warning restore SYSLIB1054

        // ファイルのアイコンと種類のキャッシュ
        private static readonly Dictionary<string, BitmapSource> FileIconCache = [];
        private static readonly Dictionary<string, string> FileTypeCache = [];

        /// <summary>
        /// ディレクトリやファイルのアイコンとファイル種類を取得します。
        /// </summary>
        /// <param name="path">情報を取得したいディレクトリやファイルのフルパス</param>
        /// <param name="is_original">オリジナルのアイコンを取得するかどうか</param>
        /// <returns>取得したファイルのアイコンとファイル種類</returns>
        /// <exception cref="Exception">ファイル情報の取得に失敗したときに発生</exception>
        private static (BitmapSource, string) GetResourceContent(string path, bool is_original = false)
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
                throw new Exception($"SHGetFileInfo Failed with error code {lastError}");
            }

            // アイコンを BitmapSource に変換
            BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // アイコンハンドルを解放
            DestroyIcon(shinfo.hIcon);

            // 取得したアイコンとファイル種類を返す
            return (icon, shinfo.szTypeName);
        }

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
                FileIconCache[key] = icon;
                FileTypeCache[key] = file_type;
            }
        }

        /// <summary>
        /// ファイルのアイコンとファイル種類をキャッシュに格納し、キャッシュのキーを取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>キャッシュに利用するキー</returns>
        private static string ReadIconAndType(string path)
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

        /// <summary>
        /// ファイルのアイコンを取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>アイコン</returns>
        public static BitmapSource GetIcon(string path)
        {
            var key = ReadIconAndType(path);
            return FileIconCache[key];
        }

        /// <summary>
        /// ファイルの種類を取得します。
        /// </summary>
        /// <param name="path">ファイルのフルパス</param>
        /// <returns>ファイルの種類</returns>
        public static string GetType(string path)
        {
            var key = ReadIconAndType(path);
            return FileTypeCache[key];
        }
    }
}
