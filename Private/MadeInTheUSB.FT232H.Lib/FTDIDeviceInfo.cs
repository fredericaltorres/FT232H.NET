using System;
using System.Runtime.InteropServices;

namespace MadeInTheUSB.FT232H
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FTDIDeviceInfo
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
