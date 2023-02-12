using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadeInTheUSB.FT232H
{
    public class PerformanceHelper : IDisposable
    {
        public static void AssertString(string s1, string s2)
        {
            if(s1.Length != s2.Length)
                throw new ArgumentException($"Different string len {s1.Length} - {s2.Length}");
            for(var i=0; i< s1.Length; i++)
            {
                if (s1[i] != s2[i])
                {
                    //throw new ArgumentException($"Different char at postion {i} c1:{s1[i]} c2:{s2[i]}");
                    Console.WriteLine($"Different char at postion {i} c1:{s1[i]} c2:{s2[i]}");
                    break;
                }
            }
        }
        public static string Get64kString(string _4CharString)
        {
            if (_4CharString.Length != 4)
                throw new ArgumentException($"The parameter _4CharString must contains a 4 chars string");

            var s = "";
            for (var i = 0; i < 1024 * 16; i++)
                s += _4CharString;
            return s;
        }
        public static byte[] GetAsciiBuffer(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }
        public static string AsciiBufferToString(byte [] buffer)
        {
            return Encoding.ASCII.GetString(buffer);
        }
        public static string Get64kStringFred()
        {
            return Get64kString("FRED");
        }
        public static string Get64kStringAbcd()
        {
            return Get64kString("ABCD");
        }
        public static string Get64kString0123()
        {
            return Get64kString("0123");
        }
        public static string Get4kStringABCD()
        {
            var s = "";
            for (var i = 0; i < 1024; i++)
                s += "ABCD";
            return s;
        }
        Stopwatch _stopwatch;
        int _byteCounter = 0;
        public PerformanceHelper Start()
        {
            _stopwatch = Stopwatch.StartNew();
            return this;
        }
        public void AddByte(int byteCount)
        {
            _byteCounter += byteCount;
        }
        public void Stop()
        {
            _stopwatch.Stop();
        }
        public string GetResultInfo()
        {
            var bytePerSecond = _byteCounter / (_stopwatch.ElapsedMilliseconds / 1000.0);
            var mbBytePerSecond = _byteCounter / (_stopwatch.ElapsedMilliseconds / 1000.0) / 1024 / 1024;
            return $"{mbBytePerSecond:0.00} Mb/S b/S:{bytePerSecond:0.00}, time:{_stopwatch.ElapsedMilliseconds / 1000.0:0.00}";
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
