using MadeInTheUSB;
using MadeInTheUSB.FT232H;
using System;
using System.IO;
using System.Linq;
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


$exe = "C:\DVT\FT232H.NET\Public\MadeInTheUSB.FT232H.Console\MCPServer\LedMcpServer\bin\Debug\net48\LedMcpServer.exe"
"exe exists: " + (Test-Path $exe)
Get-ChildItem "$env:APPDATA\Claude\logs" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 8 Name, LastWriteTime, Length | Format-Table -AutoSize | Out-String
"--- mcp-server-led.log ---"
Get-Content "$env:APPDATA\Claude\logs\mcp-server-led.log" -Tail 30 -ErrorAction SilentlyContinue
"--- mcp.log (led lines) ---"
Select-String -Path "$env:APPDATA\Claude\logs\mcp.log" -Pattern "led" -ErrorAction SilentlyContinue | Select-Object -Last 15 | ForEach-Object { $_.Line }


         */
        private static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("LED_DEBUG") == "1")
            {
                Console.WriteLine("Waiting to be attached");
                while (!System.Diagnostics.Debugger.IsAttached)
                {
                    System.Threading.Thread.Sleep(500);
                }
                System.Diagnostics.Debugger.Break();
            }

            var ft232Device = FT232HDetector.Detect();
            if (ft232Device.Ok)
            {
                var parts = ft232Device.ToString().Split(',').ToList();
                parts = parts.Select(s => s.Trim()).ToList();
                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library - MCP Server", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                ConsoleEx.WriteMenu(0, 2, $"Detected:{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", parts)}");

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
            }
            else
            {
            }

            // stdout carries ONLY protocol messages; UTF-8 without BOM.
            var utf8 = new UTF8Encoding(false);
            var input = new StreamReader(Console.OpenStandardInput(), utf8);
            var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { AutoFlush = true, NewLine = "\n" };

            new McpServer(input, output).Run();
            return 0;
        }
    }
}
