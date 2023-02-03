using System;
using System.Collections.Generic;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Based class for any FT232H derived classes
    /// </summary>
    public class FT232HDeviceBaseClass
    {
        internal static IntPtr                _spiHandle = IntPtr.Zero;
        internal static IntPtr                _i2cHandle = IntPtr.Zero;
        internal static MpsseSpiConfig        _globalConfig;

        public List<int> PowerOf2 = new List<int>()
        {
            1, 2, 4, 8, 16, 32, 64, 128,
            256, 512, 1024, 2048
        };
    }
}