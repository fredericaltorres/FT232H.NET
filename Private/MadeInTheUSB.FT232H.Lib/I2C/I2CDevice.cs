#define FT232H                // Enable only one of these defines depending on your device type
//#define FT2232H
//#define FT4232H
using FTD2XX_NET;
using System;
using System.Threading;

namespace MadeInTheUSB.FT232H
{
    public class I2CDevice
    {
        FTD2XX_NET.FTDI myFtdiDevice;

        public int I2CDeviceId;

        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

        // ###### I2C Library defines ######
        const byte I2C_Dir_SDAin_SCLin = 0x00;
        const byte I2C_Dir_SDAin_SCLout = 0x01;
        const byte I2C_Dir_SDAout_SCLout = 0x03;
        const byte I2C_Dir_SDAout_SCLin = 0x02;
        const byte I2C_Data_SDAhi_SCLhi = 0x03;
        const byte I2C_Data_SDAlo_SCLhi = 0x01;
        const byte I2C_Data_SDAlo_SCLlo = 0x00;
        const byte I2C_Data_SDAhi_SCLlo = 0x02;

        // MPSSE clocking commands
        const byte MSB_FALLING_EDGE_CLOCK_BYTE_IN = 0x24;
        const byte MSB_RISING_EDGE_CLOCK_BYTE_IN = 0x20;
        const byte MSB_FALLING_EDGE_CLOCK_BYTE_OUT = 0x11;
        const byte MSB_DOWN_EDGE_CLOCK_BIT_IN = 0x26;
        const byte MSB_UP_EDGE_CLOCK_BYTE_IN = 0x20;
        const byte MSB_UP_EDGE_CLOCK_BYTE_OUT = 0x10;
        const byte MSB_RISING_EDGE_CLOCK_BIT_IN = 0x22;
        const byte MSB_FALLING_EDGE_CLOCK_BIT_OUT = 0x13;


        public enum ClockEnum  {
            Clock600Khz_Divisor = 49,
            Clock12Mhz_Divisor = 24
        }


        ClockEnum ClockSpeed = ClockEnum.Clock600Khz_Divisor;

        // Sending and receiving
        static uint _numBytesToSend = 0;
        static uint NumBytesToRead = 0;
        uint _numBytesSent = 0;
        static uint NumBytesRead = 0;
        static byte[] _MPSSEbuffer = new byte[500];
        static byte[] _inputBuffer = new byte[500];
        static byte[] _inputBuffer2 = new byte[500];
        static uint BytesAvailable = 0;
        public bool I2C_Ack = false;
        static byte AppStatus = 0;
        static bool I2C_Status = false;
        public bool Running = true;
        //static bool DeviceOpen = false;
        // GPIO
        static byte GPIO_Low_Dat = 0;
        static byte GPIO_Low_Dir = 0;

        GpioI2CImplementationDevice _gpios;
        public IDigitalWriteRead Gpios => _gpios;

        public I2CDevice(FTD2XX_NET.FTDI myFtdiDevice, ClockEnum clockSpeed = ClockEnum.Clock600Khz_Divisor)
        {
            this.myFtdiDevice = myFtdiDevice;
            this.ClockSpeed = clockSpeed;

            this.I2C_ConfigureMpsse();
            _gpios = new GpioI2CImplementationDevice(this);
            

        }

        public bool Idle()
        {
            // Set the line AD3 as output driving low to turn off white LED on colour sensor
            // only upper 5 bits of these values have any effect as bits 2:0 are for the I2C lines
            GPIO_Low_Dat = 0x00;
            GPIO_Low_Dir = 0x08;
            AppStatus = I2C_SetLineStatesIdle();
            return (AppStatus == 0);
        }

        public bool I2C_ConfigureMpsse()
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

            ftStatus = FTDI.FT_STATUS.FT_OK;
            ftStatus |= myFtdiDevice.SetTimeouts(5000, 5000);
            ftStatus |= myFtdiDevice.SetLatency(16);
            ftStatus |= myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x00, 0x00);
            ftStatus |= myFtdiDevice.SetBitMode(0x00, 0x00);
            ftStatus |= myFtdiDevice.SetBitMode(0x00, 0x02);         // MPSSE mode        

