#define DEMO_WITH_4_8x8_LED_MATRIX_CHAINED
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using MadeInTheUSB.FT232H.Components;
using System.Reflection;
using System.Diagnostics;

namespace MadeInTheUSB.FT232H.MatrixConsole
{

    public class GpioSpiDevice : SpiDeviceBaseClass
    {
        public GpioSpiDevice(MpsseSpiConfig spiConfig) : base(spiConfig)
        {
        }
    }

    partial class Program
    {
        private const int DEFAULT_BRIGTHNESS_DEMO = MAX7219.MAX_BRITGHNESS / 2;
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

        private static List<string> Square00Bmp = new List<string>()
        {
            "B00000000",
            "B00000000",
            "B00000000",
            "B00000000",
            "B00000000",
            "B00000000",
            "B00000000",
            "B00000000",
        };

        private static List<string> Square01Bmp = new List<string>()
        {
            "B11111111",
            "B10000001",
            "B10000001",
            "B10000001",
            "B10000001",
            "B10000001",
            "B10000001",
            "B11111111",
        };

        private static List<string> Square02Bmp = new List<string>()
        {
            "B11111111",
            "B10000001",
            "B10111101",
            "B10100101",
            "B10100101",
            "B10111101",
            "B10000001",
            "B11111111",
        };

        private static List<string> Square03Bmp = new List<string>()
        {
            "B11111111",
            "B10000001",
            "B10111101",
            "B10110101",
            "B10101101",
            "B10111101",
            "B10000001",
            "B11111111",
        };

        private static List<string> Square04Bmp = new List<string>()
        {
            "B11111111",
            "B10000001",
            "B10111101",
            "B10101101",
            "B10110101",
            "B10111101",
            "B10000001",
            "B11111111",
        };

        private static List<string> Square05Bmp = new List<string>()
        {
            "B11111111",
            "B10000001",
            "B10111101",
            "B10111101",
            "B10111101",
            "B10111101",
            "B10000001",
            "B11111111",
        };

        

        private static List<string> Square06Bmp = new List<string>()
        {
            "B11111111",
            "B11111111",
            "B11111111",
            "B11111111",
            "B11111111",
            "B11111111",
            "B11111111",
            "B11111111",
        };


        private class Coordinate
        {
            public Int16 X, Y;
        }

        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if (attribs.Length > 0)
                return ((AssemblyProductAttribute)attribs[0]).Product;
            return null;
        }

        static void Cls(string deviceInfo)
        {
            Console.Clear();

            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);

            ConsoleEx.WriteMenu(-1, 4, "0) Animation demo  1) Images demo");
            ConsoleEx.WriteMenu(-1, 5, "P)erformance test  L)andscape demo  A)xis demo");
            ConsoleEx.WriteMenu(-1, 6, " T)ext demo  R)otate demo  B)rigthness demo");
            ConsoleEx.WriteMenu(-1, 7, " C)lear All  Q)uit I)nit Devices");

            ConsoleEx.TitleBar(ConsoleEx.WindowHeight - 2, "(C) MadeInTheUSB 2018", ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleEx.Bar(0, ConsoleEx.WindowHeight - 3, deviceInfo.ToString(), ConsoleColor.Black, ConsoleColor.DarkCyan);
        }

        private const int ConsoleUserStatusRow = 10;

        private static void DrawAllMatrixOnePixelAtTheTimeDemo(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex, int waitAfterClear = 350, int maxRepeat = 4)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Draw one pixel at the time demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            ConsoleEx.WriteLine(0, ConsoleUserStatusRow + 1, "".PadLeft(80), ConsoleColor.Black);
            ConsoleEx.Gotoxy(0, ConsoleUserStatusRow + 1);

