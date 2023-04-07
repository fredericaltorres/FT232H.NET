// This example is provided as-is and FTDI accept no responsibility for any damage or
// problems resulting from its use whatsoever.
// It is the responsibility of the customer to ensure the safe and correct operation of the overall
// system incorporating any content from this example.


#include "stdafx.h"
#include <windows.h>

//============================================================================
//  Use of FTDI D2XX library:
//----------------------------------------------------------------------------
//  Include the following 2 lines in your header-file
#pragma comment(lib, "FTD2XX.lib")
#include "FTD2XX.h"
//============================================================================

#include <stdlib.h>


bool bCommandEchod = false;

	FT_STATUS ftStatus;					// Status defined in D2XX to indicate operation result
	FT_HANDLE ftHandle;					// Handle of FT232H device port 

	BYTE OutputBuffer[1024];			// Buffer to hold MPSSE commands and data to be sent to FT232H
	BYTE InputBuffer[1024];				// Buffer to hold Data bytes read from FT232H
	
	DWORD dwClockDivisor = 0x00C8;		// 100khz
	
	DWORD dwNumBytesToSend = 0; 		// Counter used to hold number of bytes to be sent
	DWORD dwNumBytesSent = 0;			// Holds number of bytes actually sent (returned by the read function)

	DWORD dwNumInputBuffer = 0;			// Number of bytes which we want to read
	DWORD dwNumBytesRead = 0;			// Number of bytes actually read
	DWORD ReadTimeoutCounter = 0;		// Used as a software timeout counter when the code checks the Queue Status

	BYTE ByteDataRead[4];				// Array for storing the data which was read from the I2C Slave
	BOOL DataInBuffer  = 0;				// Flag which code sets when the GetNumBytesAvailable returned is > 0 
	BYTE DataByte = 0;					// Used to store data bytes read from and written to the I2C Slave






// #########################################################################################
// #########################################################################################
// I2C FUNCTIONS
// #########################################################################################
// #########################################################################################




// ####################################################################################################################
// Function to read 1 byte from the I2C slave (e.g. FT-X chip)
//     Clock in one byte from the I2C Slave which is the actual data to be read
//     Clock out one bit to the I2C Slave which is the ACK/NAK bit
//	   Put lines back to the idle state (idle between start and stop is clock low, data high (open-drain)
// This function reads only one byte from the I2C Slave. It therefore sends a '1' as the ACK/NAK bit. This is NAKing 
// the first byte of data, to tell the slave we dont want to read any more bytes. 
// The one byte of data read from the I2C Slave is put into ByteDataRead[0]
// ####################################################################################################################

BOOL ReadByteAndSendNAK(void)
{
	dwNumBytesToSend = 0;							// Clear output buffer
	
	// Clock one byte of data in...
	OutputBuffer[dwNumBytesToSend++] = 0x20;		// Command to clock data byte in on the clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length (low)
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length (high)   Length 0x0000 means clock ONE byte in 

	// Now clock out one bit (the ACK/NAK bit). This bit has value '1' to send a NAK to the I2C Slave
	OutputBuffer[dwNumBytesToSend++] = 0x13; 		// Command to clock data bits out on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length of 0x00 means clock out ONE bit
	OutputBuffer[dwNumBytesToSend++] = 0xFF;		// Command will send bit 7 of this byte, we send a '1' here

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)

	// This command then tells the MPSSE to send any results gathered back immediately
	OutputBuffer[dwNumBytesToSend++] = 0x87;		// Send answer back immediate command

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		// Send off the commands to the FT232H

	// ===============================================================
	// Now wait for the byte which we read to come back to the host PC
	// ===============================================================

	dwNumInputBuffer = 0;
	ReadTimeoutCounter = 0;

	ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer

	while ((dwNumInputBuffer < 1) && (ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		// Sit in this loop until
		// (1) we receive the one byte expected
		// or (2) a hardware error occurs causing the GetQueueStatus to return an error code
		// or (3) we have checked 500 times and the expected byte is not coming 
		ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer
		ReadTimeoutCounter ++;
		Sleep(1);													// short delay
	}

	// If the loop above exited due to the byte coming back (not an error code and not a timeout)
	// then read the byte available and return True to indicate success
	if ((ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		ftStatus = FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead); // Now read the data
		ByteDataRead[0] = InputBuffer[0];				// return the data read in the global array ByteDataRead
		return TRUE;									// Indicate success
	}
	else
	{
		return FALSE;									// Failed to get any data back or got an error code back
	}
}


// ##############################################################################################################
// Function to read 3 bytes from the slave (e.g. FT-X chip), writing out an ACK/NAK bit at the end of each byte
// For each byte to be read, 
//     We clock in one byte from the I2C Slave which is the actual data to be read
//     We then clock out one bit to the Slave which is the ACK/NAK bit
//	   Put lines back to the idle state (idle between start and stop is clock low, data high (open-drain)
// After the first and second bytes, we send a '0' as the ACK to tell the slave that we want to read more bytes.
// After the third byte read, we write a '1' as a NAK, to tell the slave we dont want any more bytes. 
// Returns data in ByteDataRead[0] to ByteDataRead[2]
// ##############################################################################################################

