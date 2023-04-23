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
using System;

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
            SET_CONTRAST = 0x81,
            DISPLAY_ALL_ON_RESUME = 0xA4,
            DISPLAY_ALL_ON = 0xA5,
            NORMAL_DISPLAY = 0xA6,
            INVERT_DISPLAY = 0xA7,
            DISPLAY_OFF = 0xAE,
            DISPLAY_ON = 0xAF,
            SET_DISPLAY_OFFSET = 0xD3,

            SET_COM_PINS_CONFIGURATION = 0xDA,
            SET_COM_PINS_CONFIGURATION_64_ROWS_PARAMETER = 0x12,
            SET_COM_PINS_CONFIGURATION_32_ROWS_PARAMETER = 0x02,

            SET_VCOM_DETECT = 0xDB,
            SET_VCOMDETECT_PARAMETER = 0x40,
            SEG_REMAP = 0xA0,
            SET_DISPLAY_CLOCK_DIV = 0xD5,
            SET_DISPLAY_CLOCK_DIV_PARAMETER = 0x80,

            SET_PRECHARGE = 0xD9,
            SET_MULTIPLEX = 0xA8,
            SET_LOW_COLUMN = 0x00,
            SET_HIGH_COLUMN = 0x10,
            SET_HIGH_COLUMN_PARAMETER = 0x01,

            SET_START_LINE = 0x40,
            MEMORY_MODE = 0x20,
            COLUMN_ADDR = 0x21,
            COLUMN_ADDR_START = 0,
            COLUMN_ADDR_END = 128 - 1,
            SET_COM_OUTPUT_SCAN_DIRECTION_INC = 0xC0,
            COM_SCAN_DEC = 0xC8,
            CHARGE_PUMP = 0x8D,

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
            //SH1106_SETLOWCOLUMN = 0x02,
            //SH1106_PAGE_ADDR = 0xB0,
            //SH1106_SET_SEGMENT_REMAP = 0xA1,
            PAGE_ADDR = 0x22,
            START_PAGE_ADDR = 0,
            END_PAGE_ADDR_32_ROWS = 4-1,/*(int height) this.Height == 64 ? 7 : 3;*/
            END_PAGE_ADDR_64_ROWS = 8-1,/*(int height) this.Height == 64 ? 7 : 3;*/
        };


        public static List<byte> PossibleI2COleAddress = new List<uint8_t>()  { 0x3C, 0x3D }; // 0x78, 

        public I2C_OLED_SSD1306(I2CDevice i2cDevice, int width, int height, bool debug = false) : 
            base(i2cDevice, width, height, new List<uint8_t>() { (byte)SSD1306_API.SET_START_LINE }, OledDriver.SSD1306)
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
            //this.DeviceId = this._i2cDevice.DetectI2CDevice(PossibleI2COleAddress);
            //if (this.DeviceId == -1)
            //    return false;

            this.DeviceId = 0x3D;
            this.DeviceId = 0x3C;
            
            if (this.Is64RowsDevice)
                Init128x64();
            else
                Init128x32();

            this.Clear(true);

            return true;
        }

        private void Init128x64()
        {
            this.SendCommand(SSD1306_API.DISPLAY_OFF);
            this.SendCommand(SSD1306_API.SET_DISPLAY_CLOCK_DIV, (byte)SSD1306_API.SET_DISPLAY_CLOCK_DIV_PARAMETER);

            this.SendCommand(SSD1306_API.SET_MULTIPLEX, this.Height - 1); // 64MUX for 128 x 64 version - 32MUX for 128 x 32 version

            this.SendCommand(SSD1306_API.SET_DISPLAY_OFFSET, 0x00); // no offset h64
            this.SendCommand((byte)(SSD1306_API.SET_START_LINE | 0x0));
            this.SendCommand(SSD1306_API.CHARGE_PUMP);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x10 : SSD1306_API._0x14);
            this.SendCommand(SSD1306_API.MEMORY_MODE, (byte)SSD1306_MEMORY_MODE.HORIZONTAL_MODE);


            this.SendCommand(((byte)SSD1306_API.SEG_REMAP) | 0x1); // REMAPPING OF X DIRECTION FRED

            this.SendCommand(SSD1306_API.SET_COM_OUTPUT_SCAN_DIRECTION_INC, 0);
            this.SendCommand(SSD1306_API.COM_SCAN_DEC);

            this.SendCommand(SSD1306_API.SET_COM_PINS_CONFIGURATION, (byte)SSD1306_API.SET_COM_PINS_CONFIGURATION_64_ROWS_PARAMETER);
            //this.SendCommand(SSD1306_API.SET_COM_PINS_CONFIGURATION, (byte)0x02);

            this.SendCommand(SSD1306_API.SET_CONTRAST);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x9F : SSD1306_API._0xCF);
            this.SendCommand(SSD1306_API.SET_PRECHARGE, (byte)(this.IsExternalVcc ? SSD1306_API._0x22 : SSD1306_API._0xF1));
            this.SendCommand(SSD1306_API.SET_VCOM_DETECT, (byte)SSD1306_API.SET_VCOMDETECT_PARAMETER);
            this.SendCommand(SSD1306_API.DISPLAY_ALL_ON_RESUME);
            this.SendCommand(SSD1306_API.NORMAL_DISPLAY);

            // this.SendCommand(SSD1306_API.DEACTIVATE_SCROLL);

            this.SendCommand(SSD1306_API.DISPLAY_ON);
        }

        private void Init128x32()
        {
            this.SendCommand(SSD1306_API.DISPLAY_OFF);
            this.SendCommand(SSD1306_API.SET_DISPLAY_CLOCK_DIV, (byte)SSD1306_API.SET_DISPLAY_CLOCK_DIV_PARAMETER);
            this.SendCommand(SSD1306_API.SET_START_LINE | 0x0);
            this.SendCommand(SSD1306_API.CHARGE_PUMP);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x10 : SSD1306_API._0x14);
            this.SendCommand(SSD1306_API.SET_MULTIPLEX, this.Height - 1); // 64MUX for 128 x 64 version - 32MUX for 128 x 32 version
            this.SendCommand(SSD1306_API.SET_DISPLAY_OFFSET, 0x00); // no offset h64
            this.SendCommand(SSD1306_API.MEMORY_MODE, (byte)SSD1306_MEMORY_MODE.HORIZONTAL_MODE);
            this.SendCommand(((byte)SSD1306_API.SEG_REMAP) | 0x1); //
            this.SendCommand(SSD1306_API.COM_SCAN_DEC);
            this.SendCommand(SSD1306_API.SET_COM_PINS_CONFIGURATION, (byte)(this.Is64RowsDevice ? SSD1306_API.SET_COM_PINS_CONFIGURATION_64_ROWS_PARAMETER : SSD1306_API.SET_COM_PINS_CONFIGURATION_32_ROWS_PARAMETER));
            this.SendCommand(SSD1306_API.SET_CONTRAST);
            if (this.Is64RowsDevice)
            {
                this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x9F : SSD1306_API._0xCF);
            }
            else
            {
                this.SendCommand(SSD1306_API._0x8F); // 32 rows
            }
            this.SendCommand(SSD1306_API.SET_PRECHARGE, (byte)(this.IsExternalVcc ? SSD1306_API._0x22 : SSD1306_API._0xF1));
            this.SendCommand(SSD1306_API.SET_VCOM_DETECT, (byte)SSD1306_API.SET_VCOMDETECT_PARAMETER);
            this.SendCommand(SSD1306_API.DISPLAY_ALL_ON_RESUME);
            // this.SendCommand(SSD1306_API.DISPLAYALLON_RESUME);
            this.SendCommand(SSD1306_API.NORMAL_DISPLAY);
            this.SendCommand(SSD1306_API.DEACTIVATE_SCROLL);

            //this.SendCommand(SSD1306_API.SH1106_PAGE_ADDR);
            //this.SendCommand(SSD1306_API.SETHIGHCOLUMN, (byte)SSD1306_API.SETHIGHCOLUMN_PARAMETER);
            //this.SendCommand(SSD1306_API.PAGE_ADDR, (byte)SSD1306_API.START_PAGE_ADDR, (byte)(this.Is64RowsDevice ? SSD1306_API.END_PAGE_ADDR_64_ROWS : SSD1306_API.END_PAGE_ADDR_32_ROWS));
            //this.SendCommand(SSD1306_API.COLUMNADDR, (byte)SSD1306_API.COLUMNADDR_START, (byte)SSD1306_API.COLUMNADDR_END);

            this.SendCommand((byte)SSD1306_API.COLUMN_ADDR, (byte)SSD1306_API.COLUMN_ADDR_START, (byte)SSD1306_API.COLUMN_ADDR_END);
            this.SendCommand((byte)SSD1306_API.PAGE_ADDR, (byte)SSD1306_API.START_PAGE_ADDR, (byte)(this.Height == 64 ? SSD1306_API.END_PAGE_ADDR_64_ROWS : SSD1306_API.END_PAGE_ADDR_32_ROWS));

            this.SendCommand(SSD1306_API.DISPLAY_ON);
        }

        public void Contrast(byte val)
        {
            this.SendCommand(SSD1306_API.SET_CONTRAST, val);
        }

        public void Test()
        {
            for(var i=0; i < BUF_LEN; i++) 
            {
                _buffer[i] = (byte)0xFF;
            }
            this.WriteDisplay();
        }
    }
}