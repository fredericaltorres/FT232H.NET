/*
   The class APA102LEDStrip was written with the help of the following:
   
       Adafruit_DotStar
       https://github.com/adafruit/Adafruit_DotStar/blob/master/Adafruit_DotStar.cpp
  
       apa102-arduino
       https://github.com/pololu/apa102-arduino
  
       The Wheel() function comes from the Adafruit code 
       https://github.com/adafruit/Adafruit_NeoPixel

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
  
   MIT license, all text above must be included in any redistribution* 
  
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
 
   About data transfert speed:
   ===========================
   The APA 102 RGB LED strip protocol work this way:
        Move at the begining of the strip -- StartFrame()
        For each LED we send 4 bytes
            B1 - A header + Intensity
            B2 - Blue value 0..255
            B3 - Green value 0..255
            B4 - Red value 0..255
        Send EndFrame()
    
    The old API102 Nusbio extension used Nusbio pin 4 for the clock and pin 5 for the data.
    APA 102 use a compatible SPI protocol with no CS and MISO, just CLOCK and MOSI.
 
   
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
#if COLOR_MINE
using ColorMine.ColorSpaces;
#endif
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
//using MadeInTheUSB.Component;
using System.Drawing;

namespace MadeInTheUSB.FT232H.Components.APA102
{
    /// <summary>
    /// Test a strip on 1 APA 102 RGB LED.
    /// Old Nusbio extension use nusbio pin 4 for the clock and pin 5 for the data.
    /// APA 102 use a compatible SPI protocol with no CS and MISO, just CLOCK and MOSI.
    /// WHEN CONTROLLING AN APA LED STRIP WITH NUSBIO YOU MUST KNOW THE AMP CONSUMPTION.
    /// USB DEVICE ARE LIMITED TO 500 MILLI AMP.
    /// 
    /// AN LED IN GENERAL CONSUMES FROM 20 TO 25 MILLI AMP. AN RGB LED CONSUMES 3 TIMES 
    /// MORE IF THE RED, GREEN AND BLUE ARE SET TO THE 255, 255, 255 WHICH IS WHITE
    /// AT THE MAXIMUN INTENSISTY WHICH IS 31.
    /// 
    /// YOU MUST KNOW WHAT IS THE MAXIMUN CONSUMPTION OF YOUR APA 102 RGB LEB STRIP WHEN THE 
    /// RGB IS SET TO WHITE, WHITE, WHITE AND THE BRIGTHNESS IS AT THE MAXIMUM.
    /// 
    ///    -------------------------------------------------------------------------------
    ///    --- NEVER GO OVER 300 MILLI AMP IF THE LED STRIP IS POWERED FROM THE NUSBIO ---
    ///    -------------------------------------------------------------------------------
    /// 
    /// </summary>
    public class APA102LEDStrip
    {
        public const int MAX_BRIGHTNESS       = 31;
        public const int MIN_BRIGHTNESS       = 1;
        public const byte FIRST_BYTE_SEQUENCE = 224; // B11100000

        private byte   _brightness;
        private bool   _shiftBrightNessInc = true;

        public int MaxLed;
        public List<Color> LedColors;

        ISPI _spi;
        private readonly SpiChipSelectPins _cs;

        public APA102LEDStrip SetColor(byte brightness, Color color)
        {
            this.Reset();
            for (var l = 0; l < this.MaxLed; l++)
                this.AddRGB(color, brightness);
            return this;
        }

        public void ShiftBrightness(byte value)
        {
            if (_shiftBrightNessInc)
            {
                this._brightness += value;
                if (this._brightness > MAX_BRIGHTNESS)
                {
                    this._brightness = MAX_BRIGHTNESS;
                    _shiftBrightNessInc = false;
                }
            }
            else
            {
                this._brightness -= value;
                if (this._brightness < 1)
                {
                    this._brightness = MIN_BRIGHTNESS;
                    _shiftBrightNessInc = true;
                }
            }
        }

        public byte Brightness
        {
            get { return this._brightness; }
            set
            {
                this._brightness = value;
                if (this._brightness < 0) this._brightness = 1;
                if (this._brightness > MAX_BRIGHTNESS) this._brightness = MAX_BRIGHTNESS;
            }
        }

        public APA102LEDStrip(int ledCount, ISPI spi, SpiChipSelectPins cs)
        {
            this._spi = spi;
            this._cs = cs;
            this._brightness = MAX_BRIGHTNESS / 3;
            this.MaxLed      = ledCount;
            this.Init();
            this.AllOff();
        }

        public APA102LEDStrip Reset()
        {
            this.LedColors = new List<Color>();
            return this;
        }

        public bool IsFull
        {
            get { return this.LedColors.Count >= this.MaxLed; }
        }

        public APA102LEDStrip ShowAndShiftRightAllSequence(int wait)
        {
            for (var i = 0; i < this.MaxLed; i++)
            {
                this.Show().ShiftRightSequence().Wait(wait);
            }
            return this;
        }

        public APA102LEDStrip ShiftRightSequence()
        {
            var cl = this.LedColors.Last();
            this.LedColors.RemoveAt(this.LedColors.Count - 1);
            this.LedColors.Insert(0, cl);
            return this;
        }

        public APA102LEDStrip InsertRGBSequence(int index, int brightness, params Color[] colors)
        {
            foreach (var c in colors)
            {
                if (this.IsFull)
                    break;
                else
                    this.InsertRGB(c, index, brightness);
            }
            return this;
        }

        public APA102LEDStrip AddRGBSequence(bool reset, int brightness, int times, Color color)
        {
            if (reset)
                this.Reset();

            for (var i = 0; i < times; i++)
            {
                AddRGBSequence(false, brightness, color);
            }
            return this;
        }

        public APA102LEDStrip AddRGBSequence(bool reset, int brightness, params Color[] colors)
        {
            if (reset)
                this.Reset();

            foreach (var c in colors)
            {
                if (this.IsFull)
                    break;
                else
                    this.AddRGB(c, brightness);
            }
            return this;
        }

        public APA102LEDStrip AddRGBSequence(bool reset, params Color[] colors)
        {
            return this.AddRGBSequence(reset, this.Brightness, colors);
        }

        public APA102LEDStrip ReverseSequence()
        {
            this.LedColors.Reverse();
            return this;
        }

        public APA102LEDStrip AddRGB(Color c, int brightness = -1)
        {
            Color c2 = c;

            if (brightness != -1)
            {
                c2 = Color.FromArgb(brightness, c.R, c.G, c.B);
            }
            this.LedColors.Add(c2);
            return this;
        }

        public APA102LEDStrip InsertRGB(Color c, int index, int brightness = -1)
        {
            Color c2 = c;

            if (brightness != -1)
            {
                c2 = Color.FromArgb(brightness, c.R, c.G, c.B);
            }
            this.LedColors.Insert(index, c2);
            return this;
        }

        public APA102LEDStrip AddRGB(int r, int g, int b)
        {
            this.AddRGB(Color.FromArgb(r, g, b));
            return this;
        }

        public APA102LEDStrip AllOff()
        {
            return AllToOneColor(0, 0, 0);
        }

        public APA102LEDStrip AllToOneColor(int r, int g, int b, int brightness = -1)
        {
            this.Reset();
            for (var i = 0; i < this.MaxLed; i++)
                this.AddRGB(Color.FromArgb(r, g, b), brightness);
            this.Show();
            return this;
        }

        public APA102LEDStrip AllToOneColor(Color c, int brightness = -1)
        {
            this.Reset();
            for (var i = 0; i < this.MaxLed; i++)
                this.AddRGB(c, brightness);
            return this;
        }

        public APA102LEDStrip SetColors(List<Color> colors)
        {
            this.LedColors = colors;
            this.Show();
            return this;
        }

        public APA102LEDStrip Wait(int duration = 10)
        {
            if (duration > 0)
                System.Threading.Thread.Sleep(duration);
            return this;
        }

        public APA102LEDStrip Show(int brightness = 32)
        {
            if (this.LedColors.Count != this.MaxLed)
                throw new ArgumentException(string.Format("The APA102 Led strip has {0} leds, {1} led color definitions provided", this.MaxLed, this.LedColors.Count));

            // Send color in buffer mode.
            // To set an led we must send 4 bytes:
            // - (1) Header + Brightness
            // - (2) R value 0..255
            // - (3) G value 0..255
            // - (4) B value 0..255
            // Because of the way the APA 102 work if we send the 4 bytes for LED x, it does not mean
            // it will turn on until we send some other byte for the next LED or send the end of frame
            var intBuffer = new List<byte>() {0, 0, 0, 0}; // Start frame
            foreach (Color c in this.LedColors)
            {
                byte brightN = c.A; // By default we use the Alpha value for the brightness
                if (brightN < 0 || brightN > MAX_BRIGHTNESS)
                    brightN = this._brightness; // use global brightness setting if not defined in the alpha
                intBuffer.AddRange(new List<byte>()
                {
                    (byte)(APA102LEDStrip.FIRST_BYTE_SEQUENCE | brightN), // First byte + Brightness
                    c.B, c.G, c.R
                });
            }
            intBuffer.AddRange(new List<byte>() { 0xFF, 0xFF, 0xFF, 0xFF}); // End frame
            this.Transfer(intBuffer.ToArray());
            return this;
        }

        public void Init()
        {
        }

        public void StartFrame()
        {
            this.Init();
            this.Transfer(0, 0, 0, 0);
        }

        public void EndFrame()
        {
            this.Transfer(0xFF, 0xFF, 0xFF, 0xFF);
        }

        private void Transfer(params byte[] buffer)
        {
            this._spi.Write(buffer, this._cs);
        }

        public static Color Wheel(int wheelPos)
        {
            byte b = (byte) wheelPos;
            return WheelByte((byte)(b & 255));
        }
       
        // Based on ADAFRUIT strandtes.ino for NeoPixel
        // Input a value 0 to 255 to get a color value.
        // The colours are a transition r - g - b - back to r.
        private static Color WheelByte(byte wheelPos)
        {
            if (wheelPos < 85)
            {
                return Color.FromArgb(0, wheelPos*3, 255 - wheelPos*3, 0);
            }
            else if (wheelPos < 170)
            {
                wheelPos -= 85;
                return Color.FromArgb(0, 255 - wheelPos*3, 0, wheelPos*3);
            }
            else
            {
                wheelPos -= 170;
                return Color.FromArgb(0, 0, wheelPos*3, 255 - wheelPos*3);
            }
        }

        public static string ToHexValue(Color color)
        {
            return "#" + color.R.ToString("X2") +
                   color.G.ToString("X2") +
                   color.B.ToString("X2");
        }

        public static Color ToBrighterRgb(Color color, int percent = 10)
        {
            double r = color.R*(1 + (percent/100.0));
            double g = color.G*(1 + (percent/100.0));
            double b = color.B*(1 + (percent/100.0));
            
            Color c = Color.FromArgb((byte) r, (byte) g, (byte) b);

            return c;
        }

        public static string ToDecValue(Color color)
        {
            return color.R.ToString("000") + "-" +
                   color.G.ToString("000") + "-" +
                   color.B.ToString("000");
        }

        public static Dictionary<string, Color> DrawingColors
        {
            get
            {
                var d = new Dictionary<string, Color>();
                foreach (var c in _drawingColorList)
                    d[c.Name] = c;
                return d;
            }
        }

        private static readonly List<Color> _drawingColorList = new List<Color>()
        {
            Color.AliceBlue,
            Color.Azure,
            Color.Beige,
            Color.Bisque,
            Color.Blue,
            Color.BlueViolet,
            Color.Brown,
            Color.BurlyWood,
            Color.CadetBlue,
            Color.Chartreuse,
            Color.Chocolate,
            Color.Coral,
            Color.CornflowerBlue,
            Color.Cornsilk,
            Color.Crimson,
            Color.Cyan,
            Color.DarkOrange,
            Color.DarkOrchid,
            Color.DarkRed,

            Color.DarkTurquoise,
            Color.DarkViolet,

            Color.DarkBlue,
            Color.DarkCyan,
            Color.DarkGoldenrod,
            Color.DarkGreen,
            Color.DarkMagenta,

            Color.DeepPink,
            Color.DeepSkyBlue,
            Color.DimGray,
            Color.DodgerBlue,
            Color.Firebrick,
            Color.FloralWhite,
            Color.ForestGreen,
            Color.Fuchsia,
            Color.Gainsboro,
            Color.GhostWhite,
            Color.Gold,
            Color.Goldenrod,
            Color.Gray,
            Color.Green,
            Color.GreenYellow,
            Color.Honeydew,
            Color.HotPink,
            Color.IndianRed,
            Color.Indigo,
            Color.Ivory,
            Color.Khaki,
            Color.Lavender,
            Color.LavenderBlush,
            Color.LawnGreen,
            Color.LemonChiffon,
            Color.LightBlue,
            Color.LightCoral,
            Color.LightCyan,
            Color.LightGoldenrodYellow,
            Color.LightGray,
            Color.LightGreen,
            Color.LightPink,
            Color.LightSalmon,
            Color.LightSeaGreen,
            Color.LightSkyBlue,
            Color.LightSlateGray,
            Color.LightSteelBlue,
            Color.LightYellow,
            Color.Lime,
            Color.LimeGreen,
            Color.Linen,
            Color.Magenta,
            Color.Maroon,
            Color.MediumAquamarine,
            Color.MediumBlue,
            Color.MediumOrchid,
            Color.MediumPurple,
            Color.MediumSeaGreen,
            Color.MediumSlateBlue,
            Color.MediumSpringGreen,
            Color.MediumTurquoise,
            Color.MediumVioletRed,
            Color.MidnightBlue,
            Color.MintCream,
            Color.MistyRose,
            Color.Moccasin,
            Color.NavajoWhite,
            Color.Navy,
            Color.OldLace,
            Color.Olive,
            Color.OliveDrab,
            Color.Orange,
            Color.OrangeRed,
            Color.Orchid,
            Color.PaleGoldenrod,
            Color.PaleGreen,
            Color.PaleTurquoise,
            Color.PaleVioletRed,
            Color.PapayaWhip,
            Color.PeachPuff,
            Color.Peru,
            Color.Pink,
            Color.Plum,
            Color.PowderBlue,
            Color.Purple,
            Color.Red,
            Color.RosyBrown,
            Color.RoyalBlue,
            Color.SaddleBrown,
            Color.Salmon,
            Color.SandyBrown,
            Color.SeaGreen,
            Color.SeaShell,
            Color.Sienna,
            Color.Silver,
            Color.SkyBlue,
            Color.SlateBlue,
            Color.SlateGray,
            Color.Snow,
            Color.SpringGreen,
            Color.SteelBlue,
            Color.Tan,
            Color.Teal,
            Color.Thistle,
            Color.Tomato,
            Color.Transparent,
            Color.Turquoise,
            Color.Violet,
            Color.Wheat,
            Color.White,
            Color.WhiteSmoke,
            Color.Yellow,
            Color.YellowGreen,
        };
    }
}

