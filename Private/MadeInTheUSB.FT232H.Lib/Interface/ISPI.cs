using System;

namespace MadeInTheUSB.FT232H
{
    public interface ISPI {
    
        FtdiMpsseSPIResult Write(byte[] buffer, SpiChipSelectPins cs);
        FtdiMpsseSPIResult Read(byte[] buffer, SpiChipSelectPins cs);
        FtdiMpsseSPIResult QueryReadWriteTwoTransaction(byte [] bufferOut, byte [] bufferIn, SpiChipSelectPins cs);
        FtdiMpsseSPIResult QueryReadWriteOneTransaction(byte [] bufferOut, byte [] buffer, SpiChipSelectPins cs);
        bool Ok(FtdiMpsseSPIResult spiResult);

        void LogSpiTransaction(byte[] bufferOut, byte[] bufferIn,  string message = null, int recursiveCounter = 0);
    };

}