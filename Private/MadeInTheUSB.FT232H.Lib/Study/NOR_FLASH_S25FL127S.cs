/*
   Copyright (C) 2016, 2017 MadeInTheUSB LLC
   Written by FT for MadeInTheUSB

   The MIT License (MIT)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in
        all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        THE SOFTWARE.
  
    MIT license, all text above must be included in any redistribution
*/

using MadeInTheUSB.spi;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Markup;
using int16_t = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t = System.Byte;
using MadeInTheUSB.GPIO;
using MadeInTheUSB.EEPROM;

namespace MadeInTheUSB.FLASH
{
    /// <summary>
    /// 
    /// S25FL127SABMFI101
    /// http://www.mouser.com/ds/2/100/001-98282_S25FL127S_128_MBIT_16_MBYTE_3.0V_SPI_FLA-933034.pdf
    /// </summary>
    public class NOR_FLASH_S25FL127S : NOR_FLASH_S25FLXXXX
    {
        /// <summary>
        /// Different value for S25FL116K, S25FL132K, S25FL164K and
        /// for S25FL127K
        /// </summary>
        public override int SPIFLASH_STATUSREAD_REG_1
        {
            get { return 0x05; }
        } 
        public override int SPIFLASH_STATUSREAD_REG_2
        {
            get { return 0x07; }
        } 

        public override int SPIFLASH_STATUSREAD_REG_3
        {
            get
            {
                throw new NotImplementedException();
            }
        } 

        public int SPIFLASH_READ_CONFIG_REG_1
        {
            get { return 0x35; }
        }
        
        /// <summary>
        /// http://www.mouser.com/ds/2/100/002-00497_S25FL116K_S25FL132K_S25FL164K_16_MBIT_2_-933056.pdf
        /// page 50
        /// </summary>
        public Dictionary<int, string> StatusRegister1Enum_S25FL127S = new Dictionary<int, string>()
        {
            { 1<<0, "WRITE_IN_PROGRESS"             },
            { 1<<1, "WRITE_ENABLE_LATCH"            },
            { 1<<2, "WRITE_BLOCK_SELECT_0"          },
            { 1<<3, "WRITE_BLOCK_SELECT_1"          },
            { 1<<4, "WRITE_BLOCK_SELECT_2"          },
            { 1<<5, "ERASE_ERROR"                   },
            { 1<<6, "PROGRAMMING_ERROR"             },
            { 1<<7, "STATUS_REGISTER_WRITE_DISABLE" },
        };

        [Flags]
        public enum StatusRegister1Enum : byte
        {
            Busy                       = 1<<0,
            WriteEnableLatch           = 1<<1,
            BlockProtectBits0          = 1<<2,
            BlockProtectBits1          = 1<<3,
            BlockProtectBits2          = 1<<4,
            EraseErrorOccurred         = 1<<5,
            ProgrammingErrorOccurred   = 1<<6,
            StatusRegisterWriteDisable = 1<<7,
        };

        [Flags]
        public enum ConfigurationRegister1Enum : byte
        {
            BlockProtectionAndOTPLocked                                  = 1<<0,
            QuadMode                                                     = 1<<1,
            _4kBPhysicalSectorsAtTop_highAddress                         = 1<<2,
            ConfiguresBP2_0InStatusRegisterVolatile                      = 1<<3,
            ReservedForFutureUse                                         = 1<<4,
            ConfiguresStartOfBlockProtection_BPStartsAtBottom_LowAddress = 1<<5,
            LatencyCode0                                                 = 1<<6,
            LatencyCode1                                                 = 1<<7,
        };

        [Flags]
        public enum StatusRegister2Enum : byte
        {
            ProgramSuspendMode                  = 1<<0,
            EraseSuspend                        = 1<<1,
            ReservedForFutureUse2               = 1<<2,
            ReservedForFutureUse3               = 1<<3,
            ReservedForFutureUse4               = 1<<4,
            IO3AlternateFunctionIsResetOrHold   = 1<<5,
            PageBufferWrap512Or256              = 1<<6,
            BlockEraseSize256kOr64k             = 1<<7,
        };

       

#if NUSBIO2
        public NOR_FLASH_S25FL127S() : base(NOR_FLASH_S25FL164K_kBit)
        {
        }
#else
        public NOR_FLASH_S25FL164K(
            Nusbio nusbio,
            NusbioGpio clockPin,
            NusbioGpio mosiPin,
            NusbioGpio misoPin,
            NusbioGpio selectPin,
            bool debug = false) : base(nusbio, clockPin, mosiPin, misoPin, selectPin, NOR_FLASH_S25FL164K_kBit, debug)
        {
            var b = this.MaxByte;
            var p = this.MaxPage;
            this._spi.Unselect();
        }
#endif

