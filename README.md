# MadeInTheUSB FT232H NET Library

The .NET/Windows library MadeInTheUSB.FT232H.NET provides an abstraction to program
* The SPI protocol
* The I2C protocol (Not yet supported)
* The GPIOs

for the FTDI chip FT232H using the ([Adafruit Breakout FT232H](https://www.adafruit.com/product/2264))

# External components supported or Chip

* RGB LED strip of type `APA102` are supported with examples
* 8x8, 32x8 and 64x8 LED matrix based on the `MAX7219` chip are supported with examples
* Any EPPROM and NOR and NAND Flash memory using the SPI protocol
* ADC MCP3008 and MCP3004 are supported with examples

![64 x 8 matrix](https://raw.githubusercontent.com/fredericaltorres/FT232H.NET/main/photos/Nusbio2_FT232H_SPI_4_MAX7219_Chained_8x8LedMatrix.jpg)

## Samples

### GPIOs

```csharp
static void GpioSample(IDigitalWriteRead gpios, bool oneLoopOnly = false)
{
	var waitTime = 100;
	for(var i=0; i < gpios.MaxGpio; i++)
	{
		gpios.DigitalWrite(i, PinState.High);
	}
	Thread.Sleep(waitTime);
	for(var i=0; i < gpios.MaxGpio; i++)
	{
		gpios.DigitalWrite(i, PinState.Low);
	}
	Thread.Sleep(waitTime);
}

static void Main(string[] args)
{
	var ft232Device = FT232HDetector.Detect();
	if(ft232Device.Ok)
		System.Console.WriteLine(ft232Device.ToString());

	var ft232hGpioSpiDevice = new GpioSpiDevice(MpsseSpiConfig.GetDefault());
	var gpios               = ft232hGpioSpiDevice.GPIO;
	GpioSample(gpios, true);
}
```

### SPI

```csharp
static void CypressFlashMemorySample(ISPI spi)
{
	const int EEPROM_READ_IDENTIFICATION = 0x9F;
	byte [] buffer = new byte [18];

	if(spi.Ok(spi.Query(new byte [] { EEPROM_READ_IDENTIFICATION },  buffer))) {

		var manufacturer       = (Manufacturers)buffer[0];
		var deviceID           = (CYPRESS_S25FLXXX_DEVICE_ID)((buffer[1] << 8) + buffer[2]);
		var sectorArchitecture = (CYPRESS_SECTOR_ARCHITECTURE)buffer[4];
		var familyID           = (CYPRESS_FAMILIY_ID)buffer[5];
		var packageModel       = string.Empty;
		packageModel          += ((char)buffer[6]).ToString();
		packageModel          += ((char)buffer[7]).ToString();

		System.Console.WriteLine($"FLASH Memory manufacturer:{manufacturer}, deviceID:{deviceID}, sectorArchitecture:{sectorArchitecture}, familyID:{familyID}, packageModel:{packageModel}");
	}
}

static void Main(string[] args)
{
	var ft232Device = FT232HDetector.Detect();
	if(ft232Device.Ok)
		System.Console.WriteLine(ft232Device.ToString());

	var ft232hGpioSpiDevice = new GpioSpiDevice(MpsseSpiConfig.GetDefault());
	var spi = ft232hGpioSpiDevice.SPI;

	CypressFlashMemorySample(spi);
}
```
 
## Breakouts available
 
 * The Adafruit breakout: 
 [Adafruit FT232H Breakout](https://www.adafruit.com/product/2264) General Purpose USB to GPIO+SPI+I2C
- This breakout does contains an EEPROM therefore it is possible to program the device id or description.
- This breakout does contains a switch to switch the device in I2C mode and a Stemma QT I2C adapter
- This breakout IO are 3.3V
- This break out is only $3 more expensive than the Chinese one.

	- Images

	![FT232H with 64x8 LED Matrix](https://raw.githubusercontent.com/fredericaltorres/FT232H.NET/main/FT232H_64x8%20LED%20Matrix.jpg)
	![Adafruit FT232 with 4x4 matrix](https://raw.githubusercontent.com/fredericaltorres/FT232H.NET/main/photos/Nusbio2_FT232H_SPI_EEPROM_25AA256_00.jpg)
	![Programming in Visual Studio](https://raw.githubusercontent.com/fredericaltorres/FT232H.NET/main/photos/Nusbio2_FT232H_SPI_EEPROM_25AA256_02_VisualStudio.jpg)


 * [Chinese/eBay FT232H breakout](https://www.ebay.com/sch/i.html?_from=R40&_trksid=p2380057.m570.l1313.TR12.TRC2.A0.H0.Xft232H.TRS0&_nkw=ft232H&_sacat=0)
 	- This breakout does ***not*** contains an EEPROM therefore it is ***not*** possible to program the device id or description.
	- SPI and GPIOs are working fine.
	- Images
		* Chinese FT232H Breakout with 8 GPIOs and 1 APA 102 RGB LED connected -> https://bit.ly/2RhpmZA
 
 * [Chinese/eBay FT 2 232H breakout](https://www.ebay.com/itm/1-pcs-USB-to-UART-FIFO-SPI-I2C-JTAG-RS232-module-FT2232HL-D1L2/253767822756?epid=14010988565&hash=item3b15bdada4)
	- I never tested this breakout, but seems interesting

 ## References Links

- [FT232H Datasheet](https://www.ftdichip.com/Support/Documents/DataSheets/ICs/DS_FT232H.pdf)
- [LibMPSSE](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI.htm)
- [LibMPSSE-SPI library and examples](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/LibMPSSE-SPI.zip)
- [LibMPSSE - Release notes](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/ReleaseNotes-SPI.txt)

- [FTDI Program Guides](https://www.ftdichip.com/Support/Documents/ProgramGuides.htm)
- [Application Note AN_177 User Guide For libMPSSE – I2C](https://www.ftdichip.com/Support/Documents/AppNotes/AN_177_User_Guide_For_LibMPSSE-I2C.pdf)

- [Speaking SPI & I2C With The FT-2232](http://www.devttys0.com/2011/11/speaking-spi-i2c-with-the-ft-2232/)

## SPI, I2C, GPIO Wiring

 * SPI
	- CLOCK AD0
	- MOSI  AD1
	- MISO  AD2
	- CS    5 Chip selects are available. CS0:AD3, CS1:AD4, CS2:AD5, CS3:AD6, CS4:AD7.
			The library is configured to use CS0:AD3 as the default.

 * I2C
	- CLOCK   AD0
	- SDA OUT AD1
	- SDA IN  AD2
	- SDA OUT and SDA IN need to be connected because in I2C there is only one data write.
	- The data and clock wire each requires a pull up resistor (Not sure what value probably 4.7k).

 * GPIOS
	- GPIO 0..7: AC0..C7. 
	- AC8, AC9 are special and not supported yet by the lirbary
	
 ## .NET Compilation

 * x64 : This code must be compiled in 64 bit mode

 ## Dll dependency and drivers

* The dll FTD2XX.DLL, must be in the path. The dll should be installed by the FTDI driver.
 The driver should automatically be installed by Windows 10 on the first time the FT232H or FT232RL is connected
  to the machine. For Windows 7 install the driver manually.

* This library contains the source code of the .NET wrapper for the dll FTD2XX.DLL.
The file is called FTD2XX_NET.cs. This is the last version from FTDT as 2018, that support the FT4222H.

* The dll 
[libMPSSE.dll ](https://www.ftdichip.com/Support/SoftwareExamples/MPSSE/LibMPSSE-SPI/LibMPSSE-SPI.zip)
from FTDT must be in the current folder. It is part of the source code.
