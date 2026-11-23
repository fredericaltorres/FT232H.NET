using MadeInTheUSB;
using MadeInTheUSB.FT232H;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace LedMcpServer
{
    internal static class Program
    {
        // Usage: LedMcpServer.exe [--port COM3] [--baud 115200]
        // No --port (and no LED_PORT env var) => simulated device, logs to stderr.
        private static int Main(string[] args)
        {

            var ft232Device = FT232HDetector.Detect();
            if (ft232Device.Ok)
            {
                var parts = ft232Device.ToString().Split(',').ToList();
                parts = parts.Select(s => s.Trim()).ToList();
                System.Console.Clear();
                ConsoleEx.TitleBar(0, "Nusbio /2 - FT232H Library - MCP Server", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
                ConsoleEx.WriteMenu(0, 2, $"Detected:{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", parts)}");
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
