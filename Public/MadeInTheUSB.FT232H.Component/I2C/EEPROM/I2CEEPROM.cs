using MadeInTheUSB.FT232H.Components;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MadeInTheUSB.FT232H.Component.I2C.EEPROM
{

    // AT24C256 - 32kb - 1Mhz - 64B Page Write Mode
    // http://ww1.microchip.com/downloads/en/devicedoc/atmel-8568-seeprom-at24c256c-datasheet.pdf
    public class I2CEEPROM_AT24C256 : IFlashEepromInterface
    {
        private I2CDevice _i2cDevice { get; }
        public byte DeviceId { get; set; }

        public const byte AT24C256_DEVICE_ID = 0x50; // 80 A0=GND A1= GND A2=GND == 0x50 == 80 == 01010000 - 64 32 16 8 4-A2 2-A1 1-A0

        public I2CEEPROM_AT24C256(I2CDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        public int PageSize => 64;
        public int SizeInByte => 32 * 1024;
        public int SizeInKByte => this.SizeInByte / 1024;
        public int SizeInMByte => this.SizeInByte / 1024 / 1024;
        public int MaxPage => this.SizeInByte / (int)this.PageSize;
        public int Max64KbBlock => this.SizeInByte / FlashMemory._64K_BLOCK_SIZE;

        public bool Begin(byte deviceId = AT24C256_DEVICE_ID)
        {
            this.DeviceId = deviceId;
            if (this._i2cDevice.DetectDevice(deviceId))
            {
                var eepromBufferIn = new List<byte>();
                var r = this.ReadPages(0, this.PageSize, eepromBufferIn);
                this._i2cDevice.RegisterDeviceIdForLogging((byte)this.DeviceId, this.GetType());
                return r;
            }
            else return false;
        }

        public bool WritePages(int addr, List<byte> buffer, bool verify = false, bool eraseBlock = true)
        {
            var blockCountToWrite = (buffer.Count / this.PageSize) + 1;
            var r = false;

            for(var page = 0; page < blockCountToWrite; page ++)
            {
                var tmpBuffer = buffer.Skip(page * this.PageSize).Take(this.PageSize).ToList();
                if(tmpBuffer.Count > 0)
                {
                    r = WritePage(addr + (page * this.PageSize), tmpBuffer);
                }
            }

            return r;
        }

        private bool WritePage(int addr, List<byte> buffer)
        {
            bool r;
            var buffer2 = (new byte[] { (byte)(addr >> 8), (byte)(addr & 0xFF) }).ToList();
            buffer2.AddRange(buffer);

            r = this._i2cDevice.WriteBuffer(buffer2.ToArray(), this.DeviceId);
            Thread.Sleep(10); // Unfortunately

            return r;
        }

        // https://github.com/adamjezek98/Eeprom_at24c256/blob/master/Eeprom_at24c256.cpp
        public bool ReadPages(int addr, int byteToRead, List<byte> buffer)
        {
            if (byteToRead > this.SizeInByte)
                byteToRead = this.SizeInByte;

            var outputBuffer = (new byte[] { (byte)(addr >> 8), (byte)(addr & 0xFF) }).ToList();
            outputBuffer.AddRange(buffer);

            if (this._i2cDevice.WriteBuffer(outputBuffer.ToArray(), this.DeviceId, terminateTransmission: true))
            {
                buffer.AddRange(this._i2cDevice.ReadXByte(byteToRead, this.DeviceId));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
