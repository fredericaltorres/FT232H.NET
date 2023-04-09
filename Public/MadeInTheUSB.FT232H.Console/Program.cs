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
using static MadeInTheUSB.FT232H.SpiConfig;
using MadeInTheUSB.FT232H;
using MadeInTheUSB.Adafruit;

namespace MadeInTheUSB.FT232H.Console
{
    // TODO https://www.youtube.com/watch?v=jgbUCkcGIZ4 NeoPixel
    // https://github.com/kkrizka/adafruitft232h_i2c_adapter adafruitft232h_i2c_adapter
    
    partial class Program
    {
        static void Main(string[] args)
        {
            var ft232Device = FT232HDetector.Detect();
            if (ft232Device.Ok)
            {
                Pause(ft232Device.ToString());
            }
            else
            {
                Pause("Device not detected");
            }

            while (true)
            {
                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                ConsoleEx.WriteMenu(0, 2, "I)2C Demo   2)I2C Multi Device Demo   S)PI Demo");
                ConsoleEx.WriteMenu(0, 3, "S)PI Demo   3)SPI Multi Device Demo");
                ConsoleEx.WriteMenu(0, 4, "Q)uit");

                var k = System.Console.ReadKey(true);
                if (k.Key == ConsoleKey.Q)
                {
                    System.Console.Clear();
                    Environment.Exit(0);
                }
                if (k.Key == ConsoleKey.I)
                {
                    I2CDemo();
                }
                if (k.Key == ConsoleKey.D2)
                {
                    I2CMultiDeviceDemo();
                }
                if (k.Key == ConsoleKey.D3)
                {
                    SPIMultiDeviceDemo();
                }
            }

            /*
            I2C
            */
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz);
            i2cDevice.Gpios.Animate();
            //I2CSample_Adafruit9x16LedMatrixGray(i2cDevice);
            I2CSample_AdaFruit8x8LedMatrix(i2cDevice);

            MCP9808_TemperatureSensor_Sample(i2cDevice);

            return;

            //OLED_SSD1306_Sample(i2cDevice2);
            //MCP9808_TemperatureSensor_Sample(i2cDevice2);


            /*
            const int APDS9960_ADDRESS = (0x39); 
            i2cDevice2.Init(APDS9960_ADDRESS);
            var APDS9960_ID = 0x92;
            i2cDevice2.Write(APDS9960_ID);
            var buffer = new byte[1];
            i2cDevice2.Read(buffer);
            var r = (buffer[0] == 0xAB);
            */
            // I2CSample_AdaFruit8x8LedMatrix(i2cDevice2);
            //i2cDevice.Gpios.DigitalWrite(0, PinState.High);
            //i2cDevice.Gpios.DigitalWrite(i2cDevice.Gpios.MaxGpio-1, PinState.High);
            //I2CSample_AdaFruit8x8LedMatrix(i2cDevice2);
            
            //MCP9808_TemperatureSensor_Sample(i2cDevice2);
            //I2C_Gpios(i2cDevice, i2cDevice.Gpios);

            // APDS_9900_DigitalInfraredGestureSensor_Proximity(i2cDevice);
            // APDS_9900_DigitalInfraredGestureSensor_Color(i2cDevice);
            //APDS_9900_DigitalInfraredGestureSensor_Gesture(i2cDevice);
            //OLED_SSD1306_Sample(i2cDevice2);
            //LiquidCrystal(i2cDevice);
            //PCF8574(i2cDevice);
            //ADS1015_ADC_ADC(i2cDevice2);
            //return;

            // MCP3088 and MAX7219 is limited to 10Mhz
            var clockSpeed = SpiClockSpeeds._10Mhz; // MpsseSpiConfig._30Mhz; // 
            var ft232hGpioSpiDevice = new SpiDevice(clockSpeed);
            ft232hGpioSpiDevice.Log = !true;
            var spi                 = ft232hGpioSpiDevice.SPI;
            var gpios               = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            //GpioSample(gpios, true);
            // CheetahBoosterDemo(gpios, false);

