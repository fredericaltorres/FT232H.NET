using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static MadeInTheUSB.FT232H.SpiConfig;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Implement the SPI methods
    /// </summary>
    public partial class SpiDevice : FT232HDeviceBaseClass, IDisposable, IDigitalWriteRead, ISPI
    {
        /// <summary>
        /// FT232H has only one channel, channel 0
        /// FT2232 has 2 channels, not supportted
        /// </summary>
        private SpiConfig                   _spiConfig;
        private bool                        _isDisposed;
        private MpsseChannelConfiguration   _ftdiMpsseChannelConfig;

        public SpiDevice(SpiClockSpeeds clockSpeed, SpiChipSelectPins chipSelect = SpiChipSelectPins.CsDbus3) : 
            this(SpiConfig.BuildSPI(clockSpeed, chipSelect), null)
        {
            this.GpioInit();
        }

        public SpiDevice(SpiConfig spiConfig, MpsseChannelConfiguration channelConfig)
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
                throw new SpiChannelNotConnectedException(FtdiMpsseResult.InvalidHandle);

            result = CheckResult(LibMpsse_AccessToCppDll.SPI_InitChannel(_spiHandle, ref _spiConfig));
        }

        public void Dispose()
        {
            if (this._isDisposed)
                return;
            this._isDisposed = true;
            libMPSSE_Initializator.Cleanup();
        }

        public FtdiMpsseResult CheckResult(FtdiMpsseResult spiResult)
        {
            if (spiResult != FtdiMpsseResult.Ok)
                throw new SpiChannelNotConnectedException(spiResult);
            return spiResult;
        }

        private void EnforceRightConfiguration(SpiChipSelectPins cs)
        {
            if (!_spiConfig.IsChipSelect(cs))
            {
                _spiConfig.ChangeChipSelect(cs);
                LibMpsse_AccessToCppDll.SPI_ChangeCS(_spiHandle, _spiConfig.spiConfigOptions);
            }
        }

        public bool Ok(FtdiMpsseResult spiResult)
        {
            return (spiResult == FtdiMpsseResult.Ok);
        }

        private FtdiMpsseResult _write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, SpiChipSelectPins cs, FtdiSpiTransferOptions options = FtdiSpiTransferOptions.ToogleChipSelect)
        {
            EnforceRightConfiguration(cs);
            var r = LibMpsse_AccessToCppDll.SPI_Write(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
            base.LogSpiTransaction(buffer, new byte[0]);
            return r;
        }

        public FtdiMpsseResult _read(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiSpiTransferOptions options, SpiChipSelectPins cs)
        {
            EnforceRightConfiguration(cs);
            var r = LibMpsse_AccessToCppDll.SPI_Read(_spiHandle, buffer, sizeToTransfer, out sizeTransfered, options);
            base.LogSpiTransaction(new byte[0], buffer);
            return r;
        }

        private FtdiMpsseResult _readWriteOneTransaction(byte[] bufferSend, byte[] bufferReceive, FtdiSpiTransferOptions options, SpiChipSelectPins cs)
        {
            EnforceRightConfiguration(cs);
            int sizeTransferred;
            var r = LibMpsse_AccessToCppDll.SPI_ReadWrite(_spiHandle, bufferReceive, bufferSend, bufferSend.Length, out sizeTransferred, options);
            base.LogSpiTransaction(bufferSend, bufferReceive);
            return r;
        }

        public FtdiMpsseResult Write(byte[] buffer, SpiChipSelectPins cs)
        {
            int sizeTransfered = 0;
            return _write(buffer, buffer.Length, out sizeTransfered, cs, FtdiSpiTransferOptions.ToogleChipSelect);
        }

        public FtdiMpsseResult QueryReadWriteOneTransaction(byte [] bufferSent, byte [] bufferReceived, SpiChipSelectPins cs)
        {
            var r = _readWriteOneTransaction(bufferSent, bufferReceived, FtdiSpiTransferOptions.ToogleChipSelect, cs);
            return r;
        }

        public FtdiMpsseResult Read(byte[] buffer, SpiChipSelectPins cs)
        {
            int sizeTransfered;
            var r = _read(buffer, buffer.Length, out sizeTransfered, FtdiSpiTransferOptions.ToogleChipSelect, cs);
            return r;
        }        

        public FtdiMpsseResult QueryReadWriteTwoTransaction(byte [] bufferSent, byte [] bufferReceived, SpiChipSelectPins cs)
        {
            int byteSent = 0;
            var ec = this._write(bufferSent, bufferSent.Length, out byteSent, cs, FtdiSpiTransferOptions.ChipselectEnable);
            if (ec == FtdiMpsseResult.Ok)
            {
                ec = this._read(bufferReceived, bufferReceived.Length, out byteSent, FtdiSpiTransferOptions.ChipselectDisable, cs);
                return (ec == FtdiMpsseResult.Ok) ? FtdiMpsseResult.Ok : ec;
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
