/*
    
   Copyright (C) 2015 MadeInTheUSB LLC
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

using System;
using System.Diagnostics;
using MadeInTheUSB;
using MadeInTheUSB.Adafruit;
using System.Linq;
using int16_t  = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t  = System.Byte;
using size_t   = System.Int16;
using MadeInTheUSB.WinUtil;
using System.Collections.Generic;
using MadeInTheUSB.FT232H;

namespace MadeInTheUSB.Display
{
    /// <summary>
    /// SSD1306 - https://www.adafruit.com/datasheets/SSD1306.pdf
    ///           https://cdn-shop.adafruit.com/datasheets/SSD1306.pdf
    /// </summary>
    public class OLED_SSD1306 : OLED
    {
        public OLED_SSD1306(I2CDevice i2cDevice, int width, int height, bool debug = false) : base(i2cDevice, width, height, OledDriver.SSD1306)
        {
        }

        private bool Is64RowsDevice => this.Height == 64;

        public override void Begin(bool invert = false, uint8_t contrast = 128, uint8_t Vpp = 0)
        {
            if (!this._i2cDevice.InitiateDetectionSequence(this.DeviceId))
                return;
                
            uint8_t invertSetting = (uint8_t)(invert ? 0xA7 : 0xA6);
            Vpp &= 0x03;
	
            this.SendCommand(OLED_API_DISPLAYOFF);
            this.SendCommand(OLED_API_SETDISPLAYCLOCKDIV, 0x80);
            this.SendCommand(OLED_API_SETMULTIPLEX, this.Is64RowsDevice ? 0x3F : 0x1F);
            this.SendCommand(OLED_API_SETSTARTLINE | 0x0);
            this.SendCommand(OLED_API_CHARGEPUMP);

            int vccstate = OLED_API_SWITCHCAPVCC;
            if (vccstate == OLED_API_EXTERNALVCC)
                SendCommand(0x10);
            else
                SendCommand(0x14);

            SendCommand(OLED_API_SETMULTIPLEX, this.Height-1); // 64MUX for 128 x 64 version - 32MUX for 128 x 32 version
            SendCommand(OLED_API_MEMORYMODE, 0x00); // 0x20

            SendCommand(OLED_API_SSD1306_SET_SEGMENT_REMAP | 0x1);
            SendCommand(OLED_API_COMSCANDEC);

            SendCommand(OLED_API_SETCOMPINS, this.Is64RowsDevice ? 0x12: 0x02);

            SendCommand(OLED_API_SETCONTRAST);                   // 0x81
            if (vccstate == OLED_API_EXTERNALVCC)
                SendCommand(0x9F);
            else
                SendCommand(0xCF);

            SendCommand(OLED_API_SETPRECHARGE);                  // 0xd9
            if (vccstate == OLED_API_EXTERNALVCC)
                SendCommand(0x22); 
            else
                SendCommand(0xF1); 

            SendCommand(OLED_API_SETVCOMDETECT, 0x40);                 // 0xDB
            SendCommand(OLED_API_DISPLAYALLON_RESUME);           // 0xA4
            SendCommand(OLED_API_NORMALDISPLAY);                 // 0xA6

            // SendCommand(OLED_API_DEACTIVATE_SCROLL);

            SendCommand(OLED_API_SH1106_PAGE_ADDR);                 // 0xB0
            SendCommand(OLED_API_SETHIGHCOLUMN);                 // 0x10
            SendCommand(0x01);                 // 0x10

            SendCommand(OLED_API_DISPLAYON);//--turn on oled panel

            this.Clear(true);
            
            // WriteDisplay Optimized: False does not work right first time called
            // when we have pixel. Can't understand why. This seems to fix the problem
            this.Fill(true, false);
            // this.Fill(true, false);
            this.Clear(true);
        }
    }
}