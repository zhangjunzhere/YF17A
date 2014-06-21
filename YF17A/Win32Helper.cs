using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace YF17A
{
    class Win32Helper
    {
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("ole32.dll")]
        public static extern void CoFreeUnusedLibraries();

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();
    }
}
