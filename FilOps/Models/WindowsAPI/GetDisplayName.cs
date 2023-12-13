using System.Runtime.InteropServices;

namespace FilOps.Models.WindowsAPI
{
    public static partial class WindowsGetFolderDisplayName
    {
        // SIGDN 列挙型は、SHGetNameFromIDList 関数で使用される表示名の取得方法を指定します。
        private enum SIGDN : uint
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
        // パスからpidlを取得します。
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHParseDisplayName(
            string pszName, IntPtr pbc, out IntPtr ppidl, uint sfgaoIn, out uint psfgaoOut
        );

        // pidlからフォルダ表示名を取得します。
        [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int SHGetNameFromIDList(
            IntPtr pidl, SIGDN sigdnName, out string ppszName);
    }
}
