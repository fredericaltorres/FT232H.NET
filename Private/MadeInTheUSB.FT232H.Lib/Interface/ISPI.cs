using System;

namespace MadeInTheUSB.FT232H
{
    public interface ISPI {
    
        FtdiMpsseSPIResult Write(byte[] buffer);
        FtdiMpsseSPIResult Read(byte[] buffer);
        FtdiMpsseSPIResult QueryReadWriteTwoTransaction(byte [] bufferOut, byte [] bufferIn);
        FtdiMpsseSPIResult QueryReadWriteOneTransaction(byte [] bufferOut, byte [] buffer);
        bool Ok(FtdiMpsseSPIResult spiResult);

        void LogSpiTransaction(byte[] bufferOut, byte[] bufferIn,  string message = null, int recursiveCounter = 0);
    };

}