/*
    Copyright (C) 2015, 2023 MadeInTheUSB LLC

    Adafruit 8x8 LED matrix with backpack
    This program control Adafruit 8x8 LED matrix with backpack
        https://learn.adafruit.com/adafruit-led-backpack/overview
            https://www.adafruit.com/product/872
            https://www.adafruit.com/product/1049 
    The C# files
        Components\Adafruit\Adafruit_GFX.cs
        Components\adafruit\ledbackpack.cs
    are a port from Adafruit library Adafruit-LED-Backpack-Library
    https://github.com/adafruit/Adafruit-LED-Backpack-Library
  
    Written by FT for MadeInTheUSB
    MIT license, all text above must be included in any redistribution

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
 
*/

// Adafruit_LEDBackpack.cpp
// https://github.com/adafruit/Adafruit-LED-Backpack-Library
/*************************************************** 
  This is a library for our I2C LED Backpacks

  Designed specifically to work with the Adafruit LED Matrix backpacks 
  ----> http://www.adafruit.com/products/
  ----> http://www.adafruit.com/products/

  These displays use I2C to communicate, 2 pins are required to 
  interface. There are multiple selectable I2C addresses. For backpacks
  with 2 Address Select pins: 0x70, 0x71, 0x72 or 0x73. For backpacks
  with 3 Address Select pins: 0x70 thru 0x77

  Adafruit invests time and resources providing this open source code, 
  please support Adafruit and open-source hardware by purchasing 
  products from Adafruit!

  Written by Limor Fried/Ladyada for Adafruit Industries.  
  MIT license, all text above must be included in any redistribution
 ****************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using MadeInTheUSB.WinUtil;
using MadeInTheUSB.FT232H;
using System.Diagnostics;

namespace MadeInTheUSB.Adafruit
{
    /// <summary>
    /// https://learn.adafruit.com/adafruit-led-backpack/changing-i2c-address
    /// HT16K33 https://cdn-shop.adafruit.com/datasheets/ht16K33v110.pdf
    /// </summary>
    public class LEDBackpack : Adafruit_GFX
    {
        private const string DEFAULT_I2C_ERROR_MESSAGE = "I2C command failed, check your connections";

        private const byte HT16K33_BLINK_CMD = 0x80;
        private const byte HT16K33_BLINK_DISPLAYON = 0x01;
        private const byte HT16K33_BLINK_OFF = 0;
        private const byte HT16K33_BLINK_2HZ = 1;
        private const byte HT16K33_BLINK_1HZ = 2;
        private const byte HT16K33_BLINK_HALFHZ = 3;
        private const byte HT16K33_CMD_BRIGHTNESS = 0xE0;
        private const byte HT16K33_CMD_TURN_OSCILLATOR_ON = 0x21;

        public const int MAX_ROW = 8;

        private byte[] _displayBuffer = new byte[MAX_ROW];
        private const byte DEFAULT_I2C_ADDRESS_0 = 0x70;
        private const byte DEFAULT_I2C_ADDRESS_1 = 0x71;
        /// <summary>
        /// See datasheet section "Display Memory – RAM Structure"
        /// </summary>
        List<byte> HT16K33_CMD_WRITE_DISPLAY_START_ADDRESS = new List<byte>() { 0x00, 0x2, 0x4, 0x6, 0x8, 0xA, 0xC, 0xE };
        private readonly I2CDevice _i2CDevice;

        public byte DeviceId;

        public LEDBackpack(I2CDevice i2cDevice, int16_t width, int16_t height): base(width, height)
        {
            _i2CDevice = i2cDevice;            
        }

        public void DrawPixel(int x, int y, bool color)
        {
            this.DrawPixel((int16_t)x, (int16_t)y, (uint16_t)(color ? 1 : 0));
        }

        public override void DrawPixel(int16_t x, int16_t y, uint16_t color)
        {
            if ((y < 0) || (y >= 8)) return;
            if ((x < 0) || (x >= 8)) return;

            // check _rotation, move pixel around if necessary
            switch (this._rotation)
            {
                case 1:
                    Swap(ref x, ref y);
                    x = (int16_t)(8 - x - 1);
                    break;
                case 2:
                    x = (int16_t)(8 - x - 1);
                    y = (int16_t)(8 - y - 1);
                    break;
                case 3:
                    Swap(ref x, ref y);
                    y = (int16_t)(8 - y - 1);
                    break;
            }

            // wrap around the x
            x += 7;
            x %= 8;

            if (color > 0)
            {
                _displayBuffer[y] |= (byte)(1 << x); //   [y] is the row x bit for the column
            }
            else
            {
                _displayBuffer[y] &= (byte)(~(1 << x));
            }
        }


        public bool Detect(byte addr0 = DEFAULT_I2C_ADDRESS_0, byte addr1 = DEFAULT_I2C_ADDRESS_1)
        {
            try
            {
                if (this.Begin(addr0)) return true;
                if (this.Begin(addr1)) return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public void SetRotation(int v)
        {
            base.Rotation = (byte)v;
        }

        public bool Begin(int addr = DEFAULT_I2C_ADDRESS_0)
        {
            var r = this._begin((byte)addr);
            if(r)
            {
                this._i2CDevice.RegisterDeviceIdForLogging((byte)addr, this.GetType());
            }
            return r;
        }

        private bool _begin(byte addr )
        {
            this.DeviceId = addr;

            this._i2CDevice.RegisterDeviceIdForLogging((byte)addr, this.GetType());

            if (this._i2CDevice.DetectDevice(this.DeviceId))
            {
                if (!this._i2CDevice.Write(HT16K33_CMD_TURN_OSCILLATOR_ON, this.DeviceId)) return false;
                this.Clear(true);
                this.SetBlinkRate(HT16K33_BLINK_OFF);
                this.SetBrightness(5);

                return true;
            }
            else return false;
        }

        public void AnimateSetBrightness(int MAX_REPEAT, int onWaitTime = 20, int offWaitTime = 30)
        {
            for (byte rpt = 0; rpt < MAX_REPEAT; rpt++)
            {
                for (byte b = 0; b < 15; b++)
                {
                    this.SetBrightness(b);
                    TimePeriod.Sleep(onWaitTime);
                }
                for (byte b = 15; b > 0; b--)
                {
                    this.SetBrightness(b);
                    TimePeriod.Sleep(offWaitTime);
                }
            }
        }

        public void SetBrightness(int b)
        {
            SetBrightness((byte)b);
        }

        private byte _brightness;

        public byte GetBrightness()
        {
            return this._brightness;
        }

        public void SetBrightness(byte b)
        {
            if (b > 15) b = 15;
            this._brightness = b;
            if (!this._i2CDevice.Write(HT16K33_CMD_BRIGHTNESS | b, this.DeviceId))
                throw new I2CCommunicationException(DEFAULT_I2C_ERROR_MESSAGE);
        }

        public void SetBlinkRate(byte b)
        {
            if (b > 3) b = 0; // turn off if not sure  
            if (!this._i2CDevice.Write(HT16K33_BLINK_CMD | HT16K33_BLINK_DISPLAYON | (b << 1), this.DeviceId))
                throw new I2CCommunicationException(DEFAULT_I2C_ERROR_MESSAGE);
        }

        public virtual void Clear(bool refresh = false, byte value = 0)
        {
            for (var i = 0; i < MAX_ROW; i++)
            {
                _displayBuffer[i] = value;
            }
            if (refresh)
                this.WriteDisplay();
        }

        public bool WriteDisplay()
        {
            var buf = new List<byte>();
            buf.Add(HT16K33_CMD_WRITE_DISPLAY_START_ADDRESS[0]);
            for (var i = 0; i < MAX_ROW; i++)
            {
                buf.Add((uint8_t)(_displayBuffer[i] & 0xFF));    // 8 bit of columns
                buf.Add((uint8_t)(_displayBuffer[i] >> 8));
            }
            return this._i2CDevice.WriteBuffer(buf.ToArray(), this.DeviceId);
        }

        //public bool WriteDisplay()
        //{
        //    for (var i = 0; i < MAX_ROW; i++)
        //        WriteLineDisplay(i);
        //    return true;
        //}

        public bool WriteLineDisplay(int row)
        {
            var buf = new List<byte>();
            buf.Add(HT16K33_CMD_WRITE_DISPLAY_START_ADDRESS[row]);
            buf.Add((uint8_t)(_displayBuffer[row] & 0xFF));
            buf.Add((uint8_t)(_displayBuffer[row] >> 8));
            return this._i2CDevice.WriteBuffer(buf.ToArray(), this.DeviceId);
        }
    }
}

