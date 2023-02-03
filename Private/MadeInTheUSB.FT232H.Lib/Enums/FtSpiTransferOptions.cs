using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    [Flags]
    public enum FtSpiTransferOptions : int
    {
        None              = 0x00000000,

        SizeInBytes       = 0x00000000,
        SizeInBits        = 0x00000001,

        ToogleChipSelect  = 0x6,
        ChipselectEnable  = 0x00000002,
        ChipselectDisable = 0x00000004,
    }
}
