using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace MUSB_FS
{
    public class ErrorManager
    {
        public static void Handle(System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }
}