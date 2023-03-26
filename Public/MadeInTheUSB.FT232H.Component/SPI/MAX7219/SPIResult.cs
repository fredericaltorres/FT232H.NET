using System;
using System.Linq;
using MadeInTheUSB.WinUtil;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using int16_t  = System.Int16; // Nice C# feature allowing to use same Arduino/C type
using uint16_t = System.UInt16;
using uint8_t  = System.Byte;
using size_t   = System.Int16;

namespace MadeInTheUSB
{
    public class SPIResult
    {
        public List<byte> Buffer = new List<uint8_t>();
        public bool OperationSucceeded;
        public int Value;

        public SPIResult Failed()
        {
            this.OperationSucceeded = false;
            return this;
        }
        public SPIResult Succeed(List<byte> buffer = null)
        {
            if(buffer != null)
                this.Buffer = buffer;
            this.OperationSucceeded = true;
            return this;
        }
        public int Val0
        {
            get
            {
                if (this.Buffer == null) return -1;
                if (this.Buffer.Count == 0) return -1;
                return this.Buffer[0];
            }
        }
    }
}