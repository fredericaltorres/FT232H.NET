using System;
using System.Collections.Generic;
using System.Threading;

namespace MadeInTheUSB.FT232H
{
    public class I2CGpioIImplementationDevice  : IDigitalWriteRead
    {
        private int _values;
        private int _directions;

        I2CDevice _i2cDevice;

        protected const int _gpioStartIndex = 0;
        protected const int _maxGpio = 8;

        public byte GpioStartIndex { get { return _gpioStartIndex; } }

        public byte MaxGpio
        {
            get { return _maxGpio; }
        }

        public List<int> PowerOf2 = new List<int>()
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048
        };

        public I2CGpioIImplementationDevice(I2CDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            this.GpioInit();
        }

        internal void GpioInit()
        {
            _directions = 0xFF;
            _values = 0x00;
            WriteGPIOMask(_directions, _values);
        }

        private bool WriteGPIOMask(int directions, int values)
        {
            _values = values;
            _directions = directions;
            _i2cDevice.WriteGpios((byte)_directions, (byte)_values);
            return true;
        }

        public PinState DigitalRead(int pin)
        {
            var gpioMask = this.ReadGPIOMask();
            if (gpioMask == -1)
                return PinState.Unknown;
            return (gpioMask & PowerOf2[pin]) == PowerOf2[pin] ? PinState.High : PinState.Low;
        }

        private int ReadGPIOMask()
        {
            return _i2cDevice.ReadGPIOMask();
        }

        public void DigitalWrite(int pin, PinState mode)
        {
            if (mode == PinState.High)
                _values |= PowerOf2[pin];
            else
                _values &= ~PowerOf2[pin];

            _i2cDevice.WriteGpios((byte)_directions, (byte)_values);
        }

        public byte GetGpioMask(bool forceRead = false)
        {
            var values = this.ReadGPIOMask();
            if (values == -1)
                throw new GpioException(FtdiMpsseResult.IoError, nameof(GetGpioMask));
            return (byte)values;
        }

        private void AllGpios(bool on)
        {
            for (int i = 0; i < this.MaxGpio; i++)
                this.DigitalWrite(i, on ? PinState.High : PinState.Low);
        }

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

        public void SetGpioMask(byte mask)
        {
            WriteGPIOMask(_directions, mask);
        }

        public void SetPinMode(int pin, PinMode pinMode)
        {
            if (pinMode == PinMode.Output)
                _directions |= PowerOf2[pin];
            else
                _directions &= ~PowerOf2[pin];

            var r = WriteGPIOMask(_directions, _values);
            if (!r)
            {
                throw new GpioException("SetPinMode");
            }
        }

        public void SetPullUp(int p, PinState d)
        {
            throw new NotImplementedException();
        }
    }
}


