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
    public abstract class NOR_FLASH_S25FLXXXX : EEPROM_25AAXXX_BASE
    {
        public const int MAX_TRY = 16;

        public const int NOR_FLASH_S25FL116K_kBit = 16  * 1024;  // 64 Mbit = 8 Mbyte
        public const int NOR_FLASH_S25FL132K_kBit = 32  * 1024;  // 64 Mbit = 8 Mbyte
        public const int NOR_FLASH_S25FL164K_kBit = 64  * 1024;  // 64 Mbit = 8 Mbyte
        public const int NOR_FLASH_S25FL127S_kBit = 128 * 1024;  // 128 Mbit = 16 Mbyte

             /// <summary>
        /// According S25FL127S datasheet
        /// - READ is limited to 50Mhz
        /// - FAST_READ 108 Mhz Read Data Bytes at Higher Speed
        /// FT4222H is limimited to 40Mhz so this does not affect us.
        /// </summary>
        public const int FAST_READ = 0x0b;
        public const int PAGE_PROGRAM        = 0x02; /* Page Program  */
        public const int SE        = 0x20; /* Sector Erase (4k)  */
        public const int BE        = 0x20; /* Block Erase (64k)  */
        public const int CE        = 0xc7; /* Erase entire chip  */
        public const int DP        = 0xb9; /* Deep Power-down  */
        public const int RES       = 0xab; /* Release Power-down, return Device ID */
        public const int RDID      = 0x9F; /* Read Manufacture ID, memory type ID, capacity ID */

         public const int SPIFLASH_WRITEENABLE      = 0x06; // write enable
        public const int SPIFLASH_WRITEDISABLE     = 0x04; // write disable
        public const int SPIFLASH_BLOCKERASE_4K    = 0x20; // erase one 4K block of flash memory
        public const int SPIFLASH_BLOCKERASE_32K   = 0x52; // erase one 32K block of flash memory
        public const int SPIFLASH_BLOCKERASE_64K   = 0xD8; // erase one 64K block of flash memory
        public const int SPIFLASH_CHIPERASE        = 0x60; // chip erase (may take several seconds depending on size)

        public const int SPIFLASH_STATUSWRITE_REG  = 0x01;
        public const int SPIFLASH_STATUS_CLEAR_REG = 0x30;

        public abstract int SPIFLASH_STATUSREAD_REG_1 { get; }
        public abstract int SPIFLASH_STATUSREAD_REG_2 { get; }
        public abstract int SPIFLASH_STATUSREAD_REG_3 { get; }

                public const int SPIFLASH_CONFIGURATION_REG = 0x35; // read status register
        public const int SPIFLASH_BANK_ADDRESS_REG  = 0x16;
        public const int SPIFLASH_BANK_WRITE_REG    = 0x17;

        public const int SPIFLASH_STATUSWRITE       = 0x01; // write status register
        public const int SPIFLASH_ARRAYREAD         = 0x0B; // read array (fast, need to add 1 dummy byte after 3 address bytes)
        public const int SPIFLASH_ARRAYREADLOWFREQ  = 0x03; // read array (low frequency)
        public const int SPIFLASH_SLEEP             = 0xB9; // deep power down
        public const int SPIFLASH_WAKE              = 0xAB; // deep power wake up
        public const int SPIFLASH_BYTEPAGEPROGRAM   = 0x02; // write (1 to 256bytes)
        public const int SPIFLASH_IDREAD            = 0x9F; // read JEDEC manufacturer and device ID (2 bytes, specific bytes for each manufacturer and device)
                                                 // Example for Atmel-Adesto 4Mbit AT25DF041A: 0x1F44 (page 27: http://www.adestotech.com/sites/default/files/datasheets/doc3668.pdf)
                                                 // Example for Winbond 4Mbit W25X40CL: 0xEF30 (page 14: http://www.winbond.com/NR/rdonlyres/6E25084C-0BFE-4B25-903D-AE10221A0929/0/W25X40CL.pdf)
        public const int SPIFLASH_MACREAD           = 0x4B; // read unique ID number (MAC)

         public enum Manufacturers
        {
            Cypress = 1
        }
        public enum CYPRESS_S25FLXXX_DEVICE_TYPE // Based on the device id
        {
            Undefined            = 0,
            S25FL_116K_132K_164K = 0x40, // Based on 0x9F ManufacturerID, DeviceType, Capacity
            S25FL127S            = 8216
        }
        public CYPRESS_S25FLXXX_DEVICE_TYPE FlashType = CYPRESS_S25FLXXX_DEVICE_TYPE.Undefined;
        
        
       
        public enum CYPRESS_S25FLXXX_DEVICE_ID
        {
            Undefined      = 0,
            S25FL116K_2Mb  = 0x14,
            S25FL132K_4Mb  = 0x15,
            S25FL164K_8Mb  = 0x16,
            S25FL127S_16MB = 8216
        }
        public CYPRESS_S25FLXXX_DEVICE_ID DeviceID;

        public enum CYPRESS_S25FLXXX_CAPACITY // Based on the device id
        {
            Undefined     = 0,
            S25FL116K_2Mb = 0x15, // Based on 0x9F ManufacturerID, DeviceType, Capacity
            S25FL132K_4Mb = 0x16,
            S25FL164K_8Mb = 0x17,
            S25FL127S     = 8216
        }
        public CYPRESS_S25FLXXX_CAPACITY Capacity;

        public enum CYPRESS_FAMILIY_ID
        {
            FL_S_Family = 0x80,
            Undefined = 0,          
        }
        public CYPRESS_FAMILIY_ID FamilyID = CYPRESS_FAMILIY_ID.Undefined;

         /// <summary>
        /// http://www.mouser.com/ds/2/100/002-00497_S25FL116K_S25FL132K_S25FL164K_16_MBIT_2_-933056.pdf
        /// 6.4 Security Register 0 — Serial Flash Discoverable Parameters (SFDP — JEDEC JESD216B)
        /// </summary>
        public const int SPIFLASH_SERIAL_FLASH_DISCOVERABLE_PARAMETERS = 0x5A;
        public const int SPIFLASH_STATUSREAD_REG_1_WRITE_ENABLED = 0x02; // Flasg set when write enable

        public enum CYPRESS_SECTOR_ARCHITECTURE
        {
            SectorArchitecture_Uniform256KBSectors                       = 0,
            SectorArchitecture_4kBParameterSectorsWithUniform64kBSectors = 1,
            Undefined = 128
        }
        public CYPRESS_SECTOR_ARCHITECTURE SectorArchitecture = CYPRESS_SECTOR_ARCHITECTURE.Undefined;

        protected static Dictionary<string, string> CYPRESS_PACKAGE_MODEL_NUMBERS = new Dictionary<string, string>()
        {
            { "00", "SOIC 16" },
            { "10", "SOIC 8 or WSON" },
            { "C0", "5x5 Ball BGA" },
            { "D0", "4x6 Ball BGA" },
            { ""  , "Undefined" },
        };
        public string PackageModel;
        
        public Manufacturers Manufacturer { get { return (Manufacturers)ManufacturerID; } }
        public int ManufacturerID;
        public int DeviceType;
        public int SectorSize = 4 * 1024;
        public int LargeSectorSize = 64 * 1024;
        public int MaxSector { get { return this.MaxByte / this.SectorSize; } }

#if NUSBIO2
        public NOR_FLASH_S25FLXXXX(int kBit) : base(kBit)
        {
        }
#else
        public NOR_FLASH_S25FLXXXX(
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

        protected int ReadStatusRegister(int reg, int byteToSend = 2)
        {
            uint8_t status = 0;

            if(byteToSend == 2)
            {
                var r0 = this.SpiTransfer(new List<byte>() { (byte)reg, 0 });
                status = r0.Buffer[1];
            }
            else
            {
                var r0 = this.SpiTransfer(new List<byte>() { (byte)reg });
                status = r0.Buffer[1];
            }
            return status;
        }


        protected  bool Busy()
        {
            var s = ReadStatusRegister(SPIFLASH_STATUSREAD_REG_1);
            return (s & 1) == 1;
        }


        protected  void WaitForOperation(int wait = 10, int minimumWait = 1, string t = "~")
        {
            if(minimumWait > 0)
                Thread.Sleep(minimumWait);
            if (!this.Busy())
                return;

            Console.Write("minimumWait:{0} is not enough",minimumWait);

            var tryCounter = 0;
            Thread.Sleep(wait / 5);
            while (true)
            {
                if (!this.Busy()) return;
                Thread.Sleep(wait);
                if (tryCounter++ >= MAX_TRY)
                    throw new ApplicationException("Waiting for operation timeout");
                Console.Write(t);
            }
        }

        
        // Erase an entire 4k sector the location is in.
        // For example "erase_4k(300);" will erase everything from 0-3999. 
        //
        // All erase commands take time. No other actions can be preformed
        // while the chip is errasing except for reading the register
        protected  bool Erase4K(int loc)
        {
            if ((loc % (SectorSize)) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", loc, this.SectorSize));
            var b = EraseSector(loc, SPIFLASH_BLOCKERASE_4K);
            if (this.FlashType == CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL127S)
                this.WaitForOperation(15, 10, "!"); // Seems slower
            else
                this.WaitForOperation(10, 75, "!");
            return b;
        }


        protected  bool Erase64K(int loc)
        {
            if ((loc % (SectorSize)) != 0)
                throw new ArgumentException(string.Format("Address {0} must be a multiple of {1}", loc, this.SectorSize));
            var b = EraseSector(loc, SPIFLASH_BLOCKERASE_64K);
            if (this.FlashType == CYPRESS_S25FLXXX_DEVICE_TYPE.S25FL127S)
                this.WaitForOperation(15, 75, "!"); // Seems slower
            else
                this.WaitForOperation(10, 75, "!");
            return b;
        }

        protected  bool EraseSector(int loc, byte sectorSizeCommand)
        {
            if (!this.SetWriteRegisterEnable())
                return false;

            var buffer = new List<byte>() { sectorSizeCommand, (byte)(loc >> 16), (byte)(loc >> 8), (byte)(loc & 0xFF) };
            var r = this.SpiTransfer(buffer, true);
            return r.Succeeded;
        }

        
        public bool ClearStatusRegister()
        {
            if(true) // base.SetWriteRegisterEnable()
            {
                var r0 = this.SpiTransfer(new List<byte>()
                {
                    SPIFLASH_STATUS_CLEAR_REG
                });
                var status = r0.Buffer[0];
                return true;
            }
            return false;
        }


    }
}
