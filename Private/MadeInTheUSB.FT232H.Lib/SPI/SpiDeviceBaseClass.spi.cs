using System;
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
        private MpsseSpiConfig            _spiConfig; 
        private bool                      _isDisposed;
        private MpsseChannelConfiguration _ftdiMpsseChannelConfig;

        protected SpiDeviceBaseClass(MpsseSpiConfig spiConfig) : this(spiConfig, null)
        {
            this.GpioInit();
        }
        protected SpiDeviceBaseClass(MpsseSpiConfig spiConfig, MpsseChannelConfiguration channelConfig)
        {
            this._ftdiMpsseChannelConfig = channelConfig ?? MpsseChannelConfiguration.FtdiMpsseChannelZeroConfiguration;
            this._spiConfig              = spiConfig;
            this.InitLibAndHandle();
        }
        private void InitLibAndHandle()
        {
            if (_spiHandle != IntPtr.Zero)
                return;

            LibMpsse.Init();

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
            LibMpsse.Cleanup();
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
        public FtdiMpsseSPIResult Write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtSpiTransferOptions options = FtSpiTransferOptions.ToogleChipSelect)
        {
            EnforceRightConfiguration();
            return LibMpsse_AccessToCppDll.SPI_Write(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
        }
        public FtdiMpsseSPIResult Write(byte[] buffer)
        {
            int sizeTransfered = 0;
            return Write(buffer, buffer.Length, out sizeTransfered, FtSpiTransferOptions.ToogleChipSelect);
        }
        public FtdiMpsseSPIResult Read(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtSpiTransferOptions options)
        {
            EnforceRightConfiguration();
            return LibMpsse_AccessToCppDll.SPI_Read(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
        }
        public FtdiMpsseSPIResult ReadWrite(byte[] bufferSend, byte [] bufferReceive, FtSpiTransferOptions options)
        {
            EnforceRightConfiguration();
            int sizeTransferred;
            return LibMpsse_AccessToCppDll.SPI_ReadWrite(_spiHandle, bufferReceive, bufferSend, bufferSend.Length, out sizeTransferred, options);
        }
        public FtdiMpsseSPIResult QueryReadWrite(byte [] bufferSent, byte [] bufferReceived)
        {
            var r = ReadWrite(bufferSent, bufferReceived, FtSpiTransferOptions.ToogleChipSelect);
            return r;
        }
        public FtdiMpsseSPIResult Read(byte[] buffer)
        {
            int sizeTransfered;
            return Read(buffer, buffer.Length, out sizeTransfered, FtSpiTransferOptions.ToogleChipSelect);
        }        
        public FtdiMpsseSPIResult Query(byte [] bufferSent, byte [] bufferReceived)
        {
            int byteSent = 0;
            var ec = this.Write(bufferSent, bufferSent.Length, out byteSent, FtSpiTransferOptions.ChipselectEnable);
            if (ec == FtdiMpsseSPIResult.Ok)
            {
                ec = this.Read(bufferReceived, bufferReceived.Length, out byteSent, FtSpiTransferOptions.ChipselectDisable);
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
