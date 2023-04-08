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
       https://github.com/Matiasus/SSD1306  (Matiasus )

*/

using System;
using MadeInTheUSB.Adafruit;
using System.Linq;
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using size_t = System.Int16;
using System.Collections.Generic;
using MadeInTheUSB.FT232H;
using static MadeInTheUSB.Display.I2C_OLED_SSD1306;

namespace MadeInTheUSB.Display
{
    /// <summary>
    /// SSD1306 - https://www.adafruit.com/datasheets/SSD1306.pdf
    /// https://github.com/chadwyck-w/FT232H-MPSSE-I2C-SSD1306-OLED/blob/master/i2c_lib.c
    /// </summary>
    public class I2C_OLED : Adafruit_GFX
    {
        public enum OledDriver 
        {
            SH1106,
            SSD1306
        }

        public int DeviceId;

        public const int SH1106_COMMAND     = 0;
        public const int SH1106_DATA        = 1;

        public const int SH1106_X_PIXELS    = 128;
        public const int SH1106_Y_PIXELS    = 32;
        public const int SH1106_ROWS        = 8;
        public const int BUF_LEN            =  512; // (512*8)/128 == 32 Rows
        private uint8_t[] _buffer           = new uint8_t[BUF_LEN];

        public const int SH1106_SUCCESS = 1;
        public const int SH1106_ERROR   = 0;

        private int _position;

        public OledDriver Driver = OledDriver.SH1106;

        private List<byte> _writeDisplayCommands = new List<byte>();

        protected I2CDevice _i2cDevice;
        public byte Deviceid;


        public I2C_OLED(I2CDevice i2cDevice, int width, int height,
            List<byte> writeDisplayCommands,
            OledDriver driver = OledDriver.SSD1306, bool debug = false) : base((Int16)width, (Int16)height)
        {
            this.Driver     = driver;
            this.Width      = (short)width;
            this.Height     = (short)height;

            this._i2cDevice = i2cDevice;
            this._writeDisplayCommands = writeDisplayCommands;
        }

        public virtual void Begin(byte deviceId, bool invert = false, uint8_t contrast = 128, uint8_t Vpp = 0)
        {
            this.Deviceid = deviceId;
        }

        public override void DrawPixel(size_t x, size_t y, uint16_t color)
        {
            this.SetPixel(x, y, color == 1);
        }

        public void WriteDisplay()
        {
            var bytePerRows = 16;
            var x = 0;
            this.SendCommand((byte)SSD1306_API.COLUMNADDR, (byte)SSD1306_API.COLUMNADDR_START, (byte)SSD1306_API.COLUMNADDR_END);
            this.SendCommand((byte)SSD1306_API.PAGE_ADDR, (byte)SSD1306_API.START_PAGE_ADDR,
                             (byte)(this.Height == 64 ? SSD1306_API.END_PAGE_ADDR_64_ROWS : SSD1306_API.END_PAGE_ADDR_32_ROWS));

            while (true)
            {
                var tmpBuffer = this._buffer.ToList().Skip(x * bytePerRows).Take(bytePerRows).ToList();
                if (tmpBuffer.Count == 0)
                    break;
                var buffer2 = new List<byte>();
                
                buffer2.AddRange(_writeDisplayCommands);
                buffer2.AddRange(tmpBuffer);
                this._i2cDevice.WriteBuffer(buffer2.ToArray(), this.Deviceid);
                x += 1;
            }
        }
        
        public void SetBuffer(int index, byte val, bool refresh = false)
        {
            this._buffer[index] = val;
            if(refresh)
                WriteDisplay();
        }

        public void SetPixel(int x, int y, bool on)
        {
	        if (x >= SH1106_X_PIXELS || y >= SH1106_Y_PIXELS)
                return;

            if (y < 0) y = 0;
            if (x < 0) x = 0;

            uint8_t bank    = (byte)(y / 8);
	        uint8_t bitMask = (byte)(1 << (y % 8));
            int index       = (SH1106_X_PIXELS * bank) + x;
	        int b           = this._buffer[index];
	        if (on)
		        b |= bitMask;
	        else
		        b &= ~bitMask;

            this._buffer[index] = (byte)b;
        }

        public void Fill(bool refresh = false, bool optimized = true)
        {
            this._buffer = new uint8_t[BUF_LEN];
            for (var i = 0; i < BUF_LEN; i++)
                this._buffer[i] = 0xFF;
            if (refresh)
                this.WriteDisplay();
        }

