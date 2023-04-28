using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// https://ftdichip.com/wp-content/uploads/2020/08/AN_177_User_Guide_For_LibMPSSE-I2C.pdf
    /// DRIVER: C:\DVT\FT232H.NET\Private\MadeInTheUSB.FT232H.Lib\libmpsse-windows-1.0.3\release\Project\VisualStudio\libmpsse.sln
    /// </summary>
    public class I2CDevice : FT232HDeviceBaseClass
    {
        private static IntPtr _handle = IntPtr.Zero;
        private static SpiChannelConfig _currentGlobalConfig;

        private SpiChannelConfig _config;
        private I2CConfiguration _i2cConfig;
        private I2CClockSpeeds _clockSpeed;
        private const int _latencyTimer = 200; //255; // Hz
        private byte _direction;
        private byte _gpo;
        public  FtdiI2CTransferOptions FastModeFlag = FtdiI2CTransferOptions.None;

        public IDigitalWriteRead Gpios = null;
        public I2CGpioIImplementationDevice GpiosPlus;
        public bool HardwareProgressBarOn = false;

        public I2CDevice(I2CClockSpeeds clockSpeed = I2CClockSpeeds.FAST_MODE_400Khz, bool hardwareProgressBarOn = false, bool fastMode = true)
        {
            if(fastMode)
            {
                FastModeFlag = FtdiI2CTransferOptions.FastTransfer;
            }
            _clockSpeed = clockSpeed;
            this.Init();
            HardwareProgressBarOn = hardwareProgressBarOn;
        }

        private bool __Init()
        {
            var config = new SpiChannelConfig
            {
                ClockRate = (int)_clockSpeed,
                LatencyTimer = _latencyTimer
            };

            _i2cConfig = _i2cConfig ?? I2CConfiguration.ChannelZeroConfiguration;
            _config = config;

            InitLibAndHandle();

            this.GpiosPlus = new I2CGpioIImplementationDevice(this);
            this.Gpios = this.GpiosPlus;

            return true;
        }

        public bool Init()
        {
            return __Init();
        }

        void InitLibAndHandle()
        {
            FtdiMpsseResult result;

            if (_handle != IntPtr.Zero)
                return;

            libMPSSE_Initializator.Init();
            var num_channels = 0;
            var channels = LibMpsse_AccessToCppDll.I2C_GetNumChannels(out num_channels);

            CheckResult(channels);

            if (num_channels > 0)
            {
                for (var i = 0; i < num_channels; i++)
                {
                    FTDIDeviceInfo cInfo;
                    var channelInfoStatus = LibMpsse_AccessToCppDll.I2C_GetChannelInfo(i, out cInfo);
                    CheckResult(channelInfoStatus);
                    //System.Console.WriteLine($"Flags: {cInfo.Flags}");
                    //System.Console.WriteLine($"Type: {cInfo.Type}");
                    //System.Console.WriteLine($"ID: {cInfo.ID}");
                    //System.Console.WriteLine($"LocId: {cInfo.LocId}");
                    System.Console.WriteLine($"Nusbio/2 (FT232H-I2C mode) SerialNumber: {cInfo.SerialNumber}, Description: {cInfo.Description}");
                }
            }

            result = LibMpsse_AccessToCppDll.I2C_OpenChannel(_i2cConfig.ChannelIndex, out _handle);

            CheckResult(result);

            if (_handle == IntPtr.Zero)
                throw new I2CException(FtdiMpsseResult.InvalidHandle);

            result = LibMpsse_AccessToCppDll.I2C_InitChannel(_handle, ref _config);

            CheckResult(result);
            _currentGlobalConfig = _config;
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            throw new NotImplementedException();
        }

        public void OnHardwareProgressBar()
        {
            if(HardwareProgressBarOn)
                this.GpiosPlus.ProgressNext();
        }

        public bool WriteBuffer(byte[] array, int deviceId, bool terminateTransmission = true)
        {
            OnHardwareProgressBar();
            return _write(array, deviceId, terminateTransmission);
        }

        bool _write(byte[] array, int deviceId, bool terminateTransmission = true)
        {
            int writtenAmount;

            base.LogI2CTransaction(I2CTransactionType.WRITE, (byte)deviceId, array, null);
            var flags = FastModeFlag | FtdiI2CTransferOptions.StartBit | FtdiI2CTransferOptions.BreakOnNack;
            if(terminateTransmission)
                flags |= FtdiI2CTransferOptions.StopBit;

            var result = _write(array, array.Length, out writtenAmount, flags, (byte)deviceId);

            return result == FtdiMpsseResult.Ok;
        }

        public bool Write(int value, byte deviceId, bool checkForNAck = false)
        {
            this.OnHardwareProgressBar();
            var array = new byte[1];
            array[0] = Convert.ToByte(value);
            int writtenAmount;

            /*
            This parameter specifies data transfer options. The bit positions
            defined for each of these options are:
            BIT0: if set then a start condition is generated in the I2C bus
            before the transfer begins.A bit mask is defined for this options
            in file ftdi_i2c.h as I2C_TRANSFER_OPTIONS_START_BIT
            BIT1: if set then a stop condition is generated in the I2C bus
            after the transfer ends.A bit mask is defined for this options in
            file ftdi_i2c.h as I2C_TRANSFER_OPTIONS_STOP_BIT
            BIT2: if set then the function will return when a device nAcks
            after a byte has been transferred. If not set then the function
            will continue transferring the stream of bytes even if the device
            nAcks.A bit mask is defined for this options in file ftdi_i2c.h as
            I2C_TRANSFER_OPTIONS_BREAK_ON_NACK
            BIT3: reserved(only used in I2C_DeviceRead)
            BIT4: setting this bit will invoke a multi byte I2C transfer
            without having delays between the START, ADDRESS, DATA and
            STOP phases.Size of the transfer in parameters sizeToTransfer
            and sizeTransferred are in bytes.The bit mask defined for this
            bit is I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES *
            BIT5: setting this bit would invoke a multi bit transfer without
            having delays between the START, ADDRESS, DATA and STOP
            phases.Size of the transfer in parameters sizeToTransfer and
            sizeTransferred are in bytes.The bit mask defined for this bit is
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS *
            BIT6: the deviceAddress parameter is ignored if this bit is set.
            This feature may be useful in generating a special I2C bus
            conditions that do not require any address to be passed. Setting
            this bit is effective only when either
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES or
            I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS is set.The bit
            mask defined for this bit is
            I2C_TRANSFER_OPTIONS_NO_ADDRESS*
            BIT7 – BIT31: reserved
            *The I2C_DeviceRead and I2C_DeviceWrite functions send commands to
            */

            var flags = FastModeFlag | FtdiI2CTransferOptions.StartBit | FtdiI2CTransferOptions.StopBit | FtdiI2CTransferOptions.BreakOnNack;
            
            if (checkForNAck)
            {
                // flags |= FtdiI2CTransferOptions.BreakOnNack | FtdiI2CTransferOptions.NackLastByte;
                flags |= FtdiI2CTransferOptions.NackLastByte;
            }
            var result = _write(array, array.Length, out writtenAmount, flags, deviceId);
            return result == FtdiMpsseResult.Ok;
        }
        // https://www.ftdichip.com/Support/Documents/AppNotes/AN_177_User_Guide_For_LibMPSSE-I2C.pdf
        // https://github.com/DonRuss/libMPSSE-EEPROM/blob/master/libMPSSE_EEPROM.py
        public FtdiMpsseResult _write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiI2CTransferOptions options, byte deviceId)
        {
            var result = LibMpsse_AccessToCppDll.I2C_DeviceWrite(_handle, deviceId, sizeToTransfer, buffer, out sizeTransfered, options);
            if(result == FtdiMpsseResult.Ok)
                base.LogI2CTransaction(I2CTransactionType.WRITE, deviceId, null, buffer);
            else
                base.LogI2CTransaction(I2CTransactionType.ERROR, deviceId, null, null);
            return result;
        }

        public bool Write1ByteReadUInt16WithRetry(byte reg, UInt16 expected, byte deviceId)
        {
            if (this.Write1ByteReadUInt16(reg, deviceId) == expected)
                return true;
            Thread.Sleep(100);
            if (this.Write1ByteReadUInt16(reg, deviceId) == expected)
                return true;
            return false;
        }

        public UInt16 Write1ByteReadUInt16(byte reg, byte deviceId)
        {
            this.Write(reg, deviceId);
            var buffer = this.ReadXByte(2, deviceId);
            var value = (buffer[0] << 8) + buffer[1];
            return (UInt16)value;
        }

        public List<byte> ReadXByte(int count, byte deviceId)
        {
            var buffer = new byte[count];
            if (_readBuffer(buffer, deviceId))
            {
                base.LogI2CTransaction(I2CTransactionType.READ, deviceId, null, buffer);
                return buffer.ToList();
            }
            else
            {
                base.LogI2CTransaction(I2CTransactionType.ERROR, deviceId, null, null);
                return null;
            }
        }

        public int Read1Byte(byte deviceId)
        {
            var buffer = new byte[1];
            if (_readBuffer(buffer, deviceId))
            {
                return buffer[0];
            }
            else return -1;
        }

        public int Write1ByteRead2Byte(byte reg, byte deviceId)
        {
            this.Write(reg, deviceId);
            var buffer = this.ReadXByte(2, deviceId);
            var value = (buffer[0] << 8) + buffer[1];

            return (UInt16)value;
        }

        private FtdiMpsseResult _read2(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtdiI2CTransferOptions options, byte deviceid)
        {
            var result = LibMpsse_AccessToCppDll.I2C_DeviceRead(_handle, deviceid, sizeToTransfer, buffer, out sizeTransfered, options);
            CheckResult(result);
            if (result == FtdiMpsseResult.Ok)
                base.LogI2CTransaction(I2CTransactionType.READ, deviceid, null, buffer);
            else
                base.LogI2CTransaction(I2CTransactionType.ERROR, deviceid, null, null);

            return result;
        }

        public bool DetectDevice(byte deviceId)
        {
            byte[] buffer = new byte[1] { 0 };
            int sizeTransfered = 0;
            var flags = FastModeFlag | FtdiI2CTransferOptions.StartBit | FtdiI2CTransferOptions.StopBit;//| FtdiI2CTransferOptions.BreakOnNack;
            var result = LibMpsse_AccessToCppDll.I2C_DeviceRead(_handle, deviceId, buffer.Length, buffer, out sizeTransfered, flags);
            base.LogI2CTransaction(I2CTransactionType.DETECT_DEVICE, deviceId, null, null, $"{result == FtdiMpsseResult.Ok}");
            return result == FtdiMpsseResult.Ok;
        }

        private bool _readBuffer(byte[] buffer, byte deviceId)
        {
            int sizeTransfered = 0;
            // var flags = FtdiI2CTransferOptions.StartBit | FtdiI2CTransferOptions.StopBit; // DOES NOT WORK FOR EEPROM
            //var flags = FtdiI2CTransferOptions.StartBit ;
            var flags = FastModeFlag | FtdiI2CTransferOptions.StartBit | FtdiI2CTransferOptions.StopBit | FtdiI2CTransferOptions.BreakOnNack;

            var result = LibMpsse_AccessToCppDll.I2C_DeviceRead( _handle, deviceId, buffer.Length, buffer, out sizeTransfered, flags);
            CheckResult(result);
            if (result == FtdiMpsseResult.Ok)
                base.LogI2CTransaction(I2CTransactionType.READ, deviceId, null, buffer);
            else
                base.LogI2CTransaction(I2CTransactionType.ERROR, deviceId, null, null);
            return result == FtdiMpsseResult.Ok;
        }

        protected static void CheckResult(FtdiMpsseResult result)
        {
            if (result != FtdiMpsseResult.Ok)
                throw new I2CException(result);
        }

        //Parts of this code are from http://www.chd.at/sites/default/files/files/FTDI.cs
        //pin = pin number; dir = 0:=input; 1:=output
        public bool SetGPIODirection(byte pin, byte dir)
        {
            if (_handle == IntPtr.Zero)
            {
                return false;
            }
            if (dir == 1)
            {
                _direction |= (byte)(1 << pin);
            }
            else
            {
                _direction &= ((byte)~(1 << pin));
            }
            _gpo &= ((byte)~(1 << pin));

                WriteGpios(_direction, _gpo);
            return true;
        }

        public void WriteGpios(byte direction, byte gpo)
        {
            var status = LibMpsse_AccessToCppDll.FT_WriteGPIO(_handle, direction, gpo);
            CheckResult(status);
        }

        public bool SetGPIOOn(byte pin)
        {
            if (_handle == IntPtr.Zero)
            {
                return false;
            }
            _gpo = (byte)(_gpo | (byte)(1 << pin));

            WriteGpios(_direction, _gpo);
            return true;
        }

        public bool SetGPIOOff(byte pin)
        {
            if (_handle == IntPtr.Zero)
            {
                return false;
            }

            _gpo &= ((byte)~(1 << pin));

            WriteGpios(_direction, _gpo);
            return true;
        }

        public int ReadGPIOMask()
        {
            int value;
            var status = LibMpsse_AccessToCppDll.FT_ReadGPIO(_handle, out value);
            CheckResult(status);
            if (status != FtdiMpsseResult.Ok)
                return -1;
            return value;
        }

        public bool ReadGPIO(byte pin, out bool val)
        {
            val = false;
            if (_handle == IntPtr.Zero)
                return false;

            int value = ReadGPIOMask();
            if(value == -1)
                return false;

            var valShift = (value >> pin) & 1;
            val = valShift == 1;
            return true;
        }
    }
}