            if (ftStatus != FTDI.FT_STATUS.FT_OK)
                return false;

            I2C_Status = FlushBuffer();

            /***** Synchronize the MPSSE interface by sending bad command 0xAA *****/
            _numBytesToSend = 0;
            _MPSSEbuffer[_numBytesToSend++] = 0xAA;
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status) return false;
            NumBytesToRead = 2;
            I2C_Status = Receive_Data(2);
            if (!I2C_Status) return false;

            if ((_inputBuffer2[0] == 0xFA) && (_inputBuffer2[1] == 0xAA))
            {
                // Bad Command Echo successful
            }
            else
            {
                return false;
            }

            /***** Synchronize the MPSSE interface by sending bad command 0xAB *****/
            _numBytesToSend = 0;
            _MPSSEbuffer[_numBytesToSend++] = 0xAB;
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status) return false;
            NumBytesToRead = 2;
            I2C_Status = Receive_Data(2);
            if (!I2C_Status) return false;

            if ((_inputBuffer2[0] == 0xFA) && (_inputBuffer2[1] == 0xAB))
            {
                // Bad Command Echo successful
            }
            else
            {
                return false;
            }

            _numBytesToSend = 0;
            _MPSSEbuffer[_numBytesToSend++] = 0x8A; 	// Disable clock divide by 5 for 60Mhz master clock
            _MPSSEbuffer[_numBytesToSend++] = 0x97;	// Turn off adaptive clocking
            _MPSSEbuffer[_numBytesToSend++] = 0x8C; 	// Enable 3 phase data clock, used by I2C to allow data on both clock edges
            // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
            // SK frequency  = 60MHz /((1 +  [(1 +0xValueH*256) OR 0xValueL])*2)
            _MPSSEbuffer[_numBytesToSend++] = 0x86; 	//Command to set clock divisor
            _MPSSEbuffer[_numBytesToSend++] = (byte)(((int)this.ClockSpeed) & 0x00FF);	//Set 0xValueL of clock divisor
            _MPSSEbuffer[_numBytesToSend++] = (byte)((((int)this.ClockSpeed) >> 8) & 0x00FF);	//Set 0xValueH of clock divisor
            _MPSSEbuffer[_numBytesToSend++] = 0x85;  // loopback off

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = 0x9E;       //Enable the FT232H's drive-zero mode with the following enable mask...
            _MPSSEbuffer[_numBytesToSend++] = 0x07;       // ... Low byte (ADx) enables - bits 0, 1 and 2 and ... 
            _MPSSEbuffer[_numBytesToSend++] = 0x00;       //...High byte (ACx) enables - all off

            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLhi | (GPIO_Low_Dat & 0xF8)); // SDA and SCL both output high (open drain)
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));
#else
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));  	// SDA and SCL set low but as input to mimic open drain
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLin | (GPIO_Low_Dir & 0xF8));	//

#endif

            _MPSSEbuffer[_numBytesToSend++] = 0x80; 	//Command to set directions of lower 8 pins and force value on bits set as output 
            _MPSSEbuffer[_numBytesToSend++] = (byte)(ADbusVal);
            _MPSSEbuffer[_numBytesToSend++] = (byte)(ADbusDir);


            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        
        public byte I2C_ReadByte(bool ACK)
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

#if (FT232H)
            // Clock in one data byte
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            // clock out one bit as ack/nak bit
            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;     // Clock data bit out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                               // Length of 0 means 1 bit
            if (ACK == true)
                _MPSSEbuffer[_numBytesToSend++] = 0x00;                           // Data bit to send is a '0'
            else
                _MPSSEbuffer[_numBytesToSend++] = 0xFF;                           // Data bit to send is a '1'

            // I2C lines back to idle state 
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));
            _MPSSEbuffer[_numBytesToSend++] = 0x80;                               //       ' Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                            //      ' Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                             //     ' Set the directions
