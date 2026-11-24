using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace LedMcpServer
{
    /// <summary>
    /// Minimal MCP server over stdio (newline-delimited JSON-RPC 2.0).
    /// Supports: initialize, ping, tools/list, tools/call.
    /// </summary>
    public sealed class McpServer
    {
        private const string FallbackProtocolVersion = "2025-06-18";
        private readonly ILedDevice _device;
        private readonly TextReader _in;
        private readonly TextWriter _out;

        public static string Trace(string message, [CallerMemberName] string methodName = "")
        {
            return Logger.Trace(message, null, methodName: methodName);
        }

        public McpServer(TextReader input, TextWriter output)
        {
            Trace("McpServer.Starting....");
            _in = input;
            _out = output;
            _device = new SimulatedLedDevice();
        }

        public void Run()
        {
            Trace("McpServer.Running....");
            string line;
            while ((line = _in.ReadLine()) != null)
            {
                if (line.Trim().Length == 0) continue;

                JObject request;
                try
                {
                    Trace($"McpServer.Running line: {line}");
                    request = JObject.Parse(line);
                }
                catch (Exception ex)
                {
                    Trace($"[ERROR]McpServer.Running(1) {ex}");
                    Send(Error(JValue.CreateNull(), -32700, "Parse error"));
                    continue;
                }

                try
                {
                    var response = Handle(request);
                    if (response != null) Send(response);
                }
                catch (Exception ex)
                {
                    Trace($"[ERROR]McpServer.Running(2) {ex}");
                    JToken id = request["id"];
                    if (id != null) Send(Error(id.DeepClone(), -32603, "Internal error"));
                }
            }
        }

        private JObject Handle(JObject req)
        {
            string method = (string)req["method"];
            JToken id = req["id"];
            bool isNotification = id == null;

            Trace($"McpServer.Handle method: {method}");

            switch (method)
            {
                case "initialize":
                    {
                        string version = (string)req.SelectToken("params.protocolVersion") ?? FallbackProtocolVersion;
                        return Result(id, new JObject
                        {
                            ["protocolVersion"] = version,
                            ["capabilities"] = new JObject { ["tools"] = new JObject() },
                            ["serverInfo"] = new JObject { ["name"] = "led-mcp-server", ["version"] = "1.0.0" }
                        });
                    }

                case "notifications/initialized":
                case "notifications/cancelled":
                    return null;

                case "ping":
                    return isNotification ? null : Result(id, new JObject());

                case "tools/list":
                    return Result(id, new JObject { ["tools"] = new JArray(SetLedToolDefinition()) });

                case "tools/call":
                    return Result(id, CallTool(req["params"] as JObject));

                default:
                    return isNotification ? null : Error(id, -32601, "Method not found: " + method);
            }
        }

        private static JObject SetLedToolDefinition()
        {
            return new JObject
            {
                ["name"] = "set_led",
                ["description"] = "NUSBIO USB Device, Turn on or off the LED with the given index on the connected USB device. Valid indexes are integers from 0 to 7. Set 'on' to false to turn the LED off (default: true, turns it on).",
                ["inputSchema"] = new JObject
                {
                    ["type"] = "object",
                    ["properties"] = new JObject
                    {
                        ["index"] = new JObject
                        {
                            ["type"] = "integer",
                            ["minimum"] = 0,
                            ["maximum"] = 7,
                            ["description"] = "LED index, 0 to 7"
                        },
                        ["on"] = new JObject
                        {
                            ["type"] = "boolean",
                            ["default"] = true,
                            ["description"] = "true = turn the LED on (default), false = turn it off"
                        }
                    },
                    ["required"] = new JArray("index"),
                    ["additionalProperties"] = false
                }
            };
        }

        private JObject CallTool(JObject p)
        {
            string name = p == null ? null : (string)p["name"];

            Trace($"McpServer.CallTool name: {name}");

            if (name != "set_led")
                return ToolResult("Unknown tool: " + name, true);

            JToken indexToken = p["arguments"] == null ? null : p["arguments"]["index"];
            if (indexToken == null || indexToken.Type != JTokenType.Integer)
                return ToolResult("Argument 'index' is required and must be an integer 0-7.", true);


            Trace($"McpServer.CallTool indexToken: {indexToken}");

            long index = (long)indexToken;
            if (index < 0 || index > 7)
                return ToolResult("Argument 'index' must be between 0 and 7 (got " + index + ").", true);

            bool on = true;
            JToken onToken = p["arguments"]["on"];

            Trace($"McpServer.CallTool on: {onToken}");

            if (onToken != null && onToken.Type != JTokenType.Null)
            {
                if (onToken.Type != JTokenType.Boolean)
                    return ToolResult("Argument 'on' must be a boolean (true = on, false = off).", true);
                on = (bool)onToken;
            }

            try
            {
                _device.SetLed((int)index, on);
                return ToolResult("LED " + index + (on ? " turned on." : " turned off."), false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Device error: " + ex);
                return ToolResult("Device error: " + ex.Message, true);
            }
        }

        private static JObject ToolResult(string text, bool isError)
        {
            return new JObject
            {
                ["content"] = new JArray(new JObject { ["type"] = "text", ["text"] = text }),
                ["isError"] = isError
            };
        }

        private static JObject Result(JToken id, JObject result)
        {
            return new JObject { ["jsonrpc"] = "2.0", ["id"] = id.DeepClone(), ["result"] = result };
        }

        private static JObject Error(JToken id, int code, string message)
        {
            return new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id,
                ["error"] = new JObject { ["code"] = code, ["message"] = message }
            };
        }

        private void Send(JObject message)
        {
            // Single line, no embedded newlines (Formatting.None is the default for ToString(Formatting)).
            _out.WriteLine(message.ToString(Newtonsoft.Json.Formatting.None));
            _out.Flush();
        }
    }
}
