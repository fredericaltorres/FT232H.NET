﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Implement the SPI methods
    /// </summary>
    public abstract partial class SpiDeviceBaseClass : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead, ISPI
    {
        /// <summary>
        /// FT232H has only one channel, channel 0
        /// FT2232 has 2 channels, not supportted
        /// </summary>
        private SpiConfig            _spiConfig; 
        private bool                      _isDisposed;
        private MpsseChannelConfiguration _ftdiMpsseChannelConfig;

        protected SpiDeviceBaseClass(SpiConfig spiConfig) : this(spiConfig, null)
        {
            this.GpioInit();
        }
        protected SpiDeviceBaseClass(SpiConfig spiConfig, MpsseChannelConfiguration channelConfig)
        {
            this._ftdiMpsseChannelConfig = channelConfig ?? MpsseChannelConfiguration.FtdiMpsseChannelZeroConfiguration;
            this._spiConfig              = spiConfig;
            this.InitLibAndHandle();
        }
        private void InitLibAndHandle()
        {
            if (_spiHandle != IntPtr.Zero)
                return;

            libMPSSE_Initializator.Init();

            var result = CheckResult(LibMpsse_AccessToCppDll.SPI_OpenChannel(_ftdiMpsseChannelConfig.ChannelIndex, out _spiHandle));

            if (_spiHandle == IntPtr.Zero)
                throw new SpiChannelNotConnectedException(FtdiMpsseSPIResult.InvalidHandle);

            result = CheckResult(LibMpsse_AccessToCppDll.SPI_InitChannel(_spiHandle, ref _spiConfig));
            _globalConfig = this._spiConfig;
        }
        public void Dispose()
        {
            if (this._isDisposed)
                return;
            this._isDisposed = true;
            libMPSSE_Initializator.Cleanup();
        }
        public FtdiMpsseSPIResult CheckResult(FtdiMpsseSPIResult spiResult)
        {
            if (spiResult != FtdiMpsseSPIResult.Ok)
                throw new SpiChannelNotConnectedException(spiResult);
            return spiResult;
        }
        private void EnforceRightConfiguration()
        {
            if (_globalConfig.spiConfigOptions != _spiConfig.spiConfigOptions)
            {
                LibMpsse_AccessToCppDll.SPI_ChangeCS(_spiHandle, _spiConfig.spiConfigOptions);
                _globalConfig = _spiConfig;
            }
        }
        // ISPI implementation
        public bool Ok(FtdiMpsseSPIResult spiResult)
        {
            return (spiResult == FtdiMpsseSPIResult.Ok);
        }
        public FtdiMpsseSPIResult Write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiSpiTransferOptions options = FtdiSpiTransferOptions.ToogleChipSelect)
        {
            EnforceRightConfiguration();
            return LibMpsse_AccessToCppDll.SPI_Write(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
        }
        public FtdiMpsseSPIResult Write(byte[] buffer)
        {
            int sizeTransfered = 0;
            base.LogSpiTransaction(buffer, new byte[0]);
            return Write(buffer, buffer.Length, out sizeTransfered, FtdiSpiTransferOptions.ToogleChipSelect);
        }
        public FtdiMpsseSPIResult Read(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiSpiTransferOptions options)
        {
            EnforceRightConfiguration();
            var r = LibMpsse_AccessToCppDll.SPI_Read(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
            base.LogSpiTransaction(new byte[0], buffer);
            return r;
        }
        public FtdiMpsseSPIResult ReadWrite(byte[] bufferSend, byte [] bufferReceive, FtdiSpiTransferOptions options)
        {
            EnforceRightConfiguration();
            int sizeTransferred;
            var r = LibMpsse_AccessToCppDll.SPI_ReadWrite(_spiHandle, bufferReceive, bufferSend, bufferSend.Length, out sizeTransferred, options);
            base.LogSpiTransaction(bufferSend, bufferReceive);
            return r;
        }
        public FtdiMpsseSPIResult QueryReadWrite(byte [] bufferSent, byte [] bufferReceived)
        {
            var r = ReadWrite(bufferSent, bufferReceived, FtdiSpiTransferOptions.ToogleChipSelect);
            base.LogSpiTransaction(bufferSent, bufferReceived);
            return r;
        }
        public FtdiMpsseSPIResult Read(byte[] buffer)
        {
            int sizeTransfered;
            var r = Read(buffer, buffer.Length, out sizeTransfered, FtdiSpiTransferOptions.ToogleChipSelect);
            base.LogSpiTransaction(new byte[0], buffer);
            return r;
        }        
        public FtdiMpsseSPIResult Query(byte [] bufferSent, byte [] bufferReceived)
        {
            int byteSent = 0;
            var ec = this.Write(bufferSent, bufferSent.Length, out byteSent, FtdiSpiTransferOptions.ChipselectEnable);
            if (ec == FtdiMpsseSPIResult.Ok)
            {
                ec = this.Read(bufferReceived, bufferReceived.Length, out byteSent, FtdiSpiTransferOptions.ChipselectDisable);

                base.LogSpiTransaction(bufferSent, bufferReceived);

                return (ec == FtdiMpsseSPIResult.Ok) ? FtdiMpsseSPIResult.Ok : ec;
            }
            else return ec;
        }
        private byte[] MakeBuffer(int count)
        {
            // Buffer contains 0. Value does not matter. all we need is to send some clock to the slave to read the value
            return new byte[count];
        }
    }
}
