using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pash.Implementation.Native
{
    /// <summary>
    /// Windows Shell32 library.
    /// </summary>
    internal static class Shell32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        /// <summary>
        /// String constant <c>"MZ"</c>, packed into lowest word of <see cref="Int32"/>.
        /// </summary>
        public static readonly ushort MZ = BitConverter.ToUInt16(Encoding.ASCII.GetBytes("MZ"), 0);

        /// <summary>
        /// String constant <c>"PE"</c>, packed into lowest word of <see cref="Int16"/>.
        /// </summary>
        public static readonly ushort PE = BitConverter.ToUInt16(Encoding.ASCII.GetBytes("PE"), 0);

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref Shell32.SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_DISPLAYNAME = 0x000000200;
        public const uint SHGFI_TYPENAME = 0x000000400;
        public const uint SHGFI_ATTRIBUTES = 0x000000800;
        public const uint SHGFI_ICONLOCATION = 0x000001000;
        public const uint SHGFI_EXETYPE = 0x000002000;
        public const uint SHGFI_SYSICONINDEX = 0x000004000;
        public const uint SHGFI_LINKOVERLAY = 0x000008000;
        public const uint SHGFI_SELECTED = 0x000010000;
        public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_SHELLICONSIZE = 0x000000004;
        public const uint SHGFI_PIDL = 0x000000008;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_ADDOVERLAYS = 0x000000020;
        public const uint SHGFI_OVERLAYINDEX = 0x000000040;
    }
}
