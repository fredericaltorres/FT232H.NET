﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public enum FtdiMpsseResult
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
