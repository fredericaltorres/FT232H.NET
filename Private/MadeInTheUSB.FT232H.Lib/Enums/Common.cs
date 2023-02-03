using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    public enum PinMode : byte // Same value as the Arduino library
    {
        Input       = 0, 
        Output      = 1,
        InputPullUp = 2, // Not Support by Nusbio, Just by Arduino controlled by Nusbio
    }

    public enum PinState : byte
    {
        Low = 0,
        High = 1,
        Unknown = 2
    }
}
