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
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint32_t = System.UInt32;
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using static MadeInTheUSB.APDS_9900_DigitalInfraredGestureSensor;
using MadeInTheUSB.FT232H.Component.I2C.EEPROM;
using BufferUtil;

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

        public static void OledCircleDemo(I2C_OLED oled, int wait)
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
                if(wait > 0)
                    Thread.Sleep(wait);
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

        static void I2C_Gpios(I2CDevice_MPSSE_NotUsed i2cDevice, IDigitalWriteRead gpios)
        {
            var gpioPullUpIndex = 0;
            var gpioOutputIndex = 1;
            gpios.SetPinMode(gpioPullUpIndex, PinMode.InputPullUp);
            gpios.SetPinMode(gpioOutputIndex, PinMode.Output);

            var state = i2cDevice.Gpios.DigitalRead(gpioPullUpIndex);

            gpios.DigitalWrite(gpioOutputIndex, PinState.Low);
            state = gpios.DigitalRead(gpioPullUpIndex);

            gpios.DigitalWrite(gpioOutputIndex, PinState.High);
            state = gpios.DigitalRead(gpioPullUpIndex);
        }


        static void APDS_9900_DigitalInfraredGestureSensor_Gesture(I2CDevice i2cDevice)
        {
            i2cDevice.HardwareProgressBarOn = false; // Not supported because the APDS_9900 need a gpio for the interrupt
            var sensor = new APDS_9900_DigitalInfraredGestureSensor(i2cDevice, 0);
            if (sensor.Begin())
            {
                sensor.enableProximity(true);
                sensor.enableGesture(true);

                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Gesture Sensor");
                ConsoleEx.WriteMenu(-1, 2, "Q)uit");
                var quit = false;
                while (!quit)
                {
                    Gestures gesture = sensor.readGesture();
                    if (gesture == APDS_9900_DigitalInfraredGestureSensor.Gestures.APDS9960_DOWN)   System.Console.WriteLine("DOWN");
                    if (gesture == APDS_9900_DigitalInfraredGestureSensor.Gestures.APDS9960_UP)     System.Console.WriteLine("UP");
                    if (gesture == APDS_9900_DigitalInfraredGestureSensor.Gestures.APDS9960_LEFT)   System.Console.WriteLine("LEFT");
                    if (gesture == APDS_9900_DigitalInfraredGestureSensor.Gestures.APDS9960_RIGHT)  System.Console.WriteLine("RIGHT");

                    if (System.Console.KeyAvailable)
                    {
                        var k = System.Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q)
                            quit = true;
                    }
                    Thread.Sleep(100);
                }
            }
        }


        static void APDS_9900_DigitalInfraredGestureSensor_Color(I2CDevice i2cDevice)
        {
            var sensor = new APDS_9900_DigitalInfraredGestureSensor(i2cDevice, 0);
            if (sensor.Begin())
            {
                //enable proximity mode
                sensor.enableColor(true);

                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Color Sensor");
                ConsoleEx.WriteMenu(-1, 2, "Q)uit");
                var quit = false;
                ConsoleEx.WriteLine(1, 10, "Waiting for interrupt:", ConsoleColor.White);
                while (!quit)
                {
                    uint16_t r = 0, g = 0, b = 0, c = 0;
                    while (!sensor.colorDataReady())
                        Thread.Sleep(10);

                    sensor.getColorData(ref r, ref g, ref b, ref c);
                    System.Console.WriteLine($"r:{r:000000}, g:{g:000000}, b:{b:000000}, c:{c:000000}");

                    if (System.Console.KeyAvailable)
                    {
                        var k = System.Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q)
                            quit = true;
                    }
                    Thread.Sleep(750);
                }
            }
        }


        static void Gpio_Output_InputMode(I2CDevice i2cDevice)
        {
            i2cDevice.HardwareProgressBarOn = false;
            var gpios = i2cDevice.Gpios;

            for(var g=0; g < gpios.MaxGpio; g++)
            {
                gpios.SetPinMode(g, PinMode.Output);
            }

            // Gpios Test Extension - output mode - gpio 0,1,2,3 send current to gpio 4,5,6,7
            for (var g = 0; g < gpios.MaxGpio/2; g++)
            {
                gpios.DigitalWrite(g, PinState.High);
                gpios.DigitalWrite(g, PinState.Low);
            }

            for (var g = 0; g < gpios.MaxGpio/2; g++)
            {
                gpios.SetPinMode(g, PinMode.Input);
            }
            //gpios.DigitalWrite(0, PinState.High);
            //gpios.DigitalWrite(0, PinState.Low);
            //var pin = gpios.DigitalRead(0);

            //gpios.SetPullUp(0, PinState.High);
            //var pin = gpios.DigitalRead(0);
            //gpios.SetPinMode(0, PinMode.Output);
            //gpios.DigitalWrite(0, PinState.High);
            //gpios.DigitalWrite(0, PinState.Low);
        }

        static void APDS_9900_DigitalInfraredGestureSensor_Proximity(I2CDevice i2cDevice)
        {
            i2cDevice.HardwareProgressBarOn = false;
            var sensor = new APDS_9900_DigitalInfraredGestureSensor(i2cDevice, 0);
            if (sensor.Begin())
            {
                //enable proximity mode
                sensor.enableProximity(true);
                sensor.setProximityInterruptThreshold(0, 1); // 175
                sensor.enableProximityInterrupt();

                System.Console.Clear();
                ConsoleEx.TitleBar(0, "APDS_9900 - Proximity Sensor");
                ConsoleEx.WriteMenu(-1, 2, "Q)uit");
                var quit = false;
                ConsoleEx.WriteLine(1, 10, "Waiting for interrupt:", ConsoleColor.White);
                while (!quit)
                {
                    if(sensor.IsInterruptOn)
                    {
                        System.Console.WriteLine($"readProximity: {sensor.readProximity()}");
                        sensor.clearInterrupt();
                        sensor.clearInterrupt();
                    }

                    if (System.Console.KeyAvailable)
                    {
                        var k = System.Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q)
                            quit = true;
                    }
                    Thread.Sleep(10);
                }
            }
        }

        const string largeText = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse et placerat tortor. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Suspendisse potenti. Praesent ultricies sit amet libero nec venenatis. Pellentesque sit amet luctus nisl, et consequat orci. Nunc semper sed enim id mattis. In eget augue velit. Quisque vitae tortor lorem. Etiam a ultricies mauris. Fusce felis sem, placerat at blandit in, efficitur id nunc. Pellentesque accumsan a leo a sodales.