        public string GetFlashInfo()
        {
            var b = new System.Text.StringBuilder();

            b.AppendFormat("Manufacturer:{0} ({1}), FlashType:{2}, ", this.Manufacturer, this.ManufacturerID, this.FlashType).AppendLine();
            b.AppendFormat("DeviceID:{0} ", this.DeviceID);
            b.AppendFormat("Capacity:{0} ", this.Capacity).AppendLine();
            b.AppendFormat("MemorySize {0} K byte ", this.MaxKByte).AppendLine();
            b.AppendFormat("SectorSize:{0}, ", SectorSize);
            b.AppendFormat("MaxSector:{0} ", MaxSector);
            b.AppendFormat("ProgramWritePageSize:{0} ", GetProgramWritePageSize()).AppendLine();

            b.AppendFormat("PackageModel:{0} ", PackageModel).AppendLine();
            b.AppendFormat("FamilyID:{0} ", FamilyID).AppendLine();
            b.AppendFormat("SectorArchitecture:{0} ", SectorArchitecture).AppendLine();

            b.AppendFormat("ReadStatusRegister1Enum:{0} ", this.ReadStatusRegister1Enum()).AppendLine();
            b.AppendFormat("ReadStatusRegister2Enum:{0} ", this.ReadStatusRegister2Enum()).AppendLine();
            b.AppendFormat("ReadConfigurationRegister1Enum:{0} ", this.ReadConfigurationRegister1Enum()).AppendLine();
            
            return b.ToString();
        }


