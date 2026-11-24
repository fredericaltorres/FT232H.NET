using MadeInTheUSB.FT232H;
using System;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using static MadeInTheUSB.FT232H.SpiConfig;

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


        private static bool InitNusbio()
        {
            try
            {
                Trace("Detecting...");
                var ft232Device = FT232HDetector.Detect();
                if (ft232Device.Ok)
                {
                    var parts = ft232Device.ToString().Split(',').ToList();
                    parts = parts.Select(s => s.Trim()).ToList();
                    //System.Console.Clear();
                    Trace("Nusbio /2 - FT232H Library - MCP Server");
                    Trace($"Detected:{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", parts)}");

                    var ft232hGpioSpiDevice = new SpiDevice(SpiClockSpeeds._10Mhz);
                    SimulatedLedDevice.__ft232hGpioSpiDevice = ft232hGpioSpiDevice;

                    ft232hGpioSpiDevice.Log = true;
                    var spi = ft232hGpioSpiDevice.SPI;
                    var gpios = ft232hGpioSpiDevice.GPIO;
                    gpios.Animate();

                    gpios.SetPinMode(0, PinMode.Output);
                    gpios.SetPinMode(1, PinMode.Output);
                    gpios.SetPinMode(2, PinMode.Output);
                    gpios.SetPinMode(3, PinMode.Output);
                    gpios.SetPinMode(4, PinMode.Output);
                    gpios.SetPinMode(5, PinMode.Output);
                    gpios.SetPinMode(6, PinMode.Output);
                    gpios.SetPinMode(7, PinMode.Output);
                    return true;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Trace($"Nusbio init failed {ex}");
            }
            return false;
        }

        public void SetLed(int index, bool on)
        {
            if (__ft232hGpioSpiDevice == null)
            {
                InitNusbio();
            }

            if (__ft232hGpioSpiDevice != null)
            {
                Trace($"[nusbio] set_led({index}, {(on ? "on" : "off")})");
                __ft232hGpioSpiDevice.GPIO.DigitalWrite(index, on ? PinState.High : PinState.Low);
            }
            else
            {
                Trace($"[nusbio][not_initialized] set_led({index}, {(on ? "on" : "off")}) -- ");
            }
        }

        public void Dispose() { }
    }
}