#else
            // Ensure line is definitely an input since FT2232H and FT4232H don't have open drain
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8)); // make data input
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions
            // Clock one byte of data in from the sensor
            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;
            MPSSEbuffer[NumBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            // Change direction back to output and clock out one bit. If ACK is true, we send bit as 0 as an acknowledge
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                               // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                           // set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                           // set the directions

            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;    // Clock data bit out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            if (ACK == true)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x00;                          // Data bit to send is a '0'
            }
            else
            {
                MPSSEbuffer[NumBytesToSend++] = 0xFF;                          // Data bit to send is a '1'
            }

            // Put line states back to idle with SDA open drain high (set to input) 
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8));//make data input
            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                             //     ' Set the directions


#endif
            // This command then tells the MPSSE to send any results gathered back immediately
            _MPSSEbuffer[_numBytesToSend++] = 0x87;                                  //    ' Send answer back immediate command

            // send commands to chip
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
            {
                return 1;
            }

            // get the byte which has been read from the driver's receive buffer
            I2C_Status = Receive_Data(1);
            if (!I2C_Status)
            {
                return 1;
            }

            // InputBuffer2[0] now contains the results

            return 0;
        }


        //###################################################################################################################################
        // Sends I2C address followed by reading 2 bytes

        public byte I2C_Read2BytesWithAddr(byte Address)
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

            Address <<= 1;
            Address |= 0x01;

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // 
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            _MPSSEbuffer[_numBytesToSend++] = Address;                                // 

            // Put line back to idle (data released, clock pulled low)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo  | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// make data input
            _MPSSEbuffer[_numBytesToSend++] = 0x80;                                   // Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                               // Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data bits in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit
#else

            // Set directions of clock and data to output in preparation for clocking out a byte
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions
            // clock out one byte
            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // 
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Data length of 0x0000 means 1 byte data to clock in
            MPSSEbuffer[NumBytesToSend++] = Address;                         // Byte to send

            // Put line back to idle (data released, clock pulled low) so that sensor can drive data line
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8)); // make data input
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit

#endif

            // ------------------------------------ Clock in 1st byte and ACK ------------------------------------

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;    // Clock data bit out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                              // Sending 0 here as ACK

            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));

            _MPSSEbuffer[_numBytesToSend++] = 0x80;                               //       ' Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                            //      ' Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                             //     ' Set the directions
#else          

            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;
            MPSSEbuffer[NumBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            // Send a 0 bit as an acknowledge
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));//back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                             //     ' Set the directions

            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;    // Clock data bit out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            MPSSEbuffer[NumBytesToSend++] = 0x00;                              // Sending 0 here as ACK

            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8));//make data input

            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                             //     ' Set the directions


#endif

            // ------------------------------------ Clock in 2nd byte and NAK ------------------------------------

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;    // Clock data bit out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            _MPSSEbuffer[_numBytesToSend++] = 0xFF;                              // Sending 1 here as NAK

            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));

            _MPSSEbuffer[_numBytesToSend++] = 0x80;                               //       ' Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                            //      ' Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                             //     ' Set the directions
#else
            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BYTE_IN;      // Clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;
            MPSSEbuffer[NumBytesToSend++] = 0x00;                               // Data length of 0x0000 means 1 byte data to clock in

            // Send a 1 bit as a Nack
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));//back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                             //     ' Set the directions

            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BIT_OUT;    // Clock data bit out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                              // Length of 0 means 1 bit
            MPSSEbuffer[NumBytesToSend++] = 0xFF;                              // Sending 1 here as NAK

            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8));//make data input

            MPSSEbuffer[NumBytesToSend++] = 0x80;                               //       ' Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                            //      ' Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                             //     ' Set the directions