        public bool ReadInfo()
        {
            var buffer = new List<byte>() { 0x9F, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var r      = this.GetRidOf0xFFAtBeginning(this.SpiTransfer(buffer));

            if(r.Buffer[0] != (int)Manufacturers.Cypress)
            {
                r.Buffer.RemoveAt(0);
            }

            this.ManufacturerID =  r.Buffer[0];

            if(r.Buffer[1] == (int)CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL_116K_132K_164K)
            {
                this.FlashType = CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL_116K_132K_164K;
                this.Capacity =  (CYPRESS_S25FLXXX_CAPACITY)r.Buffer[2];
                // For now only support 164K or 8Mb, do not support smaller flash
                base._kBit = NOR_FLASH_S25FL164K.NOR_FLASH_S25FL164K_kBit;
            }
            else
            {
                // Only support CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL127S
                this.DeviceType     =  (r.Buffer[1] << 8);
                this.DeviceType     += r.Buffer[2];
                this.FlashType      = (CYPRESS_S25FLXXX_DEVICE_TYPE)this.DeviceType;
            }
                
            if(this.FlashType == CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL127S)
            {
                // datasheet: http://www.mouser.com/ds/2/100/001-98282_S25FL127S_128_MBIT_16_MBYTE_3.0V_SPI_FLA-933034.pdf
                // page 118
                base._kBit              = NOR_FLASH_S25FL164K.NOR_FLASH_S25FL127S_kBit;
                this.SectorArchitecture = (CYPRESS_SECTOR_ARCHITECTURE)r.Buffer[4];
                this.FamilyID           = (CYPRESS_FAMILIY_ID)r.Buffer[5];
                this.PackageModel         = string.Empty;
                this.PackageModel        += ((char)r.Buffer[6]).ToString();
                this.PackageModel        += ((char)r.Buffer[7]).ToString();
                base.Capacity            = CYPRESS_S25FLXXX_CAPACITY.S25FL127S;

                if(CYPRESS_PACKAGE_MODEL_NUMBERS.ContainsKey(this.PackageModel))
                    this.PackageModel = CYPRESS_PACKAGE_MODEL_NUMBERS[this.PackageModel];
                else
                    this.PackageModel = CYPRESS_PACKAGE_MODEL_NUMBERS[""];

                buffer = new List<byte>() { 0xAB, 0, 0, 0, 0, 0 };
                r  = this.GetRidOf0xFFAtBeginning(this.SpiTransfer(buffer));

                var r0 = ReadStatusRegister1Enum();
                var r1 = ReadStatusRegister2Enum();
                var rc = ReadStatusRegisterConfiguration();
                var rb = ReadStatusRegisterBankAddress();

                return true;
            }


            /*
            Cypress Manufactrer ID:1
            S25FL116K: 
                        90h(2) Manufacturer ID = 01h Device ID = 14h
                        9Fh(3) Manufacturer ID = 01h Device Type = 40h Capacity = 15h
            S25FL132K: 
                        90h(2) Manufacturer ID = 01h Device ID = 15h
                        9Fh(3) Manufacturer ID = 01h Device Type = 40h Capacity = 16h
            S25FL164K: 
                        90h(2) Manufacturer ID = 01h Device ID = 16h
                        9Fh(3) Manufacturer ID = 01h Device Type = 40h Capacity = 17h

            S25FL127S: DeviceID 17h
            */

            return false;
        }
        
        
        public int ReadStatusRegister1()
        {
            return ReadStatusRegister(SPIFLASH_STATUSREAD_REG_1);
        }
        public StatusRegister1Enum ReadStatusRegister1Enum()
        {
            return (StatusRegister1Enum)ReadStatusRegister(SPIFLASH_STATUSREAD_REG_1);
        }
        public StatusRegister2Enum ReadStatusRegister2Enum()
        {
            return (StatusRegister2Enum)ReadStatusRegister(SPIFLASH_STATUSREAD_REG_2);
        }

       
        ConfigurationRegister1Enum ReadConfigurationRegister1Enum()
        {
            return (ConfigurationRegister1Enum)ReadStatusRegister(SPIFLASH_READ_CONFIG_REG_1);
        }
        

        /* S25FL127S
        public bool SetWritePageTo256Bytes()
        {
            var r2 = ReadStatusRegister2Enum();
            if((r2 & StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512) == StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512)
            {
                r2 -= StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512;
                WriteRegister2((byte)r2);
            }
            return true;
        }

        public bool SetWritePageTo512Bytes()
        {
            var r2 = ReadStatusRegister2Enum();
            if((r2 & StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512) != StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512)
            {
                var rr2 = (int)r2 - (int)StatusRegister2Enum.PAGE_BUFFER_WRAP_256_OR_512;
                WriteRegister2((byte)rr2);
            }
            return true;
        }*/

        public  bool setWriteRegisterDisable()
        {
            return base.SetWriteRegisterDisable();
        }

        bool WriteRegister2(byte r2NewValue)
        {
            var r1 = (byte)ReadStatusRegister1();
            var rc = (byte)ReadStatusRegisterConfiguration();
            
            if(base.SetWriteRegisterEnable())
            {
                var r0 = this.SpiTransfer(new List<byte>()
                {
                    SPIFLASH_STATUSWRITE_REG, r1, rc, r2NewValue
                });
                var status = r0.Buffer[1];
                return true;
            }
            return false;
        }

        public int GetProgramWritePageSize()
        {
            
            var r = ReadStatusRegister2Enum();
            if((r & StatusRegister2Enum.PageBufferWrap512Or256) == StatusRegister2Enum.PageBufferWrap512Or256)
                return 512;
            else
                return PAGE_SIZE;
        }

        int ReadStatusRegister2()
        {
            return ReadStatusRegister(SPIFLASH_STATUSREAD_REG_2);
        }
        int ReadStatusRegister3()
        {
            return ReadStatusRegister(SPIFLASH_STATUSREAD_REG_3);
        }
        int ReadStatusRegisterConfiguration()
        {
            return ReadStatusRegister(SPIFLASH_CONFIGURATION_REG);
        }
        int ReadStatusRegisterBankAddress()
        {
            return ReadStatusRegister(SPIFLASH_BANK_ADDRESS_REG);
        }
        int ReadStatusRegisterBankWrite()
        {
            return ReadStatusRegister(SPIFLASH_BANK_WRITE_REG);
        }
        public override bool Is3BytesAddress
        {
            get { return true; }
        }

        public override int PAGE_SIZE
        {
            get { return 256; }
        }

#if !NUSBIO2
        public EEPROM_BUFFER ReadPageOptimized(int addr, int len = -1)
        {
            if (len == -1)
                len = this.PAGE_SIZE;

            var eb = new EEPROM_BUFFER();
            var nusbio = this._spi.Nusbio;
            var spi = this._spi;
            var spiBufferCmdAddr = this.GetEepromApiReadBuffer(addr);
            var spiBufferDummyData = this.GetEepromApiDataBuffer(len);
            var buffer = new List<byte>();

            buffer.AddRange(spiBufferCmdAddr);   // Command + 16 bit Address
            buffer.AddRange(spiBufferDummyData); // Dummy data to be sent which will force the EEPROM to send the right data back
            var startByteToSkip = spiBufferCmdAddr.Length;
            var finalBuffer = new List<byte>();

            this._spi.Select(); // Set select now so it is part of the bit banging sequence
            try
            {
                var byteBitBanged = 0;
                var spiSeq = new GpioSequence(nusbio.GetGpioMask(), nusbio.GetTransferBufferSize());
                // Convert the 3 Command Bytes + the 64 0 Bytes into a bit banging buffer
                // The 64 bytes part is optimized since all the value are 0 we just need to set
                // the data line once
                for (var bx = 0; bx < buffer.Count; bx++)
                {
                    for (byte bit = (1 << 7); bit > 0; bit >>= 1) // MSB - Most significant bit first
                    {
                        spiSeq.ClockBit(nusbio[spi.ClockGpio], nusbio[spi.MosiGpio], WinUtil.BitUtil.IsSet(buffer[bx], bit),
                            compactData: (bx >= spiBufferCmdAddr.Length) // For simplicity do not compact the first 3 commands byte, so we know exactly where the data start after the first 3 bytes
                            );
                    }
                    byteBitBanged++;

                    if (spiSeq.IsSpaceAvailable(8 * 2)) // If we only have left space to compute 1 byte or less
                    {
                        var peb = ReadPageOptimized_SendReadData(spiSeq, startByteToSkip, byteBitBanged, this._spi);
                        if (peb.Succeeded)
                        {
                            finalBuffer.AddRange(peb.Buffer);
                            spiSeq = new GpioSequence(nusbio.GetGpioMask(), nusbio.GetTransferBufferSize());
                            startByteToSkip = 0; // We skipped it, let's forget about it
                            byteBitBanged = 0;
                        }
                        else
                            return eb; // failed
                    }
                }
                var peb2 = ReadPageOptimized_SendReadData(spiSeq, startByteToSkip, byteBitBanged, this._spi);
                if (peb2.Succeeded)
                {
                    finalBuffer.AddRange(peb2.Buffer);
                    eb.Buffer = finalBuffer.ToArray();
                    eb.Succeeded = true;
                }
                else
                    return eb; // failed
            }
            finally
            {
                this._spi.Unselect();
            }
            return eb;
        }
#endif

        public EEPROM_BUFFER ReadSector(int sector4kStart, int len, bool optimize = false)
        {
            //this.WaitForOperation();

#if !NUSBIO2 // With Nusbio2 with high speed SPI by default
            if (optimize)
                return ReadPageOptimized(sector4kStart, len);
#endif

            var rb = new EEPROM_BUFFER();
            var tmpBuffer = new List<byte>() { READ, (byte)(sector4kStart >> 16), (byte)(sector4kStart >> 8), (byte)(sector4kStart & 0xFF) };
            var buffer = base.GetEepromApiDataBuffer((int)len);
            tmpBuffer.AddRange(buffer);
            var r = this.SpiTransfer(tmpBuffer, true);

            if (!r.Succeeded)
                return rb;

            rb.Buffer = r.Buffer.Skip(4).ToArray();
            rb.Succeeded = r.Succeeded;

            return rb;
        }

        //public EEPROM_BUFFER ReadSector_NotOptimized(int sector4kStart, int len)
        //{
        //    sector4kStart = sector4kStart * this.SectorSize;
        //    var rb        = new EEPROM_BUFFER();
        //    var tmpBuffer = new List<byte>() { READ, (byte)(sector4kStart >> 16), (byte)(sector4kStart >> 8), (byte)(sector4kStart & 0xFF) };
        //    var buffer    = base.GetEepromApiDataBuffer((int)len);
        //    tmpBuffer.AddRange(buffer);
        //    var r         = this._spi.Transfer(tmpBuffer);

        //    if (!r.Succeeded)
        //        return rb;

        //    rb.Buffer = r.Buffer.Skip(4).ToArray();
        //    rb.Succeeded = r.Succeeded;

        //    return rb;
        //}

        public bool SetWriteRegisterEnable(bool checkStatus = true)
        {
            var tryCounter = 0;

            while(tryCounter++ < MAX_TRY)
            {
                if (!base.SetWriteRegisterEnable())
                    return false;

                if(checkStatus)
                {
                    this.WaitForOperation();
                    var rr0 = ReadStatusRegister1Enum();
                    if((rr0 & StatusRegister1Enum.WriteEnableLatch) == StatusRegister1Enum.WriteEnableLatch)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Issue setting Write Enable");       
                        Debugger.Break();
                    }
                }
                else
                {
                    this.WaitForOperation();
                    return true;
                }
            }
            return false;
        }


