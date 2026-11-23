using System;
using System.IO.Ports;

namespace LedMcpServer
{
    /// <summary>Abstraction over the USB device: switch LED 0..7 on or off.</summary>
    public interface ILedDevice : IDisposable
    {
        void SetLed(int index, bool on);
    }

    /// <summary>No hardware: logs to stderr (stdout is reserved for MCP protocol messages).</summary>
    public sealed class SimulatedLedDevice : ILedDevice
    {
        public void SetLed(int index, bool on)
        {
            Console.Error.WriteLine("[sim] set_led(" + index + ", " + (on ? "on" : "off") + ")");
        }

        public void Dispose() { }
    }

    /// <summary>
    /// Device exposed as a USB virtual COM port (CDC/FTDI/CH340...).
    /// ADAPT <see cref="SetLed"/> to your device's real wire protocol.
    /// </summary>
    public sealed class SerialLedDevice : ILedDevice
    {
        private readonly SerialPort _port;
        private readonly object _lock = new object();

        public SerialLedDevice(string portName, int baudRate)
        {
            _port = new SerialPort(portName, baudRate) { WriteTimeout = 2000, NewLine = "\n" };
            _port.Open();
        }

        public void SetLed(int index, bool on)
        {
            lock (_lock)
            {
                // Placeholder protocol: ASCII line "LED <n> <1|0>\n". Replace as needed.
                _port.WriteLine("LED " + index + " " + (on ? "1" : "0"));
            }
        }

        public void Dispose()
        {
            if (_port.IsOpen) _port.Close();
            _port.Dispose();
        }
    }
}
