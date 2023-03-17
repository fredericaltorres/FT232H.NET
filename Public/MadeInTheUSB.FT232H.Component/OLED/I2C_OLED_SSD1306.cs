/*
    
   Copyright (C) 2015, 2023 MadeInTheUSB LLC
   Ported to C# and Nusbio by FT for MadeInTheUSB

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
 
   Based from:
 
        This is a "Fast SH1106 Library". It is designed to be used with
        128x64 OLED displays, driven by the SH1106 controller.
        This library uses hardware SPI of your Arduino microcontroller,
        and does not supprt 'software SPI' mode.
 
        Written by Arthur Liberman (aka 'The Coolest'). http://www.alcpu.com
        Special thanks goes out to 'robtillaart' for his help with debugging
        and optimization.

        BSD license, check license.txt for more information.
        All text above must be included in any redistribution.
 
   Also based 
       on stanleyhuangyc/MultiLCD
       https://github.com/stanleyhuangyc/MultiLCD/tree/master/MicroLCD
       officeboy/sh1106
       https://github.com/officeboy/sh1106/blob/master/firmware/sh1106.cpp
*/

using uint8_t = System.Byte;
using MadeInTheUSB.FT232H;
using System.Collections.Generic;

namespace MadeInTheUSB.Display
{
    /// <summary>
    /// SSD1306 - https://www.adafruit.com/datasheets/SSD1306.pdf
    /// </summary>
    public class I2C_OLED_SSD1306 : I2C_OLED
    {
        public enum SSD1306_VCC
        {
            EXTERNAL_VCC = 0x01,
            SWITCH_CAP_VCC = 0x02,
        }

        public enum SSD1306_MEMORY_MODE
        {
            HORIZONTAL_MODE = 0x0,
            VERTICAL_MODE = 0x1,
            PAGE_ADDRESSING_MODE_RESET = 2,
        }

        public enum SSD1306_API
        {
            SETCONTRAST = 0x81,
            DISPLAYALLON_RESUME = 0xA4,
            DISPLAYALLON = 0xA5,
            NORMALDISPLAY = 0xA6,
            INVERTDISPLAY = 0xA7,
            DISPLAYOFF = 0xAE,
            DISPLAYON = 0xAF,
            SETDISPLAYOFFSET = 0xD3,

            SET_COM_PINS_CONFIGURATION = 0xDA,
            SET_COM_PINS_CONFIGURATION_64_ROWS_PARAMETER = 0x12,
            SET_COM_PINS_CONFIGURATION_32_ROWS_PARAMETER = 0x02,

            SETVCOMDETECT = 0xDB,
            SETVCOMDETECT_PARAMETER = 0x40,
            SETDISPLAYCLOCKDIV = 0xD5,
            SETDISPLAYCLOCKDIV_PARAMETER = 0x80,

            SETPRECHARGE = 0xD9,
            SETMULTIPLEX = 0xA8,
            SSD1306_SETLOWCOLUMN = 0x00,
            SETHIGHCOLUMN = 0x10,
            SETHIGHCOLUMN_PARAMETER = 0x01,

            SETSTARTLINE = 0x40,
            MEMORYMODE = 0x20,
            COLUMNADDR = 0x21,
            COLUMNADDR_START = 0,
            COLUMNADDR_END = 128 - 1,
            COMSCANINC = 0xC0,
            COMSCANDEC = 0xC8,
            SSD1306_SET_SEGMENT_REMAP = 0xA0,
            CHARGEPUMP = 0x8D,

            //EXTERNAL_VCC = 0x01,
            //SWITCH_CAP_VCC = 0x02,
            _0x10 = 0x10,
            _0x14 = 0x14,
            _0x9F = 0x9F,
            _0xCF = 0xCF,
            _0x22 = 0x22,
            _0xF1 = 0xF1,
            _0x8F = 0x8F,

            ACTIVATE_SCROLL = 0x2F,
            DEACTIVATE_SCROLL = 0x2E,
            SET_VERTICAL_SCROLL_AREA = 0xA3,
            RIGHT_HORIZONTAL_SCROLL = 0x26,
            LEFT_HORIZONTAL_SCROLL = 0x27,
            VERTICAL_AND_RIGHT_HORIZONTAL_SCROLL = 0x29,
            VERTICAL_AND_LEFT_HORIZONTAL_SCROLL = 0x2A,
            SH1106_SETLOWCOLUMN = 0x02,
            SH1106_PAGE_ADDR = 0xB0,
            SH1106_SET_SEGMENT_REMAP = 0xA1,
            PAGE_ADDR = 0x22,
            START_PAGE_ADDR = 0,
            END_PAGE_ADDR_32_ROWS = 3,/*(int height) this.Height == 64 ? 7 : 3;*/
            END_PAGE_ADDR_64_ROWS = 7,/*(int height) this.Height == 64 ? 7 : 3;*/
        };


