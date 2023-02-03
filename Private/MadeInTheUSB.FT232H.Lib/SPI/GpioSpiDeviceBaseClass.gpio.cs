using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Implement the IDigitalWriteRead for accessing the gpio 0..7 of the FT232H
    /// </summary>
    public abstract partial class GpioSpiDeviceBaseClass : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead, ISPI
    {
        private const int _gpioStartIndex      = 0;
        private const int _maxGpio             = 8;
        private const int ValuesDefaultMask    = 0;
        private const int DirectionDefaultMask = 0xFF;
        
        private int       _values;
        private int       _directions;

        public IDigitalWriteRead GPIO
        {
            get { return this as IDigitalWriteRead; }
        }
        public ISPI SPI
        {
            get { return this as ISPI; }
        }
        public byte MaxGpio
        {
            get { return _maxGpio; }
        }
        public void DigitalWrite(PinState mode, params int [] pins)
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
            if(values==-1)
                throw new GpioException( FtdiMpsseSPIResult.IoError, nameof(GetGpioMask));
            return (byte)values;
        }
        public byte GpioStartIndex { get { return _gpioStartIndex; } }
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
    }
}
