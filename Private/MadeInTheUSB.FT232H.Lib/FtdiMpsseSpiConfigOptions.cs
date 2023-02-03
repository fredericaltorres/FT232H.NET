using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
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