#endif
            // This command then tells the MPSSE to send any results gathered back immediately
            _MPSSEbuffer[_numBytesToSend++] = 0x87;                                //  ' Send answer back immediate command

            // Send off the commands
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
            {
                return 1;
            }

            // Read back the ack from the address phase and the 2 bytes read
            I2C_Status = Receive_Data(3);
            if (!I2C_Status)
            {
                return 1;
            }

            // Check if address phase was acked
            if ((_inputBuffer2[0] & 0x01) == 0)
            {
                I2C_Ack = true;
            }
            else
            {
                I2C_Ack = false;
            }

            // Get the two data bytes to put back to the calling function - InputBuffer2[0..1] now contains the results
            _inputBuffer2[0] = _inputBuffer2[1];
            _inputBuffer2[1] = _inputBuffer2[2];

            return 0;

        }


        //###################################################################################################################################

        public byte I2C_SendDeviceAddrAndCheckACK(byte Address, bool Read)
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

            Address <<= 1;
            if (Read == true)
                Address |= 0x01;

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // 
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            _MPSSEbuffer[_numBytesToSend++] = Address;           //  Byte to send

            // Put line back to idle (data released, clock pulled low)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// make data input
            _MPSSEbuffer[_numBytesToSend++] = 0x80;                                   // Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                               // Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data bits in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit
#else

            // Set directions of clock and data to output in preparation for clocking out a byte
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions
            // clock out one byte
            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // 
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Data length of 0x0000 means 1 byte data to clock in
            MPSSEbuffer[NumBytesToSend++] = Address;                         // Byte to send

            // Put line back to idle (data released, clock pulled low) so that sensor can drive data line
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8)); // make data input
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit

#endif
            // This command then tells the MPSSE to send any results gathered (in this case the ack bit) back immediately
            _MPSSEbuffer[_numBytesToSend++] = 0x87;                                //  ' Send answer back immediate command

            // send commands to chip
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
            {
                return 1;
            }

            // read back byte containing ack
            I2C_Status = Receive_Data(1);
            if (!I2C_Status)
            {
                return 1;            // can also check NumBytesRead
            }

            // if ack bit is 0 then sensor acked the transfer, otherwise it nak'd the transfer
            if ((_inputBuffer2[0] & 0x01) == 0)
            {
                I2C_Ack = true;
            }
            else
            {
                I2C_Ack = false;
            }

            return 0;

        }

        //###################################################################################################################################
        // Writes one byte to the I2C bus

        public byte I2C_SendByteAndCheckACK(byte DataByteToSend)
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

#if (FT232H)
            _MPSSEbuffer[_numBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // 
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   //  Data length of 0x0000 means 1 byte data to clock in
            _MPSSEbuffer[_numBytesToSend++] = DataByteToSend;// DataSend[0];          //  Byte to send

            // Put line back to idle (data released, clock pulled low)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// make data input
            _MPSSEbuffer[_numBytesToSend++] = 0x80;                                   // Command - set low byte
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;                               // Set the values
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            _MPSSEbuffer[_numBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data bits in
            _MPSSEbuffer[_numBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit
#else

            // Set directions of clock and data to output in preparation for clocking out a byte
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));// back to output
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions
            // clock out one byte
            MPSSEbuffer[NumBytesToSend++] = MSB_FALLING_EDGE_CLOCK_BYTE_OUT;        // clock data byte out
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // 
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Data length of 0x0000 means 1 byte data to clock in
            MPSSEbuffer[NumBytesToSend++] = DataByteToSend;                         // Byte to send

            // Put line back to idle (data released, clock pulled low) so that sensor can drive data line
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8)); // make data input
            MPSSEbuffer[NumBytesToSend++] = 0x80;                                   // Command - set low byte
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;                               // Set the values
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;                               // Set the directions

            // CLOCK IN ACK
            MPSSEbuffer[NumBytesToSend++] = MSB_RISING_EDGE_CLOCK_BIT_IN;           // clock data byte in
            MPSSEbuffer[NumBytesToSend++] = 0x00;                                   // Length of 0 means 1 bit

