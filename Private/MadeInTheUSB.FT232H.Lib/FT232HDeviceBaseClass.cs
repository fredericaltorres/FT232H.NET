﻿using System;
using System.Collections.Generic;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Based class for any FT232H derived classes
    /// </summary>
    public class FT232HDeviceBaseClass
    {
        internal static IntPtr _spiHandle = IntPtr.Zero;
        //internal static IntPtr                _i2cHandle = IntPtr.Zero;
        protected I2CDevice _i2cDevice = null;
        internal static SpiConfig _globalConfig;
        protected const int _maxGpio = 8;
        protected const int ValuesDefaultMask = 0;
        protected const int DirectionDefaultMask = 0xFF;
                
        protected const int _gpioStartIndex = 0;

        public byte GpioStartIndex { get { return _gpioStartIndex; } }

        public byte MaxGpio
        {
            get { return _maxGpio; }
        }


        public List<int> PowerOf2 = new List<int>()
        {
            1, 2, 4, 8, 16, 32, 64, 128,
            256, 512, 1024, 2048
        };
    }
}