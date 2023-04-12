using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.FT232H.Components
{
    /// <summary>
    /// GOOD PDF ABOUT (MPSSE) http://www.ftdichip.com/Support/Documents/AppNotes/AN_135_MPSSE_Basics.pdf
    /// </summary>
    public partial class FlashMemory
    {
        // https://www.application-datasheet.com/pdf/macronix/mx25l6406embi-12g.pdf
        // https://files.seeedstudio.com/wiki/github_weiruanexample/Flash_P25Q16H-UXH-IR_Datasheet.pdf

        public enum FLASH_COMMAND {
            WRITE_STATUS_REGISTER_WRSR      = 0x01,    // Write Status Register Instruction 
            WRITE_PAGE_PROGRAM_PP           = 0x02,
            EEPROM_READ                     = 0x03,
            RESET_WRITE_ENABLE_LATCH_WRDI   = 0x04,    // Write Disable
            READ_STATUS_REGISTER_RDSR       = 0x05,    // Read Status Register Instruction 
            WRITE_ENABLE_LATCH_WREN         = 0x06,    // Write Enable
            READ_STATUS_REGISTER_2          = 0x07,    // Read Status Register Instruction 
            READ_IDENTIFICATION             = 0x9F,
            PAGE_ERASE_PE                   = 0x81,
            WINBOND_GET_INFO                = 0x90,
        }

        public enum ERASE_BLOCK_SIZE {
            SECTOR_ERASE    = 0x20,   // Erase one sector/4K block of flash memory
            BLOCK_32K       = 0x52,   // erase one 32K block of flash memory
            BLOCK_64K       = 0xD8,   // erase one 64K block of flash memory
            CHIP_ERASE      = 0x60    // chip erase (may take several seconds depending on size)
        }
        
        public const int _64K_BLOCK_SIZE = 64 * 1024;
        const int EEPROM_DEFAULT_PAGE_SIZE_WRITE = 512;
        const int MAX_TRY = 8;
    }
}