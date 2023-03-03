using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using DynamicSugar;
using MadeInTheUSB.Adafruit;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {



        static LEDBackpack _ledMatrix00;
        static LEDBackpack _ledMatrix01;
        static MultiLEDBackpackManager _backpackManager;

        static void I2CSample_Adafruit8x8LedMatrix(I2CDevice i2cDevice)
        {
            _backpackManager = new MultiLEDBackpackManager();

            _ledMatrix00 = _backpackManager.Add(i2cDevice, 8, 8, 0x70);
            //_ledMatrix01 = _backpackManager.Add(i2cDevice, 8, 8, 0x71);

            i2cDevice.Gpios.ProgressNext(clear: true);
            _backpackManager.SetBrightness(1);
            Animate();
        }

        private static List<string> smileBmp = new List<string>()
        {
            "B00111100",
            "B01000010",
            "B10100101",
            "B10000001",
            "B10100101",
            "B10011001",
            "B01000010",
            "B00111100",
        };

        private static List<string> neutralBmp = new List<string>()
        {
            "B00111100",
            "B01000010",
            "B10100101",
            "B10000001",
            "B10111101",
            "B10000001",
            "B01000010",
            "B00111100",
        };

        private static List<string> frownbmp = new List<string>()
        {
            "B00111100",
            "B01000010",
            "B10100101",
            "B10000001",
            "B10011001",
            "B10100101",
            "B01000010",
            "B00111100",
        };

        public static List<int> ParseBinary(List<string> binaryValues)
        {
            var l = new List<int>();
            foreach (var bv in binaryValues)
                l.Add(ParseBinary(bv));
            return l;
        }

        public static int ParseBinary(string s)
        {
            if (s.ToUpperInvariant().StartsWith("B"))
            {
                return Convert.ToInt32(s.Substring(1), 2);
            }
            else throw new ArgumentException(string.Format("Invalid binary value:{0}", s));
        }

        static void DisplayImage()
        {
            int MAX_REPEAT = 5;
            int wait = 400;

            ConsoleEx.Bar(0, 5, "DrawBitmap Demo", ConsoleColor.Yellow, ConsoleColor.Red);
            for (byte rpt = 0; rpt <= MAX_REPEAT; rpt++)
            {
                var images = new List<List<string>> { neutralBmp, smileBmp, neutralBmp, frownbmp };
                foreach (var image in images)
                {
                    _backpackManager.Clear(refresh: false);
                    _backpackManager.DrawBitmap(0, 0, ParseBinary(image), 8, 8, 1);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                }
            }
        }

        private static void SetDefaultOrientations()
        {
            _backpackManager.SetRotation(0);
            _backpackManager.SetRotation(2);
        }

        private static void SetBrightnesses()
        {
            _backpackManager.SetBrightness(1);
        }

        static void Animate()
        {
            int wait = 100;
            int waitPixelDemo = 10;
            int maxRepeat = 5;

            DrawRoundRectDemo(wait, maxRepeat);

            _backpackManager.SetRotation(0);
            DrawPixelDemo(maxRepeat, waitPixelDemo);

            _backpackManager.SetRotation(1);
            DrawPixelDemo(maxRepeat, waitPixelDemo);

            _backpackManager.SetRotation(2);
            DrawPixelDemo(maxRepeat, waitPixelDemo);

            _backpackManager.SetRotation(3);
            DrawPixelDemo(maxRepeat, waitPixelDemo);

            SetDefaultOrientations();
            BrightnessDemo(maxRepeat);
            SetBrightnesses();

            DrawCircleDemo(wait);
            DrawRectDemo(maxRepeat, wait);
        }

        private static void DrawRectDemo(int MAX_REPEAT, int wait)
        {
            ConsoleEx.Bar(0, 5, "DrawRect Demo", ConsoleColor.Yellow, ConsoleColor.Red);
            _backpackManager.Clear();

            for (byte rpt = 0; rpt <= MAX_REPEAT; rpt += 3)
            {
                _backpackManager.Clear();
                var y = 0;
                while (y <= 4)
                {
                    _backpackManager.DrawRect(y, y, 8 - (y * 2), 8 - (y * 2), true);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    y += 1;
                }
                Thread.Sleep(wait);
                y = 4;
                while (y >= 1)
                {
                    _backpackManager.DrawRect(y, y, 8 - (y * 2), 8 - (y * 2), false);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    y -= 1;
                }
            }
            _backpackManager.Clear(true);
        }

        private class Coordinate
        {
            public Int16 X, Y;
        }

        private static void DrawCircleDemo(int wait)
        {
            ConsoleEx.Bar(0, 5, "DrawCircle Demo", ConsoleColor.Yellow, ConsoleColor.Red);
            _backpackManager.Clear();

            var circleLocations = new List<Coordinate>()
            {
                new Coordinate { X = 4, Y = 4},
                new Coordinate { X = 3, Y = 3},
                new Coordinate { X = 5, Y = 5},
                new Coordinate { X = 2, Y = 2},
            };

            foreach (var circleLocation in circleLocations)
            {
                for (byte ray = 0; ray <= 4; ray++)
                {
                    _backpackManager.Clear();
                    _backpackManager.DrawCircle(circleLocation.X, circleLocation.Y, ray, 1);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait * 2);
                }
            }
        }

        private static void BrightnessDemo(int maxRepeat)
        {
            ConsoleEx.Bar(0, 5, "Brightness Demo", ConsoleColor.Yellow, ConsoleColor.Red);
            _backpackManager.AnimateSetBrightness(maxRepeat - 2);
            _backpackManager.Clear(true);
        }

        private static void DrawPixelDemo(int maxRepeat, int wait = 13)
        {
            maxRepeat = 4;
            ConsoleEx.Bar(0, 5, "DrawPixel Demo", ConsoleColor.Yellow, ConsoleColor.Red);
            for (byte rpt = 0; rpt < maxRepeat; rpt += 2)
            {
                _backpackManager.Clear();
                Thread.Sleep(wait * 20);
                for (var r = 0; r < _ledMatrix00.Height; r++)
                {
                    for (var c = 0; c < _ledMatrix00.Width; c++)
                    {
                        _backpackManager.DrawPixel(r, c, true);
                        if ((true) || (c % 2 != 0)) // Reduce the number of refresh and improve speed
                        {
                            _backpackManager.WriteDisplay();
                            Thread.Sleep(wait);
                        }
                    }
                }
            }
        }

        private static void DrawRoundRectDemo(int wait, int maxRepeat)
        {
            ConsoleEx.Bar(0, 5, "DrawRoundRect Demo", ConsoleColor.Yellow, ConsoleColor.Red);

            for (byte rpt = 0; rpt <= maxRepeat; rpt += 2)
            {
                _backpackManager.Clear(true);
                var yy = 0;
                while (yy <= 3)
                {
                    _backpackManager.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 1);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    yy += 1;
                }
                Thread.Sleep(wait);
                yy = 2;
                while (yy >= 0)
                {
                    _backpackManager.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 0);
                    _backpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    yy -= 1;
                }
                _backpackManager.Clear(true);
                Thread.Sleep(wait);
            }
        }
    }
}