#endif
            // This command then tells the MPSSE to send any results gathered (in this case the ack bit) back immediately
            _MPSSEbuffer[_numBytesToSend++] = 0x87;                                //  ' Send answer back immediate command

            // send commands to chip
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
            {
                return 1;
            }

            // read back byte containing ack
            I2C_Status = Receive_Data(1);
            if (!I2C_Status)
            {
                return 1;            // can also check NumBytesRead
            }

            // if ack bit is 0 then sensor acked the transfer, otherwise it nak'd the transfer
            if ((_inputBuffer2[0] & 0x01) == 0)
            {
                I2C_Ack = true;
            }
            else
            {
                I2C_Ack = false;
            }

            return 0;

        }

        //###################################################################################################################################
        // Sets I2C Start condition

        public byte I2C_SetStart()
        {
            byte Count = 0;
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;


#if (FT232H)
            // SDA high, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLhi | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // on FT232H lines always output

            for (Count = 0; Count < 6; Count++)
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA lo, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLhi | (GPIO_Low_Dat & 0xF8));

            for (Count = 0; Count < 6; Count++)	// Repeat commands to ensure the minimum period of the start setup time ie 600ns is achieved
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA lo, SCL lo
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));

            for (Count = 0; Count < 6; Count++)	// Repeat commands to ensure the minimum period of the start setup time ie 600ns is achieved
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }

            // Release SDA
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLlo | (GPIO_Low_Dat & 0xF8));

            _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction


# else

            // Both SDA and SCL high (setting to input simulates open drain high)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLin | (GPIO_Low_Dir & 0xF8));

            for (Count = 0; Count < 6; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA low, SCL high (setting to input simulates open drain high)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLin | (GPIO_Low_Dir & 0xF8));

            for (Count = 0; Count < 6; Count++)	// Repeat commands to ensure the minimum period of the start setup time
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA low, SCL low
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));//
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));//as above

            for (Count = 0; Count < 6; Count++)	// Repeat commands to ensure the minimum period of the start setup time
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }

            // Release SDA (setting to input simulates open drain high)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));//
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLout | (GPIO_Low_Dir & 0xF8));//as above

            MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
            MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
            MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction



# endif
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return 1;
            else
                return 0;

        }

        //###################################################################################################################################
        // Sets I2C Stop condition

        public byte I2C_SetStop()
        {
            byte Count = 0;
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

#if (FT232H)
            // SDA low, SCL low
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // on FT232H lines always output

            for (Count = 0; Count < 6; Count++)
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }
           
            // SDA low, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLhi | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // on FT232H lines always output

            for (Count = 0; Count < 6; Count++)
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA high, SCL high
            ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLhi | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));        // on FT232H lines always output

            for (Count = 0; Count < 6; Count++)	
            {
                _MPSSEbuffer[_numBytesToSend++] = 0x80;	    // ADbus GPIO command
                _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
                _MPSSEbuffer[_numBytesToSend++] = ADbusDir;	// Set direction
            }
           
# else

            // SDA low, SCL low
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));

            for (Count = 0; Count < 6; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }


            // SDA low, SCL high (note: setting to input simulates open drain high)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLin | (GPIO_Low_Dir & 0xF8));

            for (Count = 0; Count < 6; Count++)
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }

            // SDA high, SCL high (note: setting to input simulates open drain high)
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLin | (GPIO_Low_Dir & 0xF8));

            for (Count = 0; Count < 6; Count++)	// Repeat commands to hold states for longer time
            {
                MPSSEbuffer[NumBytesToSend++] = 0x80;	    // ADbus GPIO command
                MPSSEbuffer[NumBytesToSend++] = ADbusVal;   // Set data value
                MPSSEbuffer[NumBytesToSend++] = ADbusDir;	// Set direction
            }
