using Swifter.Json;
using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Swifter.Test
{
    internal class Program
    {
        public unsafe static void Main()
        {
            Application.Run(new MyForm());
        }
    }
}