        public static List<byte> PossibleI2COleAddress = new List<uint8_t>()  { 0x3C, 0x3D };

        public I2C_OLED_SSD1306(I2CDevice i2cDevice, int width, int height, bool debug = false) : 
            base(i2cDevice, width, height, new List<uint8_t>() { (byte)SSD1306_API.SETSTARTLINE }, OledDriver.SSD1306)
        {
        }

        private bool Is64RowsDevice => this.Height == 64;

        private SSD1306_VCC _vccState = SSD1306_VCC.SWITCH_CAP_VCC;
        private bool IsExternalVcc => this._vccState == SSD1306_VCC.EXTERNAL_VCC;

        protected void SendCommand(SSD1306_API command, params int[] commands)
        {
            base.SendCommand((byte)command, commands);
        }

        public bool Begin(bool invert = false, uint8_t contrast = 128, uint8_t Vpp = 0)
        {
            this.DeviceId = this._i2cDevice.DetectI2CDevice(PossibleI2COleAddress);
            if (this.DeviceId == -1)
                return false;
	
            this.SendCommand(SSD1306_API.DISPLAYOFF);
            this.SendCommand(SSD1306_API.SETDISPLAYCLOCKDIV, (byte)SSD1306_API.SETDISPLAYCLOCKDIV_PARAMETER);
            this.SendCommand(SSD1306_API.SETSTARTLINE | 0x0);
            this.SendCommand(SSD1306_API.CHARGEPUMP);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x10 : SSD1306_API._0x14);
            this.SendCommand(SSD1306_API.SETMULTIPLEX, this.Height-1); // 64MUX for 128 x 64 version - 32MUX for 128 x 32 version

            this.SendCommand(SSD1306_API.SETDISPLAYOFFSET, 0x00); // no offset h64
            this.SendCommand(SSD1306_API.MEMORYMODE, (byte)SSD1306_MEMORY_MODE.HORIZONTAL_MODE);
            this.SendCommand(((byte)SSD1306_API.SSD1306_SET_SEGMENT_REMAP) | 0x1); // 
            this.SendCommand(SSD1306_API.COMSCANDEC);
            this.SendCommand(SSD1306_API.SET_COM_PINS_CONFIGURATION, (byte)(this.Is64RowsDevice ? SSD1306_API.SET_COM_PINS_CONFIGURATION_64_ROWS_PARAMETER : SSD1306_API.SET_COM_PINS_CONFIGURATION_32_ROWS_PARAMETER));


            this.SendCommand(SSD1306_API.SETCONTRAST);
            if(this.Is64RowsDevice)
            {
                this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x9F : SSD1306_API._0xCF);
            }
            else
            {
                this.SendCommand(SSD1306_API._0x8F); // 32 rows
            }
            
            this.SendCommand(SSD1306_API.SETPRECHARGE, (byte)(this.IsExternalVcc ? SSD1306_API._0x22 : SSD1306_API._0xF1));

            this.SendCommand(SSD1306_API.SETVCOMDETECT, (byte)SSD1306_API.SETVCOMDETECT_PARAMETER);
            this.SendCommand(SSD1306_API.DISPLAYALLON_RESUME);
            this.SendCommand(SSD1306_API.DISPLAYALLON_RESUME);
            this.SendCommand(SSD1306_API.NORMALDISPLAY);
            this.SendCommand(SSD1306_API.DEACTIVATE_SCROLL);

            //this.SendCommand(SSD1306_API.SH1106_PAGE_ADDR);
            //this.SendCommand(SSD1306_API.SETHIGHCOLUMN, (byte)SSD1306_API.SETHIGHCOLUMN_PARAMETER);
            //this.SendCommand(SSD1306_API.PAGE_ADDR, (byte)SSD1306_API.START_PAGE_ADDR, (byte)(this.Is64RowsDevice ? SSD1306_API.END_PAGE_ADDR_64_ROWS : SSD1306_API.END_PAGE_ADDR_32_ROWS));
            //this.SendCommand(SSD1306_API.COLUMNADDR, (byte)SSD1306_API.COLUMNADDR_START, (byte)SSD1306_API.COLUMNADDR_END);

            this.SendCommand(SSD1306_API.DISPLAYON);

            this.Clear(true);

            return true;
        }

        public void Contrast(byte val)
        {
            this.SendCommand(SSD1306_API.SETCONTRAST, val);
        }
    }
}