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
        public enum SpiClockSpeeds
        {
            _30Mhz      = 30 * 1000 * 1000,
            _25Mhz      = 25 * 1000 * 1000,
            _20Mhz      = 20 * 1000 * 1000,
            _15Mhz      = 15 * 1000 * 1000,
            _16Mhz      = 16 * 1000 * 1000,
            _10Mhz      = 10 * 1000 * 1000,
            _1Mhz       = 01 * 1000 * 1000,
            _2Mhz       = 02 * 1000 * 1000,
            _4Mhz       = 04 * 1000 * 1000,
            _8Mhz       = 08 * 1000 * 1000,
            _0_5Mhz     = 01 * 1000 * 1000 / 2,
            _0_25Mhz    = 01 * 1000 * 1000 / 4, // 31k
            _TestMhz    = 01 * 1000 * 1000,
        }

        public int ClockRate;
        public byte LatencyTimer;
        public FtdiMpsseSpiConfigOptions spiConfigOptions;
        public int Pin;
        public short reserved;

        public bool IsChipSelect(SpiChipSelectPins cs)
        {
            var cs2 = (FtdiMpsseSpiConfigOptions)cs;
            return (this.spiConfigOptions & cs2) == cs2;
        }

        public void ChangeChipSelect(SpiChipSelectPins cs)
        {
            this.spiConfigOptions = FtdiMpsseSpiConfigOptions.Mode0 | FtdiMpsseSpiConfigOptions.CsActivelow | ((FtdiMpsseSpiConfigOptions)cs);
        }

        public static SpiConfig Init(SpiClockSpeeds clockRate, SpiChipSelectPins selectPin)
        {
            var f = new SpiConfig {
                ClockRate = (int)clockRate,
                LatencyTimer = 10,
                spiConfigOptions = FtdiMpsseSpiConfigOptions.Mode0 | FtdiMpsseSpiConfigOptions.CsActivelow | ((FtdiMpsseSpiConfigOptions)selectPin)
            };
            return f;
        }

        public static SpiConfig GetDefault()
        {
            return SpiConfig.Init(SpiClockSpeeds._30Mhz, SpiChipSelectPins.CsDbus3);
        }

        public static SpiConfig BuildSPI(SpiClockSpeeds speed, SpiChipSelectPins chipSelect = SpiChipSelectPins.CsDbus3)
        {
            return SpiConfig.Init(speed, chipSelect);
        }
    }
}
