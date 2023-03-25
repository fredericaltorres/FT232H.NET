using System;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// 5 pins on the FT232H that can be used as select
    /// Define as a seperate enum
    /// </summary>
    [Flags]
    public enum SpiChipSelectPins
    {
        // 5 pin on the FT232H that can be used as select
        CsDbus3 = 0x00000000, /* 00000 - 0  */
        CsDbus4 = 0x00000004, /* 00100 - 4  */
        CsDbus5 = 0x00000008, /* 01000 - 8  */
        CsDbus6 = 0x0000000C, /* 01100 - 12 */
        CsDbus7 = 0x00000010, /* 10000 - 16 */
    }
}
