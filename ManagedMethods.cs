using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace UETK7
{
    internal static class ManagedMethods
    {
        public static Color GetWindowColorizationColor(bool opaque)
        {
            DWM_COLORIZATION_PARAMS temp = new DWM_COLORIZATION_PARAMS();
            NativeMethods.DwmGetColorizationParameters(out temp);

            return Color.FromArgb((byte)(opaque ? 255 : temp.clrColor >> 24),
                (byte)(temp.clrColor >> 16),
                (byte)(temp.clrColor >> 8),
                (byte)temp.clrColor);
        }
    }
}