BOOL Read3BytesAndSendNAK(void)
{
	dwNumBytesToSend = 0;			//Clear output buffer
	
	// Read the first byte of data over I2C and ACK it

	//Clock one byte in
	OutputBuffer[dwNumBytesToSend++] = 0x20; 		// Command to clock data byte in MSB first on clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data length of 0x0000 means 1 byte data to clock in

	// Clock out one bit...send ack bit as '0'
	OutputBuffer[dwNumBytesToSend++] = 0x13;		// Command to clock data bit out MSB first on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length of 0x00 means 1 bit
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data value to clock out is in bit 7 of this value

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)

	// Read the second byte of data over I2C and ACK it

	//Clock one byte in
	OutputBuffer[dwNumBytesToSend++] = 0x20; 		// Command to clock data byte in MSB first on clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data length of 0x0000 means 1 byte data to clock in

	// Clock out one bit...send ack bit as '0'
	OutputBuffer[dwNumBytesToSend++] = 0x13;		// Command to clock data bit out MSB first on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length of 0x00 means 1 bit
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data value to clock out is in bit 7 of this value

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)
	
	// Read the third byte of data over I2C and NACK it

	//Clock one byte in
	OutputBuffer[dwNumBytesToSend++] = 0x20; 		// Command to clock data byte in MSB first on clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data length of 0x0000 means 1 byte data to clock in

	// Clock out one bit...send ack bit as '1'
	OutputBuffer[dwNumBytesToSend++] = 0x13;		// Command to clock data bit out MSB first on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Length of 0x00 means 1 bit
	OutputBuffer[dwNumBytesToSend++] = 0xFF;		// Data value to clock out is in bit 7 of this value

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)

	// This command then tells the MPSSE to send any results gathered back immediately
	OutputBuffer[dwNumBytesToSend++] = '\x87';		// Send answer back immediate command

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands

	// ===============================================================
	// Now wait for the 3 bytes which we read to come back to the host PC
	// ===============================================================

	dwNumInputBuffer = 0;
	ReadTimeoutCounter = 0;

	ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer

	while ((dwNumInputBuffer < 3) && (ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		// Sit in this loop until
		// (1) we receive the 3 bytes expected
		// or (2) a hardware error occurs causing the GetQueueStatus to return an error code
		// or (3) we have checked 500 times and the expected byte is not coming 
		ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer
		ReadTimeoutCounter ++;
		Sleep(1);													// short delay
	}

	// If the loop above exited due to the bytes coming back (not an error code and not a timeout)
	// then read the bytes available and return True to indicate success
	if ((ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		ftStatus = FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead); // Now read the data
		ByteDataRead[0] = InputBuffer[0];				// return the first byte of data read
		ByteDataRead[1] = InputBuffer[1];				// return the second byte of data read
		ByteDataRead[2] = InputBuffer[2];				// return the third byte of data read
		return TRUE;									// Indicate success
	}
	else
	{
		return FALSE;									// Failed to get any data back or got an error code back
	}
}


// ##############################################################################################################
// Function to write 1 byte, and check if it returns an ACK or NACK by clocking in one bit
//     We clock one byte out to the I2C Slave
//     We then clock in one bit from the Slave which is the ACK/NAK bit
//	   Put lines back to the idle state (idle between start and stop is clock low, data high (open-drain)
// Returns TRUE if the write was ACKed
// ##############################################################################################################

