using System.Runtime.InteropServices;

namespace HunterPie.UI.Platform.Windows.Native
{
    internal static class Gdi32
    {
        const string DLL_NAME = "gdi32.dll";

        [DllImport(DLL_NAME, SetLastError = true)]
        public static extern int AddFontResourceW([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        [DllImport(DLL_NAME, SetLastError = true)]
        public static extern int RemoveFontResourceW([In][MarshalAs (UnmanagedType.LPWStr)] string lpFileName);
    }
}
