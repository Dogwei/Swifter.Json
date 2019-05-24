using Swifter.Json;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Swifter.Test.Debug
{
    public class Demo
    {
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public bool Boolean { get; set; }
        public string String { get; set; }

        public static void Main()
        {
            var json = "{,}";

            var demo = JsonFormatter.DeserializeObject<Demo>(json);

            Console.WriteLine(JsonFormatter.SerializeObject(demo));
        }
    }
}
