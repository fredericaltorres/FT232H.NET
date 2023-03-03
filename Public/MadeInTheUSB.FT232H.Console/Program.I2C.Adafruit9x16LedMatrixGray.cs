using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using DynamicSugar;
using MadeInTheUSB.Adafruit;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static IS31FL3731 _IS31FL3731;

        static void I2CSample_Adafruit9x16LedMatrixGray(I2CDevice i2cDevice)
        {
            _IS31FL3731 = new IS31FL3731(i2cDevice);
            _IS31FL3731.Begin();
            _IS31FL3731.Clear();
            _IS31FL3731.DrawRect(0, 0, 8, 8, true);
            _IS31FL3731.UpdateDisplay(0);
        }
    }
}

