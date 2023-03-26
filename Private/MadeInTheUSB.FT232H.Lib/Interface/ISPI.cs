using System;

namespace MadeInTheUSB.FT232H
{
    public interface ISPI {
        FtdiMpsseSPIResult Write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiSpiTransferOptions options = FtdiSpiTransferOptions.ToogleChipSelect);
        FtdiMpsseSPIResult Write(byte[] buffer);
        FtdiMpsseSPIResult Read(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiSpiTransferOptions options);
        FtdiMpsseSPIResult Read(byte[] buffer);
        /// <summary>
        /// Send the bufferOut then read the bufferIn
        /// </summary>
        /// <param name="bufferOut"></param>
        /// <param name="bufferIn"></param>
        /// <returns></returns>
        FtdiMpsseSPIResult Query(byte [] bufferOut, byte [] bufferIn);
        /// <summary>
        /// Send and Read the buffers at the same time
        /// </summary>
        /// <param name="bufferOut"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        FtdiMpsseSPIResult QueryReadWrite(byte [] bufferOut, byte [] buffer);
        bool Ok(FtdiMpsseSPIResult spiResult);
        
    };

}