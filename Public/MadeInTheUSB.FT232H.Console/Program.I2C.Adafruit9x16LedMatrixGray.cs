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
        static IS31FL3731 ledMatrix16x9;

        static void I2CSample_Adafruit9x16LedMatrixGray(I2CDevice i2cDevice)
        {
            ledMatrix16x9 = new IS31FL3731(i2cDevice);
            if (!ledMatrix16x9.Begin())
                return;

            ledMatrix16x9.Clear();
            //ledMatrix16x9.DrawRect(0, 0, 8, 8, 100);
            ledMatrix16x9.UpdateDisplay(0);

            //BarScrollDemo(ledMatrix16x9);
            //FadeInFadeout(ledMatrix16x9);
            IntensistyScrollingDemo(ledMatrix16x9);
            //IntensistyScrollingDemo2(ledMatrix16x9);
            //LandscapeDemo(ledMatrix16x9);
            //IntensistyDemo(ledMatrix16x9);
        }

       

        private static bool BarScrollDemoInternal(IS31FL3731 ledMatrix16x9)
        {
            var doubleBufferIndex = 1;
            ledMatrix16x9.Clear(doubleBufferIndex);
            ledMatrix16x9.SelectFrame(doubleBufferIndex);

            for (var x = 0; x < ledMatrix16x9.Width; x++)
            {
                ledMatrix16x9.Clear(doubleBufferIndex, false);

                for (int y = 0; y < ledMatrix16x9.Height; y++)
                {
                    ledMatrix16x9.SetLedPwm(x, y, 32, doubleBufferIndex);
                }
                var bytePerSecond = ledMatrix16x9.UpdateDisplay(doubleBufferIndex);
                ConsoleEx.WriteLine(0, 1, string.Format("x:{0:000}, {1:0.00} K byte/sec sent", x, bytePerSecond / 1024.0), ConsoleColor.Cyan);

                doubleBufferIndex = doubleBufferIndex == 1 ? 0 : 1;
                if (System.Console.KeyAvailable && System.Console.ReadKey(true).Key == ConsoleKey.Q) return false;
                Thread.Sleep(15);
            }
            return true;
        }

        static void BarScrollDemo(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Bar Scroll Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(-1, 5, "Q)uit");
            var quit = false;
            ledMatrix16x9.Clear();
            while (!quit)
            {
                if (!BarScrollDemoInternal(ledMatrix16x9)) quit = true;
                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q)
                        quit = true;
                }
            }
        }



        static bool FadeInFadeout(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Fade In, Fade Out Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");

            var quit = false;

            while (!quit)
            {
                var maxIntensisty = 48;
                var nextFrameIndex = 1;
                for (var incr = 1; incr < maxIntensisty; incr++)
                {
                    ledMatrix16x9.SelectFrame(nextFrameIndex);
                    for (int x = 0; x < ledMatrix16x9.Width; x++)
                        for (int y = 0; y < ledMatrix16x9.Height; y++)
                            ledMatrix16x9.SetLedPwm(x, y, incr, nextFrameIndex);
                    ledMatrix16x9.UpdateDisplay(nextFrameIndex);
                    nextFrameIndex = nextFrameIndex == 1 ? 0 : 1;
                    if (incr > 128)
                        break;
                    if (System.Console.KeyAvailable && System.Console.ReadKey(true).Key == ConsoleKey.Q) return false;
                }

                Thread.Sleep(150);

                for (var incr = maxIntensisty; incr > 1; incr--)
                {
                    ledMatrix16x9.SelectFrame(nextFrameIndex);
                    for (int x = 0; x < ledMatrix16x9.Width; x++)
                        for (int y = 0; y < ledMatrix16x9.Height; y++)
                            ledMatrix16x9.SetLedPwm(x, y, incr, nextFrameIndex);
                    ledMatrix16x9.UpdateDisplay(nextFrameIndex);
                    nextFrameIndex = nextFrameIndex == 1 ? 0 : 1;
                    if (incr > 128)
                        break;
                    if (System.Console.KeyAvailable && System.Console.ReadKey(true).Key == ConsoleKey.Q) return false;
                }

                Thread.Sleep(260);
            }

            return true;
        }

        static void IntensistyScrollingDemo(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Intensisty Scrolling Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            var sweep = new List<int>() {
                1, 2, 3, 4, 6, 8, 10, 15, 20, 30, 40, 60, 60, 40, 30, 20, 15, 10, 8, 6, 4, 3, 2, 1
            };

            var quit = false;
            var incr = 0;
            var doubleBufferIndex = 1;
            ledMatrix16x9.Clear();
            int modulo = 24;

            while (!quit)
            {
                for (int y = 0; y < ledMatrix16x9.Height; y++)
                {
                    ConsoleEx.Write(0, y + 4, string.Format("{0:00} - ", y), ConsoleColor.Cyan);
                    for (int x = 0; x < ledMatrix16x9.Width; x++)
                    {
                        var intensity = sweep[(x + y + incr) % modulo];
                        ledMatrix16x9.DrawPixel(x, y, intensity);
                        System.Console.Write("{0:000} ", intensity);
                    }
                }
                var bytePerSecond = ledMatrix16x9.UpdateDisplay(doubleBufferIndex);
                ConsoleEx.WriteLine(0, 15, string.Format("{0:0.00} K byte/sec sent", bytePerSecond / 1024.0), ConsoleColor.Cyan);
                doubleBufferIndex = doubleBufferIndex == 1 ? 0 : 1;
                incr++;

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q)
                        quit = true;
                }
            }
        }

        static void IntensistyScrollingDemo2(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Intensisty Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            var sweep = new List<int>() {
                1, 2, 8, 16, 24, 32, 44, 64, 80, 96, 120, 128, 120, 96, 80, 64, 44, 32, 24, 16, 8, 4, 2, 1
            };

            var quit = false;
            var incr = 1;

            var moduleStepIndex = 0;
            var moduleSteps = new List<int>() { 2, 4, 6, 8, 10, 12, 16, 18, 20, 22 };
            moduleSteps = new List<int>() { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 };

            var doubleBufferIndex = 1;
            ledMatrix16x9.Clear();

            while (!quit)
            {
                for (int y = 0; y < ledMatrix16x9.Height; y++)
                {
                    ConsoleEx.Write(0, y + 4, string.Format("{0:00} - ", y), ConsoleColor.Cyan);

                    for (int x = 0; x < ledMatrix16x9.Width; x++)
                    {
                        var intensity = sweep[(x + y + incr) % moduleSteps[moduleStepIndex]];
                        ledMatrix16x9.DrawPixel(x, y, intensity);
                        System.Console.Write("{0:000} ", intensity);

                        moduleStepIndex++;
                        if (moduleStepIndex >= moduleSteps.Count)
                            moduleStepIndex = 0;
                    }
                }
                var bytePerSecond = ledMatrix16x9.UpdateDisplay(doubleBufferIndex);
                ConsoleEx.WriteLine(0, 15, string.Format("{0:0.00} K byte/sec sent", bytePerSecond / 1024.0), ConsoleColor.Cyan);
                doubleBufferIndex = doubleBufferIndex == 1 ? 0 : 1;
                incr++;
                Thread.Sleep(50);

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q)
                        quit = true;
                }
            }
        }

        private static void LandscapeDemo(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Random Landscape Demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit  F)ull speed");
            var landscape = new LandscapeIS31FL3731(ledMatrix16x9);

            var speed = 32;
            ledMatrix16x9.SelectFrame(0);
            ledMatrix16x9.Clear();
            var quit = false;
            var fullSpeed = false;

            while (!quit)
            {
                var bytePerSecond = landscape.Redraw();
                ConsoleEx.WriteLine(0, 15, string.Format("{0:0.00} K byte/sec sent", bytePerSecond / 1024.0), ConsoleColor.Cyan);

                ConsoleEx.WriteLine(0, 4, landscape.ToString(), ConsoleColor.Cyan);
                if (!fullSpeed)
                    Thread.Sleep(speed);

                if (System.Console.KeyAvailable)
                {
                    switch (System.Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Q: quit = true; break;
                        case ConsoleKey.F:
                            fullSpeed = !fullSpeed; break;
                    }
                }
            }
        }



        static void IntensistyDemo(IS31FL3731 ledMatrix16x9)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Intensisty Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            var quit = false;
            var doubleBufferIndex = 1;

            // 8 intensities for each row
            // After 160, the intensity remain the same
            var intensities = new List<int>() { 2, 8, 16, 32, 32 + 12, 64, 96, 96 + 24, 96 + 32 };
            var intensityIndex = 0;

            ledMatrix16x9.Clear();

            while (!quit)
            {
                intensityIndex = 0;
                for (int y = 0; y < ledMatrix16x9.Height; y++)
                {
                    ConsoleEx.Write(0, y + 4, string.Format("{0:00} - ", y), ConsoleColor.Cyan);
                    for (int x = 0; x < ledMatrix16x9.Width; x++)
                    {
                        ledMatrix16x9.DrawPixel(x, y, intensities[intensityIndex]);
                        System.Console.Write("{0:000} ", intensities[intensityIndex]);
                    }
                    if (++intensityIndex >= intensities.Count)
                        intensityIndex = 0;
                }
                var bytePerSecond = ledMatrix16x9.UpdateDisplay(doubleBufferIndex);
                ConsoleEx.WriteLine(0, 15, string.Format("{0:0.00} K byte/sec sent", bytePerSecond / 1024.0), ConsoleColor.Cyan);
                doubleBufferIndex = doubleBufferIndex == 1 ? 0 : 1;

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q)
                        quit = true;
                }
            }
        }
    }
}

