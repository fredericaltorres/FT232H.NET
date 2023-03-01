using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{

    public class Interfaces
    {
        public IDigitalWriteRead Gpios;
        public ISPI Spi;
    }


    public class ProgressNextImpl
    {
        static bool _progressModeInitialized = false;
        static int _progressModeIndex = -1;
        public static void ProgressNext(IDigitalWriteRead gpios, bool clear = false)
        {
            if (!_progressModeInitialized)
            {
                _progressModeInitialized = true;
                for (int i = 0; i < gpios.MaxGpio; i++)
                    gpios.DigitalWrite(i, PinState.Low);
            }

            if (clear)
            {
                for (int i = 0; i < gpios.MaxGpio; i++)
                    gpios.DigitalWrite(i, PinState.Low);
                _progressModeIndex = -1;
            }
            else
            {
                if (_progressModeIndex >= 0)
                    gpios.DigitalWrite(_progressModeIndex, PinState.Low);
                _progressModeIndex += 1;

                if (_progressModeIndex == gpios.MaxGpio)
                    _progressModeIndex = 0;

                gpios.DigitalWrite(_progressModeIndex, PinState.High);
            }
        }
    }

    /// <summary>
    /// Implement the IDigitalWriteRead for accessing the gpio 0..7 of the FT232H
    /// </summary>
    public abstract partial class GpioSpiImplementationDeviceBaseClass : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead, ISPI
    {
        private int _values;
        private int _directions;

        public Interfaces Interfaces => new Interfaces { Spi = this.SPI, Gpios = this.GPIO };

        public IDigitalWriteRead GPIO
        {
            get { return this as IDigitalWriteRead; }
        }
        public ISPI SPI
        {
            get { return this as ISPI; }
        }

     
        public void DigitalWrite(PinState mode, params int[] pins)
        {
            foreach (var p in pins)
                this.DigitalWrite(p, mode);
        }
        internal void GpioInit()
        {
            this.WriteGPIOMask(directions: DirectionDefaultMask, values: ValuesDefaultMask);
        }
        public bool IsGpioOn(int pin)
        {
            return DigitalRead(pin) == PinState.High;
        }
        public PinState DigitalRead(int pin)
        {
            var gpioMask = this.ReadGPIOMask();
            if (gpioMask == -1)
                return PinState.Unknown;
            return (gpioMask & PowerOf2[pin]) == PowerOf2[pin] ? PinState.High : PinState.Low;
        }
        public void DigitalWrite(int pin, PinState mode)
        {
            if (mode == PinState.High)
                _values |= PowerOf2[pin];
            else
                _values &= ~PowerOf2[pin];

            var r = LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, _directions, _values);
            if (r != FtdiMpsseSPIResult.Ok)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new GpioException(r, nameof(DigitalWrite));
            }
        }


        private void AllGpios(bool on)
        {
            for (int i = 0; i < this.MaxGpio; i++)
                this.DigitalWrite(i, on ? PinState.High : PinState.Low);
        }

        public void SetPinMode(int pin, PinMode pinMode)
        {
            if (pinMode == PinMode.Output)
                _directions |= PowerOf2[pin];
            else
                _directions &= ~PowerOf2[pin];

            var r = LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, _directions, _values);
            if (r != FtdiMpsseSPIResult.Ok)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new GpioException(r, nameof(SetPinMode));
            }
        }
        public void SetGpioMask(byte mask)
        {
            var r = this.WriteGPIOMask(mask);
            if (r != FtdiMpsseSPIResult.Ok)
                throw new GpioException(r, nameof(SetGpioMask));
        }
        public byte GetGpioMask(bool forceRead = false)
        {
            var values = this.ReadGPIOMask();
            if (values == -1)
                throw new GpioException(FtdiMpsseSPIResult.IoError, nameof(GetGpioMask));
            return (byte)values;
        }
        
        public void SetPullUp(int p, PinState d)
        {
            throw new NotImplementedException();
        }
        private FtdiMpsseSPIResult WriteGPIOMask(byte values)
        {
            return LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, values);
        }
        private FtdiMpsseSPIResult WriteGPIOMask(byte directions, byte values)
        {
            _values = values;
            _directions = directions;
            return LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, directions, values);
        }
        private int ReadGPIOMask()
        {
            int vals;
            var r = LibMpsse_AccessToCppDll.FT_ReadGPIO(_spiHandle, out vals);
            if (r == FtdiMpsseSPIResult.Ok)
                return vals;
            else
                return -1;
        }
        public void ProgressNext(bool clear = false)
        {
            ProgressNextImpl.ProgressNext(this, clear);
        }
    }
}
