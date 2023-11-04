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
using BufferUtil;
using System.IO;
using BufferUtil.Lib;
using MadeInTheUSB.Display;
using MadeInTheUSB.FT232H.Component.I2C.EEPROM;
using System.Drawing.Printing;

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
                ConsoleEx.WriteMenu(0, 2, "I)2C Demo   2) I2C Demo   S)PI Demo    4) SPI Extension Demo 5) I2C Extension Demo");
                ConsoleEx.WriteMenu(0, 3, "S)PI Demo   3) SPI Multi Device Demo   G)PIO Demo   ");
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
                if (k.Key == ConsoleKey.D4)
                {
                    SPIExtensionDemo();
                }
                if (k.Key == ConsoleKey.S)
                {
                    SPIDemo();
                }
                if (k.Key == ConsoleKey.G)
                {
                    GPIODemo();
                }
                if (k.Key == ConsoleKey.D2)
                {
                    I2CMultiDeviceDemo();
                }
                if (k.Key == ConsoleKey.D3)
                {
                    SPIMultiDeviceDemo();
                }
                if (k.Key == ConsoleKey.D5)
                {
                    I2CExtensionDemo();
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

        private static void AssertState(PinState expected, PinState actual, string msg)
        {
            var passed = expected == actual;
            if (passed)
                ConsoleEx.WriteLine($"[PASSED] {msg}, expected:{expected}, actual:{actual}", ConsoleColor.Green);
            else
                ConsoleEx.WriteLine($"[FAILED] {msg}, expected:{expected}, actual:{actual}", ConsoleColor.Red);
        }

        private static void GPIODemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");

            var ft232hGpioSpiDevice = new SpiDevice(SpiClockSpeeds._10Mhz);

            Cls();

            ft232hGpioSpiDevice.Log = !true;
            var spi = ft232hGpioSpiDevice.SPI;
            var gpios = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            gpios.SetPinMode(0, PinMode.Output);
            gpios.SetPinMode(1, PinMode.Output);
            gpios.SetPinMode(2, PinMode.Output);
            gpios.SetPinMode(3, PinMode.Output);

            gpios.SetPinMode(7, PinMode.Input);
            gpios.SetPinMode(4, PinMode.Input);
            gpios.SetPinMode(5, PinMode.Input);
            gpios.SetPinMode(6, PinMode.Input);
            

            while (true)
            {
                Cls();

                ConsoleEx.WriteLine(0, 3, $"{DateTime.Now}", ConsoleColor.Cyan);

                var pinState = PinState.Low;
                gpios.DigitalWrite(0, pinState);
                gpios.DigitalWrite(1, pinState);
                gpios.DigitalWrite(2, pinState);
                gpios.DigitalWrite(3, pinState);

                AssertState(pinState, gpios.DigitalRead(4), $"Test input pin {4}");
                AssertState(pinState, gpios.DigitalRead(5), $"Test input pin {5}");
                AssertState(pinState, gpios.DigitalRead(6), $"Test input pin {6}");
                AssertState(pinState, gpios.DigitalRead(7), $"Test input pin {7}");

                Thread.Sleep(1000);

                pinState = PinState.High;
                gpios.DigitalWrite(0, pinState);
                gpios.DigitalWrite(1, pinState);
                gpios.DigitalWrite(2, pinState);
                gpios.DigitalWrite(3, pinState);

                AssertState(pinState, gpios.DigitalRead(4), $"Test input pin {4}");
                AssertState(pinState, gpios.DigitalRead(5), $"Test input pin {5}");
                AssertState(pinState, gpios.DigitalRead(6), $"Test input pin {6}");
                AssertState(pinState, gpios.DigitalRead(7), $"Test input pin {7}");

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q) return;
                }
                Thread.Sleep(1000);
            }

            void Cls()
            {
                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                ConsoleEx.TitleBar(1, "G P I O Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                ConsoleEx.WriteMenu(0, 2, "Q)uit");
                System.Console.WriteLine("");
            }
        }

        public class Panel5Buttons
        {
            private static bool IsInRange(double value, double refValue, int percentage)
            {
                var delta =  (refValue * percentage / 100);
                return value >= (refValue - delta) && value <= (refValue + delta);
            }

            private static Dictionary<int, double> _refValues = new Dictionary<int, double>()
            {
                { 1, 0 }, { 2, 0.47 }, { 3, 0.99 }, { 4, 1.98 }, { 5, 1.42 } // 3.3 volts reference
            };

            public static int GetButtonPressed(double voltage, int percent = 4)
            {
                return _refValues.FirstOrDefault(v => IsInRange(Math.Round(voltage, 2), v.Value, percent)).Key;
            }

            public static void SetI2CExtensionADS1115Values()
            {
                _refValues = new Dictionary<int, double>()
                {
                    { 1, 0 }, { 2, 0.47 }, { 3, 1.07 }, { 4, 1.63 }, { 5, 2.39 } // 3.3 volts reference
                };
            }
        }

        private static void SPIExtensionDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");

            var ft232hGpioSpiDevice = new SpiDevice(SpiClockSpeeds._10Mhz);
            ft232hGpioSpiDevice.Log = !true;
            var spi = ft232hGpioSpiDevice.SPI;
            var gpios = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "S P I Demo - SPI Extension", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            var flash = new FlashMemory(spi, SpiChipSelectPins.CsDbus7);
            flash.ReadIdentification();
            var maxPage = flash.MaxPage;
            var pageBufferCount = 1;
            var bufferSize = pageBufferCount * flash.PageSize;
            var flashBufferAddr = 0;
            var adc = new MCP3008(spi, SpiChipSelectPins.CsDbus6); // On The SPI Extension the CS for the MCP 3008 ois on D6
            const double referenceVoltage = 3.3;

            while (true)
            {
                gpios.ProgressNext();

                ConsoleEx.WriteLine(0, 3, $"{DateTime.Now}", ConsoleColor.Cyan);

                var tmpBuffer = new List<byte>();
                flash.ReadPages(flashBufferAddr, bufferSize, tmpBuffer);
                var bufferRepr = HexaString.ConvertTo(tmpBuffer.ToArray(), max: 4, itemFormat: "{0}, ");
                if (bufferRepr.Contains(", "))
                {
                    ConsoleEx.WriteLine(0, 5, $"FLASH Addr: {flashBufferAddr}, bufferSize: {bufferSize}b, Data: {bufferRepr}", ConsoleColor.Cyan);
                    flashBufferAddr += flash.PageSize;
                }
                
                for (var adcPort = 0; adcPort < 2/*adc.MaxAdConverter*/; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    var adcMessage = $"ADC[{adcPort}]: {adcValue:0000}, voltage:{voltageValue:0.00}";
                    if (adcPort == 0)
                    {
                        var buttonPressed = Panel5Buttons.GetButtonPressed(voltageValue);
                        ConsoleEx.WriteLine(0, 7 + adcPort, $"{adcMessage}, Button Pressed: {buttonPressed}                ", ConsoleColor.Cyan);
                    }
                    else if (adcPort == 1)
                    {
                        var motionSensorState = voltageValue > 0 ? "Detected" : "None";
                        ConsoleEx.WriteLine(0, 7 + adcPort, $"{adcMessage}, Motion Sensor: {motionSensorState}              ", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleEx.WriteLine(0, 7 + adcPort, adcMessage, ConsoleColor.Cyan);
                    }
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q) return;
                }
                Thread.Sleep(500);
            }
        }
        private static void SPIDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");

            var ft232hGpioSpiDevice = new SpiDevice(SpiClockSpeeds._10Mhz);
            ft232hGpioSpiDevice.Log = !true;
            var spi = ft232hGpioSpiDevice.SPI;
            var gpios = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "S P I Demo - SPI Extension", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            var flash = new FlashMemory(spi, SpiChipSelectPins.CsDbus7);
            flash.ReadIdentification();
            var maxPage = flash.MaxPage;
            var pageBufferCount = 256; //  256 * 256 = 65536kb buffer
            var flashPageAddr = 0;
            var adc = new MCP3008(spi, SpiChipSelectPins.CsDbus6); // On The SPI Extension the CS for the MCP 3008 ois on D6
            const double referenceVoltage = 3.3;

            while (true)
            {
                gpios.ProgressNext();

                var tmpBuffer = new List<byte>();
                flash.ReadPages(flashPageAddr * flash.PageSize, pageBufferCount * flash.PageSize, tmpBuffer);
                var bufferRepr = HexaString.ConvertTo(tmpBuffer.ToArray(), max: 32, itemFormat: "{0}, ");
                if (bufferRepr.StartsWith("41, 41, 41, "))
                {
                    ConsoleEx.WriteLine(0, 3, $"Page 0 read from Flash OK", ConsoleColor.Cyan);
                }

                ConsoleEx.WriteLine(0, 5, $"{DateTime.Now}", ConsoleColor.Cyan);
                for (var adcPort = 0; adcPort < 3/*adc.MaxAdConverter*/; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    var adcMessage = $"ADC [{adcPort}] = {adcValue:0000}, voltage:{voltageValue:0.00}";
                    if (adcPort == 0)
                    {
                        var buttonPressed = Panel5Buttons.GetButtonPressed(voltageValue);
                        ConsoleEx.WriteLine(0, 6 + adcPort, $"Button Pressed: {buttonPressed}, {adcMessage}", ConsoleColor.Cyan);
                    }
                    else if (adcPort == 1)
                    {
                        var motionSensorState = voltageValue > 0 ? "Detected" : "";
                        ConsoleEx.WriteLine(0, 6 + adcPort, $"Motion Sensor: {motionSensorState}, {adcMessage}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleEx.WriteLine(0, 6 + adcPort, adcMessage, ConsoleColor.Cyan);
                    }
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q) return;
                }
                Thread.Sleep(500);
            }
        }

        private static void SPIMultiDeviceDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");

            var ft232hGpioSpiDevice = new SpiDevice(SpiClockSpeeds._10Mhz);
            ft232hGpioSpiDevice.Log = !true;
            var spi = ft232hGpioSpiDevice.SPI;
            var gpios = ft232hGpioSpiDevice.GPIO;
            gpios.Animate();

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "S P I   Multi Device Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            var flash = new FlashMemory(spi, SpiChipSelectPins.CsDbus3);
            flash.ReadIdentification();
            var maxPage = flash.MaxPage;
            var pageBufferCount = 256; //  256 * 256 = 65536kb buffer
            var flashPageAddr = 0;
            var adc = new MCP3008(spi, SpiChipSelectPins.CsDbus7);
            const double referenceVoltage = 3.3;

            while (true)
            {
                gpios.ProgressNext();

                for (var adcPort = 0; adcPort < 3; adcPort++)
                {
                    var adcValue = adc.Read(adcPort);
                    var voltageValue = adc.ComputeVoltage(referenceVoltage, adcValue);
                    ConsoleEx.WriteLine(0, 4 + adcPort, $"ADC [{adcPort}] = {adcValue:0000}, voltage:{voltageValue:0.00}{Environment.NewLine}", ConsoleColor.Cyan);
                }

                var tmpBuffer = new List<byte>();
                flash.ReadPages(flashPageAddr * flash.PageSize, pageBufferCount * flash.PageSize, tmpBuffer);
                var bufferRepr = HexaString.ConvertTo(tmpBuffer.ToArray(), max: 32, itemFormat:"{0}, ");
                
                ConsoleEx.WriteLine(0, 9, $"FLASH {flash.GetInformation()}", ConsoleColor.Gray);
                ConsoleEx.WriteLine(0,10, $"FLASH Page:{flashPageAddr}, Size: {tmpBuffer.Count / 1024} Kb, Buffer:{bufferRepr}{Environment.NewLine}", ConsoleColor.Yellow);

                flashPageAddr += flash.PageSize;
                if (flashPageAddr > (64 * 1024 * 10))
                    flashPageAddr = 0;

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q) return;
                }

                Thread.Sleep(750);
            }
        }

        private static bool _MultiLEDBackpackManagerDemoOn = true;

        private static void MultiLEDBackpackManagerDemo(MultiLEDBackpackManager _multiLEDBackpackManager)
        {
            var wait = 0;
            
            var yy = 0;

            if (_MultiLEDBackpackManagerDemoOn)
            {
                _multiLEDBackpackManager.Clear(true);
                while (yy <= 3)
                {
                    _multiLEDBackpackManager.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 1);
                    _multiLEDBackpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    yy += 1;
                }
                Thread.Sleep(wait);
            }
            else
            {
                yy = 2;
                while (yy >= 0)
                {
                    _multiLEDBackpackManager.DrawRoundRect(yy, yy, 8 - (yy * 2), 8 - (yy * 2), 2, 0);
                    _multiLEDBackpackManager.WriteDisplay();
                    Thread.Sleep(wait);
                    yy -= 1;
                }
                _multiLEDBackpackManager.Clear(true);
                Thread.Sleep(wait);
            }
            _MultiLEDBackpackManagerDemoOn = !_MultiLEDBackpackManagerDemoOn;
        }

        private static void I2CExtensionDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detect/initialize Nusbio/2 (FT232H)");
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz, hardwareProgressBarOn: true, fastMode: true);
            i2cDevice.Log = true;

            System.Console.WriteLine("Detect/initialize EEPROM AT24C256 32Kb");
            var eeprom = new I2CEEPROM_AT24C256(i2cDevice);
            var eepromPage = 0;
            var eepromChar = ' ';
            if (!eeprom.Begin())
                eeprom = null;

            System.Console.WriteLine("Detect/initialize OLED Device 128x32");
            var oled = new I2C_OLED_SSD1306(i2cDevice, 128, 32);
            if (!oled.Begin())
                oled = null;

            System.Console.WriteLine("Detect/initialize ADC ADS1x15");
            var adc = new ADS1X15_ADC(i2cDevice, ADS1X15_ADC.ADS1x15_Type.ADS1115_16b);
            adc.Gain = ADS1X15_ADC.adsGain_t.GAIN_ONE__4_096V;
            if (!adc.Begin())
                adc = null;
            Thread.Sleep(1111);

            Panel5Buttons.SetI2CExtensionADS1115Values();

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "I 2 C  Extension Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");
            
            while (true)
            {
                i2cDevice.ForceWriteLogCache();
                var lcdInfo = $"";

                if (eeprom != null)
                {
                    var eepromBufferIn = new List<byte>();
                    var rIn = eeprom.ReadPages(eepromPage * eeprom.PageSize, eeprom.PageSize, eepromBufferIn);
                    var actualBuffer = PerformanceHelper.AsciiBufferToString(eepromBufferIn.ToArray());
                    eepromChar = actualBuffer[1];
                    ConsoleEx.WriteLine(0, 5, $"[{DateTime.Now}] EEPROM Page:{eepromPage}", ConsoleColor.White);
                    ConsoleEx.WriteLine(0, 6, $"{actualBuffer}", ConsoleColor.Gray);
                    eepromPage += 1;
                    if (eepromPage > eeprom.MaxPage)
                        eepromPage = 0;
                }

                if (adc != null)
                {
                    for (var adcIndex = 0; adcIndex < 2; adcIndex++)
                    {
                        var v = adc.readADC_SingleEnded(adcIndex);
                        var volt = adc.ComputeVolts(v);
                        
                        if(adcIndex == 0)
                        {
                            var buttonPressed = Panel5Buttons.GetButtonPressed(volt, 15);
                            ConsoleEx.WriteLine(0, 8 + adcIndex, $"[{DateTime.Now}] ADC[{adcIndex}] voltagle:{volt:0.00}, {v:000000}, buttonPressed:{buttonPressed:00}", ConsoleColor.White);
                        }
                        else
                        {
                            ConsoleEx.WriteLine(0, 8 + adcIndex, $"[{DateTime.Now}] ADC[{adcIndex}] voltagle:{volt:0.00}, {v:000000}", ConsoleColor.White);
                        }
                    }
                }

                if (oled != null)
                {
                    oled.DrawWindow(" EEPROM ", $"Page: {eepromPage}, Char: {eepromChar}");
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q)
                    {
                        oled.Clear(true);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        private static void I2CMultiDeviceDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detect/initialize Nusbio/2 (FT232H)");
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz, hardwareProgressBarOn: true, fastMode: true);
            i2cDevice.Log = true;

            System.Console.WriteLine("Detect/initialize 8x8 Matrix Device");
            var ledBackPackManager = new MultiLEDBackpackManager();
            ledBackPackManager.Add(i2cDevice, 8, 8, 0x70);
            ledBackPackManager.SetBrightness(1);

            System.Console.WriteLine("Detect/initialize temperature sensor MCP9808 Device");
            var tempSensor = new MCP9808_TemperatureSensor(i2cDevice);
            if (!tempSensor.Begin())
                tempSensor = null;

            System.Console.WriteLine("Detect/initialize EEPROM AT24C256 32Kb");
            var eeprom = new I2CEEPROM_AT24C256(i2cDevice);
            var eepromPage = 0;
            if (!eeprom.Begin())
                eeprom = null;

            System.Console.WriteLine("Detect/initialize OLED Device 128x32");
            var oled = new I2C_OLED_SSD1306(i2cDevice, 128, 32);
            if (!oled.Begin())
                oled = null;

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "I 2 C   Multi Device Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.WriteMenu(0, 2, "Q)uit");
            System.Console.WriteLine("");

            while (true)
            {
                i2cDevice.ForceWriteLogCache();                
                var tempInfoString = "No temperature info";
                if (tempSensor != null)
                {
                    var FahrenheitTemp = tempSensor.GetTemperature(TemperatureType.Fahrenheit, resetTime: 0);
                    var celciusTemp = tempSensor.GetTemperature(TemperatureType.Celsius, resetTime:0);
                    tempInfoString = $"{FahrenheitTemp:0.00}F / {celciusTemp:0.00}C";
                    ConsoleEx.WriteLine($"[{DateTime.Now}] Temp:{tempInfoString}", ConsoleColor.White);
                    tempInfoString = $"{FahrenheitTemp:0.00}F";
                }

                if(eeprom != null)
                {
                    var eepromBufferIn = new List<byte>();
                    var rIn = eeprom.ReadPages(eepromPage * eeprom.PageSize, eeprom.PageSize, eepromBufferIn);
                    var actualBuffer = PerformanceHelper.AsciiBufferToString(eepromBufferIn.ToArray());
                    ConsoleEx.WriteLine($"[{DateTime.Now}] EEPROM Page:{eepromPage}", ConsoleColor.White);
                    ConsoleEx.WriteLine($"{actualBuffer}", ConsoleColor.Gray);
                    eepromPage += 1;
                    if (eepromPage > eeprom.MaxPage)
                        eepromPage = 0;
                }

                if (oled != null)
                {
                    oled.DrawWindow(" OLED ", $"{tempInfoString}, EEPROM:{eepromPage}");
                }

                if (ledBackPackManager != null)
                {
                    MultiLEDBackpackManagerDemo(ledBackPackManager);
                }

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Q)
                    {
                        oled.Clear(true);
                        return;
                    }
                }
            }
        }

        static void I2CDemo()
        {
            System.Console.Clear();
            System.Console.WriteLine("Detecting/Initializing device");
            var i2cDevice = new I2CDevice(I2CClockSpeeds.FAST_MODE_1_Mhz, hardwareProgressBarOn: true);
            i2cDevice.Log = true;
            

            System.Console.Clear();
            ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(1, "I 2 C   D e m o ", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            System.Console.WriteLine("");

            i2cDevice.Gpios.Animate();
             ADS1115_ADC_ADC(i2cDevice);
            //I2CEEPROM_AT24C256_Sample(i2cDevice);
            //I2C_Temperature_To_EEPROM_AT24C256_PocoFileSystemSample(i2cDevice);
            //OLED_SSD1306_Sample(i2cDevice);
            //I2CSample_Adafruit9x16LedMatrixGray(i2cDevice);
            //I2CSample_AdaFruit8x8LedMatrix(i2cDevice);
            //MCP9808_TemperatureSensor_Sample(i2cDevice);
            // APDS_9900_DigitalInfraredGestureSensor_Gesture(i2cDevice);
            //APDS_9900_DigitalInfraredGestureSensor_Proximity(i2cDevice);
            //Gpio_Output_InputMode(i2cDevice);
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

