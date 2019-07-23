using Swifter.Json;
using Swifter.Reflection;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Swifter.Test
{
    public class Demo
    {
        public static void Main()
        {
            FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.IndexId64;
            FastObjectRW.DefaultOptions &= ~FastObjectRWOptions.IgnoreCase;

            DataTableRW.DefaultOptions = DataTableRWOptions.SetFirstRowsTypeToColumnTypes |
                DataTableRWOptions.WriteToArrayFromBeginningSecondRows;

            Application.Run(new MyForm());
        }
    }
}