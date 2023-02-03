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
    /// </summary>
    internal class LibMpsse_AccessToCppDll
    {
        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_OpenChannel(int index, out IntPtr handle);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_CloseChannel(IntPtr handle);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_GetNumChannels(out int numChannels);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_GetChannelInfo(int index, out MpsseDeviceInfo chanInfo);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_InitChannel(IntPtr handle, ref MpsseSpiConfig config);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_ChangeCS(IntPtr handle, FtdiMpsseSpiConfigOptions spiConfigOptions);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_IsBusy(IntPtr handle, out bool state);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_Read(
            IntPtr handle,
            byte[] buffer,
            int sizeToTransfer,
            out int sizeTransfered,
            FtSpiTransferOptions options);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_ReadWrite(
            IntPtr handle,
            byte[] inBuffer,
            byte[] outBuffer,
            int sizeToTransfer,
            out int sizeTransferred,
            FtSpiTransferOptions transferOptions);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult SPI_Write(
            IntPtr handle,
            byte[] buffer,
            int sizeToTransfer,
            out int sizeTransfered,
            FtSpiTransferOptions options);

        // Written by Fred on 01.2016
        // http://www.ftdichip.com/Support/Documents/AppNotes/AN_178_User%20Guide%20for%20LibMPSSE-SPI.pdf
        //Private Declare Function MPSEE_SPI_GPIOWrite Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, 
        // ByVal Direction As Byte, ByVal Value As Byte) As UInt32
        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult FT_WriteGPIO(
            IntPtr handle,
            int direction /*0-in 1-out*/,
            int value /*0-low 1-high*/);

        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public extern static FtdiMpsseSPIResult FT_WriteGPIO(
            IntPtr handle,
            int value /*0-low 1-high*/);
        
        //Private Declare Function MPSSE_I2C_GetChannels Lib "libmpsse" Alias "I2C_GetNumChannels" (ByRef NumberOfChannels As UInt32) As UInt32
        //Private Declare Function MPSSE_SPI_I2CChannelinfo Lib "libmpsse" Alias "I2C_GetChannelInfo" (ByVal Index As UInt32, ByRef DeviceInfo As DeviceInfoStructure) As UInt32
        //Private Declare Function MPSEE_SPI_GPIORead Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, ByRef Value As Byte) As UInt32

        //Private Declare Function MPSEE_SPI_GPIORead Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, ByRef Value As Byte) As UInt32
        [DllImport(LibMpsse.DllName, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern FtdiMpsseSPIResult FT_ReadGPIO(IntPtr handle, out int value);
    }
}
/*
From http://www.eevblog.com/forum/projects/ftdi-2232h-in-mpsse-spi-mode-toil-and-trouble-example-code-needed/
 * 
Structure ChannelConfig
        Dim clockrate As UInt32
        Dim latencytimer As Byte
        Dim ConfigOptions As UInt32
        Dim Pins As UInt32
        Dim Reserved As UInt16
End Structure
Dim FtdiMpsseChannelConfiguration As ChannelConfig
Dim MPSSE_Status As UInt32
Dim MPSSE_Handle As UInt32

Private Declare Function MPSSE_SPI_GetChannels Lib "libmpsse" Alias "SPI_GetNumChannels" (ByRef NumberOfChannels As UInt32) As UInt32
Private Declare Function MPSSE_SPI_GetChannelinfo Lib "libmpsse" Alias "SPI_GetChannelInfo" (ByVal Index As UInt32, ByRef DeviceInfo As DeviceInfoStructure) As UInt32
Private Declare Function MPSSE_SPI_OpenChannel Lib "libmpsse" Alias "SPI_OpenChannel" (ByVal Index As UInt32, ByRef SPI_ProbeHandle As UInt32) As UInt32
Private Declare Function MPSSE_SPI_InitChannel Lib "libmpsse" Alias "SPI_InitChannel" (ByVal SPI_ProbeHandle As UInt32, ByRef FtdiMpsseChannelConfiguration As ChannelConfig) As UInt32
Private Declare Function MPSSE_SPI_CloseChannel Lib "libmpsse" Alias "SPI_Closechannel" (ByVal SPI_ProbeHandle As UInt32) As UInt32
Private Declare Function MPSSE_SPI_Read Lib "libmpsse" Alias "SPI_read" (ByVal SPI_ProbeHandle As UInt32, ByRef Readbuffer() As Byte, ByVal SizeToRead As UInt32, ByRef SizeRead As UInt32, ByVal TransferOptions As UInt32) As UInt32
Private Declare Function MPSEE_SPI_Write Lib "libmpsse" Alias "SPI_Write" (ByVal SPI_ProbeHandle As UInt32, ByVal Writebuffer() As Byte, ByVal SizeToWrite As UInt32, ByRef SizeWritten As UInt32, ByVal TransferOptions As UInt32) As UInt32
Private Declare Function MPSEE_SPI_ReadWrite Lib "libmpsse" Alias "SPI_ReadWrite" (ByVal SPI_ProbeHandle As UInt32, ByRef Readbuffer() As Byte, ByVal Writebuffer() As Byte, ByVal SizeTotransfer As UInt32, ByRef Sizetransferred As UInt32, ByVal TransferOptions As UInt32) As UInt32
Private Declare Function MPSEE_SPI_CheckBusy Lib "libmpsse" Alias "SPI_IsBusy" (ByVal SPI_ProbeHandle As UInt32, ByRef State As Byte) As UInt32
Private Declare Function MPSEE_SPI_ChangeCS Lib "libmpsse" Alias "SPI_ChangeCS" (ByVal SPI_ProbeHandle As UInt32, ByVal Options As UInt32) As UInt32
Private Declare Function MPSEE_SPI_GPIOWrite Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, ByVal Direction As Byte, ByVal Value As Byte) As UInt32
Private Declare Function MPSEE_SPI_GPIORead Lib "libmpsse" Alias "FT_WriteGPIO" (ByVal SPI_ProbeHandle As UInt32, ByRef Value As Byte) As UInt32

Private Declare Function MPSSE_I2C_GetChannels Lib "libmpsse" Alias "I2C_GetNumChannels" (ByRef NumberOfChannels As UInt32) As UInt32
Private Declare Function MPSSE_SPI_I2CChannelinfo Lib "libmpsse" Alias "I2C_GetChannelInfo" (ByVal Index As UInt32, ByRef DeviceInfo As DeviceInfoStructure) As UInt32

    Const Options_SPI_Mode0 As UInt32 = 0
    Const Options_SPI_Mode1 As UInt32 = 1
    Const Options_SPI_Mode2 As UInt32 = 2
    Const Options_SPI_Mode3 As UInt32 = 3
    Const Options_CS_DB3 As UInt32 = 0
    Const Options_CS_DB4 As UInt32 = 4
    Const Options_CS_DB5 As UInt32 = 8
    Const Options_CS_DB6 As UInt32 = 12
    Const Options_CS_DB7 As UInt32 = 16
    Const Options_CSmode_ActiveHigh As UInt32 = 0
    Const Options_CSmode_ActiveLow As UInt32 = 32
    Const TransferOptions_Inbits As UInt32 = 1
    Const TransferOptions_Inbytes As UInt32 = 0
    Const TransferOptions_CSAtStart As UInt32 = 2
    Const TransferOptions_CSAtStop As UInt32 = 4
 
 */