BOOL SendByteAndCheckACK(BYTE dwDataSend)
{
	dwNumBytesToSend = 0;			// Clear output buffer
	FT_STATUS ftStatus = FT_OK;

	OutputBuffer[dwNumBytesToSend++] = 0x11; 		// command to clock data bytes out MSB first on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// 
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data length of 0x0000 means 1 byte data to clock out
	OutputBuffer[dwNumBytesToSend++] = dwDataSend;	// Actual byte to clock out

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)

	OutputBuffer[dwNumBytesToSend++] = 0x22; 	// Command to clock in bits MSB first on clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;	// Length of 0x00 means to scan in 1 bit

	// This command then tells the MPSSE to send any results gathered back immediately
	OutputBuffer[dwNumBytesToSend++] = 0x87;	//Send answer back immediate command

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands
	
	// ===============================================================
	// Now wait for the byte which we read to come back to the host PC
	// ===============================================================

	dwNumInputBuffer = 0;
	ReadTimeoutCounter = 0;

	ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer

	while ((dwNumInputBuffer < 1) && (ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		// Sit in this loop until
		// (1) we receive the one byte expected
		// or (2) a hardware error occurs causing the GetQueueStatus to return an error code
		// or (3) we have checked 500 times and the expected byte is not coming 
		ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer
		ReadTimeoutCounter ++;
		Sleep(1);													// short delay
	}

	// If the loop above exited due to the byte coming back (not an error code and not a timeout)

	if ((ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
	{
		ftStatus = FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead); // Now read the data
	
		if (((InputBuffer[0] & 0x01)  == 0x00))		//Check ACK bit 0 on data byte read out
		{	
			return TRUE;							// Return True if the ACK was received
		}
		else
			//printf("Failed to get ACK from I2C Slave \n");
			return FALSE; //Error, can't get the ACK bit 
		}
	else
	{
		return FALSE;									// Failed to get any data back or got an error code back
	}

}


// ##############################################################################################################
// Function to write 1 byte, and check if it returns an ACK or NACK by clocking in one bit
// This function combines the data and the Read/Write bit to make a single 8-bit value
//     We clock one byte out to the I2C Slave
//     We then clock in one bit from the Slave which is the ACK/NAK bit
//	   Put lines back to the idle state (idle between start and stop is clock low, data high (open-drain)
// Returns TRUE if the write was ACKed by the slave
// ##############################################################################################################

BOOL SendAddrAndCheckACK(BYTE dwDataSend, BOOL Read)
{
	dwNumBytesToSend = 0;			// Clear output buffer
	FT_STATUS ftStatus = FT_OK;

	// Combine the Read/Write bit and the actual data to make a single byte with 7 data bits and the R/W in the LSB
	if(Read == TRUE)
	{
		dwDataSend = ((dwDataSend << 1) | 0x01);
	}
	else
	{
		dwDataSend = ((dwDataSend << 1) & 0xFE);
	}

	OutputBuffer[dwNumBytesToSend++] = 0x11; 		// command to clock data bytes out MSB first on clock falling edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// 
	OutputBuffer[dwNumBytesToSend++] = 0x00;		// Data length of 0x0000 means 1 byte data to clock out
	OutputBuffer[dwNumBytesToSend++] = dwDataSend;	// Actual byte to clock out

	// Put I2C line back to idle (during transfer) state... Clock line driven low, Data line high (open drain)
	OutputBuffer[dwNumBytesToSend++] = 0x80;		// Command to set lower 8 bits of port (ADbus 0-7 on the FT232H)
	OutputBuffer[dwNumBytesToSend++] = 0xFE;		// Set the value of the pins (only affects those set as output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;		// Set the directions - all pins as output except Bit2(data_in)
	
	// AD0 (SCL) is output driven low
	// AD1 (DATA OUT) is output high (open drain)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs driven high (not used in this application)

	OutputBuffer[dwNumBytesToSend++] = 0x22; 	// Command to clock in bits MSB first on clock rising edge
	OutputBuffer[dwNumBytesToSend++] = 0x00;	// Length of 0x00 means to scan in 1 bit

	// This command then tells the MPSSE to send any results gathered back immediately
	OutputBuffer[dwNumBytesToSend++] = 0x87;	//Send answer back immediate command

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands
	
	//Check if ACK bit received by reading the byte sent back from the FT232H containing the ACK bit
	ftStatus = FT_Read(ftHandle, InputBuffer, 1, &dwNumBytesRead);  	//Read one byte from device receive buffer
	
	if ((ftStatus != FT_OK) || (dwNumBytesRead == 0))
	{
		//printf("Failed to get ACK from I2C Slave \n");
		return FALSE; //Error, can't get the ACK bit
	}
	else 
	{
		if (((InputBuffer[0] & 0x01)  != 0x00))		//Check ACK bit 0 on data byte read out
		{	
			//printf("Failed to get ACK from I2C Slave \n");
			return FALSE; //Error, can't get the ACK bit 
		}
		
	}
	return TRUE;		// Return True if the ACK was received
}


// ##############################################################################################################
// Function to set all lines to idle states
// For I2C lines, it releases the I2C clock and data lines to be pulled high externally
// For the remainder of port AD, it sets AD3/4/5/6/7 as inputs as they are unused in this application
// For the LED control, it sets AC6 as an output with initial state high (LED off)
// For the remainder of port AC, it sets AC0/1/2/3/4/5/7 as inputs as they are unused in this application
// ##############################################################################################################

void SetI2CLinesIdle(void)
{
	dwNumBytesToSend = 0;			//Clear output buffer

	// Set the idle states for the AD lines
	OutputBuffer[dwNumBytesToSend++] = 0x80;	// Command to set directions of ADbus and data values for pins set as o/p
	OutputBuffer[dwNumBytesToSend++] = 0xFF;    // Set all 8 lines to high level (only affects pins which are output)
	OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in

	// IDLE line states are ...
	// AD0 (SCL) is output high (open drain, pulled up externally)
	// AD1 (DATA OUT) is output high (open drain, pulled up externally)
	// AD2 (DATA IN) is input (therefore the output value specified is ignored)
	// AD3 to AD7 are inputs (not used in this application)

	// Set the idle states for the AC lines
	OutputBuffer[dwNumBytesToSend++] = 0x82;	// Command to set directions of ACbus and data values for pins set as o/p
	OutputBuffer[dwNumBytesToSend++] = 0xFF;	// Set all 8 lines to high level (only affects pins which are output)
	OutputBuffer[dwNumBytesToSend++] = 0x40;	// Only bit 6 is output

	// IDLE line states are ...
	// AC6 (LED) is output driving high
	// AC0/1/2/3/4/5/7 are inputs (not used in this application)

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands
}


// ##############################################################################################################
// Function to set the I2C Start state on the I2C clock and data lines
// It pulls the data line low and then pulls the clock line low to produce the start condition
// It also sends a GPIO command to set bit 6 of ACbus low to turn on the LED. This acts as an activity indicator
// Turns on (low) during the I2C Start and off (high) during the I2C stop condition, giving a short blink.  
// ##############################################################################################################
void SetI2CStart(void)
{
	dwNumBytesToSend = 0;			//Clear output buffer
	DWORD dwCount;
	
	// Pull Data line low, leaving clock high (open-drain)
	for(dwCount=0; dwCount < 4; dwCount++)	// Repeat commands to ensure the minimum period of the start hold time is achieved
	{
		OutputBuffer[dwNumBytesToSend++] = 0x80;	// Command to set directions of ADbus and data values for pins set as o/p
		OutputBuffer[dwNumBytesToSend++] = 0xFD;	// Bring data out low (bit 1)
		OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in
	}
	
	// Pull Clock line low now, making both clcok and data low
	for(dwCount=0; dwCount < 4; dwCount++)	// Repeat commands to ensure the minimum period of the start setup time is achieved
	{
		OutputBuffer[dwNumBytesToSend++] = 0x80; 	// Command to set directions of ADbus and data values for pins set as o/p
		OutputBuffer[dwNumBytesToSend++] = 0xFC; 	// Bring clock line low too to make clock and data low
		OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in
	}

	// Turn the LED on by setting port AC6 low.
	OutputBuffer[dwNumBytesToSend++] = 0x82;	// Command to set directions of upper 8 pins and force value on bits set as output
	OutputBuffer[dwNumBytesToSend++] = 0xBF;	// Bit 6 is going low 
	OutputBuffer[dwNumBytesToSend++] = 0x40;	// Only bit 6 is output

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands
}



// ##############################################################################################################
// Function to set the I2C Stop state on the I2C clock and data lines
// It takes the clock line high whilst keeping data low, and then takes both lines high
// It also sends a GPIO command to set bit 6 of ACbus high to turn off the LED. This acts as an activity indicator
// Turns on (low) during the I2C Start and off (high) during the I2C stop condition, giving a short blink.  
// ##############################################################################################################

void SetI2CStop(void)
{
	dwNumBytesToSend = 0;			//Clear output buffer
	DWORD dwCount;

	// Initial condition for the I2C Stop - Pull data low (Clock will already be low and is kept low)
	for(dwCount=0; dwCount<4; dwCount++)		// Repeat commands to ensure the minimum period of the stop setup time is achieved
	{
		OutputBuffer[dwNumBytesToSend++] = 0x80;	// Command to set directions of ADbus and data values for pins set as o/p
		OutputBuffer[dwNumBytesToSend++] = 0xFC;	// put data and clock low
		OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in
	}

	// Clock now goes high (open drain)
	for(dwCount=0; dwCount<4; dwCount++)		// Repeat commands to ensure the minimum period of the stop setup time is achieved
	{
		OutputBuffer[dwNumBytesToSend++] = 0x80;	// Command to set directions of ADbus and data values for pins set as o/p
		OutputBuffer[dwNumBytesToSend++] = 0xFD;	// put data low, clock remains high (open drain, pulled up externally)
		OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in
	}

	// Data now goes high too (both clock and data now high / open drain)
	for(dwCount=0; dwCount<4; dwCount++)	// Repeat commands to ensure the minimum period of the stop hold time is achieved
	{
		OutputBuffer[dwNumBytesToSend++] = 0x80;	// Command to set directions of ADbus and data values for pins set as o/p
		OutputBuffer[dwNumBytesToSend++] = 0xFF;	// both clock and data now high (open drain, pulled up externally)
		OutputBuffer[dwNumBytesToSend++] = 0xFB;	// Set all pins as output except bit 2 which is the data_in
	}
		
	// Turn the LED off by setting port AC6 high.
		OutputBuffer[dwNumBytesToSend++] = 0x82;	// Command to set directions of upper 8 pins and force value on bits set as output
		OutputBuffer[dwNumBytesToSend++] = 0xFF;	// All lines high (including bit 6 which drives the LED) 
		OutputBuffer[dwNumBytesToSend++] = 0x40;	// Only bit 6 is output

	ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		//Send off the commands
}






// #########################################################################################
// #########################################################################################
// OPENING DEVICE AND MPSSE CONFIGURATION
// #########################################################################################
// #########################################################################################

int _tmain(int argc, _TCHAR* argv[])
{
	DWORD dwCount;
	DWORD devIndex = 0;
	char Buf[64];
		
	// Open the UM232H module by it's description in the EEPROM
	// Note: See FT_OpenEX in the D2xx Programmers Guide for other options available
	//ftStatus = FT_OpenEx("UM232H", FT_OPEN_BY_DESCRIPTION, &ftHandle);
	ftStatus = FT_OpenEx("Nusbio /2", FT_OPEN_BY_DESCRIPTION, &ftHandle);
	
	// Check if Open was successful
	if (ftStatus != FT_OK)
	{
		printf("Can't open FT232H device! \n");
		getchar();
		return 1;
	}
	else
    {   
		// #########################################################################################
		// After opening the device, Put it into MPSSE mode
		// #########################################################################################
				
		// Print message to show port opened successfully
		printf("Successfully opened FT232H device! \n");

		// Reset the FT232H
		ftStatus |= FT_ResetDevice(ftHandle); 	
		
		// Purge USB receive buffer ... Get the number of bytes in the FT232H receive buffer and then read them
		ftStatus |= FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	 
		if ((ftStatus == FT_OK) && (dwNumInputBuffer > 0))
		{
			FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead);  	
		}

		ftStatus |= FT_SetUSBParameters(ftHandle, 65536, 65535);			// Set USB request transfer sizes
		ftStatus |= FT_SetChars(ftHandle, false, 0, false, 0);				// Disable event and error characters
		ftStatus |= FT_SetTimeouts(ftHandle, 5000, 5000);					// Set the read and write timeouts to 5 seconds
		ftStatus |= FT_SetLatencyTimer(ftHandle, 16);						// Keep the latency timer at default of 16ms
		ftStatus |= FT_SetBitMode(ftHandle, 0x0, 0x00); 					// Reset the mode to whatever is set in EEPROM
		ftStatus |= FT_SetBitMode(ftHandle, 0x0, 0x02);	 					// Enable MPSSE mode
		
		// Inform the user if any errors were encountered
		if (ftStatus != FT_OK)
		{
			printf("failure to initialize FT232H device! \n");
			getchar();
			return 1;
		}

		Sleep(50);	

		// #########################################################################################
		// Synchronise the MPSSE by sending bad command AA to it
		// #########################################################################################

		dwNumBytesToSend = 0;																// Used as an index to the buffer
		OutputBuffer[dwNumBytesToSend++] = 0xAA;											// Add an invalid command 0xAA
		ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		// Send off the invalid command
		
		// Check if the bytes were sent off OK
		if(dwNumBytesToSend != dwNumBytesSent)
		{
			printf("Write timed out! \n");
			getchar();
			return 1;
		}

		// Now read the response from the FT232H. It should return error code 0xFA followed by the actual bad command 0xAA
		// Wait for the two bytes to come back 

		dwNumInputBuffer = 0;
		ReadTimeoutCounter = 0;

		ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);						// Get number of bytes in the input buffer

		while ((dwNumInputBuffer < 2) && (ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
		{
			// Sit in this loop until
			// (1) we receive the two bytes expected
			// or (2) a hardware error occurs causing the GetQueueStatus to return an error code
			// or (3) we have checked 500 times and the expected byte is not coming 
			ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer
			ReadTimeoutCounter ++;
			Sleep(1);													// short delay
		}

		// If the loop above exited due to the byte coming back (not an error code and not a timeout)
		// then read the bytes available and check for the error code followed by the invalid character
		if ((ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
		{
			ftStatus = FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead); // Now read the data

			// Check if we have two consecutive bytes in the buffer with value 0xFA and 0xAA
			bCommandEchod = false;
			for (dwCount = 0; dwCount < dwNumBytesRead - 1; dwCount++)							
			{
				if ((InputBuffer[dwCount] == BYTE(0xFA)) && (InputBuffer[dwCount+1] == BYTE(0xAA)))
				{
					bCommandEchod = true;
					break;
				}
			}
		}
		// If the device did not respond correctly, display error message and exit.

			if (bCommandEchod == false) 
		{	
			printf("fail to synchronize MPSSE with command 0xAA \n");
			getchar();
			return 1;
		}
	
	

		// #########################################################################################
		// Synchronise the MPSSE by sending bad command AB to it
		// #########################################################################################

		dwNumBytesToSend = 0;																// Used as an index to the buffer
		OutputBuffer[dwNumBytesToSend++] = 0xAB;											// Add an invalid command 0xAB
		ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);		// Send off the invalid command
		
		// Check if the bytes were sent off OK
		if(dwNumBytesToSend != dwNumBytesSent)
		{
			printf("Write timed out! \n");
			getchar();
			return 1;
		}


		// Now read the response from the FT232H. It should return error code 0xFA followed by the actual bad command 0xAA
		// Wait for the two bytes to come back 

		dwNumInputBuffer = 0;
		ReadTimeoutCounter = 0;

		ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);						// Get number of bytes in the input buffer

		while ((dwNumInputBuffer < 2) && (ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
		{
			// Sit in this loop until
			// (1) we receive the two bytes expected
			// or (2) a hardware error occurs causing the GetQueueStatus to return an error code
			// or (3) we have checked 500 times and the expected byte is not coming 
			ftStatus = FT_GetQueueStatus(ftHandle, &dwNumInputBuffer);	// Get number of bytes in the input buffer
			ReadTimeoutCounter ++;
			Sleep(1);													// short delay
		}

		// If the loop above exited due to the byte coming back (not an error code and not a timeout)
		// then read the bytes available and check for the error code followed by the invalid character
		if ((ftStatus == FT_OK) && (ReadTimeoutCounter < 500))
		{
			ftStatus = FT_Read(ftHandle, &InputBuffer, dwNumInputBuffer, &dwNumBytesRead); // Now read the data

			// Check if we have two consecutive bytes in the buffer with value 0xFA and 0xAB
			bCommandEchod = false;
			for (dwCount = 0; dwCount < dwNumBytesRead - 1; dwCount++)							
			{
				if ((InputBuffer[dwCount] == BYTE(0xFA)) && (InputBuffer[dwCount+1] == BYTE(0xAB)))
				{
					bCommandEchod = true;
					break;
				}
			}
		}
		// If the device did not respond correctly, display error message and exit.

			if (bCommandEchod == false) 
		{	
			printf("fail to synchronize MPSSE with command 0xAB \n");
			getchar();
			return 1;
		}


		printf("MPSSE synchronized with BAD command \n");


		// #########################################################################################
		// Configure the MPSSE settings
		// #########################################################################################

		dwNumBytesToSend = 0;							// Clear index to zero
		OutputBuffer[dwNumBytesToSend++] = 0x8A; 		// Disable clock divide-by-5 for 60Mhz master clock
		OutputBuffer[dwNumBytesToSend++] = 0x97;		// Ensure adaptive clocking is off
		OutputBuffer[dwNumBytesToSend++] = 0x8C; 		// Enable 3 phase data clocking, data valid on both clock edges for I2C

		OutputBuffer[dwNumBytesToSend++] = 0x9E; 		// Enable the FT232H's drive-zero mode on the lines used for I2C ...
		OutputBuffer[dwNumBytesToSend++] = 0x07;		// ... on the bits 0, 1 and 2 of the lower port (AD0, AD1, AD2)...
		OutputBuffer[dwNumBytesToSend++] = 0x00;		// ...not required on the upper port AC 0-7

		OutputBuffer[dwNumBytesToSend++] = 0x85;		// Ensure internal loopback is off

		ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);	// Send off the commands

		// Now configure the dividers to set the SCLK frequency which we will use
		// The SCLK clock frequency can be worked out by the algorithm (when divide-by-5 is off)
		// SCLK frequency  = 60MHz /((1 +  [(1 +0xValueH*256) OR 0xValueL])*2)
		dwNumBytesToSend = 0;													// Clear index to zero
		OutputBuffer[dwNumBytesToSend++] = 0x86; 								// Command to set clock divisor
		OutputBuffer[dwNumBytesToSend++] = dwClockDivisor & 0xFF;				// Set 0xValueL of clock divisor
		OutputBuffer[dwNumBytesToSend++] = (dwClockDivisor >> 8) & 0xFF;		// Set 0xValueH of clock divisor
		ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);	// Send off the commands

		Sleep(20);																// Short delay 	
				
		// #########################################################################################
		// Configure the I/O pins of the MPSSE
		// #########################################################################################

		// Call the I2C function to set the lines of port AD to their required states
		SetI2CLinesIdle();

		// Also set the required states of port AC0-7. Bit 6 is used as an active-low LED, the others are unused
		// After this instruction, bit 6 will drive out high (LED off)
		//dwNumBytesToSend = 0;						// Clear index to zero
		//OutputBuffer[dwNumBytesToSend++] = '\x82';	// Command to set directions of upper 8 pins and force value on bits set as output
		//OutputBuffer[dwNumBytesToSend++] = '\xFF';  // Write 1's to all bits, only affects those set as output
		//OutputBuffer[dwNumBytesToSend++] = '\x40';	// Set bit 6 as an output
		//ftStatus = FT_Write(ftHandle, OutputBuffer, dwNumBytesToSend, &dwNumBytesSent);	// Send off the commands

		Sleep(30);		//Delay for a while
	}

	BOOL bSucceed = TRUE;












	// #########################################################################################
	// #########################################################################################
	// MAIN PROGRAM
	// #########################################################################################
	// #########################################################################################



	// #########################################################################################
	// Check the USB State
	// #########################################################################################

	SetI2CLinesIdle();								// Set idle line condition
	SetI2CStart();									// Set the start condition on the lines
	
	//bSucceed = SendAddrAndCheckACK(0x18, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	bSucceed = SendAddrAndCheckACK(0x74, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	

	bSucceed = SendAddrAndCheckACK(0x00, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	bSucceed = SendByteAndCheckACK(0x16);			// Send the USB State command   
		
	SetI2CLinesIdle();								// Set idle line condition as part of repeated start
	SetI2CStart();									// Send the start condition as part of repeated start
		
	bSucceed = SendAddrAndCheckACK(0x22, TRUE);		// Send the device address 0x22 rd (I2C = 0x45)
	bSucceed = ReadByteAndSendNAK();				// Read 1 byte from the device, and send NAK 

	SetI2CStop();									// Send the stop condition	
		
	printf("USB State value is: %x\n", InputBuffer[0]);	// Print the value returned

	// #########################################################################################
	// Read the I2C IDs
	// #########################################################################################

	SetI2CLinesIdle();								// Set idle line condition
	SetI2CStart();									// Send the start condition
		
	bSucceed = SendAddrAndCheckACK(0x7C, FALSE);	// Send the Device ID command 0x7C wr (I2C = 0xF8)
	bSucceed = SendAddrAndCheckACK(0x22, TRUE);		// followed by the device address 0x22 rd (i2C = 0x45)
				
	SetI2CLinesIdle();								// Set idle line condition as part of repeated start
	SetI2CStart();									// Send the start condition as part of repeated start
		
	bSucceed = SendAddrAndCheckACK(0x7C, TRUE);		// Send the Device ID command 0x7C rd (I2C = 0xF9)
	Read3BytesAndSendNAK();							// Read 3 bytes, ACK the first two and then NAK the last one

	SetI2CStop();									// Send the stop condition	
		
	printf("USB ID value 1 is: %x\n", InputBuffer[0]); 	// Print the result
	printf("USB ID value 2 is: %x\n", InputBuffer[1]);
	printf("USB ID value 3 is: %x\n", InputBuffer[2]);

	printf("\n");
	Sleep(100);
	
	// #########################################################################################
	// Reading values from the MTP Memory locations 0x00 to 0x7C (byte addressing)
	// #########################################################################################
		
	unsigned char MTP_ReadLoop = 0;
	unsigned char MemBuffer[0x7C];
	unsigned char ValueToWrite = 0x00;

	for(MTP_ReadLoop = 0x00; MTP_ReadLoop <0x7C; MTP_ReadLoop ++)
	{
		SetI2CLinesIdle();							// Set idle line condition
		SetI2CStart();								// Send the start condition
	
		bSucceed = SendAddrAndCheckACK(0x00, FALSE);// Send the general call address 0x00 wr (I2C = 0x00)
		bSucceed = SendByteAndCheckACK(0x10);		// Send the MTP address command 0x10 
	
		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Set the start condition as part of repeated start
	
		bSucceed = SendAddrAndCheckACK(0x22, FALSE);// Send the device address 0x22 wr (I2C = 0x44)
		bSucceed = SendByteAndCheckACK(0x00);		// Send the Most Significant byte of the MTP address
		bSucceed = SendByteAndCheckACK(MTP_ReadLoop);// Send the Least Significant byte of the MTP address

		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Send the start condition as part of repeated start

		bSucceed = SendAddrAndCheckACK(0x00, FALSE);// Send the general call address 0x00 wr (I2C = 0x00)
		bSucceed = SendByteAndCheckACK(0x14);		// Send the MTP read command 0x14

		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Send the start condition as part of repeated start

		bSucceed = SendAddrAndCheckACK(0x22, TRUE);	// Send the device address 0x22 rd (I2C = 0x45)
		ReadByteAndSendNAK();						// Read 1 byte from the device, and send NAK

		SetI2CStop();								// Send the stop condition	
	
		MemBuffer[MTP_ReadLoop] = InputBuffer[0];

		Sleep(20);
		printf("MTP byte location %x value is: %x\n", MTP_ReadLoop, MemBuffer[MTP_ReadLoop]);
	}

	// #########################################################################################
	// Write a value to the MTP Memory
	// #########################################################################################

	SetI2CLinesIdle();								// Set idle line condition
	SetI2CStart();									// Send the start condition
	
	bSucceed = SendAddrAndCheckACK(0x00, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	bSucceed = SendByteAndCheckACK(0x10);			// Send the MTP address command 0x10   
	
	SetI2CLinesIdle();								// Set idle line condition as part of repeated start
	SetI2CStart();									// Send the start condition as part of repeated start
	
	bSucceed = SendAddrAndCheckACK(0x22, FALSE);	// Send the device address 0x22 wr (I2C = 0x44)
	bSucceed = SendByteAndCheckACK(0x00);			// Send the Most Significant byte of the MTP address
	bSucceed = SendByteAndCheckACK(0x24);			// Send the Least Significant byte (writing to 0x24 here)

	SetI2CLinesIdle();								// Set idle line condition as part of repeated start
	SetI2CStart();									// Send the start condition as part of repeated start

	bSucceed = SendAddrAndCheckACK(0x00, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	bSucceed = SendByteAndCheckACK(0x12);			// Send the MTP Write command 0x12

	SetI2CLinesIdle();								// Set idle line condition as part of repeated start
	SetI2CStart();									// Send the start condition as part of repeated start

	bSucceed = SendAddrAndCheckACK(0x22, FALSE);	// Send the device address 0x22 wr (I2C = 0x44)
	
	if(MemBuffer[0x24] == 0x00)						// Toggle the byte to be written so that we alternate 
		ValueToWrite = 0x55;						// between 0x00 and 0x55 each time the program is run
	else
		ValueToWrite = 0x00;
	
	bSucceed = SendByteAndCheckACK(ValueToWrite);	// Write out the byte (0x00 or 0x55) to the MTP

	SetI2CStop();									// Send the stop condition	

	// #########################################################################################
	// Reading values from the MTP Memory locations 0x00 to 0x7C (byte addressing)
	// #########################################################################################
		
	MTP_ReadLoop = 0;
	MemBuffer[0x7C];
	ValueToWrite = 0x00;

	for(MTP_ReadLoop = 0x00; MTP_ReadLoop <0x7C; MTP_ReadLoop ++)
	{
		SetI2CLinesIdle();							// Set idle line condition
		SetI2CStart();								// Send the start condition
	
		bSucceed = SendAddrAndCheckACK(0x00, FALSE);// Send the general call address 0x00 wr (I2C = 0x00)
		bSucceed = SendByteAndCheckACK(0x10);		// Send the MTP address command 0x10 
	
		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Set the start condition as part of repeated start
	
		bSucceed = SendAddrAndCheckACK(0x22, FALSE);// Send the device address 0x22 wr (I2C = 0x44)
		bSucceed = SendByteAndCheckACK(0x00);		// Send the Most Significant byte of the MTP address
		bSucceed = SendByteAndCheckACK(MTP_ReadLoop);// Send the Least Significant byte of the MTP address

		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Send the start condition as part of repeated start

		bSucceed = SendAddrAndCheckACK(0x00, FALSE);// Send the general call address 0x00 wr (I2C = 0x00)
		bSucceed = SendByteAndCheckACK(0x14);		// Send the MTP read command 0x14

		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Send the start condition as part of repeated start

		bSucceed = SendAddrAndCheckACK(0x22, TRUE);	// Send the device address 0x22 rd (I2C = 0x45)
		ReadByteAndSendNAK();						// Read 1 byte from the device, and send NAK

		SetI2CStop();								// Send the stop condition	
	
		MemBuffer[MTP_ReadLoop] = InputBuffer[0];

		Sleep(20);
		printf("MTP byte location %x value is: %x\n", MTP_ReadLoop, MemBuffer[MTP_ReadLoop]);
	}

	// #########################################################################################
	// Flush the buffer
	// #########################################################################################

	SetI2CLinesIdle();								// Set idle line condition
	SetI2CStart();									// Set the start condition on the lines
		
	bSucceed = SendAddrAndCheckACK(0x00, FALSE);	// Send the general call address 0x00 wr (I2C = 0x00)
	bSucceed = SendByteAndCheckACK(0x0E);			// Send the Flush Buffer command  
		
	SetI2CStop();									// Send the stop condition	
		
	printf("Buffer flushed\n");	
	Sleep(100);


	
	printf("Press a key to continue to the reading/writing data mode\n");	
	getchar();



	while(1)
	{
		// #########################################################################################
		// Reading the Data Available register of an FT201X device on the I2C bus
		// #########################################################################################

		SetI2CLinesIdle();							// Set idle line condition
		SetI2CStart();								// Send the start condition
			
		bSucceed = SendAddrAndCheckACK(0x00, FALSE);// Send the general call address 0x00 wr (I2C = 0x00)
		bSucceed = SendByteAndCheckACK(0x0C);		// Send the DataAvailable command 0x0C 
			
		SetI2CLinesIdle();							// Set idle line condition as part of repeated start
		SetI2CStart();								// Send the start condition as part of repeated start
			
		bSucceed = SendAddrAndCheckACK(0x22, TRUE);	// Send the device address 0x22 rd (I2C = 0x45)
		ReadByteAndSendNAK();						// Read 1 byte from the device, and send NAK

		SetI2CStop();								// Send the stop condition	
			
		printf("ReadDataAvail value is: %x\n", InputBuffer[0]);
		printf("\n");

		// Now set a flag to show whether there were any bytes available so that we can read them next
		if(InputBuffer[0] > 0)						
			DataInBuffer = TRUE;
		else
			DataInBuffer = FALSE;

		// #########################################################################################
		// If data is available, now read a byte from the buffer
		// #########################################################################################
			
		if(DataInBuffer == TRUE)
		{
			SetI2CLinesIdle();						// Set idle line condition
			SetI2CStart();							// Send the start condition

			bSucceed = SendAddrAndCheckACK(0x22, TRUE);// Send the device address 0x22 rd (I2C = 0x45)
			ReadByteAndSendNAK();					// Read 1 byte from the device, and send NAK at the end since we only want one byte
			
			SetI2CStop();							// Send the stop condition	

			DataByte = InputBuffer[0];

			printf("Data value (in hex ascii) is: %x\n", DataByte);
			printf("Data value (as character) is: %c\n", DataByte);
			printf("\n");
    		
		}
			
		// #########################################################################################
		// Now send the character which we just read back to the FT-X
		// #########################################################################################

		if(DataInBuffer == TRUE)
		{
			SetI2CLinesIdle();						// Set idle line condition
			SetI2CStart();							// Send the start condition
				
			bSucceed = SendAddrAndCheckACK(0x22, FALSE);// Send the device address 0x22 wr (I2C = 0x44)
			bSucceed = SendByteAndCheckACK(byte(DataByte));	// Send the byte (which has just been read above) back to the FT-X
				
			SetI2CStop();							// Send the stop condition	

			printf("Sent the received character back to the FT-X\n");
			printf("\n");

			Sleep(2);
		}

		// Printing a line to separate each character	
		printf("------------------------------------------\n");
		printf("\n");

		// Delay to prevent the screen scrolling too quickly
		Sleep(1000);
	}
	return 0;
}