            for (byte rpt = 0; rpt < maxRepeat; rpt++)
            {
                matrix.Clear(deviceIndex, refresh: true);
                Thread.Sleep(waitAfterClear);
                for (var r = 0; r < matrix.Height; r++)
                {
                    for (var c = 0; c < matrix.Width; c++)
                    {
                        matrix.CurrentDeviceIndex = deviceIndex;
                        matrix.DrawPixel(r, c, true);
                        // Only refresh the row when we light up an led
                        // This is 8 time faster than a full refresh
                        matrix.WriteRow(deviceIndex, r);
                        Console.Write('.');
                    }
                }
            }
        }

        private static void DrawRoundRectDemo(MAX_7219_SPI_8x8_Matrix matrix, int wait, int maxRepeat, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Draw Round Rectangle Demo");

            matrix.CurrentDeviceIndex = deviceIndex;

            for (byte rpt = 0; rpt <= maxRepeat; rpt += 2)
            {
                matrix.Clear(deviceIndex);
                var yy = 0;
                while (yy <= 3)
                {
                    matrix.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 1);
                    matrix.CopyToAll(deviceIndex, true);
                    Thread.Sleep(wait);
                    yy += 1;
                }
                Thread.Sleep(wait);
                yy = 2;
                while (yy >= 0)
                {
                    matrix.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 0);
                    matrix.CopyToAll(deviceIndex, true);
                    Thread.Sleep(wait);
                    yy -= 1;
                }
                matrix.Clear(deviceIndex);
                matrix.CopyToAll(deviceIndex, true);
                Thread.Sleep(wait);
            }
        }
        private static void SetDefaultOrientations(MAX_7219_SPI_8x8_Matrix matrix)
        {
            matrix.SetRotation(0);
        }

        private static void BrightnessDemo(MAX_7219_SPI_8x8_Matrix matrix, int maxRepeat, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Brightness Demo");

            matrix.Clear(deviceIndex);
            matrix.CurrentDeviceIndex = deviceIndex;

            var y = 0;
            for (y = 0; y < matrix.Height; y++)
            {
                matrix.DrawLine(0, y, matrix.Width, y, true);
                matrix.WriteDisplay(deviceIndex);
            }
            matrix.AnimateSetBrightness(maxRepeat - 2, deviceIndex: deviceIndex);
            matrix.Clear(deviceIndex);
        }

        private static void DrawCircleDemo(MAX_7219_SPI_8x8_Matrix matrix, int wait, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "DrawCircle Demo");

            matrix.CurrentDeviceIndex = deviceIndex;
            matrix.Clear(deviceIndex);
            matrix.CopyToAll(deviceIndex, refreshAll: true);

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
                    matrix.Clear(deviceIndex);
                    matrix.DrawCircle(circleLocation.X, circleLocation.Y, ray, 1);
                    matrix.CopyToAll(deviceIndex, refreshAll: true);
                    Thread.Sleep(wait);
                }
            }
        }


        private static void SetBrightnesses(MAX_7219_SPI_8x8_Matrix matrix)
        {
            var brightness = DEFAULT_BRIGTHNESS_DEMO;
            if (matrix.DeviceCount > 1)
                brightness /= 2;

            for (var deviceIndex = 0; deviceIndex < matrix.DeviceCount; deviceIndex++)
                matrix.SetBrightness(brightness, deviceIndex);
        }

        private static void DrawRectDemo(MAX_7219_SPI_8x8_Matrix matrix, int MAX_REPEAT, int wait, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Draw Rectangle Demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit");

            matrix.Clear(deviceIndex);
            matrix.CopyToAll(deviceIndex, refreshAll: true);
            matrix.CurrentDeviceIndex = deviceIndex;

            for (byte rpt = 0; rpt < MAX_REPEAT; rpt += 3)
            {
                matrix.Clear();
                var y = 0;
                while (y <= 4)
                {
                    matrix.DrawRect(y, y, 8 - (y * 2), 8 - (y * 2), true);
                    matrix.CopyToAll(deviceIndex, refreshAll: true);
                    Thread.Sleep(wait);
                    y += 1;
                }
                Thread.Sleep(wait);
                y = 4;
                while (y >= 1)
                {
                    matrix.DrawRect(y, y, 8 - (y * 2), 8 - (y * 2), false);
                    matrix.CopyToAll(deviceIndex, refreshAll: true);
                    Thread.Sleep(wait);
                    y -= 1;
                }
            }
            matrix.Clear(deviceIndex);
        }

        static void Animate(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            int wait = 100;
            int maxRepeat = 5;

            matrix.CurrentDeviceIndex = deviceIndex;

            DrawRoundRectDemo(matrix, wait, maxRepeat, deviceIndex);

            //matrix.SetRotation(0);
            DrawAllMatrixOnePixelAtTheTimeDemo(matrix, deviceIndex);

            //matrix.SetRotation(1);
            //DrawAllMatrixOnePixelAtTheTimeDemo(matrix, maxRepeat);

            //matrix.SetRotation(2);
            //DrawAllMatrixOnePixelAtTheTimeDemo(matrix, maxRepeat);

            //matrix.SetRotation(3);
            //DrawAllMatrixOnePixelAtTheTimeDemo(matrix, maxRepeat);

            SetDefaultOrientations(matrix);
            BrightnessDemo(matrix, maxRepeat, deviceIndex);
            SetBrightnesses(matrix);

            DrawCircleDemo(matrix, wait, deviceIndex);
            DrawRectDemo(matrix, maxRepeat, wait, deviceIndex);

            matrix.CurrentDeviceIndex = 0;
        }

        static void DisplayImageSequence(MAX_7219_SPI_8x8_Matrix matrix, string title, int deviceIndex, int maxRepeat, int wait, List<List<string>> images)
        {
            matrix.CurrentDeviceIndex = deviceIndex;
            Console.Clear();
            ConsoleEx.TitleBar(0, title);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");

            for (byte rpt = 0; rpt < maxRepeat; rpt++)
            {
                foreach (var image in images)
                {
                    matrix.Clear(deviceIndex, refresh: false);
                    matrix.DrawBitmap(0, 0, image, 8, 8, 1);
                    matrix.CopyToAll(deviceIndex, refreshAll: true);
                    Thread.Sleep(wait);

                    if (Console.KeyAvailable)
                        if (Console.ReadKey().Key == ConsoleKey.Q) return;
                }
            }
            matrix.Clear(deviceIndex, refresh: true);
        }

        static void DisplaySquareImage1(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            var images = new List<List<string>>
            {
                Square00Bmp, Square01Bmp, Square02Bmp,

                Square03Bmp, Square04Bmp, Square05Bmp, Square04Bmp, Square03Bmp,
                Square04Bmp, Square05Bmp, Square04Bmp, Square03Bmp,

                Square06Bmp,
                Square01Bmp, Square00Bmp, Square01Bmp, Square00Bmp, Square01Bmp,
            };
            DisplayImageSequence(matrix, "Display Images Demo", deviceIndex, 2, 200, images);
        }

        static void DisplaySquareImage2(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            var images = new List<List<string>>
            {
                Square03Bmp, Square04Bmp, Square05Bmp,
            };
            DisplayImageSequence(matrix, "Display Images Demo 2", deviceIndex, 8, 250, images);
        }

        private static void DrawAxis(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            ConsoleEx.Bar(0, ConsoleUserStatusRow, "Draw Axis Demo", ConsoleColor.Yellow, ConsoleColor.Red);

            Console.Clear();
            ConsoleEx.TitleBar(0, "Draw Axis Demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit");


            matrix.Clear(deviceIndex);
            matrix.CurrentDeviceIndex = deviceIndex;

            matrix.Clear(deviceIndex);
            matrix.CurrentDeviceIndex = deviceIndex;
            matrix.DrawLine(0, 0, matrix.Width, 0, true);
            matrix.DrawLine(0, 0, 0, matrix.Height, true);
            matrix.WriteDisplay(deviceIndex);

            for (var i = 0; i < matrix.Width; i++)
            {
                matrix.SetLed(deviceIndex, i, i, true, true);
            }
            var k = Console.ReadKey();
        }

        private static void RotateMatrix(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Rotate Demo");
            ConsoleEx.WriteMenu(0, 2, "Rotate:  L)eft  R)ight  Q)uit");

            matrix.Clear(deviceIndex);
            matrix.CurrentDeviceIndex = deviceIndex;
            matrix.DrawLine(0, 0, 0, matrix.Height, true);
            matrix.DrawLine(7, 0, 7, matrix.Height, true);
            matrix.DrawLine(0, 2, matrix.Width, 2, true);
            matrix.WriteDisplay(deviceIndex);

            while (true)
            {
                var k = Console.ReadKey(true).Key;
                switch (k)
                {
                    case ConsoleKey.Q: return; break;
                    case ConsoleKey.L: matrix.RotateLeft(deviceIndex); break;
                    case ConsoleKey.R: matrix.RotateRight(deviceIndex); break;
                }
                matrix.WriteDisplay(deviceIndex);
            }
        }

        static void ScrollDemo(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Scroll Demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit");

            matrix.Clear(all: true, refresh: true);

            for (var d = 0; d < matrix.DeviceCount; d++)
            {
                for (var x = 0; x < matrix.Width; x++)
                {
                    matrix.SetLed(d, x, 0, true);
                    matrix.SetLed(d, x, 7, true);
                    matrix.SetLed(d, 0, x, true);
                }
            }
            matrix.WriteDisplay(all: true);
            Thread.Sleep(1000);

            for (var z = 0; z < 8 * 3; z++)
            {

                matrix.ScrollPixelLeftDevices(3, 0);
                matrix.WriteDisplay(all: true);
            }
        }

        private static void ScrollText(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex = 0)
        {
            var quit = false;
            var speed = 10;
            var text = "Hello World!      ";

            if (matrix.DeviceCount == 1 && matrix.MAX7219Wiring == MAX_7219_SPI_8x8_Matrix.MAX7219_WIRING_TO_8x8_LED_MATRIX.OriginBottomRightCorner)
                speed = speed * 3;

            while (!quit)
            {
                Console.Clear();
                ConsoleEx.TitleBar(0, "Scroll Text");
                ConsoleEx.WriteMenu(0, 2, string.Format("Q)uit  F)aster  S)lower   Speed:{0:000}", speed));

                matrix.Clear(all: true);
                matrix.WriteDisplay(all: true);

                for (var ci = 0; ci < text.Length; ci++)
                {
                    var c = text[ci];

                    ConsoleEx.WriteMenu(ci, 4, c.ToString());

                    matrix.WriteChar(deviceIndex, c); // See property matrix.MAX7218Wiring for more info
                    matrix.WriteDisplay(all: true);

                    if (speed > 0)
                    {
                        Thread.Sleep(speed);
                        // Provide a better animation
                        if (matrix.DeviceCount == 1 && matrix.MAX7219Wiring == MAX_7219_SPI_8x8_Matrix.MAX7219_WIRING_TO_8x8_LED_MATRIX.OriginBottomRightCorner)
                            Thread.Sleep(speed * 12);
                    }

                    for (var i = 0; i < MAX7219.MATRIX_ROW_SIZE; i++)
                    {
                        matrix.ScrollPixelLeftDevices(matrix.DeviceCount - 1, 0, 1);
                        matrix.WriteDisplay(all: true);

                        // Do not wait when we scrolled the last pixel, we will wait when we display the new character
                        if (i < MAX7219.MATRIX_ROW_SIZE - 1)
                            if (speed > 0) Thread.Sleep(speed);

                        if (Console.KeyAvailable)
                        {
                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.Q: quit = true; i = 100; ci = 10000; break;
                                case ConsoleKey.S: speed += 10; break;
                                case ConsoleKey.F: speed -= 10; if (speed < 0) speed = 0; break;
                            }
                            ConsoleEx.WriteMenu(0, 2, string.Format("Q)uit  F)aster  S)lower   Speed:{0:000}", speed));
                        }
                    }
                }
            }
        }

        static void PerformanceTest(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Performance Test");
            ConsoleEx.WriteLine(0, 2, "Drawing images as fast as possible", ConsoleColor.Cyan);

            int maxRepeat = 16;
            matrix.CurrentDeviceIndex = deviceIndex;

            var images = new List<List<string>> {
                Square00Bmp, Square02Bmp
            };

            maxRepeat = 128;

            ConsoleEx.WriteLine(0, 5, "Fast mode first", ConsoleColor.Cyan);
            ConsoleEx.Gotoxy(0, 6);
            matrix.BytesSentOutCounter = 0;
            var sw = Stopwatch.StartNew();
            int writeDisplayCount = 0;
            for (byte rpt = 0; rpt < maxRepeat; rpt++)
            {
                foreach (var image in images)
                {
                    matrix.Clear(deviceIndex, refresh: false);
                    matrix.DrawBitmap(0, 0, image, 8, 8, 1);
                    matrix.WriteDisplay(deviceIndex);
                    writeDisplayCount++;
                    //Console.Write(".");
                }
            }
            sw.Stop();
            Console.WriteLine("");
            Console.WriteLine("Display Refresh:{0}, {1:0.0} Refresh/S, Bytes Sent:{2}, {3:0.0} K Byte/S, Duration:{4}",
                writeDisplayCount,
                writeDisplayCount * 1000.0 / sw.ElapsedMilliseconds,
                matrix.BytesSentOutCounter,
                matrix.BytesSentOutCounter / (sw.ElapsedMilliseconds / 1000.0) / 1024.0,
                sw.ElapsedMilliseconds
                );
            Console.WriteLine("Hit any key to continue");
            var k = Console.ReadKey();
        }

        private static void LandscapeDemo(MAX_7219_SPI_8x8_Matrix matrix, int deviceIndex = 0)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, "Random Landscape Demo");
            ConsoleEx.WriteMenu(0, 2, "Q)uit  F)ull speed");
            var landscape = new NusbioLandscapeMatrix(matrix, 0);

            var speed = 100;
            if(matrix.DeviceCount == 1)
                speed = 150;

            matrix.Clear(all: true);
            var quit = false;
            var fullSpeed = false;

            while (!quit)
            {
                landscape.Redraw();

                ConsoleEx.WriteLine(0, 4, landscape.ToString(), ConsoleColor.Cyan);
                if(!fullSpeed)
                    Thread.Sleep(speed);
                
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Q: quit = true; break;
                        case ConsoleKey.F:
                            fullSpeed = !fullSpeed; break;
                    }
                }
            }
        }



        static void Main(string[] args)
        {
            var ft232Device = FT232HDetector.Detect();
            if (ft232Device.Ok)
            {
                System.Console.WriteLine(ft232Device.ToString());
            }
            else
            {
                Environment.Exit(1);
            }
                

            Cls(ft232Device.ToString());

            // MAX7219 is limited to 10Mhz
            var ft232hGpioSpiDevice = new GpioSpiDevice(MpsseSpiConfig.BuildSPI(MpsseSpiConfig._10Mhz));
            var spi = ft232hGpioSpiDevice.SPI;

#if DEMO_WITH_4_8x8_LED_MATRIX_CHAINED
                var matrixChainedCount = 8;
                var origin = MAX_7219_SPI_8x8_Matrix.MAX7219_WIRING_TO_8x8_LED_MATRIX.OriginUpperLeftCorner; // Different Wiring for 4 8x8 LED Matrix sold by MadeInTheUSB
#else
                var matrixChainedCount = 1;
                var origin = NusbioMatrix.MAX7219_WIRING_TO_8x8_LED_MATRIX.OriginBottomRightCorner;
#endif

            var matrix = MAX_7219_SPI_8x8_Matrix.Initialize(spi, origin, matrixChainedCount);

            matrix.DrawRect(1, 1, 4, 4, true);
            matrix.WriteDisplay(0);

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.D0)
                        Animate(matrix, 0);
                    if (k == ConsoleKey.D1)
                        DisplaySquareImage1(matrix, 0);
                    if (k == ConsoleKey.D2)
                        DisplaySquareImage2(matrix, 0);
                    if (k == ConsoleKey.Q)
                        break;
                    if (k == ConsoleKey.A)
                        DrawAxis(matrix, 0);
                    if (k == ConsoleKey.R)
                        RotateMatrix(matrix, 0);
                    if (k == ConsoleKey.S)
                        ScrollDemo(matrix, 0);
                    if (k == ConsoleKey.P)
                        PerformanceTest(matrix, 0); // Speed test
                    if (k == ConsoleKey.C)
                        matrix.Clear(all: true, refresh: true);
                    if (k == ConsoleKey.T)
                        ScrollText(matrix);
                    if (k == ConsoleKey.L)
                        LandscapeDemo(matrix);
                    Cls(ft232Device.ToString());
                    matrix.Clear(all: true, refresh: true);
                }
            }
        }
    }
}
