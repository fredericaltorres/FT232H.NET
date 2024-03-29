﻿using BufferUtil.Lib;
using DynamicSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.FT232H
{
    /// <summary>
    /// Based class for any FT232H derived classes
    /// </summary>
    public class FT232HDeviceBaseClass
    {
        internal static IntPtr _spiHandle = IntPtr.Zero;
        protected I2CDevice_MPSSE_NotUsed _i2cDevice = null;
        protected const int _maxGpio = 8;
        protected const int ValuesDefaultMask = 0;
        protected const int DirectionDefaultMask = 0xFF;
        protected const int _gpioStartIndex = 0;

        public byte GpioStartIndex { get { return _gpioStartIndex; } }

        public byte MaxGpio
        {
            get { return _maxGpio; }
        }


        public List<int> PowerOf2 = new List<int>()
        {
            1, 2, 4, 8, 16, 32, 64, 128,
            256, 512, 1024, 2048
        };

        public static string LogFile = @"c:\temp\nusbio.protocol.log";
        public bool Log = false;

        public static int LogTransactionBufferMaxLength = 1024;

        public enum I2CTransactionType
        {
            READ,
            WRITE,
            ERROR,
            WR_RD_START,
            WR_RD_END,
            DETECT_DEVICE,
            MESSAGE_START,
            MESSAGE_END,
            READ_REGISTER,
            WRITE_REGISTER
        }

        private static Dictionary<byte, string> registeredDeviceForLogging = new Dictionary<byte, string>();

        public void RegisterDeviceIdForLogging(byte deviceId, Type netType)
        {
            if(!registeredDeviceForLogging.ContainsKey(deviceId))
            {
                registeredDeviceForLogging.Add(deviceId, netType.Name);
            }
        }

        static List<string> _cachedRows = new List<string>();
        const int _cachedRowsSize = 64;
        public void ForceWriteLogCache()
        {
            WriteCache(null, forceWriteCache: true);
        }

        public static void WriteCache(string s, bool forceWriteCache = false)
        {
            if(s != null)
                _cachedRows.Add(s);
            if (_cachedRows.Count == _cachedRowsSize || forceWriteCache)
            {
                var sb = new StringBuilder();
                foreach (var ss in _cachedRows)
                    sb.AppendLine(ss);
                File.AppendAllText(LogFile, sb.ToString());
                _cachedRows.Clear();
            }
        }

        public static string TrimPad(string s, int max)
        {
            if(s.Length > max) 
                s = s.Substring(0, max);
            return s.PadLeft(max);
        }

        public int LogI2CTransactionMessage(byte deviceId, string message, Func<int> a)
        {
            var sw = Stopwatch.StartNew();
            LogI2CTransaction(I2CTransactionType.MESSAGE_START, deviceId, 0, null, null, message);
            var r = a();
            sw.Stop();
            LogI2CTransaction(I2CTransactionType.MESSAGE_END, deviceId, sw.ElapsedMilliseconds, null, null, message);
            return r;
        }

        public void LogI2CTransaction(I2CTransactionType transactionType, byte deviceId, long durationMS, byte[] bufferOut, byte[] bufferIn, string value = null, int recursiveCounter = 0)
        {
            bool forceToFlush = false;
            if (this.Log)
            {
                if(transactionType == I2CTransactionType.MESSAGE_START)
                {
                    forceToFlush = true;
                }

                var sb = new StringBuilder();

                sb.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] ");
                sb.Append($"IC2 {transactionType.ToString().PadRight(14)} ");

                if (registeredDeviceForLogging.ContainsKey(deviceId))
                    sb.Append(TrimPad(registeredDeviceForLogging[deviceId], 16)).Append(" ");
                else
                    sb.Append(TrimPad("", 16)).Append(" ");

                sb.Append($"0x{deviceId:X} ");

                sb.Append($"{durationMS:0000}ms ");
                if(durationMS > 100)
                {
                    if (Debugger.IsAttached)
                    {
                        forceToFlush = true;
                        //Debugger.Break();
                    }
                }

                if (value != null)
                {
                    sb.Append($"{value} ");
                }

                if (bufferOut!= null && bufferOut.Length > 0)
                {
                    sb.Append("OUT:[");
                    sb.Append(HexaString.ConvertTo(bufferOut, itemFormat: "{0}, ", max: LogTransactionBufferMaxLength));
                    sb.Append("]");
                }

                if (bufferIn != null && bufferIn.Length > 0)
                {
                    if (bufferOut != null && bufferOut.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("IN: [");
                    sb.Append(HexaString.ConvertTo(bufferIn, itemFormat: "{0}, ", max: LogTransactionBufferMaxLength));
                    sb.Append("]");
                }

                try
                {
                    WriteCache(sb.ToString());
                }
                catch (System.Exception ex)
                {
                    if (recursiveCounter == 0)
                    {
                        Thread.Sleep(111);
                        LogI2CTransaction(transactionType, deviceId, durationMS, bufferOut, bufferIn, value, recursiveCounter + 1);
                    }
                    else throw;
                }
            }
            if(forceToFlush) {
                ForceWriteLogCache();
            }
        }

        public void LogSpiTransaction(byte[] bufferOut, byte[] bufferIn, string message = null, int recursiveCounter = 0)
        {
            if (this.Log)
            {
                var sb = new StringBuilder();

                sb.Append($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}]");
                sb.Append("SPI_TRAN ");

                if (message != null )
                {
                    sb.Append(message);
                }

                if (bufferOut != null && bufferOut.Length > 0)
                {
                    sb.Append("OUT:[");
                    sb.Append(HexaString.ConvertTo(bufferOut, itemFormat: "{0}, ", max: LogTransactionBufferMaxLength));
                    sb.Append("]");
                }

                if (bufferIn != null && bufferIn.Length > 0)
                {
                    if (bufferOut != null && bufferOut.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("IN: [");
                    sb.Append(HexaString.ConvertTo(bufferIn, itemFormat: "{0}, ", max: LogTransactionBufferMaxLength));
                    sb.Append("]");
                }

                try
                {
                    WriteCache(sb.ToString());
                }
                catch(System.Exception ex)
                {
                    if (recursiveCounter == 0)
                    {
                        Thread.Sleep(111);
                        LogSpiTransaction(bufferOut, bufferIn, message, recursiveCounter + 1);
                    }
                    else throw;
                }
            }
        }
    }
}