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
    public partial class CypressFlashMemory //: GpioSpiDeviceBaseClass
    {
        public int SizeInByte;
        
        public enum DeviceState
        {
            Initializing,
            Reading,
            Writing,
            Idle,
            AllOff
        }

        const int READ_GPIO     = 0;
        const int WRITE_GPIO    = 1;

        public bool TraceOn { get; set; } = false;

        private readonly ISPI _spi;

        public void SetUserHardwareInterfaceState(DeviceState state)
        {
            //this.DigitalWrite(PinState.Low, READ_GPIO, WRITE_GPIO);
            //switch (state)
            //{
            //    case DeviceState.Initializing: this.DigitalWrite(PinState.High, READ_GPIO, WRITE_GPIO); break;
            //    case DeviceState.Reading: this.DigitalWrite(PinState.High, READ_GPIO); break;
            //    case DeviceState.Writing: this.DigitalWrite(PinState.High, WRITE_GPIO); break;
            //    case DeviceState.Idle: break;
            //    case DeviceState.AllOff: break;
            //}
        }

        public int MaxPage
        {
            get { return this.SizeInByte / PAGE_SIZE; }
        }

        public int MaxBlock
        {
            get { return this.SizeInByte / CypressFlashMemory.BLOCK_SIZE; }
        }

        private void Trace(string m)
        {
            if(this.TraceOn)
                Debug.WriteLine(m);
        }
        
        public CypressFlashMemory(ISPI spi) 
        {
            this.SetUserHardwareInterfaceState(DeviceState.Initializing);
            this._spi = spi;
        }
                
        public StatusRegister1Enum ReadRegister1Enum()
        {
            return (StatusRegister1Enum)ReadRegister1();
        }

        public int ReadRegister1()
        {
            var buffer = new List<byte>();
            if (this.SPIQuery(EEPROM_READ_STATUS_REGISTER_1, 1, buffer))
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
            if (this.SPIQuery(EEPROM_READ_STATUS_REGISTER_2, 1, buffer))
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

        public void ReadIdentification()
        {
            var buffer = new List<byte>();
            if (this.SPIQuery(EEPROM_READ_IDENTIFICATION, 18, buffer))
            {
                this.Manufacturer = (Manufacturers)buffer[0];
                this.DeviceID = (CYPRESS_S25FLXXX_DEVICE_ID)((buffer[1] << 8) + buffer[2]);

                this.SectorArchitecture = (CYPRESS_SECTOR_ARCHITECTURE)buffer[4];
                this.FamilyID = (CYPRESS_FAMILIY_ID)buffer[5];
                this.PackageModel = string.Empty;
                this.PackageModel += ((char)buffer[6]).ToString();
                this.PackageModel += ((char)buffer[7]).ToString();

                switch(this.DeviceID)
                {
                    case CYPRESS_S25FLXXX_DEVICE_ID.S25FL127S_16MB:
                        this.SizeInByte = 16 * 1024 * 1024;
                        break;
                }
            }
        }

        public int GetDeviceSizeInMb()
        {
            return SizeInByte / 1024 / 1024;
        }

        public string GetDeviceInfo()
        {
            return $@"DeviceID:{this.DeviceID}, Size:{this.GetDeviceSizeInMb()} Mb, Manufacturer:{this.Manufacturer}, MaxPage:{this.MaxPage}, Page Size:{PAGE_SIZE}, Page Size Write:{this.GetProgramWritePageSize()}, MaxBlock:{this.MaxBlock}, BlockSize:{CypressFlashMemory.BLOCK_SIZE}";
        }

        private bool SetWriteRegisterEnable(bool checkStatus = true)
        {
            var result = false;

            if (!this.SendCommand(EEPROM_WRITE_ENABLE_CMD))
                return result;

            this.WaitForOperation(5, 5, "w");
            if (checkStatus)
            {
                var rr0 = this.ReadRegister1Enum();
                if ((rr0 & StatusRegister1Enum.WriteEnableLatch) == StatusRegister1Enum.WriteEnableLatch)
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
            return this.SendCommand(EEPROM_WRITE_DISABLE_CMD);
        }

        /// <summary>
        /// Write the buffer starting at address addr. The buffer must contains a multiple of PAGE_SIZE (512).
        /// </summary>
        /// <param name="addr">Start address</param>
        /// <param name="buffer">The buffer to write</param>
        /// <param name="setUserHardwareInterfaceState">If true notify UI of write operation, not implemented</param>
        /// <param name="verify">If true verify the data written</param>
        /// <param name="format">If true format the 64k starting at addr</param>
        /// <returns></returns>
        public bool WritePages(int addr, List<byte> buffer, bool setUserHardwareInterfaceState = true, bool verify = false, bool format = false)
        {
            try
            {
                if(format)
                {
                    if (!this.Erase64KPage(addr)) return false;
                }

                this.Trace($"WritePages addr:{addr}, Len:{buffer.Count}");

                if(setUserHardwareInterfaceState)
                    this.SetUserHardwareInterfaceState(DeviceState.Writing);

                if (buffer.Count % PAGE_SIZE == 0)
                {
                    var howManyPage = buffer.Count / PAGE_SIZE;
                    for (var p = 0; p < howManyPage; p++)
                    {
                        var address = addr + (p * PAGE_SIZE);
                        var buffer2 = buffer.GetRange(p * PAGE_SIZE, PAGE_SIZE);
                        if (!this.__WriteOnePage(address, buffer2))
                            return false;
                    }

                    if (verify && buffer.Count <= BLOCK_SIZE)
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
            finally
            {
                if (setUserHardwareInterfaceState)
                    this.SetUserHardwareInterfaceState(DeviceState.Idle);
            }
        }

        private bool __WriteOnePage(int address, List<byte> buffer)
        {
            this.Trace($"__WriteOnePage addr:{address}, Len:{buffer.Count}");

            if (!SetWriteRegisterEnable())
                return false;

            var spiBufferWrite = GetEepromApiWriteBuffer(address, buffer);
            var r1 = this._spi.Write(spiBufferWrite) == FtdiMpsseSPIResult.Ok;

            this.WaitForOperation(30, 7, "w");

            return r1;
        }

        private bool EraseCommand(int addr, int sectorSizeCmd)
        {
            if (!this.SetWriteRegisterEnable())
                return false;
            var buffer = new List<byte>() { (byte)sectorSizeCmd, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
            var r = this.SPISend(buffer);

            return r;
        }

        private bool ErasePage(int addr)
        {
            this.Trace($"ErasePage addr:{addr}");

            if ((addr % (this.GetProgramWritePageSize())) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", addr, this.GetProgramWritePageSize()));
            var b = EraseCommand(addr, EEPROM_SECTOR_ERASE);
            this.WaitForOperation(200, 100, "!"); // Seems slower

            return b;
        }

        // Erase an entire 4k sector the location is in.
        // For example "erase_4k(300);" will erase everything from 0-3999. 
        //
        // All erase commands take time. No other actions can be preformed
        // while the chip is errasing except for reading the register
        private bool Erase4K(int addr)
        {
            this.Trace($"Erase4K addr:{addr}");

            if ((addr % (PAGE_SIZE)) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", addr, this.GetProgramWritePageSize()));
            var b = EraseCommand(addr, EEPROM_BLOCKERASE_4K);
            this.WaitForOperation(100, 20, "!"); // Seems slower

            return b;
        }

        private bool Erase64KPage(int addr)
        {
            this.Trace($"Erase64K addr:{addr}");

            if ((addr % (PAGE_SIZE)) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", addr, this.GetProgramWritePageSize()));
            var b = EraseCommand(addr, EEPROM_BLOCKERASE_64K);
            if(addr == 0)
                this.WaitForOperation(500, 1000, "!"); // For some reason sector 0 is way longer to erase
            else
                this.WaitForOperation(50, 25, "!");

            return b;
        }

        //public bool FormatFlashFAT(int fatIndex)
        //{
        //    this.Trace($"FormatFlashFAT fatIndex:{fatIndex}");

        //    if (fatIndex == 0 || fatIndex == 1)
        //        return this.Erase64KPage(fatIndex * CypressFlashMemory.BLOCK_SIZE);

        //    throw new ArgumentException($"FAT index {fatIndex} invalid");
        //}

        public bool FormatFlash(Action<int> notify, int notifyEvery = 8, int blockCount = -1)
        {
            if (blockCount == -1)
                blockCount = this.MaxBlock;

            this.Trace($"FormatFlash blockCount:{blockCount}");

            for (var b = 0; b < blockCount; b++)
            {
                if(b != 0 && b % notifyEvery == 0 && notify != null)
                    notify(b);
                if (!this.Erase64KPage(b * CypressFlashMemory.BLOCK_SIZE))
                    return false;
            }

            return true;
        }

        public bool ReadPages(int address, int size, List<byte> buffer)
        {
            // Compute how many block of 64k we need to read the file
            // in SPI 64k transactions
            var _64kBlock = (size / EEPROM_MAX_BLOCK_READ_LEN);
            if (size % EEPROM_MAX_BLOCK_READ_LEN != 0)
                _64kBlock += 1;

            this.SetUserHardwareInterfaceState(DeviceState.Reading);
            try
            {
                for (var b = 0; b < _64kBlock; b++)
                {
                    var tmpBuffer = new List<byte>();
                    var sizeToRead = EEPROM_MAX_BLOCK_READ_LEN;
                    if(sizeToRead > size)
                        sizeToRead = size;
                    if (this.ReadBuffer(address + (b * EEPROM_MAX_BLOCK_READ_LEN), sizeToRead, tmpBuffer))
                        buffer.AddRange(tmpBuffer);
                    else return false;
                }
                //if(buffer.Count > size)
                //{
                //    buffer = buffer.Take(size).ToList();
                //}
                return true;
            }
            finally {
                this.SetUserHardwareInterfaceState(DeviceState.Idle);
            }
        }

        /// <summary>
        /// Read a buffer of data at the address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="size">Size to read limited to 64k max</param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private bool ReadBuffer(int address, int size, List<byte> buffer)
        {
            this.Trace($"ReadPages addr:{address}, sieze:{size}");
            if (size > EEPROM_MAX_BLOCK_READ_LEN)
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

            //var ec = base.Write(spiBufferWrite, spiBufferWrite.Length, out byteSent, FtSpiTransferOptions.ChipselectEnable);
            //if (ec == FtdiMpsseSPIResult.Ok)
            //{
            //    var spiBufferRead = GetEepromApiDataBuffer(size);
            //    ec = base.Read(spiBufferRead, spiBufferRead.Length, out byteSent, FtSpiTransferOptions.ChipselectDisable);
            //    if (ec == FtdiMpsseSPIResult.Ok)
            //    {
            //        buffer.AddRange(spiBufferRead);
            //        return true;
            //    }
            //}
            //return false;
        }

    }
}
