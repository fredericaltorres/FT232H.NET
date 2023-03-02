using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MadeInTheUSB
{
    public static class ConsoleEx
    {
        [StructLayout(LayoutKind.Sequential)]
        struct POSITION
        {
            public short x;
            public short y;
        }
 
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int GetStdHandle(int nStdHandle);
 
        [DllImport("kernel32.dll", EntryPoint = "SetConsoleCursorPosition", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetConsoleCursorPosition(int hConsoleOutput, POSITION dwCursorPosition);

        public static void Gotoxy(int x, int y)
        {
            const int STD_OUTPUT_HANDLE = -11;
            int hConsoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            POSITION position;
            position.x = (short) x;
            position.y = (short) y;
            SetConsoleCursorPosition(hConsoleHandle, position);
        }

        public static void WriteMenu(int x, int y, string text)
        {
            if (x == -1)
                x = (GetMaxCol() - text.Length)/2;
            Gotoxy(x, y);
            var i = 0;
            while(i < text.Length)
            {
                if (i < text.Length - 1)
                {
                    if (text[i + 1] == ')')
                    {
                        Write(text[i], ConsoleColor.Cyan);
                        Write(text[i + 1], ConsoleColor.DarkGray);
                        i += 1;
                    }
                    else
                    {
                        Write(text[i], ConsoleColor.DarkCyan);
                    }
                }
                else
                {
                    Write(text[i], ConsoleColor.DarkCyan);
                }
                i += 1;
            }
            Console.WriteLine();
        }

        public static void WriteLine(int x, int y, string text, ConsoleColor c)
        {
            Gotoxy(x, y);
            WriteLine(text, c);
        }

        public static void WriteLine(string text, ConsoleColor c)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(text);
            Console.ForegroundColor = color;
        }

        public static void Write(int x, int y, string text, ConsoleColor c)
        {
            Gotoxy(x, y);
            Write(text, c);
        }

        static int GetMaxCol()
        {
            return Console.WindowWidth;
        }

        public static int WindowHeight
        {
            get
            {                
                return 25;
            }
        }

        public static void Bar(int x, int y, string text, ConsoleColor textColor, ConsoleColor backGroundColor)
        {
            Gotoxy(0, y);

            Write("".PadLeft(GetMaxCol(), ' '), textColor, backGroundColor);

            Gotoxy(x, y);
            Write(text, textColor, backGroundColor);
        }

        public static void TitleBar(int y, string text, ConsoleColor textColor = ConsoleColor.Yellow, ConsoleColor backGroundColor = ConsoleColor.DarkBlue)
        {
            Gotoxy(0, y);

            Write("".PadLeft(GetMaxCol(), ' '), textColor, backGroundColor);

            Gotoxy((GetMaxCol() - text.Length)/2, y);
            Write(text, textColor, backGroundColor);
        }

        private static void Write(char car, ConsoleColor textColor)
        {
            Write(car.ToString(), textColor);
        }

        public static void Write(int x, int y, string text, ConsoleColor textColor, ConsoleColor? backGroundColor = null)
        {
            if (x == -1)
                x = (GetMaxCol() - text.Length)/2;
            Gotoxy(x, y);
            Write(text, textColor, backGroundColor);
        }

        public static T WaitOnComponentToBePlugged<T>(string componentName, Func<T> initComponentCode)
        {
            while(true) {

                var component = initComponentCode();
                if(component != null)
                    return component;

                var r = ConsoleEx.Question(1, string.Format("Component {0} not found. R)etry A)bandon", componentName), new List<char> { 'R', 'A'});
                if(r == 'A')
                   return default(T);
            }
        }

        public static char Question(int y, string message, List<char> answers)
        {
            Write(0, y, "".PadLeft(80,' '), ConsoleColor.Yellow, ConsoleColor.Red);
            Write(0, y, message+" ?", ConsoleColor.Yellow, ConsoleColor.Red);
            Gotoxy(message.Length+2, y);
            while (true)
            {
                var k = Console.ReadKey();
                var c = k.KeyChar.ToString().ToUpperInvariant()[0];
                if (answers.Contains(c))
                {
                    return c;
                }
            }
        }

        public static void Write(string text, ConsoleColor textColor, ConsoleColor? backGroundColor = null)
        {
            var bTextColor       = Console.ForegroundColor;
            var bBackGroundColor = Console.BackgroundColor;

            Console.ForegroundColor = textColor;
            if (backGroundColor.HasValue)
                Console.BackgroundColor = backGroundColor.Value;

            Console.Write(text);

            Console.ForegroundColor = bTextColor;
            Console.BackgroundColor = bBackGroundColor;
        }

        public static void Wait(int second)
        {
            Thread.Sleep(second*1000);
        }
    }
}
