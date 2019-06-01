using Swifter.Json;
using Swifter.Reflection;
using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Swifter.Test
{
    public class Demo
    {
        public static void Main()
        {
            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

            // Console.WriteLine((int)'\r');
            Application.Run(new MyForm());
        }
    }
}