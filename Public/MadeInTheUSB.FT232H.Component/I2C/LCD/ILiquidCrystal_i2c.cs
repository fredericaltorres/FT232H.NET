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
    /// </summary>
    public class LiquidCrystal_I2C_PCF8574 : LiquidCrystalBase
    {
        protected const byte En = 4; // B00000100  // Enable bit
        protected const byte Rw = 2; // B00000010  // Read/Write bit
        protected const byte Rs = 1; // B00000001  // Register select bit

        /// <summary>
        /// The PCF8574 i2c serial extender only support speed up to
        /// 76800*2 = 153600 ~~ 16Kbyte/S which is about 150 000 Khz
        /// Nusbio default is 900 000,
        /// 76800*4 = 307200 Mhz does not work all the time.
        /// The I2C_PCF8574 is a very cheap chip after all.
        /// </summary>
        public const int MAX_BAUD_RATE = 76800 * 2; // 153600 Mhz

        int       _Addr;
        int       _displayfunction;
        int       _displaycontrol;
        int       _displaymode;
        int       _backlightval;


        public int DeviceID;
        public I2CDevice _i2cDevice;

        private static List<int> I2C_IDS = new List<int>() { 0x27, 0x3F };

        public static LiquidCrystal_I2C_PCF8574 Detect(I2CDevice i2cDevice, int maxColumn, int maxRow, 
            List<int> i2cIDs = null,
            bool displayMessage = true)
        {
            if (i2cIDs == null)
                i2cIDs = I2C_IDS;

            foreach (var lcdI2cId in i2cIDs)
            {
                var lcdI2C = new LiquidCrystal_I2C_PCF8574(i2cDevice, maxColumn, maxRow, deviceId: lcdI2cId);
                if (lcdI2C.Begin(maxColumn, maxRow))
                {
                    if(displayMessage)
                        Console.WriteLine("I2C LCD ID:0x{0:X} found", lcdI2cId);
                    System.Threading.Thread.Sleep(1000);
                    return lcdI2C;
                }
                else
                {
                    if (displayMessage)
                        Console.WriteLine("I2C LCD ID:0x{0:X} not found", lcdI2cId);
                }
            }
            return null;
        }

        public LiquidCrystal_I2C_PCF8574(I2CDevice i2cDevice, int cols, int rows, int deviceId = 0x27, bool debug = false, int backlight = LCD_NOBACKLIGHT) 
        {
            NumCols       = (uint8_t)cols;
            NumLines      = (uint8_t)rows;
            this._backlightval = backlight;
            this.DeviceID      = deviceId;
            this._i2cDevice = i2cDevice;
        }

        public override string Print(string format, params object[] args)
        {
            var text = String.Format(format, args);
            foreach (var c in text)
            {
                this.Write((uint8_t)c);
            }
            return text;
        }
        
        public string Print(int x, int y, string format, params object[] args)
        {
            var text = string.Format(format, args);
            if (x == -1)
                x = (this.NumCols - text.Length)/2;
            this.SetCursor(x, y);
            return this.Print(format, args);
        }

        void init_priv()
        {
            this._displayfunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;
            Begin(this.NumCols, this.NumLines);
        }

        public override bool Begin(uint8_t cols, uint8_t lines, int16_t dotsize = -1)
        {
            var r = true;

            if (dotsize == -1)
                dotsize = LCD_5x8DOTS;

            if (lines > 1)
            {
                _displayfunction |= LCD_2LINE;
            }
            this.NumLines = lines;
            this.NumCols  = cols;

            // for some 1 line displays you can select a 10 pixel high font
            if ((dotsize != 0) && (lines == 1))
            {
                _displayfunction |= LCD_5x10DOTS;
            }
            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands. Arduino can turn on way befer 4.5V so we'll wait 50
            DelayMicroseconds(50000);

            // Now we pull both RS and R / W low to Begin commands
            if (ExpanderWrite(_backlightval)) // reset expanderand turn Backlight off (Bit 8 =1)
            {
                System.Diagnostics.Debug.WriteLine("I2C LCD ID :{0} appear to be the right one", this.DeviceID);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("I2C LCD ID :{0} appear to be the wrong one", this.DeviceID);
                r = false;
            }
            Delay(100);

            //put the LCD into 4 bit mode
            // this is according to the hitachi HD44780 datasheet
            // figure 24, pg 46

            // we start in 8bit mode, try to set 4 bit mode
            Write4bits(0x30); // On non i2c lcd the value is 0x03
            DelayMicroseconds(4500); // wait min 4.1ms

            // second try
            Write4bits(0x30);// On non i2c lcd the value is 0x03
            DelayMicroseconds(4500); // wait min 4.1ms

            // third go!
            Write4bits(0x30);// On non i2c lcd the value is 0x03
            DelayMicroseconds(150);

            // finally, set to 4-bit interface
            Write4bits(0x20);// On non i2c lcd the value is 0x02

            // set # lines, font size, etc.
            Command(LCD_FUNCTIONSET | _displayfunction);

            // turn the Display on with no Cursor or blinking default
            _displaycontrol = LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF;
            Display();

            // Clear it off
            Clear();

            // Initialize to default text direction (for roman languages)
            _displaymode = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;

            // set the entry mode
            Command(LCD_ENTRYMODESET | _displaymode);

            Home();

            return r;
        }

        /********** high level commands, for the user! */
        public void Clear()
        {
            Command(LCD_CLEARDISPLAY);// Clear Display, set Cursor position to zero
            DelayMicroseconds(2000);  // this Command takes a long time!
        }

        public void Home()
        {
            Command(LCD_RETURNHOME);  // set Cursor position to zero
            DelayMicroseconds(2000);  // this Command takes a long time!
        }

        public void SetCursor(int col, int row)
        {
            this.SetCursor((uint8_t)col, (uint8_t)row);
        }

        public void SetCursor(uint8_t col, uint8_t row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };
            if (row > NumLines)
            {
                row = (byte)(NumLines - 1);    // we count rows starting w/0
            }
            Command(LCD_SETDDRAMADDR | (col + row_offsets[row]));
        }

        // Turn the Display on/off (quickly)
        public void NoDisplay()
        {
            _displaycontrol &= ~LCD_DISPLAYON;
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        public void Display()
        {
            _displaycontrol |= LCD_DISPLAYON;
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turns the underline Cursor on/off
        public void NoCursor()
        {
            _displaycontrol &= ~LCD_CURSORON;
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        public void Cursor()
        {
            _displaycontrol |= LCD_CURSORON;
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turn on and off the blinking Cursor
        public void NoBlink()
        {
            _displaycontrol &= (~LCD_BLINKON);
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        public void Blink()
        {
            _displaycontrol |= LCD_BLINKON;
            Command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // These commands scroll the Display without changing the RAM
        public void ScrollDisplayLeft()
        {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
        }

        public void ScrollDisplayRight()
        {
            Command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
        }

        // This is for text that flows Left to Right
        public void LeftToRight()
        {
            _displaymode |= LCD_ENTRYLEFT;
            Command(LCD_ENTRYMODESET | _displaymode);
        }

        // This is for text that flows Right to Left
        public void RightToLeft()
        {
            _displaymode &= ~LCD_ENTRYLEFT;
            Command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'right justify' text from the Cursor
        public void Autoscroll()
        {
            _displaymode |= LCD_ENTRYSHIFTINCREMENT;
            Command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'left justify' text from the Cursor
        public void NoAutoscroll()
        {
            _displaymode &= ~LCD_ENTRYSHIFTINCREMENT;
            Command(LCD_ENTRYMODESET | _displaymode);
        }

        // TODO:Inherit
        public void CreateChar(uint8_t location, int [] charmap)
        {
            var l = new List<byte>();
            foreach(var v in charmap)
                l.Add((byte)v);
            this.CreateChar(location, l.ToArray());
        }

        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        public void CreateChar(uint8_t location, uint8_t[] charmap)
        {
            location &= 0x7; // we only have 8 locations 0-7
            Command(LCD_SETCGRAMADDR | (location << 3));
            for (int i = 0; i < 8; i++)
            {
                Write(charmap[i]);
            }
        }

        // Turn the (optional) Backlight off/on
        public void NoBacklight()
        {
            _backlightval = LCD_NOBACKLIGHT;
            ExpanderWrite(0);
        }

        public void Backlight()
        {
            _backlightval = LCD_BACKLIGHT;
            ExpanderWrite(0);
        }

        /*********** mid level commands, for sending data/cmds */

        private void Command(int value)
        {
            Command((uint8_t)value);
        }

        private void Command(uint8_t value)
        {
            Send((byte)value, (byte)0);
        }

        private size_t Write(int value)
        {
            return Write((uint8_t)value);
        }

        public size_t Write(uint8_t value)
        {
            Send(value, Rs);
            return 0;
        }

        /************ low level data pushing commands **********/

        void __trace(string m, int val)
        {
            #if TRACE_CONSOLE
            Console.WriteLine("{0}-{1}", m, val);
            #endif
        }
        
        void Send(int value, int mode)
        {
            Send((uint8_t)value, (uint8_t)mode);
        }

        // Write either Command or data
        void Send(uint8_t value, uint8_t mode)
        {
            uint8_t highnib = (uint8_t)(value & 0xF0);
	        uint8_t lownib  = (uint8_t)(value << 4);

            #if FAST_MODE
                Write4bits_FAST((byte)((highnib) | mode), (byte)((lownib) | mode));
            #else
                Write4bits((highnib) | mode);
                Write4bits((lownib) | mode);
            #endif
        }
        
        void Write4bits_FAST(uint8_t value1, uint8_t value2)
        {
            ExpanderWriteAndPusle(value1);
            ExpanderWriteAndPusle(value2);

            //ExpanderWriteAndPusle(value1, value2);
        }
        /* Not faster
        bool ExpanderWriteAndPusle(uint8_t _data1, uint8_t _data2)
        {
            var currentBaud = this._nusbio.GetBaudRate();
            var resetBaudRate = false;
            if (currentBaud != MAX_BAUD_RATE)
            {
                resetBaudRate = true;
                this._nusbio.SetBaudRate(MAX_BAUD_RATE);
            }

            var buffer = new List<uint8_t>();
            buffer.Add((uint8_t) (_data1 | _backlightval));
            buffer.Add((uint8_t) ((_data1 | En) | _backlightval)); // pusle on 
            buffer.Add((uint8_t) ((_data1 & ~En) | _backlightval)); // pusle off 

            buffer.Add((uint8_t) (_data2 | _backlightval));
            buffer.Add((uint8_t) ((_data2 | En) | _backlightval)); // pusle on 
            buffer.Add((uint8_t) ((_data2 & ~En) | _backlightval)); // pusle off 

            var r1 = this._i2c.WriteBuffer(buffer.ToArray(), false);

            if (resetBaudRate)
            {
                this._nusbio.SetBaudRate(currentBaud);
            }

            return true;
        }*/

        bool ExpanderWriteAndPusle(uint8_t _data)
        {
            //var currentBaud = this._nusbio.GetBaudRate();
            //var resetBaudRate = false;
            //if (currentBaud != MAX_BAUD_RATE)
            //{
            //    resetBaudRate = true;
            //    this._nusbio.SetBaudRate(MAX_BAUD_RATE);
            //}

            
            var buffer = new List<byte>()
            {
                (uint8_t)(_data | _backlightval),
                (uint8_t)((_data | En) | _backlightval), // pusle on 
                (uint8_t)((_data & ~En) | _backlightval) // pusle off 
            };
            //var r1 = Ii2cOutImpl.i2c_WriteBuffer(buffer.ToArray());
            this._i2cDevice.WriteBuffer(this.DeviceID, buffer.ToArray());

            //if (resetBaudRate)
            //{
            //    this._nusbio.SetBaudRate(currentBaud);
            //}
            return true;
        }

        void Write4bits(int value)
        {
            Write4bits((uint8_t)value);
        }

        void Write4bits(uint8_t value)
        {
            ExpanderWrite(value);
            PulseEnable(value);
        }

        bool ExpanderWrite(int _data)
        {
            return ExpanderWrite((uint8_t)_data);
        }

        bool ExpanderWrite(uint8_t _data)
        {
            var r = this._i2cDevice.Send1ByteCommand(this.DeviceID, (uint8_t)(_data | _backlightval));
            return r;
        }

        void PulseEnable(uint8_t _data)
        {
           // trace("pu1-", _data | En);
            ExpanderWrite(_data | En);	// En high

            //DelayMicroseconds(1);		// enable pulse must be >450ns

            //trace("pu2-", _data & ~En);
            ExpanderWrite(_data & ~En);	// En low

            //DelayMicroseconds(1);		// commands need > 37us to settle
        }

        // Alias functions

        void cursor_on()
        {
            Cursor();
        }

        void cursor_off()
        {
            NoCursor();
        }

        void blink_on()
        {
            Blink();
        }

        void blink_off()
        {
            NoBlink();
        }

        void load_custom_character(uint8_t char_num, uint8_t[] rows)
        {
            CreateChar(char_num, rows);
        }

        void setBacklight(uint8_t new_val)
        {
            if (new_val > 0)
            {
                Backlight();		// turn Backlight on
            }
            else
            {
                NoBacklight();		// turn Backlight off
            }
        }

        void printstr(char[] c)
        {
            //This function is not identical to the function used for "real" I2C displays
            //it's here so the user sketch doesn't have to be changed 
            //print(c);
        }

        // unsupported API functions
        void off()
        {
            throw new NotImplementedException();
        }
        void on()
        {
            throw new NotImplementedException();
        }
        void setDelay(int cmdDelay, int charDelay)
        {
            throw new NotImplementedException();
        }
        uint8_t status()
        {
            return 0;
        }
        uint8_t keypad()
        {
            return 0;
        }
        uint8_t init_bargraph(uint8_t graphtype)
        {
            return 0;
        }
        void draw_horizontal_graph(uint8_t row, uint8_t column, uint8_t len, uint8_t pixel_col_end)
        {
            throw new NotImplementedException();
        }
        void draw_vertical_graph(uint8_t row, uint8_t column, uint8_t len, uint8_t pixel_row_end)
        {
            throw new NotImplementedException();
        }
        void setContrast(uint8_t new_val)
        {
            throw new NotImplementedException();
        }

        public void Flash(int count, int waitTime = 230)
        {
            for (var i = 0; i < count; i++)
            {
                this.NoDisplay();
                TimePeriod.Sleep(waitTime);
                this.Display();
                TimePeriod.Sleep(waitTime);
            }
        }

        public void ProgressBar(int x, int y, int maxChar, int percent, string text = null)
        {
            this.SetCursor(x, y);
            if(text != null)
                this.Print(text);

            for (var i = 0; i < maxChar; i++)
            {
                if(i < (maxChar * percent / 100))
                    this.Write(0xFF);
                else
                    this.Write(32);
            }
        }

        //bool Ii2cOut.i2c_Send1ByteCommand(byte c)
        //{
        //    return this._i2c.Send1ByteCommand(c);
        //}

        //bool Ii2cOut.i2c_Send2ByteCommand(byte c0, byte c1)
        //{
        //    return this._i2c.Send2BytesCommand(c0, c1);
        //}

        //bool Ii2cOut.i2c_WriteBuffer(byte[] buffer)
        //{
        //    return this._i2c.WriteBuffer(buffer);
        //}

        //bool Ii2cOut.i2c_WriteReadBuffer(byte[] writeBuffer, byte[] readBuffer)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
 