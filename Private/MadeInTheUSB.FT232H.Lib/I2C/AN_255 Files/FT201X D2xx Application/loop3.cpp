// This code is provided as an example only and is not supported or guaranteed by FTDI
// It is the responsibility of the recipient to ensure correct operation of their overall system

//#include "stdafx.h"
#include <windows.h>
#include <stdio.h>
#include "ftd2xx.h"

int main(int argc, char* argv[])
{
	FT_HANDLE fthandle;	
	FT_STATUS status;
	DWORD numdev = 0;

	// Open the first device connected to the system (which has index 0). You can use the various other 
	// functions such as open_by_description to make this much more flexible and user friendly to let the user 
	// choose which device to open. See the D2xx Programmers Guide for more information

	// Check how many FTDI devices are connected and installed. If one or more connected, open the first one
	status = FT_CreateDeviceInfoList(&numdev);

	if 	((status == FT_OK) && (numdev > 0) )
	{
		// Open the device now
		status = FT_Open(0, &fthandle);
		if(status != FT_OK) 
			printf("status not ok %d\n", status);

	// Set the In transfer size. You can set up to 64K if required. Ideally, use a small value like this for 
	// receiving a few bytes at a time or a larger value if you will be transferring large amounts of data
		status = FT_SetUSBParameters(fthandle, 256, 0);
		if(status != FT_OK) 
			printf("status not ok %d\n", status); 

	// Reset the device 
		status = FT_ResetDevice(fthandle);
		if(status != FT_OK)
			printf("status not ok %d\n", status);

	// Set the handshaking mode in the driver, for I2C chips this has no affect on the external I2C interface 
	// since it does not have handshake lines but this enables internal handshake in the driver
		status = FT_SetFlowControl(fthandle, FT_FLOW_RTS_CTS, FT_STOP_BITS_1, FT_PARITY_NONE);
		if(status != FT_OK) 
			printf("status not ok %d\n", status);

	// Set Timeouts to ensure a Read or Write will return if unable to be completed
	// Setting both read and write timeouts to 5 seconds 
		status = FT_SetTimeouts(fthandle, 5000, 5000);
		if(status != FT_OK) 
			printf("status not ok %d\n", status);

	// Set Latency Timer (keeping it at default of 16ms here)
		status = FT_SetLatencyTimer(fthandle, 16);
		if(status != FT_OK) 
			printf("status not ok %d\n", status);

	// Now write some data to the chips buffer
		char data_out[12] = "HELLO WORLD";
		DWORD w_data_len = 12;
		DWORD data_written;	

		status = FT_Write(fthandle, data_out, w_data_len, &data_written);
		if(status != FT_OK) 
			printf("status not ok %d\n", status);
		else
			printf("12 Bytes Sent, waiting for bytes to come back\n");
/**********************************************************************/
// The I2C Master should now be able to read these 12 bytes from the FT-X over I2C. This example expects the 
// I2C Master to send the bytes back over I2C to the FT-X
/**********************************************************************/

	// Now read the data which the I2C master has written to the FT-X
		char data_in[12];
		DWORD data_read;	
		DWORD MyBytesReceived = 0;
		DWORD SoftwareTimeout = 0;
		
	// Wait for the FT-X to send our 12 bytes back to the PC
		while((MyBytesReceived <12) && (SoftwareTimeout < 500))
		{
			FT_GetQueueStatus(fthandle, &MyBytesReceived);
			Sleep(1);
			SoftwareTimeout ++;
		}

	// Check if the loop exited due to timing out or receiving 12 bytes
		if(SoftwareTimeout == 500)
		{
			printf("Timed out waiting for data\n");
		}
		else
		{
			// Now read the received bytes
			status = FT_Read(fthandle, data_in, MyBytesReceived, &data_read);
			if(status != FT_OK) 
				printf("status not ok %d\n", status);
			else 
				printf("data read %s\n", data_in);
		}
	// Close the device
		status = FT_Close(fthandle);
	}
	else
	{
		printf("No FTDI devices connected to the computer \n");		
	}
	
	printf("Press Return To End Program");
	getchar();
	printf("closed \n");
	return 0;
}