        public void Clear(bool refresh = false)
        {
            this._buffer = new uint8_t[BUF_LEN];
            if (refresh)
                this.WriteDisplay();
        }

        protected void SendCommand(byte command, params int [] commands)
        {
            SendCommandOneByte(command);
            if (commands.Length > 0)
                foreach (var c in commands)
                    SendCommandOneByte(c);
        }

        protected void SendCommandOneByte(int command)
        {
            this._i2cDevice.WriteBuffer(new List<byte>() { SH1106_COMMAND, (byte)command }.ToArray(), this.Deviceid);
        }
        
        public void WriteString(int x, int y, string s, bool clearText = false)
        {
            // We cannot set y to a specific location. We can only set it to a multiple 8
            y = (y / 8) * 8; // y can only be a multiple of 8

            if (x == -1) // Center
            {
                x = ((this.Width - (s.Length * ASCII_CHAR_TABLE_CHAR_WIDTH)) / 2);
            }

            x = this.Width - x - 1;

            var chars = s.ToArray().ToList();
            chars.Reverse(); // 2023/03 For this implementation I had to reverse the chars list, but it is working

            foreach (var c in chars)
            {
                var code            = ((int)c) - ASCII_CHAR_TABLE_OFFSET;
                var charDefinition  = ASCII_CHAR_TABLE[code];

                // 2023/03 For this implementation I had to reverse the chars implementation
                for (var r = ASCII_CHAR_TABLE_CHAR_WIDTH-1; r >= 0;  r--)
                {
                    uint8_t bank                 = (byte)(y / 8);
                    this._position               = (SH1106_X_PIXELS * bank) + x;
                    this._buffer[this._position] = clearText ? (byte)0 : charDefinition[r];
                    x--;
                }
            }
        }

        public void DrawWindow(string title, string text = null)
        {
            var oledDisplay = this;
            oledDisplay.Clear();
            oledDisplay.DrawRect(0, 0, oledDisplay.Width, oledDisplay.Height, true);
            for (var i = 0; i < 4; i++)
            {
                oledDisplay.DrawLine(0, i * 2, oledDisplay.Width, i * 2, true);
            }
            oledDisplay.WriteString(-1, 0, title);
            if(text != null) 
                oledDisplay.WriteString(-1, (2 * 8), text);
            oledDisplay.WriteDisplay();
        }

        public const int ASCII_CHAR_TABLE_CHAR_WIDTH = 6;
        public const int ASCII_CHAR_TABLE_OFFSET = 0x20;