        public bool Write64kSector(int sector64kStart, List<byte> buffer, bool erase = true)
        {
            if(buffer.Count != 65536)
                throw new ArgumentException("Write4kSector() can only write 4k byte of data");
            
            if (erase)
                if (!this.Erase64K(sector64kStart))
                    return false;

            //var r1Erase = this.ReadStatusRegister1Enum();

            var addr = sector64kStart;
            int written = 0;

            var pageWriteSize = this.GetProgramWritePageSize();

            for (var b = 0; b < buffer.Count; b += pageWriteSize)
            {
                if(!SetWriteRegisterEnable())
                    return false;

                var tmpBuffer = new List<byte>() { PAGE_PROGRAM, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
                tmpBuffer.AddRange(buffer.Skip(written).Take(pageWriteSize));
                var r = this.SpiTransfer(tmpBuffer, true);

                if (!r.Succeeded)
                    return false;

                //var r1 = this.ReadStatusRegister1Enum();

                written += pageWriteSize;
                addr    += pageWriteSize;
                this.WaitForOperation(0);
            }
            return true;
        }

        public bool Write4kSector(int sector4kStart, List<byte> buffer, bool erase = true)
        {
            if(buffer.Count > this.SectorSize)
                throw new ArgumentException("Write4kSector() can only write 4k byte of data");
            
            if (erase)
                if (!this.Erase4K(sector4kStart))
                    return false;

            var r1Erase = this.ReadStatusRegister1Enum();

            var addr = sector4kStart;
            int written = 0;

            var pageWriteSize = this.GetProgramWritePageSize();

            for (var b = 0; b < buffer.Count; b += pageWriteSize)
            {
                if(!SetWriteRegisterEnable())
                    return false;

                var tmpBuffer = new List<byte>() { PAGE_PROGRAM, (byte)(addr >> 16), (byte)(addr >> 8), (byte)(addr & 0xFF) };
                tmpBuffer.AddRange(buffer.Skip(written).Take(pageWriteSize));
                var r = this.SpiTransfer(tmpBuffer, true);

                if (!r.Succeeded)
                    return false;

                var r1 = this.ReadStatusRegister1Enum();

                written += pageWriteSize;
                addr    += pageWriteSize;
                this.WaitForOperation(0);
            }
            this.WaitForOperation();
            return true;
        }

        private SPIResult SpiTransfer(List<byte> buffer, bool writeRead)
        {
#if NUSBIO2
            var r = new SPIResult();
            if(writeRead)
            {
                var tmpReadBuffer = new byte[buffer.Count];
                var ok            = Nusbio2NAL.SPI_Helper_SingleReadWrite(buffer.ToArray(), tmpReadBuffer, buffer.Count);
                if (ok)
                {
                    r.Buffer    = tmpReadBuffer.ToList();
                    r.Succeeded = true;
                }
            }
            else
            {
                r.Succeeded = Nusbio2NAL.SPI_Helper_Write(buffer.ToArray(), buffer.Count);
            }
            return r;
#else
            return this._spi.Transfer(buffer);
#endif
        }
    }
}
