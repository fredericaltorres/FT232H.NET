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
    public partial class FlashMemory //: GpioSpiDeviceBaseClass
    {
        public int SizeInByte;
        public int SizeInKByte => this.SizeInKByte / 1024;
        public int SizeInMByte => this.SizeInKByte / 1024 / 1024;

        

        //const int READ_GPIO     = 0;
        //const int WRITE_GPIO    = 1;

        public bool TraceOn { get; set; } = false;

        private readonly ISPI _spi;

   

        public int MaxPage
        {
            get { return this.SizeInByte / (int)this.PageSize; }
        }

        public int MaxBlock
        {
            get { return this.SizeInByte / FlashMemory.MAX_BLOCK_SIZE; }
        }

        private void Trace(string m)
        {
            if(this.TraceOn)
                Debug.WriteLine(m);
        }
        
        public FlashMemory(ISPI spi) 
        {
            this._spi = spi;
        }
                
        public StatusRegister1Enum ReadRegister1Enum()
        {
            return (StatusRegister1Enum)ReadRegister1();
        }

        public int ReadRegister1()
        {
            var buffer = new List<byte>();
            if (this.SPIQuery(FLASH_COMMAND.READ_STATUS_REGISTER_RDSR, 1, buffer))
                return buffer[0];
            else
                return -1;
        }

        public StatusRegister2Enum ReadRegister2Enum()
        {
            return (StatusRegister2Enum)ReadRegister2();
        }

        public int ReadRegister2()
        {
            var buffer = new List<byte>();
            if (this.SPIQuery(FLASH_COMMAND.READ_STATUS_REGISTER_2, 1, buffer))
                return buffer[0];
            else
                return -1;
        }
        
        private int _ProgramWritePageSize = -1;
        public int GetProgramWritePageSize()
        {
            if (_ProgramWritePageSize == -1)
            {
                var r = ReadRegister2Enum();
                if ((r & StatusRegister2Enum.PageBufferWrap512Or256) == StatusRegister2Enum.PageBufferWrap512Or256)
                    _ProgramWritePageSize = EEPROM_DEFAULT_PAGE_SIZE_WRITE;
            }
            return _ProgramWritePageSize;
        }

        public bool Busy()
        {
            return (this.ReadRegister1() & (int)StatusRegister1Enum.Busy) == (int)StatusRegister1Enum.Busy;
        }

        private void WaitForOperation(int wait = 10, int minimumWait = 10, string t = "~")
        {
            Thread.Sleep(minimumWait);
            if (!this.Busy())
                return;

            var tryCounter = 0;
            Thread.Sleep(wait);
            while (true)
            {
                if (!this.Busy()) return;
                Thread.Sleep(wait);
                if (tryCounter++ >= MAX_TRY)
                {
                    Console.Write("minimumWait:{0} is not enough ", minimumWait);
                    throw new ApplicationException("Waiting for operation timeout");
                }
                Console.Write(t);
            }
        }

        public bool ReadIdentification(FLASH_DEVICE_ID deviceId = FLASH_DEVICE_ID.Undefined)
        {
            if (deviceId != FLASH_DEVICE_ID.Undefined)
            {
                this.DeviceID = FLASH_DEVICE_ID.EEPROM_25AA1024_128Kb;
                this.PageSize = 256;
                this.AddressSize = FLASH_ADDR_SIZE.ThreeBytes;
                this.SizeInByte = 1024 * 1024 / 8;
                this.Manufacturer = Manufacturers.Microchip;
            }
            else
            {
                var buffer = new List<byte>();
                if (this.SPIQuery(FLASH_COMMAND.READ_IDENTIFICATION, 18, buffer))
                {
                    this.Manufacturer = (Manufacturers)buffer[0];
                    var deviceIdValue = (buffer[1] << 8) + buffer[2];
                    this.DeviceID = (FLASH_DEVICE_ID)deviceIdValue;

                    if (this.Manufacturer == Manufacturers.Cypress)
                    {
                        this.SectorArchitecture = (CYPRESS_SECTOR_ARCHITECTURE)buffer[4];
                        this.FamilyID = (CYPRESS_FAMILIY_ID)buffer[5];
                        this.PackageModel = string.Empty;
                        this.PackageModel += ((char)buffer[6]).ToString();
                        this.PackageModel += ((char)buffer[7]).ToString();
                    }
                    else if (this.Manufacturer == Manufacturers.Winbond)
                    {
                        // Winbond specific
                        var spiBufferWrite = this.GenerateBuffer(FLASH_COMMAND.WINBOND_GET_INFO, 0);
                        var spiBufferRead = GetEepromApiDataBuffer(2);
                        if (this._spi.Query(spiBufferWrite, spiBufferRead) == FtdiMpsseSPIResult.Ok)
                        {
                            var isWinbond = (Manufacturers)(spiBufferRead[0]) == Manufacturers.Winbond;
                            var winBondDeviceId = (WINBOND_FLASH_DEVICE_ID)spiBufferRead[1];
                        }
                    }

                    switch (this.DeviceID)
                    {
                        case FLASH_DEVICE_ID.WINBOND_25Q128JV_IN_IQ_JQ_16MB:
                            this.SizeInByte = 16 * 1024 * 1024;
                            this.PageSize = 256;
                            break;
                        case FLASH_DEVICE_ID.CYPRESS_S25FL127S_16MB:
                            this.SizeInByte = 16 * 1024 * 1024;
                            break;
                    }
                
                }
            }
            return this.SizeInByte > 0;
        }

        public int GetDeviceSizeInMb()
        {
            return SizeInByte / 1024 / 1024;
        }

        public int GetDeviceSizeInKb()
        {
            return SizeInByte / 1024;
        }

        public string GetDeviceInfo()
        {
            var sizeUnit = "Mb";
            var size = this.GetDeviceSizeInMb();
            if (size == 0) {
                size = this.GetDeviceSizeInKb();
                sizeUnit = "Kb";
            }

            return $@"DeviceID:{this.DeviceID}, Size:{size} {sizeUnit}, Manufacturer:{this.Manufacturer}, MaxPage:{this.MaxPage}, Page Size:{this.PageSize}, MaxBlock:{this.MaxBlock}, BlockSize:{FlashMemory.MAX_BLOCK_SIZE}";
        }

        private bool IsWriteRegisterEnable() 
        {
            var rr0 = this.ReadRegister1Enum();
            return (rr0 & StatusRegister1Enum.WriteEnableLatch) == StatusRegister1Enum.WriteEnableLatch;
        }

        private bool SetWriteRegisterEnable(bool checkStatus = true)
        {
            var result = false;

            if (!this.SendCommand(FLASH_COMMAND.WRITE_ENABLE_LATCH_WREN))
                return result;

            this.WaitForOperation(5, 3, "w");
            if (checkStatus)
            {
                if (this.IsWriteRegisterEnable())
                    result = true;
                else
                    if(Debugger.IsAttached) Debugger.Break();
            }
            else
                result = true;

            return result;
        }

        private bool SetWriteRegisterDisable()
        {
            return this.SendCommand(FLASH_COMMAND.RESET_WRITE_ENABLE_LATCH_WRDI);
        }

        /// <summary>
        /// Write the buffer starting at address addr. The buffer must contains a multiple of PAGE_SIZE (512).
        /// </summary>
        /// <param name="addr">Start address</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="setUserHardwareInterfaceState">If true notify UI of write operation, not implemented</param>
        /// <param name="verify">If true verify the data written</param>
        /// <param name="eraseBlock">If true format the 64k starting at addr</param>
        /// <returns></returns>
        public bool WritePages(int addr, List<byte> buffer, bool verify = false, bool eraseBlock = true)
        {
            if(eraseBlock)
            {
                if(buffer.Count == 64 * 1024)
                { 
                    if (!this.ErasePage(addr, ERASE_BLOCK_SIZE.BLOCK_64K)) return false;
                }
                else if (buffer.Count == 32 * 1024)
                {
                    if (!this.ErasePage(addr, ERASE_BLOCK_SIZE.BLOCK_32K)) return false;
                }
                else if (buffer.Count == 4 * 1024)
                {
                    if (!this.ErasePage(addr, ERASE_BLOCK_SIZE.SECTOR_ERASE)) return false;
                }
                else if (buffer.Count == this.PageSize)
                {
                    // If the address is at a starting 4k block we have to call erase
                    if(addr % (4 * 1024) == 0)
                    {
                        if (!this.ErasePage(addr, ERASE_BLOCK_SIZE.SECTOR_ERASE)) return false;
                        var r = this.IsWriteRegisterEnable();
                    }
                }
                else  throw new ArgumentException($"Cannot format flash length:{buffer.Count}, addr:{addr}");
            }

            this.Trace($"WritePages addr:{addr}, Len:{buffer.Count}");

            var pageSize = ((int)this.PageSize);
            if (buffer.Count % pageSize == 0)
            {
                var howManyPage = buffer.Count / pageSize;
                for (var p = 0; p < howManyPage; p++)
                {
                    var address = addr + (p * pageSize);
                    var buffer2 = buffer.GetRange(p * pageSize, pageSize);
                    if (!this.__WriteOnePage(address, buffer2))
                        return false;
                    Console.Write(".");
                    var r = this.IsWriteRegisterEnable();
                }

                if (verify && buffer.Count <= MAX_BLOCK_SIZE)
                {
                    var buffer3 = new List<byte>();
                    this.ReadPages(addr, buffer.Count, buffer3);
                    if (!MUSB_FS.BinSerializer.Compare(buffer.ToArray(), buffer3.ToArray()))
                    {
                        if (Debugger.IsAttached)
                            Debugger.Break();
                        return false;
                    }
                }
                return true;
            }
            else throw new ArgumentException($"Invalid buffer size:{buffer.Count}");
        }

        private bool __WriteOnePage(int address, List<byte> buffer)
        {
            this.Trace($"__WriteOnePage addr:{address}, Len:{buffer.Count}");

            if (!SetWriteRegisterEnable())
                return false;

            var spiBufferWrite = GetEepromApiWriteBuffer(address, buffer);
            var r1 = this._spi.Write(spiBufferWrite) == FtdiMpsseSPIResult.Ok;

            this.WaitForOperation(30, 4, "w");

            // Added for the EEPROM, Not needed for CYPRESS - FRED TODO
            //if(!SetWriteRegisterDisable())
            //    return false;

            return r1;
        }

        private bool EraseCommand(int addr, ERASE_BLOCK_SIZE sectorSizeCmd)
        {
            if (!this.SetWriteRegisterEnable())
                return false;

            var buffer = GenerateBuffer((byte)sectorSizeCmd, addr).ToList();
            var r = this.SPISend(buffer);
            
            return r;
        }

        public bool ErasePage(int addr, ERASE_BLOCK_SIZE blockSize)
        {
            this.Trace($"ErasePage addr:{addr}, BlockSize:{blockSize}");

            if ((addr % ((int)this.PageSize)) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", addr, this.GetProgramWritePageSize()));

            var b = EraseCommand(addr, blockSize);
            if(addr == 0)
                this.WaitForOperation(500, 1000, "!"); // For some reason sector 0 is way longer to erase
            else
                this.WaitForOperation(50, 25, "!");

            //if (!this.SetWriteRegisterDisable())
            //    return false;

            return b;
        }


        public bool EraseFlash()
        {
            return this.ErasePage(0, ERASE_BLOCK_SIZE.CHIP_ERASE);
        }

        public bool ReadPages(int address, int size, List<byte> buffer)
        {
            var tmpBuffer = new List<byte>();
            var sizeToRead = Math.Min(size, MAX_BLOCK_SIZE);
            if (this.ReadBuffer(address, sizeToRead, tmpBuffer))
            {
                buffer.AddRange(tmpBuffer);
                return true;
            }
            else return false;
        }

        private bool ReadBuffer(int address, int size, List<byte> buffer)
        {
            this.Trace($"ReadPages addr:{address}, sieze:{size}");
            if (size > MAX_BLOCK_SIZE)
                throw new ArgumentException($"ReadPage cannot read buffer size:{size}");

            int byteSent = 0;
            var spiBufferWrite = GetEepromApiReadBuffer(address);
            var spiBufferRead = GetEepromApiDataBuffer(size);

            if (this._spi.Query(spiBufferWrite, spiBufferRead) == FtdiMpsseSPIResult.Ok)
            {
                buffer.AddRange(spiBufferRead);
                return true;
            }

            return false;
        }
    }
}
