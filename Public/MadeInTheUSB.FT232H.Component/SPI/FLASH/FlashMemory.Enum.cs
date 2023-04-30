using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.FT232H.Components
{
    public partial class FlashMemory // : GpioSpiDeviceBaseClass
    {
        public const int DEFAULT_PAGE_SIZE = 512;

        public enum Manufacturers : int
        {
            Unknown = 0,
            Cypress = 1,
            Microchip = 2, // This may not be the right id
            Winbond = 0xEF,
        }

        public Manufacturers Manufacturer = Manufacturers.Unknown;
        
        public enum FLASH_DEVICE_ID
        {
            Undefined      = 0,
            CYPRESS_S25FL116K_2Mb  = 0x14,
            CYPRESS_S25FL132K_4Mb  = 0x15,
            CYPRESS_S25FL164K_8Mb  = 0x16,
            CYPRESS_S25FL127S_16MB = 8216, // http://www.cypress.com/file/177961/download

            EEPROM_25AA1024_128Kb  = -2, //SERIAL EEPROM 1MBIT 2 https://ww1.microchip.com/downloads/en/DeviceDoc/1-Mbit-SPI-Bus-Serial-EEPROM-Data-Sheet-20001836K.pdf

            // https://github.com/adafruit/nvm.toml/tree/main/flash/winbond
            // https://www.adafruit.com/product/5634
            // datasheet https://www.winbond.com/hq/product/code-storage-flash-memory/serial-nor-flash/?__locale=en&partNo=W25Q128JV
            // datasheet https://www.winbond.com/hq/support/documentation/levelOne.jsp?__locale=en&DocNo=DA00-W25Q128JV
            //W25Q128JVSSIQ_16MB = 16408,
            WINBOND_25Q128JV_IN_IQ_JQ_16MB = 0x4018,
            WINBOND_25Q128JV_IM_JM = 0x7018,
        }

        public FLASH_DEVICE_ID DeviceID;

        public enum WINBOND_FLASH_DEVICE_ID
        {
            WINBOND_25Q128JV_16MB = 0x17,
        }

        public enum FLASH_ADDR_SIZE
        {
            TwoBytes    = 2,
            ThreeBytes  = 3,
        };

        public FLASH_ADDR_SIZE AddressSize = FLASH_ADDR_SIZE.ThreeBytes;

        public enum CYPRESS_SECTOR_ARCHITECTURE
        {
            SectorArchitecture_Uniform256KBSectors                       = 0,
            SectorArchitecture_4kBParameterSectorsWithUniform64kBSectors = 1,
            Undefined                                                    = 128
        }
        public CYPRESS_SECTOR_ARCHITECTURE SectorArchitecture = CYPRESS_SECTOR_ARCHITECTURE.Undefined;

        protected static Dictionary<string, string> CYPRESS_PACKAGE_MODEL_NUMBERS = new Dictionary<string, string>()
        {
            { "00", "SOIC 16"        },
            { "10", "SOIC 8 or WSON" },
            { "C0", "5x5 Ball BGA"   },
            { "D0", "4x6 Ball BGA"   },
            { ""  , "Undefined"      },
        };
        public string PackageModel;
                
        public enum CYPRESS_FAMILIY_ID
        {
            FL_S_Family = 0x80,
            Undefined = 0,
        }
        public CYPRESS_FAMILIY_ID FamilyID = CYPRESS_FAMILIY_ID.Undefined;

        [Flags]
        public enum StatusRegister1Enum : byte
        {
            Busy                       = 1 << 0,
            WriteEnableLatch           = 1 << 1,
            BlockProtectBits0          = 1 << 2,
            BlockProtectBits1          = 1 << 3,
            BlockProtectBits2          = 1 << 4,
            EraseErrorOccurred         = 1 << 5,
            ProgrammingErrorOccurred   = 1 << 6,
            StatusRegisterWriteDisable = 1 << 7,
        };

        [Flags]
        public enum StatusRegister2Enum : byte
        {
            ProgramSuspendMode                = 1 << 0,
            EraseSuspend                      = 1 << 1,
            ReservedForFutureUse2             = 1 << 2,
            ReservedForFutureUse3             = 1 << 3,
            ReservedForFutureUse4             = 1 << 4,
            IO3AlternateFunctionIsResetOrHold = 1 << 5,
            PageBufferWrap512Or256            = 1 << 6,
            BlockEraseSize256kOr64k           = 1 << 7,
        };
    }
}
