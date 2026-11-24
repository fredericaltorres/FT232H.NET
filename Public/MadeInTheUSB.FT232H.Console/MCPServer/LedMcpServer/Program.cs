using MadeInTheUSB;
using MadeInTheUSB.FT232H;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static MadeInTheUSB.FT232H.SpiConfig;

namespace LedMcpServer
{
    internal static class Program
    {
        /*
         
            Get-Content "$env:APPDATA\Claude\claude_desktop_config.json" -Raw | ConvertFrom-Json | Out-Null; "valid JSON"
            Get-Process LedMcpServer -ErrorAction SilentlyContinue | Select-Object Id, StartTime, Path

            C:\Users\FredericTorres\AppData\Roaming\Claude
            C:\Users\FredericTorres\AppData\Roaming\Claude\claude_desktop_config.json
            C:\Users\FredericTorres\AppData\Local\Claude-Data\claude_pzs8sxrjxfjjc\claude_desktop_config.json
            C:\Users\FredericTorres\AppData\Local\Claude\logs


$exe = "C:\DVT\FT232H.NET\Public\MadeInTheUSB.FT232H.Console\MCPServer\LedMcpServer\bin\Debug\net48\LedMcpServer.exe"
"exe exists: " + (Test-Path $exe)
Get-ChildItem "$env:APPDATA\Claude\logs" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 8 Name, LastWriteTime, Length | Format-Table -AutoSize | Out-String
"--- mcp-server-led.log ---"
Get-Content "$env:APPDATA\Claude\logs\mcp-server-led.log" -Tail 30 -ErrorAction SilentlyContinue
"--- mcp.log (led lines) ---"
Select-String -Path "$env:APPDATA\Claude\logs\mcp.log" -Pattern "led" -ErrorAction SilentlyContinue | Select-Object -Last 15 | ForEach-Object { $_.Line }


NUSBIO USB Device, Turn on LED 0.
Light the first LED.
Set the LED at index 0.
         */

        public static string Trace(string message, [CallerMemberName] string methodName = "")
        {
            return Logger.Trace(message, null, methodName: methodName);
        }

        public const bool DEBUG = false;

        private static int Main(string[] args)
        {
            try
            {
                Trace($"Starting....  {Environment.CommandLine} ", null);

                if (DEBUG && Environment.GetEnvironmentVariable("LED_DEBUG") == "1")
                {
                    Trace("Waiting to be attached");
                    while (!System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                    System.Diagnostics.Debugger.Break();
                }

                var nusbioInitialized = InitNusbio();

                // stdout carries ONLY protocol messages; UTF-8 without BOM.
                var utf8 = new UTF8Encoding(false);
                var input = new StreamReader(Console.OpenStandardInput(), utf8);
                var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { AutoFlush = true, NewLine = "\n" };

                Trace($"Nusbio /2 - FT232H Library - MCP Server - RUNNING - nusbioInitialized: {nusbioInitialized}");
                new McpServer(input, output).Run();
            }
            catch (Exception ex)
            {
                Trace($"[ERROR]{ex}");
            }
            return 0;
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
            catch(Exception ex)
            {
                Trace($"Nusbio init failed {ex}");
            }
            return false;
        }
    }
}
