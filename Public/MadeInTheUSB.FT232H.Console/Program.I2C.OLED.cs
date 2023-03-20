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
using MadeInTheUSB.Display;
using System.Diagnostics;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static void DrawCircleFractal(I2C_OLED _oledDisplay, int x, int y, int r, int sleep = 20)
        {
            _oledDisplay.DrawCircle(x, y, r, true);
            _oledDisplay.DrawCircle(x + r, y, r / 2, true);
            _oledDisplay.DrawCircle(x - r, y, r / 2, true);
            _oledDisplay.WriteDisplay();
            Thread.Sleep(sleep);
        }

        public static void OledCircleFractalDemo(I2C_OLED oled, bool clearScreen)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Circle fractal demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            var rnd = new Random();
            oled.Clear(refresh: true);
            Thread.Sleep(1000 * 2);

            oled.Clear(refresh: true);
            var r = 16;
            var x = oled.Width / 2 - 1;
            var y = oled.Height / 2 - 1;
            var maxStep = 16;

            foreach (var loop in DS.Range(2))
            {
                for (var i = 0; i < maxStep; i += r / 4)
                {
                    if (clearScreen) oled.Clear(false);
                    DrawCircleFractal(oled, x + i, y - i, r);
                }
                for (var i = 0; i < maxStep; i += r / 4)
                {
                    if (clearScreen) oled.Clear(false);
                    DrawCircleFractal(oled, x - i, y + i, r);
                }
                for (var i = 0; i < maxStep; i += r / 4)
                {
                    if (clearScreen) oled.Clear(false);
                    DrawCircleFractal(oled, x + i, y + i, r);
                }
                for (var i = 0; i < maxStep; i += r / 4)
                {
                    if (clearScreen) oled.Clear(false);
                    DrawCircleFractal(oled, x - i, y - i, r);
                }
            }
            Thread.Sleep(1000 * 2);
        }

        public static void OledCircleDemo(I2C_OLED oled)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Circle Demo");
            ConsoleEx.WriteMenu(0, 1, "Q)uit");
            var rnd = new Random();
            oled.Clear(refresh: true);
            Thread.Sleep(1000 * 2);

            oled.Clear(refresh: true);
            for (var i = 0; i < 16; i += 1)
            {
                var r = rnd.Next(2, 16);
                var x = rnd.Next(r + 1, oled.Width - r);
                var y = rnd.Next(r + 1, oled.Height - r);
                oled.DrawCircle(x, y, r, true);
                oled.WriteDisplay();

                System.Console.WriteLine("Circle {0:000},{1:000} r:{2:000}", x, y, r);
                Thread.Sleep(125);
            }
            Thread.Sleep(1000 * 2);
        }

        public static void OledRectangleDemo(I2C_OLED oled, int wait)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Rectangle demo");
            ConsoleEx.WriteMenu(-1, 6, "Q)uit");
            oled.Clear();
            const string windowTitle = " Rectangle Demo ";
            oled.DrawWindow(windowTitle, (wait > 0) ? "Slow Speed" : "Fast Speed");
            Thread.Sleep(1000 * 2);

            var sw = Stopwatch.StartNew();

            for (var j = 1; j < oled.Height; j++)
            {
                oled.Clear(refresh: false);
                for (var i = 0; i < oled.Height; i += j)
                {
                    oled.DrawRect(i, i, oled.Width - (i * 2), oled.Height - (i * 2), true);
                    System.Console.WriteLine("Rectangle {0:000},{1:000} {2:000},{3:000}", i, i, oled.Width - (i * 2), oled.Height - (i * 2));
                }
                oled.WriteDisplay();
                if (wait > 0)
                    Thread.Sleep(wait);
            }
            sw.Stop();
            oled.DrawWindow(windowTitle, "The End.");
            System.Console.WriteLine("Execution Time:{0}. Hit space to continue", sw.ElapsedMilliseconds);
            Thread.Sleep(1000 * 2);
        }

        static void APDS_9900_DigitalInfraredGestureSensor(I2CDevice i2cDevice)
        {
            var sensor = new APDS_9900_DigitalInfraredGestureSensor(i2cDevice);
            if (sensor.Begin())
            {
                
            }
        }

        static void OLED_SSD1306_Sample(I2CDevice i2cDevice)
        {
            var oled = new I2C_OLED_SSD1306(i2cDevice, 128 , 64);
            if (oled.Begin())
            {
                oled.DrawLine(0, 0, oled.Width, 0, 1); oled.WriteDisplay();
                oled.DrawLine(0, 16, oled.Width, 16, 1); oled.WriteDisplay();
                oled.DrawLine(0, 32, oled.Width, 32, 1); oled.WriteDisplay();
                oled.DrawLine(0, 48, oled.Width, 48, 1); oled.WriteDisplay();
                oled.DrawLine(0, 64, oled.Width, 64, 1); oled.WriteDisplay();
                //oled.DrawLine(0, 0, oled.Width, oled.Height, 1);

                oled.DrawWindow(" Hello ",  "Fast Speed");
                OledRectangleDemo(oled, 20);
                OledCircleDemo(oled);
                OledCircleFractalDemo(oled, true);
                OledCircleFractalDemo(oled, !true);
                oled.Clear(true);
            }
        }
    }
}

