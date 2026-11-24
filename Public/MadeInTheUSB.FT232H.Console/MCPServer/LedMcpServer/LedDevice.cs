using MadeInTheUSB.FT232H;
using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;

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

        public static string Trace(string message, [CallerMemberName] string methodName = "")
        {
            return Logger.Trace(message, null, methodName: methodName);
        }

        public void SetLed(int index, bool on)
        {
            //Console.Error.WriteLine("[sim] set_led(" + index + ", " + (on ? "on" : "off") + ")");
            Trace("[sim] set_led(" + index + ", " + (on ? "on" : "off") + ")");
            __ft232hGpioSpiDevice.GPIO.DigitalWrite(index, on ? PinState.High: PinState.Low);
        }

        public void Dispose() { }
    }
}
