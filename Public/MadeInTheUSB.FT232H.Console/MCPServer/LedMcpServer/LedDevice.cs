using MadeInTheUSB.FT232H;
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
        public static SpiDevice __ft232hGpioSpiDevice = null;

        public void SetLed(int index, bool on)
        {
            Console.Error.WriteLine("[sim] set_led(" + index + ", " + (on ? "on" : "off") + ")");
            __ft232hGpioSpiDevice.GPIO.DigitalWrite(index, on ? PinState.High: PinState.Low);
        }

        public void Dispose() { }
    }
}