        public static readonly List<List<byte>> ASCII_CHAR_TABLE = new List<List<uint8_t>>()
        {
            new List<byte> {0x00,0x00,0x00,0x00,0x00,0x00},   //   0x20 32
            new List<byte> {0x00,0x00,0x00,0x6f,0x00,0x00},   // ! 0x21 33
            new List<byte> {0x00,0x00,0x07,0x00,0x07,0x00},   // " 0x22 34
            new List<byte> {0x00,0x14,0x7f,0x14,0x7f,0x14},   // # 0x23 35
            new List<byte> {0x00,0x24,0x2A,0x7F,0x2A,0x12},   // $ 0x24 36
            new List<byte> {0x00,0x23,0x13,0x08,0x64,0x62},   // % 0x25 37
            new List<byte> {0x00,0x36,0x49,0x56,0x20,0x50},   // & 0x26 38
            new List<byte> {0x00,0x00,0x00,0x07,0x00,0x00},   // ' 0x27 39
            new List<byte> {0x00,0x00,0x1c,0x22,0x41,0x00},   // ( 0x28 40
            new List<byte> {0x00,0x00,0x41,0x22,0x1c,0x00},   // ) 0x29 41
            new List<byte> {0x00,0x14,0x08,0x3e,0x08,0x14},   // * 0x2a 42
            new List<byte> {0x00,0x08,0x08,0x3e,0x08,0x08},   // + 0x2b 43
            new List<byte> {0x00,0x00,0x50,0x30,0x00,0x00},   // , 0x2c 44
            new List<byte> {0x00,0x08,0x08,0x08,0x08,0x08},   // - 0x2d 45
            new List<byte> {0x00,0x00,0x60,0x60,0x00,0x00},   // . 0x2e 46
            new List<byte> {0x00,0x20,0x10,0x08,0x04,0x02},   // / 0x2f 47
            new List<byte> {0x00,0x3e,0x51,0x49,0x45,0x3e},   // 0 0x30 48
            new List<byte> {0x00,0x00,0x42,0x7f,0x40,0x00},   // 1 0x31 49
            new List<byte> {0x00,0x42,0x61,0x51,0x49,0x46},   // 2 0x32 50
            new List<byte> {0x00,0x21,0x41,0x45,0x4b,0x31},   // 3 0x33 51
            new List<byte> {0x00,0x18,0x14,0x12,0x7f,0x10},   // 4 0x34 52
            new List<byte> {0x00,0x27,0x45,0x45,0x45,0x39},   // 5 0x35 53
            new List<byte> {0x00,0x3c,0x4a,0x49,0x49,0x30},   // 6 0x36 54
            new List<byte> {0x00,0x01,0x71,0x09,0x05,0x03},   // 7 0x37 55
            new List<byte> {0x00,0x36,0x49,0x49,0x49,0x36},   // 8 0x38 56
            new List<byte> {0x00,0x06,0x49,0x49,0x29,0x1e},   // 9 0x39 57
            new List<byte> {0x00,0x00,0x36,0x36,0x00,0x00},   // : 0x3a 58
            new List<byte> {0x00,0x00,0x56,0x36,0x00,0x00},   // ; 0x3b 59
            new List<byte> {0x00,0x08,0x14,0x22,0x41,0x00},   // < 0x3c 60
            new List<byte> {0x00,0x14,0x14,0x14,0x14,0x14},   // = 0x3d 61
            new List<byte> {0x00,0x00,0x41,0x22,0x14,0x08},   // > 0x3e 62
            new List<byte> {0x00,0x02,0x01,0x51,0x09,0x06},   // ? 0x3f 63
            new List<byte> {0x00,0x3e,0x41,0x5d,0x49,0x4e},   // @ 0x40 64
            new List<byte> {0x00,0x7e,0x09,0x09,0x09,0x7e},   // A 0x41 65
            new List<byte> {0x00,0x7f,0x49,0x49,0x49,0x36},   // B 0x42 66
            new List<byte> {0x00,0x3e,0x41,0x41,0x41,0x22},   // C 0x43 67
            new List<byte> {0x00,0x7f,0x41,0x41,0x41,0x3e},   // D 0x44 68
            new List<byte> {0x00,0x7f,0x49,0x49,0x49,0x41},   // E 0x45 69
            new List<byte> {0x00,0x7f,0x09,0x09,0x09,0x01},   // F 0x46 70
            new List<byte> {0x00,0x3e,0x41,0x49,0x49,0x7a},   // G 0x47 71
            new List<byte> {0x00,0x7f,0x08,0x08,0x08,0x7f},   // H 0x48 72
            new List<byte> {0x00,0x00,0x41,0x7f,0x41,0x00},   // I 0x49 73
            new List<byte> {0x00,0x20,0x40,0x41,0x3f,0x01},   // J 0x4a 74
            new List<byte> {0x00,0x7f,0x08,0x14,0x22,0x41},   // K 0x4b 75
            new List<byte> {0x00,0x7f,0x40,0x40,0x40,0x40},   // L 0x4c 76
            new List<byte> {0x00,0x7f,0x02,0x0c,0x02,0x7f},   // M 0x4d 77
            new List<byte> {0x00,0x7f,0x04,0x08,0x10,0x7f},   // N 0x4e 78
            new List<byte> {0x00,0x3e,0x41,0x41,0x41,0x3e},   // O 0x4f 79
            new List<byte> {0x00,0x7f,0x09,0x09,0x09,0x06},   // P 0x50 80
            new List<byte> {0x00,0x3e,0x41,0x51,0x21,0x5e},   // Q 0x51 81
            new List<byte> {0x00,0x7f,0x09,0x19,0x29,0x46},   // R 0x52 82
            new List<byte> {0x00,0x46,0x49,0x49,0x49,0x31},   // S 0x53 83
            new List<byte> {0x00,0x01,0x01,0x7f,0x01,0x01},   // T 0x54 84
            new List<byte> {0x00,0x3f,0x40,0x40,0x40,0x3f},   // U 0x55 85
            new List<byte> {0x00,0x0f,0x30,0x40,0x30,0x0f},   // V 0x56 86
            new List<byte> {0x00,0x3f,0x40,0x30,0x40,0x3f},   // W 0x57 87
            new List<byte> {0x00,0x63,0x14,0x08,0x14,0x63},   // X 0x58 88
            new List<byte> {0x00,0x07,0x08,0x70,0x08,0x07},   // Y 0x59 89
            new List<byte> {0x00,0x61,0x51,0x49,0x45,0x43},   // Z 0x5a 90
            new List<byte> {0x00,0x3c,0x4a,0x49,0x29,0x1e},   // [ 0x5b 91
            new List<byte> {0x00,0x02,0x04,0x08,0x10,0x20},   // \ 0x5c 92
            new List<byte> {0x00,0x00,0x41,0x7f,0x00,0x00},   // ] 0x5d 93
            new List<byte> {0x00,0x04,0x02,0x01,0x02,0x04},   // ^ 0x5e 94
            new List<byte> {0x00,0x40,0x40,0x40,0x40,0x40},   // _ 0x5f 95
            new List<byte> {0x00,0x00,0x00,0x03,0x04,0x00},   // ` 0x60 96
            new List<byte> {0x00,0x20,0x54,0x54,0x54,0x78},   // a 0x61 97
            new List<byte> {0x00,0x7f,0x48,0x44,0x44,0x38},   // b 0x62 98
            new List<byte> {0x00,0x38,0x44,0x44,0x44,0x20},   // c 0x63 99
            new List<byte> {0x00,0x38,0x44,0x44,0x48,0x7f},   // d 0x64 100
            new List<byte> {0x00,0x38,0x54,0x54,0x54,0x18},   // e 0x65 101
            new List<byte> {0x00,0x08,0x7e,0x09,0x01,0x02},   // f 0x66 102
            new List<byte> {0x00,0x0c,0x52,0x52,0x52,0x3e},   // g 0x67 103
            new List<byte> {0x00,0x7f,0x08,0x04,0x04,0x78},   // h 0x68 104
            new List<byte> {0x00,0x00,0x44,0x7d,0x40,0x00},   // i 0x69 105
            new List<byte> {0x00,0x20,0x40,0x44,0x3d,0x00},   // j 0x6a 106
            new List<byte> {0x00,0x00,0x7f,0x10,0x28,0x44},   // k 0x6b 107
            new List<byte> {0x00,0x00,0x41,0x7f,0x40,0x00},   // l 0x6c 108
            new List<byte> {0x00,0x7c,0x04,0x18,0x04,0x78},   // m 0x6d 109
            new List<byte> {0x00,0x7c,0x08,0x04,0x04,0x78},   // n 0x6e 110
            new List<byte> {0x00,0x38,0x44,0x44,0x44,0x38},   // o 0x6f 111
            new List<byte> {0x00,0x7c,0x14,0x14,0x14,0x08},   // p 0x70 112
            new List<byte> {0x00,0x08,0x14,0x14,0x18,0x7c},   // q 0x71 113
            new List<byte> {0x00,0x7c,0x08,0x04,0x04,0x08},   // r 0x72 114
            new List<byte> {0x00,0x48,0x54,0x54,0x54,0x20},   // s 0x73 115
            new List<byte> {0x00,0x04,0x3f,0x44,0x40,0x20},   // t 0x74 116
            new List<byte> {0x00,0x3c,0x40,0x40,0x20,0x7c},   // u 0x75 117
            new List<byte> {0x00,0x1c,0x20,0x40,0x20,0x1c},   // v 0x76 118
            new List<byte> {0x00,0x3c,0x40,0x30,0x40,0x3c},   // w 0x77 119
            new List<byte> {0x00,0x44,0x28,0x10,0x28,0x44},   // x 0x78 120
            new List<byte> {0x00,0x0c,0x50,0x50,0x50,0x3c},   // y 0x79 121
            new List<byte> {0x00,0x44,0x64,0x54,0x4c,0x44},   // z 0x7a 122
            new List<byte> {0x00,0x00,0x08,0x36,0x41,0x41},   // { 0x7b 123
            new List<byte> {0x00,0x00,0x00,0x7f,0x00,0x00},   // | 0x7c 124
            new List<byte> {0x00,0x41,0x41,0x36,0x08,0x00},   // } 0x7d 125
            new List<byte> {0x00,0x04,0x02,0x04,0x08,0x04},   // ~ 0x7e 126
        };


