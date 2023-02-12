using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FTD2XX_NET.FTDI;
using MadeInTheUSB.FT232H.Components.APA102;
using DynamicSugar;

namespace MadeInTheUSB.FT232H.Console
{
    partial class Program
    {
        static void GpioSample(IDigitalWriteRead gpios, bool oneLoopOnly = false)
        {
            var goOn           = true;
            const int waitTime = 65;
            var gpioIndexes = DS.Range(0, gpios.MaxGpio, 1);

            while (goOn) {

                gpioIndexes.ForEach((gpioIndex) => {
                    gpios.DigitalWrite(gpioIndex, PinState.High);
                    Thread.Sleep(waitTime);
                });
                Thread.Sleep(waitTime);

                gpioIndexes.ForEach((gpioIndex) => {
                    gpios.DigitalWrite(gpioIndex, PinState.Low);
                    Thread.Sleep(waitTime);
                });
                Thread.Sleep(waitTime);

                if(System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey();
                    if(k.Key == ConsoleKey.Q)
                        goOn = false;
                }
                if(oneLoopOnly)
                    goOn = false;
            }
        }

        private static void Sequence(IDigitalWriteRead gpios, params int [] pins)
        {
            AllOff(gpios);
            foreach(var i in pins)
                gpios.DigitalWrite(i, PinState.High);
        }

        static void CheetahBoosterDemo(IDigitalWriteRead gpios, bool oneLoopOnly = false)
        {
            var goOn = true;
            const int waitTime = 333;
            AllOff(gpios);

            while (goOn)
            {
                Sequence(gpios, 0);
                Thread.Sleep(waitTime);

                Sequence(gpios, 0, 1);
                Thread.Sleep(waitTime);

                Sequence(gpios, 0, 2, 4);
                Thread.Sleep(waitTime);

                Sequence(gpios, 1, 3, 5);
                Thread.Sleep(waitTime);

                Sequence(gpios, 2, 4, 6);
                Thread.Sleep(waitTime);

                Sequence(gpios, 3, 5, 7);
                Thread.Sleep(waitTime);

                Sequence(gpios, 4, 6, 7);
                Thread.Sleep(waitTime);

                Sequence(gpios, 6, 7);
                Thread.Sleep(waitTime);

                Sequence(gpios, 7);
                Thread.Sleep(waitTime);

                if (System.Console.KeyAvailable)
                {
                    var k = System.Console.ReadKey();
                    if (k.Key == ConsoleKey.Q)
                        goOn = false;
                }
                if (oneLoopOnly)
                    goOn = false;
            }
        }

        private static void AllOff(IDigitalWriteRead gpios)
        {
            for (var i = 0; i < gpios.MaxGpio; i++)
                gpios.DigitalWrite(i, PinState.Low);
        }
    }
}