#endif
            // send the buffer of commands to the chip 
            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return 1;
            else
                return 0;

        }



        //###################################################################################################################################
        // Sets GPIO values on low byte and puts I2C lines (bits 0, 1, 2) to idle outwith transaction state

        public byte I2C_SetLineStatesIdle()
        {
            byte ADbusVal = 0;
            byte ADbusDir = 0;
            _numBytesToSend = 0;

#if (FT232H)
           // '######## Combine the I2C line state for bits 2..0 with the GPIO for bits 7..3 ########
           ADbusVal = (byte)(0x00 | I2C_Data_SDAhi_SCLhi | (GPIO_Low_Dat & 0xF8));
           ADbusDir = (byte)(0x00 | I2C_Dir_SDAout_SCLout | (GPIO_Low_Dir & 0xF8));    // FT232H always output due to open drain capability    
           
#else
            ADbusVal = (byte)(0x00 | I2C_Data_SDAlo_SCLlo | (GPIO_Low_Dat & 0xF8));
            ADbusDir = (byte)(0x00 | I2C_Dir_SDAin_SCLin | (GPIO_Low_Dir & 0xF8));       // FT2232H/FT4232H use input to mimic open drain
#endif

            _MPSSEbuffer[_numBytesToSend++] = 0x80;       // ADbus GPIO command
            _MPSSEbuffer[_numBytesToSend++] = ADbusVal;   // Set data value
            _MPSSEbuffer[_numBytesToSend++] = ADbusDir;   // Set direction

            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return 1;
            else
                return 0;
        }





        //###################################################################################################################################
        // Gets GPIO values from low byte

        public int I2C_GetGPIOValuesLow()
        {
            _numBytesToSend = 0;

            _MPSSEbuffer[_numBytesToSend++] = 0x81;       // ADbus GPIO command for reading low byte
            _MPSSEbuffer[_numBytesToSend++] = 0x87;        // Send answer back immediate command

            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return -1;

            I2C_Status = Receive_Data(1);
            if (!I2C_Status)
            {
                return -1;
            }

            var ADbusReadVal = (byte)(_inputBuffer2[0] & 0xF8); // mask the returned value to show only 5 GPIO lines (bits 0/1/2 are I2C)

            return ADbusReadVal;
        }



        //###################################################################################################################################
        // Sets GPIO values on high byte

        public byte I2C_SetGPIOValuesHigh(byte ACbusDir, byte ACbusVal)
        {
            _numBytesToSend = 0;

#if (FT4232H)

           return 1;
           
#else
            _MPSSEbuffer[_numBytesToSend++] = 0x82;       // ACbus GPIO command
            _MPSSEbuffer[_numBytesToSend++] = ACbusVal;   // Set data value
            _MPSSEbuffer[_numBytesToSend++] = ACbusDir;   // Set direction

            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return 1;
            else
                return 0;

#endif
        }



        //###################################################################################################################################
        // Gets GPIO values from high byte

        public int I2C_GetGPIOValuesHigh()
        {
            _numBytesToSend = 0;

#if (FT4232H)
                return 1;       // no high byte on FT4232H
#else

            _MPSSEbuffer[_numBytesToSend++] = 0x83;           // ACbus read GPIO command
            _MPSSEbuffer[_numBytesToSend++] = 0x87;            // Send answer back immediate command

            I2C_Status = Send_Data(_numBytesToSend, _MPSSEbuffer);
            if (!I2C_Status)
                return -1;

            I2C_Status = Receive_Data(1);
            if (!I2C_Status)
                return -1;

            var ACbusReadVal = (byte)(_inputBuffer2[0]);      // Return via global variable for calling function to read

            return ACbusReadVal;
#endif
        }






        //###################################################################################################################################
        //###################################################################################################################################
        //##################                                          D2xx Layer                                        #####################
        //###################################################################################################################################
        //###################################################################################################################################


        // Read a specified number of bytes from the driver receive buffer

        private bool Receive_Data(uint BytesToRead)
        {
            uint NumBytesInQueue = 0;
            uint QueueTimeOut = 0;
            uint Buffer1Index = 0;
            uint Buffer2Index = 0;
            uint TotalBytesRead = 0;
            bool QueueTimeoutFlag = false;
            uint NumBytesRxd = 0;

            // Keep looping until all requested bytes are received or we've tried 5000 times (value can be chosen as required)
            while ((TotalBytesRead < BytesToRead) && (QueueTimeoutFlag == false))
            {
                ftStatus = myFtdiDevice.GetRxBytesAvailable(ref NumBytesInQueue);       // Check bytes available

                if ((NumBytesInQueue > 0) && (ftStatus == FTDI.FT_STATUS.FT_OK))
                {
                    ftStatus = myFtdiDevice.Read(_inputBuffer, NumBytesInQueue, ref NumBytesRxd);  // if any available read them

                    if ((NumBytesInQueue == NumBytesRxd) && (ftStatus == FTDI.FT_STATUS.FT_OK))
                    {
                        Buffer1Index = 0;

                        while (Buffer1Index < NumBytesRxd)
                        {
                            _inputBuffer2[Buffer2Index] = _inputBuffer[Buffer1Index];     // copy into main overall application buffer
                            Buffer1Index++;
                            Buffer2Index++;
                        }
                        TotalBytesRead = TotalBytesRead + NumBytesRxd;                  // Keep track of total
                    }
                    else
                        return false;

                    QueueTimeOut++;
                    if (QueueTimeOut == 5000)
                        QueueTimeoutFlag = true;
                    else
                        Thread.Sleep(0);                                                // Avoids running Queue status checks back to back
                }
            }
            // returning globals NumBytesRead and the buffer InputBuffer2
            NumBytesRead = TotalBytesRead;

            if (QueueTimeoutFlag == true)
                return false;
            else
                return true;
        }


        //###################################################################################################################################
        // Write a buffer of data and check that it got sent without error

        private bool Send_Data(uint bytesToSend, byte[] buffer)
        {
            _numBytesToSend = bytesToSend;
            uint numBytesSent = 0;

            // Send data. This will return once all sent or if times out
            ftStatus = myFtdiDevice.Write(buffer, _numBytesToSend, ref numBytesSent);

            // Ensure that call completed OK and that all bytes sent as requested
            if ((numBytesSent != _numBytesToSend) || (ftStatus != FTDI.FT_STATUS.FT_OK))
                return false;
            else
                return true;
        }
        
        private bool FlushBuffer()
        {
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref BytesAvailable);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
                return false;

            if (BytesAvailable > 0)
            {
                ftStatus = myFtdiDevice.Read(_inputBuffer, BytesAvailable, ref NumBytesRead);  	//Read out the data from receive buffer
                return (ftStatus == FTDI.FT_STATUS.FT_OK);
            }
            else return true;
        }

        public bool Send1ByteCommand(byte c)
        {
            var appStatus = 0;
            try
            {
                appStatus = this.I2C_SetStart();
                if (appStatus != 0) return false;

                appStatus = this.I2C_SendDeviceAddrAndCheckACK((byte)(this.I2CDeviceId), false);     // I2C ADDRESS (for write)
                if (appStatus != 0) return false;
                if (!this.I2C_Ack) return false;

                appStatus = this.I2C_SendByteAndCheckACK(c);
                if (appStatus != 0) return false;
                if (this.I2C_Ack != true) return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                appStatus = this.I2C_SetStop();
            }
            return (appStatus == 0);
        }

        bool i2c_Send2ByteCommand(byte c0, byte c1)
        {
            throw new NotImplementedException();
        }

        public bool WriteBuffer(byte[] buffer)
        {
            var appStatus = 0;
            try
            {
                appStatus = this.I2C_SetStart();
                if (appStatus != 0) return false;

                appStatus = this.I2C_SendDeviceAddrAndCheckACK((byte)(this.I2CDeviceId), false);
                if (appStatus != 0) return false;
                if (!this.I2C_Ack) return false;

                foreach (var c in buffer)
                {
                    appStatus = this.I2C_SendByteAndCheckACK(c);
                    if (appStatus != 0) return false;
                    if (!this.I2C_Ack) return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                appStatus = this.I2C_SetStop();
            }
            return (appStatus == 0);
        }

        bool i2c_WriteReadBuffer(byte[] writeBuffer, byte[] readBuffer)
        {
            throw new NotImplementedException();
        }
    }
}