            //ADC_MCP3008Demo(spi, gpios);
            MAX7219_SPI_8x8_Matrix(spi, gpios);

            /*const int fatLinkedListSectorCount = 10;
            const string volumeName = "fDrive.v01";
            var files = new List<string> {
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\README.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\WRITEME.TXT",
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\MASTER.TXT",
                FDriveFAT12FileSystem.BLANK_SECTOR_COMMAND,
                @"C:\DVT\LILYGO T-Display-S3 ESP32-S3\mass storage\Files\VIEWME.JPG",
            };*/
            //FlashMemoryWriteFDriveFileSystem(spi, files, fatLinkedListSectorCount, volumeName, updateFlash: false);
            //FlashMemoryWriteFlashContentToLocalFile(spi);
            //FlashMemorySample(spi);
            //Api102RgbLedSample(spi);

            // GpioSample(gpios, false);
        }


        private static void SPIMultiDeviceDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");

            var clockSpeed = SpiClockSpeeds._10Mhz; // MpsseSpiConfig._30Mhz; // 
            var ft232hGpioSpiDevice = new SpiDevice(clockSpeed, SpiChipSelectPins.CsDbus4);
            ft232hGpioSpiDevice.Log = true;
            var spi = ft232hGpioSpiDevice.SPI;
            var gpios = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "S PI   Multi Device Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            var adc = new MCP3008(spi, SpiChipSelectPins.CsDbus4);
            const double referenceVoltage = 3.3;

            while (true)
            {
                for (var adcPort = 0; adcPort < 1; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    System.Console.WriteLine($"ADC [{adcPort}] = {adcValue}, voltage:{voltageValue}");
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q) return;
                }

                Thread.Sleep(500);
            }
        }

        private static void I2CMultiDeviceDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz, hardwareProgressBarOn: true);
            i2cDevice.Log = true;

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "I 2 C   Multi Device Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            var ledBackPack8x8 = new LEDBackpack(i2cDevice, 8, 8);
            var ledBackPack8x8MaskDefault = (byte)(1 + 4 + 16 + 64);
            var ledBackPack8x8Mask = ledBackPack8x8MaskDefault;
            if (!ledBackPack8x8.Detect(0x71))
            {
                ledBackPack8x8 = null;
            }

            var tempSensor = new MCP9808_TemperatureSensor(i2cDevice);
            if (!tempSensor.Begin())
            {
                tempSensor = null;
            }

            while (true)
            {
                if(tempSensor != null)
                {
                    var FahrenheitTemp = tempSensor.GetTemperature(TemperatureType.Fahrenheit);
                    var celciusTemp = tempSensor.GetTemperature(TemperatureType.Celsius);
                    ConsoleEx.WriteLine($"[{DateTime.Now}] Temp:{FahrenheitTemp:0.00}F /  {celciusTemp:0.00}C", ConsoleColor.White);
                }

                if(ledBackPack8x8 != null)
                {
                    ledBackPack8x8.Clear(value: ledBackPack8x8Mask);
                    ledBackPack8x8.WriteDisplay();
                    ledBackPack8x8Mask = (byte)(ledBackPack8x8Mask << 1);
                    if (ledBackPack8x8Mask == 0)
                        ledBackPack8x8Mask = ledBackPack8x8MaskDefault;
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if(k.Key == ConsoleKey.Q) return;
                }

                Thread.Sleep(500);
            }
        }

        static void I2CDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz, hardwareProgressBarOn: true);

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "I 2 C   D e m o ", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            System.Console.WriteLine("");

            i2cDevice.Gpios.Animate();
            //I2CSample_Adafruit9x16LedMatrixGray(i2cDevice);
            I2CSample_AdaFruit8x8LedMatrix(i2cDevice);
            MCP9808_TemperatureSensor_Sample(i2cDevice);
        }

        private static ConsoleKeyInfo Pause(string message = null)
        {
            if (message != null)
                System.Console.WriteLine(message);
            System.Console.WriteLine("Hit space to continue");
            return System.Console.ReadKey(true);
        }
    }
}

