using System;

namespace MadeInTheUSB.FT232H
{
    public interface ISPI {
    
        FtdiMpsseResult Write(byte[] buffer, SpiChipSelectPins cs);
        FtdiMpsseResult Read(byte[] buffer, SpiChipSelectPins cs);
        FtdiMpsseResult QueryReadWriteTwoTransaction(byte [] bufferOut, byte [] bufferIn, SpiChipSelectPins cs);
        FtdiMpsseResult QueryReadWriteOneTransaction(byte [] bufferOut, byte [] buffer, SpiChipSelectPins cs);
        bool Ok(FtdiMpsseResult spiResult);

        void LogSpiTransaction(byte[] bufferOut, byte[] bufferIn,  string message = null, int recursiveCounter = 0);
    };

}