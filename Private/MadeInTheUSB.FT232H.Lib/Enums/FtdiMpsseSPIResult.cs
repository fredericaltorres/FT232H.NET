using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    [Flags]
    public enum FtI2CTransferOptions : int
    {
        StartBit = 0x00000001,
        StopBit = 0x00000002,
        BreakOnNack = 0x00000004,
        NackLastByte = 0x00000008,
        FastTransferBytes = 0x00000010,
        FastTransferBits = 0x00000020,
        FastTransfer = 0x00000030,
        NoAddress = 0x00000040
    }
    public enum FtdiMpsseSPIResult
    {
        Ok = 0,
        InvalidHandle,
        DeviceNotFound,
        DeviceNotOpened,
        IoError,
        InsufficientResources,
        InvalidParameter,
        InvalidBaudRate,
    }
}
