using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MadeInTheUSB.FT232H
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FtDeviceInfo
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

    [Flags]
    public enum FtConfigOptions
    {

        Mode0 = 0x00000000,
        Mode1 = 0x00000001,
        Mode2 = 0x00000002,
        Mode3 = 0x00000003,


        CsDbus3 = 0x00000000, /*000 00*/
        CsDbus4 = 0x00000004, /*001 00*/
        CsDbus5 = 0x00000008, /*010 00*/
        CsDbus6 = 0x0000000C, /*011 00*/
        CsDbus7 = 0x00000010, /*100 00*/

        CsActivelow = 0x00000020,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FtChannelConfig
    {
        public int ClockRate;
        public byte LatencyTimer;
        public FtConfigOptions configOptions;
        public int Pin;
        public short reserved;
    }

    [Flags]
    public enum FtdiMpsseSpiConfigOptions
    {
        Mode0 = 0x00000000, // Different SPI modes
        Mode1 = 0x00000001,
        Mode2 = 0x00000002,
        Mode3 = 0x00000003,

        // 5 pin on the FT232H that can be used as select
        CsDbus3 = 0x00000000, /* 00000 - 0  */
        CsDbus4 = 0x00000004, /* 00100 - 4  */
        CsDbus5 = 0x00000008, /* 01000 - 8  */
        CsDbus6 = 0x0000000C, /* 01100 - 12 */
        CsDbus7 = 0x00000010, /* 10000 - 16 */

        CsActivelow = 0x00000020 /* 32 */,
    }

    /// <summary>
    /// 5 pins on the FT232H that can be used as select
    /// Define as a seperate enum
    /// </summary>
    [Flags]
    public enum FtdiMpsseSpiSelectPin
    {
        // 5 pin on the FT232H that can be used as select
        CsDbus3 = 0x00000000, /* 00000 - 0  */
        CsDbus4 = 0x00000004, /* 00100 - 4  */
        CsDbus5 = 0x00000008, /* 01000 - 8  */
        CsDbus6 = 0x0000000C, /* 01100 - 12 */
        CsDbus7 = 0x00000010, /* 10000 - 16 */
    }
}
