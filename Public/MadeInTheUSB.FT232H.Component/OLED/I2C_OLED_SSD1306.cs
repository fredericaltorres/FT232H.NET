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

namespace MadeInTheUSB.Display
{
    /// <summary>
    /// SSD1306 - https://www.adafruit.com/datasheets/SSD1306.pdf
    /// </summary>
    public class I2C_OLED_SSD1306 : I2C_OLED
    {
        public I2C_OLED_SSD1306(I2CDevice i2cDevice, int width, int height, bool debug = false) : base(i2cDevice, width, height, OledDriver.SSD1306)
        {
        }

        private bool Is64RowsDevice => this.Height == 64;

        private SSD1306_VCC _vccState = SSD1306_VCC.SWITCH_CAP_VCC;
        private bool IsExternalVcc => this._vccState == SSD1306_VCC.EXTERNAL_VCC;

        //if (_vccState == SSD1306_API.EXTERNAL_VCC)

        public bool Begin(bool invert = false, uint8_t contrast = 128, uint8_t Vpp = 0)
        {
            if (!this._i2cDevice.InitiateDetectionSequence(this.DeviceId))
                return false;
	
            this.SendCommand(SSD1306_API.DISPLAYOFF);
            this.SendCommand(SSD1306_API.SETDISPLAYCLOCKDIV, (byte)SSD1306_API.SETDISPLAYCLOCKDIV_PARAMETER);
            this.SendCommand(SSD1306_API.SETSTARTLINE | 0x0);
            this.SendCommand(SSD1306_API.CHARGEPUMP);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x10 : SSD1306_API._0x14);
            this.SendCommand(SSD1306_API.SETMULTIPLEX, this.Height-1); // 64MUX for 128 x 64 version - 32MUX for 128 x 32 version
            this.SendCommand(SSD1306_API.MEMORYMODE, (byte)SSD1306_MEMORY_MODE.HORIZONTAL_MODE);
            this.SendCommand(SSD1306_API.SSD1306_SET_SEGMENT_REMAP); // | 0x1
            this.SendCommand(SSD1306_API.COMSCANDEC);
            this.SendCommand(SSD1306_API.SETCOMPINS, (byte)(this.Is64RowsDevice ? SSD1306_API.SETCOMPINS_64_ROWS_PARAMETER: SSD1306_API.SETCOMPINS_32_ROWS_PARAMETER));
            this.SendCommand(SSD1306_API.SETCONTRAST);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x9F : SSD1306_API._0xCF);
            this.SendCommand(SSD1306_API.SETPRECHARGE);
            this.SendCommand(this.IsExternalVcc ? SSD1306_API._0x22 : SSD1306_API._0xF1);
            this.SendCommand(SSD1306_API.SETVCOMDETECT, (byte)SSD1306_API.SETVCOMDETECT_PARAMETER);
            this.SendCommand(SSD1306_API.DISPLAYALLON_RESUME);
            this.SendCommand(SSD1306_API.NORMALDISPLAY);
            this.SendCommand(SSD1306_API.SH1106_PAGE_ADDR);
            this.SendCommand(SSD1306_API.SETHIGHCOLUMN, (byte)SSD1306_API.SETHIGHCOLUMN_PARAMETER);
            this.SendCommand(SSD1306_API.PAGE_ADDR,  (byte)SSD1306_API.START_PAGE_ADDR,  (byte)(this.Is64RowsDevice ? SSD1306_API.END_PAGE_ADDR_64_ROWS: SSD1306_API.END_PAGE_ADDR_32_ROWS));
            this.SendCommand(SSD1306_API.COLUMNADDR, (byte)SSD1306_API.COLUMNADDR_START, (byte)SSD1306_API.COLUMNADDR_END);
            this.SendCommand(SSD1306_API.DISPLAYON);

            this.Clear(true);
            
            return true;
        }
    }
}