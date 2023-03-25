using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MadeInTheUSB.FT232H
{

    [StructLayout(LayoutKind.Sequential)]
    public partial struct SpiConfig
    {
        public const int _30Mhz = 30 * 1000 * 1000;
        public const int _25Mhz = 25 * 1000 * 1000;
        public const int _20Mhz = 20 * 1000 * 1000;
        public const int _15Mhz = 15 * 1000 * 1000;
        public const int _16Mhz = 16 * 1000 * 1000;
        public const int _10Mhz = 10 * 1000 * 1000;
        public const int _1Mhz  = 01 * 1000 * 1000;
        public const int _2Mhz  = 02 * 1000 * 1000;
        public const int _4Mhz  = 04 * 1000 * 1000;
        public const int _8Mhz  = 08 * 1000 * 1000;

        public const int _0_5Mhz   = 01 * 1000 * 1000 / 2;
        public const int _0_25Mhz  = 01 * 1000 * 1000 / 4; // 31k
        public const int _TestMhz = 01*1000*1000;

        public int ClockRate;
        public byte LatencyTimer;
        public FtdiMpsseSpiConfigOptions spiConfigOptions;
        public int Pin;
        public short reserved;

        public static SpiConfig Init(int clockRate, SpiChipSelectPins selectPin)
        {
            var f = new SpiConfig {
                ClockRate = clockRate,
                LatencyTimer = 10,
                spiConfigOptions = FtdiMpsseSpiConfigOptions.Mode0 | FtdiMpsseSpiConfigOptions.CsActivelow | ((FtdiMpsseSpiConfigOptions)selectPin)
            };
            return f;
        }

        public static SpiConfig GetDefault()
        {
            return SpiConfig.Init(SpiConfig._30Mhz, SpiChipSelectPins.CsDbus3);
        }

        public static SpiConfig BuildSPI(int speed, SpiChipSelectPins chipSelect = SpiChipSelectPins.CsDbus3)
        {
            return SpiConfig.Init(speed, chipSelect);
        }
    }
}
