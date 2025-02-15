﻿using DynamicSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Implement the IDigitalWriteRead for accessing the gpio 0..7 of the FT232H
    /// </summary>
    public  partial class SpiDevice : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead, ISPI
    {
        private const int _gpioStartIndex = 0;
        private const int _maxGpio = 8;
        private const int ValuesDefaultMask = 0;
        private const int DirectionDefaultMask = 0xFF;

        private int _values;
        private int _directions;

        public ISpiInterfaces Interfaces => new ISpiInterfaces { Spi = this.SPI, Gpios = this.GPIO };

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


        public List<int> GpioIndexes => DS.Range(0, MaxGpio, 1);

        public void Animate()
        {
            var wait = 55;
            this.AllGpios(false);
            for (var i = 0; i < this.MaxGpio * 5; i++)
            {
                this.ProgressNext();
                Thread.Sleep(wait);
                wait -= 8;
                if (wait < 8)
                    wait = 8;
            }
            this.AllGpios(false);
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
            if (r != FtdiMpsseResult.Ok)
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
            if (r != FtdiMpsseResult.Ok)
            {
                if (Debugger.IsAttached) Debugger.Break();
                throw new GpioException(r, nameof(SetPinMode));
            }
        }
        public void SetGpioMask(byte mask)
        {
            var r = this.WriteGPIOMask(mask);
            if (r != FtdiMpsseResult.Ok)
                throw new GpioException(r, nameof(SetGpioMask));
        }
        public byte GetGpioMask(bool forceRead = false)
        {
            var values = this.ReadGPIOMask();
            if (values == -1)
                throw new GpioException(FtdiMpsseResult.IoError, nameof(GetGpioMask));
            return (byte)values;
        }
        public byte GpioStartIndex { get { return _gpioStartIndex; } }
        public void SetPullUp(int p, PinState d)
        {
            throw new NotImplementedException();
        }
        private FtdiMpsseResult WriteGPIOMask(byte values)
        {
            return LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, values);
        }
        private FtdiMpsseResult WriteGPIOMask(byte directions, byte values)
        {
            _values = values;
            _directions = directions;
            return LibMpsse_AccessToCppDll.FT_WriteGPIO(_spiHandle, directions, values);
        }
        private int ReadGPIOMask()
        {
            int vals;
            var r = LibMpsse_AccessToCppDll.FT_ReadGPIO(_spiHandle, out vals);
            if (r == FtdiMpsseResult.Ok)
                return vals;
            else
                return -1;
        }

        static bool _progressModeInitialized = false;
        static int _progressModeIndex = -1;
        public void ProgressNext(bool clear = false)
        {
            if (!_progressModeInitialized)
            {
                _progressModeInitialized = true;
                AllGpios(false);
            }

            if (clear)
            {
                AllGpios(false);
                _progressModeIndex = -1;
            }
            else
            {
                if (_progressModeIndex >= 0)
                    this.DigitalWrite(_progressModeIndex, PinState.Low);
                _progressModeIndex += 1;

                if (_progressModeIndex == this.MaxGpio)
                    _progressModeIndex = 0;

                this.DigitalWrite(_progressModeIndex, PinState.High);
            }
        }
    }
}
