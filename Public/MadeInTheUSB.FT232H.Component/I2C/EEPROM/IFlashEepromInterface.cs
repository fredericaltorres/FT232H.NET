using System.Collections.Generic;

namespace MadeInTheUSB.FT232H.Component.I2C
{
    public interface IFlashEepromInterface
    {
        int Max64KbBlock { get; }
        int MaxPage { get; }
        int PageSize { get; }
        int SizeInByte { get; }
        int SizeInKByte { get; }
        int SizeInMByte { get; }

        bool ReadPages(int addr, int byteToRead, List<byte> buffer);
        bool WritePages(int addr, List<byte> buffer, bool verify = false, bool eraseBlock = true);
    }
}