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
                System.Console.WriteLine(ft232Device.ToString());
                foreach (var p in ft232Device.Properties) System.Console.WriteLine($"{p.Key}: {p.Value}");
            }
            

            var i2cDevice2 = new I2CDevice(I2CClockSpeeds.I2C_CLOCK_FAST_MODE_1_Mhz);
            i2cDevice2.Gpios.Animate();

            I2CSample_AdaFruit8x8LedMatrix(i2cDevice2);
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

            //i2cDevice.Gpios.DigitalWrite(0, PinState.High);
            //i2cDevice.Gpios.DigitalWrite(i2cDevice.Gpios.MaxGpio-1, PinState.High);
            //I2CSample_AdaFruit8x8LedMatrix(i2cDevice2);
            //I2CSample_Adafruit9x16LedMatrixGray(i2cDevice2);
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
            var clockSpeed = SpiClockSpeeds._2Mhz ; // MpsseSpiConfig._30Mhz; // 
            var ft232hGpioSpiDevice = new SpiDevice(clockSpeed);
            ft232hGpioSpiDevice.Log = !true;
            var spi                 = ft232hGpioSpiDevice.SPI;
            var gpios               = ft232hGpioSpiDevice.GPIO;

            //GpioSample(gpios, true);
            // CheetahBoosterDemo(gpios, false);

            //ADC_MCP3008Demo(spi, gpios);
            //MAX7219_SPI_8x8_Matrix(spi, gpios);

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
    }
}

