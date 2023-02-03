using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MadeInTheUSB.WinUtil
{
    public class BitUtil {


        public static byte ExtractBit(byte value, params int[] bitIndexes)
        {
            byte v = 0;
            foreach(var b in bitIndexes)
            {
                var bit = (1 << (b - 1)); // This is the bit we 
                if(IsSet(value, (byte)bit))
                    v = SetBit(v, (byte)bit);
            }
            return v;
        }

        public static List<int> pof2 = new List<int>()  { 1, 2, 4, 8, 16, 32, 64, 128};

        public static byte Inverse(byte v)
        {
            //byte v = (byte)BitUtil.ParseBinary("B11011101"); 
            var newV = 0;
            var j    = 0;
            for (var i = 8 - 1; i >= 0; i--)
            {
                var isSet = BitUtil.IsSet(v, (byte) pof2[i]);
                if (isSet)
                    newV += pof2[j];
                j++;
            }
            return (byte)newV;
        }

        public static List<int> Range(int start, int end)
        {
            return Enumerable.Range(start, end).ToList();
        }

        public static List<int> ParseBinary(List<string> binaryValues)
        {
            var l = new List<int>();
            foreach (var bv in binaryValues)
                l.Add(ParseBinary(bv));
            return l;
        }

        public static int ParseBinary(string s)
        {
            if (s.ToUpperInvariant().StartsWith("B"))
            {
                return Convert.ToInt32(s.Substring(1), 2);
            }
            else throw new ArgumentException(string.Format("Invalid binary value:{0}",s));
        }

        public static string BitRpr(byte [] buffer, bool newLine = true)
        {
            var t = new StringBuilder(1024);

            for (var i = 0; i < buffer.Length; i++) {

                t.AppendFormat("[{0}] {1}:{2} ", i.ToString("000"), buffer[i].ToString("000"), WinUtil.BitUtil.BitRpr(buffer[i]));
                if (newLine)
                    t.AppendLine();
            }
            return t.ToString();
        }

        public class BitRprArrayToStringResult
        {
            public int BitCount = 0;
            public string BitArrayDef;
            public UInt32 Value;
            public string StringValue;

            public byte ValueAsByte
            {
                get
                {
                    return (byte)this.Value;
                }
            }
            public double ByteCount
            {
                get
                {
                    return (this.BitCount / 8.0);
                }
            }
        }

        public static BitRprArrayToStringResult BitRprArrayGetAsciiString(BitRprArrayToStringResult r, int count)
        {
            var oemArray = new List<byte>();
            for (var i = 0; i < count; i++)
            {
                r = BitUtil.BitRprArrayGet(r, 8);
                oemArray.Add(r.ValueAsByte);
            }
            r.StringValue = System.Text.Encoding.Default.GetString(oemArray.ToArray());
            return r;
        }

        public static BitRprArrayToStringResult BitRprArrayGet(BitRprArrayToStringResult ba, int count, int shiftRight = 0, bool autoRightShift8 = false)
        {
            return BitRprArrayGet(ba.BitArrayDef, count, shiftRight, ba.BitCount, autoRightShift8);
        }

        public static BitRprArrayToStringResult BitRprArrayGet(string bitArrayDef, int count, int shiftRight = 0, int bitCount = 0, bool autoRightShift8 = false)
        {
            if(autoRightShift8) // Auto compute how much we need to shift for an 8 bit value
            {
                shiftRight = 8 - count;
            }

            var r = new BitRprArrayToStringResult() { BitCount = bitCount };

            List<UInt32> powers = null;

            if (count > 24 && count <= 32)
            {
                powers = new List<UInt32>() {
                    2147483648, 1073741824, 536870912, 268435456, 134217728, 67108864, 33554432, 16777216,
                    8388608, 4194304,2097152,1048576,524288,262144,131072,65536,
                    32768, 16384, 8192, 4096, 2048, 1024, 512, 256,
                    128, 64, 32, 16, 8, 4, 2, 1 };
            }
            else if (count > 16 && count <= 24)
            {
                powers = new List<UInt32>() {
                    8388608, 4194304,2097152,1048576,524288,262144,131072,65536,
                    32768, 16384, 8192, 4096, 2048, 1024, 512, 256,
                    128, 64, 32, 16, 8, 4, 2, 1 };
            }
            else if (count > 8 && count <= 16)
            {
                powers = new List<UInt32>() {
                    32768, 16384, 8192, 4096, 2048, 1024, 512, 256,
                    128, 64, 32, 16, 8, 4, 2, 1 };
            }
            else if (count <= 8) {
                powers = new List<UInt32>() { 128, 64, 32, 16, 8, 4, 2, 1 };
            }

            r.BitCount += count;
            var s = bitArrayDef.Substring(0, count);
            var si = 0;

            if(powers.Count > 8)
            {
                si = powers.Count - s.Length;
            }

            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == '1')
                    r.Value += powers[si];
                si++;
            }
            r.BitArrayDef = bitArrayDef.Substring(count);

            if(shiftRight>0)
            {
                r.Value = r.Value >> shiftRight;
            }
            return r;
        }

        public static string BitRprArrayToString(byte[] buffer)
        {
            var t = new StringBuilder(1024);
            for (var i = 0; i < buffer.Length; i++)
            {
                t.Append(BitRpr(buffer[i]));
            }
            return t.ToString();
        }

        public static string BitRpr(byte value, bool detail = false)
        {
            if (detail)
            {
                return string.Format("{0}:{1}", value.ToString("000"), BitRpr(value, false));
            }
            else
            {
                var s = System.Convert.ToString(value, 2);
                return s.PadLeft(8, '0');
            }
        }

        public static string BitRpr(int value)
        {
            var s = System.Convert.ToString(value, 2);
            return s.PadLeft(8, '0');
            return s;
        }

        public static byte UnsetBit(byte value, byte bit)
        {
            value &= ((byte) ~ bit);
            //if ((value & bit) == bit)
            //    value -= bit;
            return value;
        }


        public static ushort Byte2UInt16(byte high_byte, byte low_byte)
        {
            int a = ((high_byte << 8)) | (low_byte);
            return (ushort)a;
        }
        
        public static List<byte> SliceBuffer(List<byte> buffer, int start, int count)
        {
            var l = new List<byte>();
            for (var i = start; i < start + count; i++)
            {
                l.Add(buffer[i]);
            }
            return l;
        }

        public static List<byte> ByteBuffer(params  int [] integers)
        {
            var l = new List<byte>(integers.Length);
            foreach (var i in integers)
                l.Add((byte)i);
            return l;
        }

        public static byte HighByte(ushort number)
        {
            byte upper = (byte) (number >> 8);            
            return upper;
        }

        public static byte LowByte(ushort number)
        {
            byte lower = (byte) (number & 0xff);
            return lower;
        }
        
        public static bool IsSet(int value, byte bit)
        {
            return IsSet((byte) value, bit);
        }

        public static bool IsSet(byte value, byte bit)
        {
            if(value == 0 && bit ==0)
                return false;
            return (value & bit) == bit;
        }

        public static byte UnsetBitByIndex(byte value, int bitIndex)
        {
            var bit = (byte)Math.Pow(2, bitIndex);
            return UnsetBit(value, bit);

            //if ((value & bit) == bit)
            //    value -= bit;
            //return value;
        }

        public static byte SetBitIndex(byte value, int bitIndex)
        {
            var bit = (byte)Math.Pow(2, bitIndex);
            return SetBit(value, bit);

            //if ((value & bit) != bit)
            //    value += bit;
            //return value;
        }

        public static byte SetBit(byte value, byte bit)
        {
            value |= bit;
            //if ((value & bit) != bit)
            //    value += bit;
            return value;
        }

        public static byte SetBitOnOff(int value, byte bit, bool on)
        {
            if (on)
                return SetBit((byte)value, bit);
            else
                return UnsetBit((byte)value, bit);
        }
    }

    public class StopWatch : Stopwatch
    {
        public long ElapsedMicroSeconds
        {
            get
            {
                long microseconds = this.ElapsedTicks/ (Stopwatch.Frequency / (1000L*1000L));
                return microseconds;
            }
        }
        public long ElapsedNanoSeconds
        {
            get
            {
                long microseconds = this.ElapsedTicks/ (Stopwatch.Frequency / (1000L*1000L*1000L));
                return microseconds;
            }
        }
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/System.Diagnostics.Stopwatch(v=vs.110).aspx
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms686298(v=vs.85).aspx
    /// http://stackoverflow.com/questions/8894425/difference-between-elapsedticks-elapsedmilliseconds-elapsed-milliseconds-and-e
    /// </summary>
    public class TimePeriod : IDisposable
    {
        private const string WINMM = "winmm.dll";

        private static TIMECAPS timeCapabilities;

        private static int inTimePeriod;

        private readonly int period;

        private int disposed;

        [DllImport(WINMM, ExactSpelling = true)]
        private static extern int timeGetDevCaps(ref TIMECAPS ptc, int cbtc);

        [DllImport(WINMM, ExactSpelling = true)]
        private static extern int timeBeginPeriod(int uPeriod);

        [DllImport(WINMM, ExactSpelling = true)]
        private static extern int timeEndPeriod(int uPeriod);

        private static Stopwatch __sw;

        static TimePeriod()
        {
            __sw = new Stopwatch();
            int result = timeGetDevCaps(ref timeCapabilities, Marshal.SizeOf(typeof(TIMECAPS)));
            if (result != 0)
            {
                throw new InvalidOperationException("The request to get time capabilities was not completed because an unexpected error with code " + result + " occured.");
            }
        }

        public TimePeriod(int period, bool throwException = true)
        {
            if (Interlocked.Increment(ref inTimePeriod) != 1)
            {
                Interlocked.Decrement(ref inTimePeriod);
                throw new NotSupportedException("The process is already within a time period. Nested time periods are not supported.");
            }

            if (period < timeCapabilities.wPeriodMin || period > timeCapabilities.wPeriodMax)
            {
                if(throwException)
                    throw new ArgumentOutOfRangeException("period", "The request to begin a time period was not completed because the resolution specified is out of range.");
            }

            int result = timeBeginPeriod(period);
            if (result != 0)
            {
                throw new InvalidOperationException("The request to begin a time period was not completed because an unexpected error with code " + result + " occured.");
            }

            this.period = period;
        }

        public static int MinimumPeriod
        {
            get
            {
                return timeCapabilities.wPeriodMin;
            }
        }

        public static int MaximumPeriod
        {
            get
            {
                return timeCapabilities.wPeriodMax;
            }
        }
        
        public static long Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        public static long TimestampTick
        {
            get { return Stopwatch.GetTimestamp(); }
        }

        public static string GetTimeCapabilityInfo()
        {
            var b = new StringBuilder();
            b.AppendFormat("Min:{0}, ", MinimumPeriod);
            b.AppendFormat("Max:{0}, ", MaximumPeriod);
            b.AppendFormat("Freq:{0}", Frequency);
            return b.ToString();
        }

        public int Period
        {
            get
            {
                if (this.disposed > 0)
                {
                    throw new ObjectDisposedException("The time period instance has been disposed.");
                }

                return this.period;
            }
        }

        public void Dispose()
        {
            if (Interlocked.Increment(ref this.disposed) == 1)
            {
                timeEndPeriod(this.period);
                Interlocked.Decrement(ref inTimePeriod);
            }
            else
            {
                Interlocked.Decrement(ref this.disposed);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TIMECAPS
        {
            internal int wPeriodMin;

            internal int wPeriodMax;
        }

        public static double ElapsedMicroseconds(long elapsedTicks)
        {
            return 1e6 * elapsedTicks / (double)Stopwatch.Frequency;
        }

        public static void __SleepMicro(int microSeconds)
        {
            __sw.Start();
            while (ElapsedMicroseconds(__sw.ElapsedTicks) <= microSeconds);
            __sw.Stop();
        }

        public void SleepMicro(int microSeconds)
        {
            __SleepMicro(microSeconds);
        }

        public static void Sleep(int ms, bool sleepZero = false)
        {
            if (ms == 0 && sleepZero == false)
                return; // we do not even call Sleep for a 0, which has an effect
            Thread.Sleep(ms);
        }

    }
}


