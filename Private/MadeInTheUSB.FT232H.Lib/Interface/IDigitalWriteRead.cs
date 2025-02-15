using System;
using System.Collections.Generic;

namespace MadeInTheUSB.FT232H
{
    public interface IDigitalWriteRead
    {
        void SetPinMode(int pin, PinMode mode);
        void DigitalWrite(int pin, PinState state);
        PinState DigitalRead(int pin);
        void SetGpioMask(byte mask);
        byte GetGpioMask(bool forceRead = false);
        byte GpioStartIndex { get; }
        byte MaxGpio { get; }
        List<int> GpioIndexes { get; }
        void SetPullUp(int p, PinState d);

        void ProgressNext(bool clear = false);
        void Animate();
    }

}