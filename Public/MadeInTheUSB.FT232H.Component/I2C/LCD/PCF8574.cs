//#define FAST_MODE
//#define TRACE_CONSOLE
/*
   This class is based on the Arduino LiquidCrystal_I2C_PCF8574 V2.0 library
   Copyright (C) 2015 MadeInTheUSB.net
   Ported to C# and the Nusbio by Frederic Torres for MadeInTheUSB.net

   The MIT License (MIT)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
  
    MIT license, all text above must be included in any redistribution
  
    based on https://github.com/vanluynm/LiquidCrystal_I2C
    Support only the PCF8574 I2C serial extender
*/

using System;
using System.Collections.Generic;

using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using size_t = System.Int16;

using MadeInTheUSB.WinUtil;
using MadeInTheUSB.FT232H;

namespace MadeInTheUSB
{
    /// <summary>
    /// PCF8574 Remote 8-Bit I/O Expander for I2C Bus
    /// http://www.ti.com/lit/ds/symlink/pcf8574.pdf
    /// https://learn.adafruit.com/adafruit-pcf8574
    /// https://github.com/adafruit/Adafruit_PCF8574
    /// https://www.electronicshub.org/interfacing-pcf8574-with-arduino/
    /// https://www.google.com/imgres?imgurl=https%3A%2F%2Favrhelp.mcselec.com%2Ftext_lcd_o-family.png&imgrefurl=https%3A%2F%2Favrhelp.mcselec.com%2Flcd_i2c_pcf8574.htm&tbnid=AiPm0WPvOAS-GM&vet=12ahUKEwi3jp6lwMX9AhUoD2IAHaWHDAsQMygHegUIARDRAQ..i&docid=7U8J__l7BuwaJM&w=1236&h=799&q=pcf8574%20lcd%20schematic&ved=2ahUKEwi3jp6lwMX9AhUoD2IAHaWHDAsQMygHegUIARDRAQ
    /// https://alselectro.wordpress.com/2016/05/12/serial-lcd-i2c-module-pcf8574/
    /// </summary>
    /*
    
    01  02  03  04  05  06  07  08  09  10  11  12  13  14  15  16
    K   A   D7  D6  D5  D4  D3  D2  D1  D0  E   RW  RS  VEE VDD VSS
                                                        Lit VCC GND
    */
    public class PCF8574
    {
        protected const byte EnableBitMode = 4; // B00000100  // Enable bit
        protected const byte ReadWriteMode = 2; // B00000010  // Read/Write bit
        protected const byte RegisterSelectMode = 1; // B00000001  // Register select bit
        protected const byte CommandMode = 0;

        public I2CDevice_MPSSE_NotUsed _i2cDevice;
        public int DeviceId;

        public const int PCF8574_I2CADDR_DEFAULT = 0x20; ///< PCF8574 default I2C address

        uint8_t _readbuf = 0, _writebuf = 0;

        public PCF8574(I2CDevice_MPSSE_NotUsed i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public bool Begin(uint8_t deviceId)
        {
            this.DeviceId = deviceId;
            return true;
        }

        /*!
         *    @brief  Write one 'byte' of data directly to the GPIO control register
         *    @param  d The data to write
         *    @return True if we were able to write the data successfully over I2C
         */
        public bool DigitalWriteByte(uint8_t d)
        {
            _writebuf = d;
            return this._i2cDevice.Send1ByteCommand(this.DeviceId, d);
        }

        /*!
         *    @brief  Read one 'byte' of data directly from the GPIO control register
         *    @return The byte of data read from the device
         */
        public uint8_t digitalReadByte()
        {
            //i2c_dev->read(&_readbuf, 1);
            //return _readbuf;
            return (uint8_t)this._i2cDevice.Read1ByteCommand(this.DeviceId);
        }


        /*!
         *    @brief  Set one GPIO expander pin to 'high' (weak pullup) or 'low'
         * (grounded)
         *    @param  pinnum The GPIO pin number, from 0 to 7 inclusive
         *    @param  val The boolean value to write: true means activate the pullup
         *    and false means turn on the sinking transistor.
         *    @return True if we were able to write the data successfully over I2C
         *    ONLY D7, D6, D5 and D4 are working as gpio D0,1,2,3 are not connected.
         *    By default to talk to the LCD screen there is only a 4 bits bus, that is why the first for gpio are not working
         *    when usingh a PCF8574A for wired for an LCD.
         */
        public bool DigitalWrite(int pin, bool val)
        {
            if (val)
                _writebuf |= (uint8_t)(1 << pin);
            else
                _writebuf &= (uint8_t)(~(1 << pin));

            var r = this._i2cDevice.WriteBuffer(this.DeviceId, (new List<byte>() { _writebuf }).ToArray());
            return r;
        }

        /*!
        *    @brief  Set one GPIO expander pin to 'output' (grounded) or 'input' (weak
        * pullup)
        *    @param  pinnum The GPIO pin number, from 0 to 7 inclusive
        *    @param  val The value to write: INPUT or INPUT_PULLUP means activate the
        * pullup and OUTPUT means turn on the sinking transistor, as this is an open
        * drain device
        *    @return True if we were able to write the data successfully over I2C
        */
        public bool PinMode(int pinnum, GpioMode val)
        {
            if ((val == GpioMode.INPUT) || (val ==  GpioMode.INPUT_PULL_UP))
            {
                _writebuf |= (uint8_t)(1 << pinnum);
            }
            else
            {
                _writebuf &= (uint8_t)(~(1 << pinnum));
            }
            //return i2c_dev->write(&_writebuf, 1);
            var r = this._i2cDevice.WriteBuffer(this.DeviceId, (new List<byte>() { _writebuf }).ToArray());
            return r;
        }

        /*!
         *    @brief  Get a GPIO expander pin value
         *    @param  pinnum The GPIO pin number, from 0 to 7 inclusive
         *    @return True if the pin logic is NOT ground, false if the pin logic is
         * ground
         */
        public bool DigitalRead(uint8_t pinnum)
        {
            // i2c_dev->read(&_readbuf, 1);
            _readbuf = (uint8_t)this._i2cDevice.Read1ByteCommand(this.DeviceId);
            return ((_readbuf >> pinnum) & 0x1) == 0x1;
        }
    }
}
 