using System;

namespace MadeInTheUSB.FT232H
{
    [Flags]
    public enum FtdiI2CTransferOptions : int
    {
        None = 0, // Added by Fred
        StartBit = 0x00000001,
        StopBit = 0x00000002,
        BreakOnNack = 0x00000004,
        NackLastByte = 0x00000008,
        FastTransferBytes = 0x00000010,
        FastTransferBits = 0x00000020,
        FastTransfer = 0x00000030,
        NoAddress = 0x00000040
    }
}
