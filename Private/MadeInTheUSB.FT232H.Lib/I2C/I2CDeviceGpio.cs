using System;

namespace MadeInTheUSB.FT232H
{
    public class GpioI2CImplementationDevice  : FT232HDeviceBaseClass, IDigitalWriteRead
    {
        private int _values;
        private int _directions;

        public GpioI2CImplementationDevice (I2CDevice i2cDevice)
        {
            base._i2cDevice = i2cDevice;
            this.GpioInit();
        }

        internal void GpioInit()
        {
            // All gpios direction out, off
            _directions = 0xFF;
            _values = 0x00;
            WriteGPIOMask(_directions, _values);

            //var v2 = i2cDevice.I2C_GetGPIOValuesLow();
            //var v1 = i2cDevice.I2C_GetGPIOValuesHigh();
        }
        private bool WriteGPIOMask(int directions, int values)
        {
            _values = values;
            _directions = directions;
            return base._i2cDevice.I2C_SetGPIOValuesHigh((byte)_directions, (byte)_values) == 0;
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
            return base._i2cDevice.I2C_GetGPIOValuesHigh();
        }

        public void DigitalWrite(int pin, PinState mode)
        {
            if (mode == PinState.High)
                _values |= PowerOf2[pin];
            else
                _values &= ~PowerOf2[pin];

            base._i2cDevice.I2C_SetGPIOValuesHigh((byte)_directions, (byte)_values);
        }

        public byte GetGpioMask(bool forceRead = false)
        {
            var values = this.ReadGPIOMask();
            if (values == -1)
                throw new GpioException(FtdiMpsseSPIResult.IoError, nameof(GetGpioMask));
            return (byte)values;
        }

        public void ProgressNext(bool clear = false)
        {
            //ProgressNextImpl.ProgressNext(this, clear);
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