Interdum et malesuada fames ac ante ipsum primis in faucibus. Praesent ac ornare ipsum. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Donec condimentum id nisi ullamcorper interdum. Nullam eleifend diam quis purus viverra, sit amet volutpat urna condimentum. Praesent semper eros urna, nec convallis turpis fermentum in. Phasellus condimentum, nulla sed interdum efficitur, enim enim tincidunt tellus, vel faucibus ex lectus at ipsum. Nunc id orci fringilla, aliquet odio in, rutrum libero. Vestibulum eget neque sit amet urna iaculis semper. Etiam malesuada semper lectus, condimentum accumsan purus maximus eu. Donec vulputate risus vitae tortor luctus accumsan. Nulla venenatis, neque eu tristique congue, sem purus dictum leo, sed interdum nibh nibh id tortor. Curabitur id tempus tellus, vitae porta neque. Ut non molestie purus.
Donec euismod malesuada vulputate. Vestibulum lobortis consequat erat sed suscipit. Vivamus porttitor pellentesque elit, in consectetur nunc fermentum sit amet. Nullam vitae nibh et metus luctus tempus sit amet vitae urna. Morbi non lectus vitae orci volutpat rutrum. Nullam aliquet, sapien a pulvinar faucibus, enim metus maximus urna, non tristique elit massa fermentum eros. Pellentesque faucibus tortor non fringilla pulvinar. Etiam cursus, neque eget euismod eleifend, nulla magna ultrices libero, in eleifend nisi augue congue neque. Sed vitae mi vitae felis mollis fermentum in quis mauris.
Morbi ante nisl, auctor vel odio at, egestas scelerisque leo. Cras in dolor imperdiet purus congue porta. Fusce et augue eget ex congue fringilla lacinia id felis. Mauris et tortor in ex tristique volutpat. Aliquam at ex lorem. Aliquam ultricies dui vitae sapien hendrerit ultrices. Fusce quis dictum risus, sed ornare sem. Nullam a porttitor orci. Nulla nec blandit nunc. Aenean mattis tempus consequat. Integer nunc lorem, placerat in congue ac, sollicitudin ac ante. Nullam eleifend egestas nibh a sagittis. Donec posuere, arcu quis volutpat suscipit, quam tortor hendrerit magna, eu suscipit nunc tellus pharetra nisl. Ut vel ante libero. Aliquam at faucibus est, sit amet luctus tellus.
Donec at euismod lectus. Phasellus non nunc quam. Vestibulum bibendum venenatis sem consequat sagittis. Fusce pulvinar risus lectus. Cras eget dignissim urna. Integer ut auctor neque. Integer libero tellus, sagittis id rhoncus ut, malesuada sit amet tortor. Morbi tincidunt semper mauris et tempus. Donec a dui elit. Cras vel laoreet quam. Vestibulum enim ex, auctor vitae sapien sed, facilisis egestas eros. Suspendisse ac magna quis est sollicitudin egestas convallis ac mauris. Suspendisse vestibulum erat vel tortor vulputate, at blandit leo tincidunt. Duis eleifend ut ante vitae convallis. Donec eget odio scelerisque metus semper sollicitudin. Pellentesque vestibulum luctus lacus nec posuere.
";

        public class EEpromPersistedObject
        {
            public int a = 1;
            public double b = 2.0;
            public string text = largeText;
            public DateTime LastModified = DateTime.Now;

            public bool Match(EEpromPersistedObject o)
            {
                return this.a == o.a && this.b == o.b && this.text == o.text && this.LastModified == o.LastModified;
            }
        }

        static void I2CEEPROM_AT24C256_PocoFileSystemSample(I2CDevice i2cDevice)
        {
            System.Console.Clear();
            ConsoleEx.TitleBar(0, "I2C EEPROM AT24C256 - Saving Poco/Json object");
            ConsoleEx.WriteLine($"", ConsoleColor.Cyan);
            var eeprom = new I2CEEPROM_AT24C256(i2cDevice);
            if (eeprom.Begin())
            {
                ConsoleEx.WriteLine($"Writing poco object to EEPROM File System", ConsoleColor.Cyan);
                var pocoFS = new PocoFS(eeprom);
                var poco = new EEpromPersistedObject();
                pocoFS.Save(poco);
                ConsoleEx.WriteLine($"Poco written to EEPROM in {pocoFS.LastExecutionTime}ms", ConsoleColor.Cyan);

                ConsoleEx.WriteLine($"Loading poco from EEPROM File System", ConsoleColor.Cyan);
                var o = pocoFS.Load<EEpromPersistedObject>();
                ConsoleEx.WriteLine($"Poco loaded from EEPROM in {pocoFS.LastExecutionTime}ms", ConsoleColor.Cyan);
                if (!o.Match(poco))
                    throw new InvalidOperationException("Object read from EEPROM does not match");

                ConsoleEx.WriteLine($"Press any key", ConsoleColor.Cyan);
                System.Console.ReadKey();
            }
        }

        static void I2CEEPROM_AT24C256_Sample(I2CDevice i2cDevice)
        {
            byte asciValue = 64;
            var eeprom = new I2CEEPROM_AT24C256(i2cDevice);
            if(eeprom.Begin())
            {
                const bool writeMode = !true;
                if (writeMode)
                {
                    for (var page = 0; page < eeprom.MaxPage; page++)
                    {
                        System.Console.WriteLine($"Writing page:{page}/{eeprom.MaxPage}, {page * eeprom.PageSize} b written");
                        var dataOut = MakeEEPROMTestBuffer(asciValue, eeprom.PageSize); asciValue += 1;
                        var rOut = eeprom.WritePages(page * eeprom.PageSize, dataOut);
                        if (asciValue > 64 + 26)
                            asciValue = 64;
                    }
                }

                // Read the all 32Kb in one operation
                var allDataIn = new List<byte>();
                var max64KbBlock = eeprom.Max64KbBlock;
                var rInAll = eeprom.ReadPages(0, (eeprom.Max64KbBlock + 1) * 64 * 1024, allDataIn);
                var allActualBuffer = PerformanceHelper.AsciiBufferToString(allDataIn.ToArray());
                System.Console.WriteLine($"Reading page:{eeprom.SizeInByte} b written");
                System.Console.WriteLine(allActualBuffer);
                if(allActualBuffer.Length != eeprom.SizeInByte)
                {
                    throw new ApplicationException($"Un expected size expected:{eeprom.SizeInByte}, actual:{allActualBuffer.Length}");
                }

                // Read the all 32Kb page per page
                asciValue = 64;
                for (var page = 0; page < eeprom.MaxPage; page++)
                {
                    var exptectedBuffer = PerformanceHelper.AsciiBufferToString(MakeEEPROMTestBuffer(asciValue, eeprom.PageSize).ToArray()); asciValue += 1;
                    var dataIn = new List<byte>();
                    var rIn = eeprom.ReadPages(page * eeprom.PageSize, eeprom.PageSize*2, dataIn);

                    var actualBuffer = PerformanceHelper.AsciiBufferToString(dataIn.ToArray());
                    if (actualBuffer != exptectedBuffer)
                        throw new ApplicationException($"Un expected page data {page}, actual:{actualBuffer}, expected:{exptectedBuffer}");

                    System.Console.WriteLine($"Reading page:{page}/{eeprom.MaxPage}, {page * eeprom.PageSize} b written");
                    System.Console.WriteLine(actualBuffer);

                    if (asciValue > 64 + 26)
                        asciValue = 64;
                }
            }
        }

        private static List<byte> MakeEEPROMTestBuffer(byte asciValue, int size)
        {
            var buffer = BufferUtils.MakeBuffer(size, asciValue);
            buffer[0] = (byte)'[';
            buffer[buffer.Count-1] = (byte)']';
            return buffer;
        }

        static void OLED_SSD1306_Sample(I2CDevice i2cDevice)
        {
            var oled = new I2C_OLED_SSD1306(i2cDevice, 128 , 32);
            if (oled.Begin())
            {
                oled.Test();
                oled.Clear(refresh: true);
                oled.DrawWindow(" Hello ",  "Fast Speed");
                OledRectangleDemo(oled, 0);
                OledCircleDemo(oled, 0);
                OledCircleFractalDemo(oled, true);
                OledCircleFractalDemo(oled, !true);
                oled.Clear(true);
            }
        }
    }
}

