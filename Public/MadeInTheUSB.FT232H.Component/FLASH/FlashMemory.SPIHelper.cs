using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.FT232H.Components
{
    /// <summary>
    /// GOOD PDF ABOUT (MPSSE) Mhttp://www.ftdichip.com/Support/Documents/AppNotes/AN_135_MPSSE_Basics.pdf
    /// </summary>
    public partial class FlashMemory // : GpioSpiDeviceBaseClass
    {
        // Arduino Uno SPI Pin -- NUSBIO
        //  CS      10             3
        //  MOSI    11             1
        //  MISO    12             2 
        //  CLOCK   13             0

        // EEPROM_25AA256
        // CS      [] [] VCC
        // MISO/SO [] [] HOLD (Set HIGH while CLOCK is low)
        // WP(HIGH)[] [] SCK
        // GND     [] [] MOSI

        // D0 - Clock signal output.  This line can be configured as a clock that runs at speeds between ~450Hz to 30Mhz.
        // D1 - Serial data output.  This is for outputting a serial signal, like the MOSI line in a SPI connection.
        // D2 - Serial data input.  This is for reading a serial signal, like the MISO line in a SPI connection.
        // D3 - Serial select signal.  This is a chip select or chip enable signal to tell a connected device that the FT232H is ready to talk to it.

        private bool SPISend(List<byte> buffer)
        {
            return this._spi.Write(buffer.ToArray()) == FtdiMpsseSPIResult.Ok;
        }

        private bool SPIQuery(byte api, int answerLen, List<byte> buffer)
        {
            this.Trace($"SPIQuery api:{api}");

            var spiBufferWrite = new byte[1];
            spiBufferWrite[0] = api;
            var tmpBuffer = this.GetEepromApiDataBuffer(answerLen);

            if(this._spi.Query(spiBufferWrite, tmpBuffer) == FtdiMpsseSPIResult.Ok)
            {
                buffer.Clear();
                buffer.AddRange(tmpBuffer);
                return true;
            }
            return false;
        }

        public byte[] GenerateBuffer(byte command, int addr)
        {
            if (this.AddressSize == FLASH_ADDR_SIZE._3_Bytes)
                return new byte[] { command, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
            else
                return new byte[] { command, (byte)(addr >> 8), (byte)(addr & 0xFF) };
        }

        public byte[] GetEepromApiReadBuffer(int addr)
        {
            return GenerateBuffer(EEPROM_READ, addr);
            //if (this.AddressSize == FLASH_ADDR_SIZE._3_Bytes)
            //    return new byte[] { EEPROM_READ, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
            //else
            //    return new byte[] { EEPROM_READ, (byte)(addr >> 8), (byte)(addr & 0xFF) };
        }

        public byte[] GetEepromApiWriteBuffer(int addr, List<byte> data = null)
        {
            List<byte> buffer = null;

            buffer = GenerateBuffer(EEPROM_WRITE_PAGE_PROGRAM_PP, addr).ToList();

            //if (this.AddressSize == FLASH_ADDR_SIZE._3_Bytes)
            //    buffer = new List<byte>() { EEPROM_WRITE_PAGE_PROGRAM_PP, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
            //else
            //    buffer = new List<byte>() { EEPROM_WRITE_PAGE_PROGRAM_PP,  (byte)(addr >> 8), (byte)(addr & 0xFF) };

            if (data != null)
                buffer.AddRange(data);

            return buffer.ToArray();
        }

        private byte[] GetEepromApiDataBuffer(int count)
        {
            return new byte[count]; // Buffer contains 0. Value does not matter. all we need is to send some clock to the slave to read the value
        }

        private bool SendCommand(byte cmd)
        {
            var spiBufferWrite = new byte[] { cmd };
            return this._spi.Write(spiBufferWrite) == FtdiMpsseSPIResult.Ok;
        }
    }
}