        //public List<List<byte>> font8x16_terminal = new List<List<uint8_t>>()
        //{
        //    new List<byte> {0x00,0x00,0x00,0x00,0x7C,0x00,0xFE,0x1B,0xFE,0x1B,0x7C,0x00,0x00,0x00,0x00,0x00},/*"!",0*/
        //    new List<byte> {0x00,0x00,0x0E,0x00,0x1E,0x00,0x00,0x00,0x00,0x00,0x1E,0x00,0x0E,0x00,0x00,0x00},/*""",1*/
        //    new List<byte> {0x20,0x01,0xFC,0x0F,0xFC,0x0F,0x20,0x01,0x20,0x01,0xFC,0x0F,0xFC,0x0F,0x20,0x01},/*"#",2*/
        //    new List<byte> {0x38,0x06,0x7C,0x0C,0x44,0x08,0xFF,0x3F,0xFF,0x3F,0x84,0x08,0x8C,0x0F,0x18,0x07},/*"$",3*/
        //    new List<byte> {0x1C,0x18,0x14,0x1E,0x9C,0x07,0xE0,0x01,0x78,0x1C,0x1E,0x14,0x06,0x1C,0x00,0x00},/*"%",4*/
        //    new List<byte> {0xBC,0x1F,0xFE,0x10,0x42,0x10,0xC2,0x10,0xFE,0x1F,0x3C,0x0F,0x80,0x19,0x80,0x10},/*"&",5*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x10,0x00,0x1E,0x00,0x0E,0x00,0x00,0x00,0x00,0x00,0x00,0x00},/*"'",6*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0xF0,0x07,0xFC,0x1F,0x0E,0x38,0x02,0x20,0x00,0x00,0x00,0x00},/*"(",7*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x02,0x20,0x0E,0x38,0xFC,0x1F,0xF0,0x07,0x00,0x00,0x00,0x00},/*")",8*/
        //    new List<byte> {0x80,0x00,0xA0,0x02,0xE0,0x03,0xC0,0x01,0xC0,0x01,0xE0,0x03,0xA0,0x02,0x80,0x00},/*"*",9*/
        //    new List<byte> {0x80,0x00,0x80,0x00,0x80,0x00,0xE0,0x03,0xE0,0x03,0x80,0x00,0x80,0x00,0x80,0x00},/*"+",10*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x40,0x00,0x78,0x00,0x38,0x00,0x00,0x00,0x00,0x00,0x00},/*",",11*/
        //    new List<byte> {0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00},/*"-",12*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x18,0x00,0x18,0x00,0x00,0x00,0x00,0x00,0x00},/*".",13*/
        //    new List<byte> {0x00,0x18,0x00,0x1E,0x80,0x07,0xE0,0x01,0x78,0x00,0x1E,0x00,0x06,0x00,0x00,0x00},/*"/",14*/
        //    new List<byte> {0xF8,0x07,0xFC,0x0F,0x06,0x18,0xC2,0x10,0xC2,0x10,0x06,0x18,0xFC,0x0F,0xF8,0x07},/*"0",15*/
        //    new List<byte> {0x00,0x00,0x08,0x10,0x0C,0x10,0xFE,0x1F,0xFE,0x1F,0x00,0x10,0x00,0x10,0x00,0x00},/*"1",16*/
        //    new List<byte> {0x04,0x1C,0x06,0x1E,0x02,0x13,0x82,0x11,0xC2,0x10,0x62,0x10,0x3E,0x18,0x1C,0x18},/*"2",17*/
        //    new List<byte> {0x04,0x08,0x06,0x18,0x02,0x10,0x42,0x10,0x42,0x10,0x42,0x10,0xFE,0x1F,0xBC,0x0F},/*"3",18*/
        //    new List<byte> {0xC0,0x01,0xE0,0x01,0x30,0x01,0x18,0x01,0x0C,0x11,0xFE,0x1F,0xFE,0x1F,0x00,0x11},/*"4",19*/
        //    new List<byte> {0x7E,0x08,0x7E,0x18,0x42,0x10,0x42,0x10,0x42,0x10,0x42,0x10,0xC2,0x1F,0x82,0x0F},/*"5",20*/
        //    new List<byte> {0xF8,0x0F,0xFC,0x1F,0x46,0x10,0x42,0x10,0x42,0x10,0x42,0x10,0xC0,0x1F,0x80,0x0F},/*"6",21*/
        //    new List<byte> {0x06,0x00,0x06,0x00,0x02,0x00,0x02,0x1F,0xC2,0x1F,0xF2,0x00,0x3E,0x00,0x0E,0x00},/*"7",22*/
        //    new List<byte> {0xBC,0x0F,0xFE,0x1F,0x42,0x10,0x42,0x10,0x42,0x10,0x42,0x10,0xFE,0x1F,0xBC,0x0F},/*"8",23*/
        //    new List<byte> {0x3C,0x00,0x7E,0x10,0x42,0x10,0x42,0x10,0x42,0x10,0x42,0x18,0xFE,0x0F,0xFC,0x07},/*"9",24*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x00,0x30,0x0C,0x30,0x0C,0x00,0x00,0x00,0x00,0x00,0x00},/*":",26*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x20,0x60,0x3C,0x60,0x1C,0x00,0x00,0x00,0x00,0x00,0x00},/*";",27*/
        //    new List<byte> {0x80,0x00,0xC0,0x01,0x60,0x03,0x30,0x06,0x18,0x0C,0x0C,0x18,0x04,0x10,0x00,0x00},/*"<",28*/
        //    new List<byte> {0x40,0x02,0x40,0x02,0x40,0x02,0x40,0x02,0x40,0x02,0x40,0x02,0x40,0x02,0x40,0x02},/*"=",29*/
        //    new List<byte> {0x04,0x10,0x0C,0x18,0x18,0x0C,0x30,0x06,0x60,0x03,0xC0,0x01,0x80,0x00,0x00,0x00},/*">",30*/
        //    new List<byte> {0x04,0x00,0x06,0x00,0x02,0x00,0x82,0x1B,0xC2,0x1B,0x62,0x00,0x3E,0x00,0x1C,0x00},/*"?",31*/
        //    new List<byte> {0xFC,0x0F,0xFE,0x1F,0x02,0x10,0x82,0x11,0xC2,0x13,0xE2,0x13,0xFE,0x13,0xFC,0x03},/*"@",32*/
        //    new List<byte> {0xF0,0x1F,0xF8,0x1F,0x0C,0x01,0x06,0x01,0x06,0x01,0x0C,0x01,0xF8,0x1F,0xF0,0x1F},/*"A",33*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x42,0x10,0x42,0x10,0x42,0x10,0xFE,0x1F,0xBC,0x0F},/*"B",34*/
        //    new List<byte> {0xF8,0x07,0xFC,0x0F,0x06,0x18,0x02,0x10,0x02,0x10,0x02,0x10,0x06,0x18,0x0C,0x0C},/*"C",35*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x02,0x10,0x02,0x10,0x06,0x18,0xFC,0x0F,0xF8,0x07},/*"D",36*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x42,0x10,0x42,0x10,0xE2,0x10,0x06,0x18,0x06,0x18},/*"E",37*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x42,0x10,0x42,0x00,0xE2,0x00,0x06,0x00,0x06,0x00},/*"F",38*/
        //    new List<byte> {0xF8,0x07,0xFC,0x0F,0x06,0x18,0x02,0x10,0x82,0x10,0x82,0x10,0x86,0x0F,0x8C,0x1F},/*"G",39*/
        //    new List<byte> {0xFE,0x1F,0xFE,0x1F,0x40,0x00,0x40,0x00,0x40,0x00,0x40,0x00,0xFE,0x1F,0xFE,0x1F},/*"H",40*/
        //    new List<byte> {0x00,0x00,0x02,0x10,0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x02,0x10,0x02,0x10,0x00,0x00},/*"I",41*/
        //    new List<byte> {0x00,0x0C,0x00,0x1C,0x00,0x10,0x00,0x10,0x02,0x10,0xFE,0x1F,0xFE,0x0F,0x02,0x00},/*"J",42*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0xE0,0x00,0xB0,0x01,0x18,0x03,0x0E,0x1E,0x06,0x1C},/*"K",43*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x02,0x10,0x00,0x10,0x00,0x10,0x00,0x18,0x00,0x18},/*"L",44*/
        //    new List<byte> {0xFE,0x1F,0xFE,0x1F,0x18,0x00,0xF0,0x00,0xF0,0x00,0x18,0x00,0xFE,0x1F,0xFE,0x1F},/*"M",45*/
        //    new List<byte> {0xFE,0x1F,0xFE,0x1F,0x38,0x00,0x70,0x00,0xE0,0x00,0xC0,0x01,0xFE,0x1F,0xFE,0x1F},/*"N",46*/
        //    new List<byte> {0xFC,0x0F,0xFE,0x1F,0x02,0x10,0x02,0x10,0x02,0x10,0x02,0x10,0xFE,0x1F,0xFC,0x0F},/*"O",47*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x42,0x10,0x42,0x00,0x42,0x00,0x7E,0x00,0x3C,0x00},/*"P",48*/
        //    new List<byte> {0xFC,0x0F,0xFE,0x1F,0x02,0x10,0x02,0x1C,0x02,0x38,0x02,0x70,0xFE,0x5F,0xFC,0x0F},/*"Q",49*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x42,0x00,0x42,0x00,0xC2,0x00,0xFE,0x1F,0x3C,0x1F},/*"R",50*/
        //    new List<byte> {0x1C,0x0C,0x3E,0x1C,0x62,0x10,0x42,0x10,0x42,0x10,0xC2,0x10,0x8E,0x1F,0x0C,0x0F},/*"S",51*/
        //    new List<byte> {0x06,0x00,0x06,0x00,0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x02,0x10,0x06,0x00,0x06,0x00},/*"T",52*/
        //    new List<byte> {0xFE,0x0F,0xFE,0x1F,0x00,0x10,0x00,0x10,0x00,0x10,0x00,0x10,0xFE,0x1F,0xFE,0x0F},/*"U",53*/
        //    new List<byte> {0xFE,0x03,0xFE,0x07,0x00,0x0C,0x00,0x18,0x00,0x18,0x00,0x0C,0xFE,0x07,0xFE,0x03},/*"V",54*/
        //    new List<byte> {0xFE,0x07,0xFE,0x1F,0x00,0x1C,0xC0,0x07,0xC0,0x07,0x00,0x1C,0xFE,0x1F,0xFE,0x07},/*"W",55*/
        //    new List<byte> {0x0E,0x1C,0x1E,0x1E,0x30,0x03,0xE0,0x01,0xE0,0x01,0x30,0x03,0x1E,0x1E,0x0E,0x1C},/*"X",56*/
        //    new List<byte> {0x1E,0x00,0x3E,0x00,0x60,0x10,0xC0,0x1F,0xC0,0x1F,0x60,0x10,0x3E,0x00,0x1E,0x00},/*"Y",57*/
        //    new List<byte> {0x06,0x1E,0x06,0x1F,0x82,0x11,0xC2,0x10,0x62,0x10,0x32,0x10,0x1E,0x18,0x0E,0x18},/*"Z",58*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0xFE,0x1F,0xFE,0x1F,0x02,0x10,0x02,0x10,0x00,0x00,0x00,0x00},/*"[",59*/
        //    new List<byte> {0x00,0x18,0x00,0x1E,0x80,0x07,0xE0,0x01,0x78,0x00,0x1E,0x00,0x06,0x00,0x00,0x00},/*"/",60*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x02,0x10,0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x00,0x00,0x00,0x00},/*"]",61*/
        //    new List<byte> {0x20,0x00,0x30,0x00,0x18,0x00,0x0C,0x00,0x18,0x00,0x30,0x00,0x20,0x00,0x00,0x00},/*"^",62*/
        //    new List<byte> {0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80,0x00,0x80},/*"_",63*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x00,0x38,0x00,0x78,0x00,0x40,0x00,0x00,0x00,0x00,0x00},/*"`",64*/
        //    new List<byte> {0x00,0x0E,0x20,0x1F,0x20,0x11,0x20,0x11,0x20,0x11,0xE0,0x0F,0xC0,0x1F,0x00,0x10},/*"a",65*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x0F,0x20,0x10,0x20,0x10,0x60,0x10,0xC0,0x1F,0x80,0x0F},/*"b",66*/
        //    new List<byte> {0xC0,0x0F,0xE0,0x1F,0x20,0x10,0x20,0x10,0x20,0x10,0x20,0x10,0x60,0x18,0x40,0x08},/*"c",67*/
        //    new List<byte> {0x80,0x0F,0xC0,0x1F,0x60,0x10,0x20,0x10,0x22,0x10,0xFE,0x0F,0xFE,0x1F,0x00,0x10},/*"d",68*/
        //    new List<byte> {0xC0,0x0F,0xE0,0x1F,0x20,0x11,0x20,0x11,0x20,0x11,0x20,0x11,0xE0,0x19,0xC0,0x09},/*"e",69*/
        //    new List<byte> {0x00,0x00,0x20,0x10,0xFC,0x1F,0xFE,0x1F,0x22,0x10,0x22,0x00,0x06,0x00,0x04,0x00},/*"f",70*/
        //    new List<byte> {0xC0,0x4F,0xE0,0xDF,0x20,0x90,0x20,0x90,0x20,0x90,0xC0,0xFF,0xE0,0x7F,0x20,0x00},/*"g",71*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x40,0x00,0x20,0x00,0x20,0x00,0xE0,0x1F,0xC0,0x1F},/*"h",72*/
        //    new List<byte> {0x00,0x00,0x20,0x10,0x20,0x10,0xEC,0x1F,0xEC,0x1F,0x00,0x10,0x00,0x10,0x00,0x00},/*"i",73*/
        //    new List<byte> {0x00,0x60,0x00,0xC0,0x20,0x80,0x20,0x80,0xEC,0xFF,0xEC,0x7F,0x00,0x00,0x00,0x00},/*"j",74*/
        //    new List<byte> {0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x80,0x01,0x80,0x03,0xC0,0x06,0x60,0x1C,0x20,0x18},/*"k",75*/
        //    new List<byte> {0x00,0x00,0x02,0x10,0x02,0x10,0xFE,0x1F,0xFE,0x1F,0x00,0x10,0x00,0x10,0x00,0x00},/*"l",76*/
        //    new List<byte> {0xE0,0x1F,0xE0,0x1F,0x60,0x00,0xC0,0x0F,0xC0,0x0F,0x60,0x00,0xE0,0x1F,0xC0,0x1F},/*"m",77*/
        //    new List<byte> {0x20,0x00,0xE0,0x1F,0xC0,0x1F,0x20,0x00,0x20,0x00,0x20,0x00,0xE0,0x1F,0xC0,0x1F},/*"n",78*/
        //    new List<byte> {0xC0,0x0F,0xE0,0x1F,0x20,0x10,0x20,0x10,0x20,0x10,0x20,0x10,0xE0,0x1F,0xC0,0x0F},/*"o",79*/
        //    new List<byte> {0x20,0x80,0xE0,0xFF,0xC0,0xFF,0x20,0x90,0x20,0x10,0x20,0x10,0xE0,0x1F,0xC0,0x0F},/*"p",80*/
        //    new List<byte> {0xC0,0x0F,0xE0,0x1F,0x20,0x10,0x20,0x10,0x20,0x90,0xC0,0xFF,0xE0,0xFF,0x20,0x80},/*"q",81*/
        //    new List<byte> {0x20,0x10,0xE0,0x1F,0xC0,0x1F,0x60,0x10,0x20,0x00,0x20,0x00,0x60,0x00,0x40,0x00},/*"r",82*/
        //    new List<byte> {0xC0,0x08,0xE0,0x19,0x20,0x11,0x20,0x11,0x20,0x13,0x20,0x12,0x60,0x1E,0x40,0x0C},/*"s",83*/
        //    new List<byte> {0x20,0x00,0x20,0x00,0xFC,0x0F,0xFE,0x1F,0x20,0x10,0x20,0x18,0x00,0x08,0x00,0x00},/*"t",84*/
        //    new List<byte> {0xE0,0x0F,0xE0,0x1F,0x00,0x10,0x00,0x10,0x00,0x10,0xE0,0x0F,0xE0,0x1F,0x00,0x10},/*"u",85*/
        //    new List<byte> {0xE0,0x03,0xE0,0x07,0x00,0x0C,0x00,0x18,0x00,0x18,0x00,0x0C,0xE0,0x07,0xE0,0x03},/*"v",86*/
        //    new List<byte> {0xE0,0x0F,0xE0,0x1F,0x00,0x18,0x00,0x0F,0x00,0x0F,0x00,0x18,0xE0,0x1F,0xE0,0x0F},/*"w",87*/
        //    new List<byte> {0x20,0x10,0x60,0x18,0xC0,0x0C,0x80,0x07,0x80,0x07,0xC0,0x0C,0x60,0x18,0x20,0x10},/*"x",88*/
        //    new List<byte> {0xE0,0x8F,0xE0,0x9F,0x00,0x90,0x00,0x90,0x00,0x90,0x00,0xD0,0xE0,0x7F,0xE0,0x3F},/*"y",89*/
        //    new List<byte> {0x60,0x18,0x60,0x1C,0x20,0x16,0x20,0x13,0xA0,0x11,0xE0,0x10,0x60,0x18,0x20,0x18},/*"z",90*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x80,0x00,0xFC,0x1F,0x7E,0x3F,0x02,0x20,0x02,0x20,0x00,0x00},/*"{",91*/
        //    new List<byte> {0x00,0x00,0x00,0x00,0x00,0x00,0x7C,0x3E,0x7C,0x3E,0x00,0x00,0x00,0x00,0x00,0x00},/*"|",92*/
        //    new List<byte> {0x00,0x00,0x02,0x20,0x02,0x20,0x7E,0x3F,0xFC,0x1F,0x80,0x00,0x00,0x00,0x00,0x00},/*"}",93*/
        //};
    }
}


