using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// 
    /// User Guide For libMPSSE - SPI
    /// 
    ///     https://www.ftdichip.com/Support/Documents/AppNotes/AN_178_User%20Guide%20for%20LibMPSSE-SPI.pdf
    ///     
    /// http://www.eevblog.com/forum/projects/ftdi-2232h-in-mpsse-spi-mode-toil-and-trouble-example-code-needed/
    /// 
    /// I2C https://ftdichip.com/wp-content/uploads/2020/08/AN_255_USB-to-I2C-Example-using-the-FT232H-and-FT201X-devices-1.pdf
    /// https://ftdichip.com/software-examples/mpsse-projects/libmpsse-i2c-examples/
    /// https://ftdichip.com/wp-content/uploads/2020/08/AN_177_User_Guide_For_LibMPSSE-I2C.pdf
    /// // https://ftdichip.com/wp-content/uploads/2020/07/AN_355_FT232H-MPSSE-Example-I2C-Master-Interface-with-Visual-Basic.pdf
    // https://ftdichip.com/wp-content/uploads/2020/07/AN_411_FTx232H-MPSSE-I2C-Master-Example-in-Csharp.pdf
    // source https://ftdichip.com/wp-content/uploads/2020/07/AN_411_Source.zip
    // https://ftdichip.com/software-examples/code-examples/csharp-examples/
    /// </summary>
    /// 
    // https://github.com/jakkaj/xIOT
    // FTDI SOURCE CODE
    // C:\DVT\FT232H.NET\Private\MadeInTheUSB.FT232H.Lib\libmpsse-windows-1.0.3



    
    internal class LibMpsse_AccessToCppDll
    {
        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult I2C_GetNumChannels(out int numChannels);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult I2C_OpenChannel(int index, out System.IntPtr handle);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult I2C_InitChannel(System.IntPtr handle, ref SpiChannelConfig config);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult I2C_GetChannelInfo(int index, out FTDIDeviceInfo chanInfo);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult I2C_DeviceRead(System.IntPtr handle, int deviceAddress, int sizeToTransfer, byte[] buffer, out int sizeTransfered, FtI2CTransferOptions options);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult I2C_DeviceWrite(System.IntPtr handle, int deviceAddress, int sizeToTransfer, byte[] buffer, out int sizeTransfered, FtI2CTransferOptions options);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult FT_WriteGPIO(System.IntPtr handle, byte dir, byte value);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult FT_ReadGPIO(System.IntPtr handle, out int value);





        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_OpenChannel(int index, out IntPtr handle);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_CloseChannel(IntPtr handle);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_GetNumChannels(out int numChannels);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_GetChannelInfo(int index, out MpsseDeviceInfo chanInfo);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_InitChannel(IntPtr handle, ref SpiConfig config);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_ChangeCS(IntPtr handle, FtdiMpsseSpiConfigOptions spiConfigOptions);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_IsBusy(IntPtr handle, out bool state);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_Read(
            IntPtr handle,
            byte[] buffer,
            int sizeToTransfer,
            out int sizeTransfered,
            FtSpiTransferOptions options);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_ReadWrite(
            IntPtr handle,
            byte[] inBuffer,
            byte[] outBuffer,
            int sizeToTransfer,
            out int sizeTransferred,
            FtSpiTransferOptions transferOptions);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_Write(IntPtr handle, byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtSpiTransferOptions options);

        // Written by Fred on 01.2016
        // http://www.ftdichip.com/Support/Documents/AppNotes/AN_178_User%20Guide%20for%20LibMPSSE-SPI.pdf
        //Private Declare Function MPSEE_SPI_GPIOWrite Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, 
        // ByVal Direction As Byte, ByVal Value As Byte) As UInt32
        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult FT_WriteGPIO(
            IntPtr handle,
            int direction /*0-in 1-out*/,
            int value /*0-low 1-high*/);

        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult FT_WriteGPIO(IntPtr handle, int value /*0-low 1-high*/);
        

        //[DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        //public static extern FtdiMpsseSPIResult FT_ReadGPIO(IntPtr handle, out int value);
        
        [DllImport(libMPSSE_Initializator.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 MPSSE_I2C_GetChannels(UInt32 numberOfChannels);

        [DllImportAttribute(libMPSSE_Initializator.DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt32 I2C_DeviceWrite(IntPtr handle, UInt32 deviceAddress, UInt32 sizeToTransfer, byte[] buffer, out UInt32 sizeTransferred, UInt32 options);

        
    }
}
