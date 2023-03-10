using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
    public struct MpsseDeviceInfo
    {
        public int Flags;
        public int Type;
        public int ID;
        public int LocId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string SerialNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]

        public string Description;
        public IntPtr ftHandle;
    }
}
