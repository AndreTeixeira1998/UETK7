using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace UETK7
{
    internal struct DWM_COLORIZATION_PARAMS
    {
        public uint clrColor;
        public uint clrAfterGlow;
        public uint nIntensity;
        public uint clrAfterGlowBalance;
        public uint clrBlurBalance;
        public uint clrGlassReflectionIntensity;
        public bool fOpaque;
    }

    internal static class NativeMethods
    {

        [DllImport("dwmapi.dll", EntryPoint = "#127", PreserveSig = false)]
        internal static extern void DwmGetColorizationParameters(out DWM_COLORIZATION_PARAMS parameters);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();
    }
}
