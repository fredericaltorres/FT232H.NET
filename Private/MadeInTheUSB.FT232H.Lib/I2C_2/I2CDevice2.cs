using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// https://ftdichip.com/wp-content/uploads/2020/08/AN_177_User_Guide_For_LibMPSSE-I2C.pdf
    /// </summary>
    public class I2CDevice2
    {
        private static IntPtr _handle = IntPtr.Zero;
        private static FtChannelConfig _currentGlobalConfig;

        private FtChannelConfig _config;
        public int DeviceAddress;

        private I2CConfiguration _i2cConfig;

        private ClockSpeeds _clockSpeed;
        private const int LatencyTimer = 200;//255; // Hz

        private byte _direction;
        private byte _gpo;

        public IDigitalWriteRead Gpios = null;
        public GpioI2C2ImplementationDevice GpiosPlus;

        public I2CDevice2(ClockSpeeds clockSpeed = ClockSpeeds.I2C_CLOCK_FAST_MODE_400Khz)
        {
            _clockSpeed = clockSpeed;
            this.Init();
        }

        private bool __Init()
        {
            var config = new FtChannelConfig
            {
                ClockRate = (int)_clockSpeed,
                LatencyTimer = LatencyTimer
            };

            _i2cConfig = _i2cConfig ?? I2CConfiguration.ChannelZeroConfiguration;
            _config = config;

            InitLibAndHandle();

            this.GpiosPlus = new GpioI2C2ImplementationDevice(this);
            this.Gpios = this.GpiosPlus;

            return true;
        }

        public bool Init()
        {
            return __Init();
        }

        void InitLibAndHandle()
        {
            FtdiMpsseSPIResult result;

            if (_handle != IntPtr.Zero)
                return;

            LibMpsse.Init();
            var num_channels = 0;
            var channels = LibMpsse_AccessToCppDll.I2C_GetNumChannels(out num_channels);

            CheckResult(channels);

            if (num_channels > 0)
            {
                for (var i = 0; i < num_channels; i++)
                {
                    FtDeviceInfo cInfo;
                    var channelInfoStatus = LibMpsse_AccessToCppDll.I2C_GetChannelInfo(i, out cInfo);
                    CheckResult(channelInfoStatus);
                    System.Console.WriteLine($"Flags: {cInfo.Flags}");
                    System.Console.WriteLine($"Type: {cInfo.Type}");
                    System.Console.WriteLine($"ID: {cInfo.ID}");
                    System.Console.WriteLine($"LocId: {cInfo.LocId}");
                    System.Console.WriteLine($"SerialNumber: {cInfo.SerialNumber}");
                    System.Console.WriteLine($"Description: {cInfo.Description}");
                    System.Console.WriteLine($"ftHandle: {cInfo.ftHandle}");
                }
            }

            result = LibMpsse_AccessToCppDll.I2C_OpenChannel(_i2cConfig.ChannelIndex, out _handle);

            CheckResult(result);

            if (_handle == IntPtr.Zero)
                throw new I2CException(FtdiMpsseSPIResult.InvalidHandle);

            result = LibMpsse_AccessToCppDll.I2C_InitChannel(_handle, ref _config);

            CheckResult(result);
            _currentGlobalConfig = _config;
        }

        public void WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            throw new NotImplementedException();
        }


        public bool WriteBuffer(byte[] array)
        {
            return Write(array);
        }

        public bool Write(byte[] array)
        {
            int writtenAmount;

            var result = Write(array, array.Length, out writtenAmount,
                //FtI2CTransferOptions.FastTransfer |
                FtI2CTransferOptions.StartBit     | 
                FtI2CTransferOptions.StopBit);

            return result == FtdiMpsseSPIResult.Ok;
        }

        public bool Write(int value)
        {
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

            var result = Write(array, array.Length, out writtenAmount,
                //FtI2CTransferOptions.FastTransfer | 
                FtI2CTransferOptions.StartBit | FtI2CTransferOptions.StopBit);

            return result == FtdiMpsseSPIResult.Ok;
        }

        public FtdiMpsseSPIResult Write(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options)
        {
            return LibMpsse_AccessToCppDll.I2C_DeviceWrite(_handle, DeviceAddress, sizeToTransfer, buffer, out sizeTransfered, options);
        }

        public bool Write1ByteReadUInt16WithRetry(byte reg, UInt16 expected)
        {
            if (this.Write1ByteReadUInt16(reg) == expected)
                return true;
            Thread.Sleep(10);
            if (this.Write1ByteReadUInt16(reg) == expected)
                return true;
            return false;
        }

        public UInt16 Write1ByteReadUInt16(byte reg)
        {
            this.Write(reg);
            var buffer = this.ReadXByte(2);
            var value = (buffer[0] << 8) + buffer[1];
            return (UInt16)value;
        }
        public List<byte> ReadXByte(int count)
        {
            var buffer = new byte[count];
            if (Read(buffer))
            {
                return buffer.ToList();
            }
            else return null;
        }

        public int Read1Byte()
        {
            var buffer = new byte[1];
            if (Read(buffer))
            {
                return buffer[0];
            }
            else return -1;
        }

        public int Write1ByteRead2Byte(byte reg)
        {
            this.Write(reg);
            var buffer = this.ReadXByte(2);
            var value = (buffer[0] << 8) + buffer[1];

            return (UInt16)value;
        }

        public FtdiMpsseSPIResult Read(byte[] buffer, int sizeToTransfer, out int sizeTransfered, FtI2CTransferOptions options)
        {
            return LibMpsse_AccessToCppDll.I2C_DeviceRead(_handle, DeviceAddress, sizeToTransfer, buffer, out sizeTransfered, options);
        }

        public bool Read(byte[] buffer)
        {
            int sizeTransfered = 0;
            var result = LibMpsse_AccessToCppDll.I2C_DeviceRead(
                _handle, DeviceAddress,
                buffer.Length, buffer, out sizeTransfered, FtI2CTransferOptions.StartBit);

            CheckResult(result);
            return result == FtdiMpsseSPIResult.Ok;
        }

        protected static void CheckResult(FtdiMpsseSPIResult result)
        {
            if (result != FtdiMpsseSPIResult.Ok)
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
            if (status != FtdiMpsseSPIResult.Ok)
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


