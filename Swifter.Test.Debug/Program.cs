using Swifter.Json;
using Swifter.Formatters;
using Swifter.MessagePack;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Swifter.RW;

namespace Swifter.Test.Debug
{
    public sealed class Demo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static unsafe void Main()
        {
            var obj = new Demo();

            var json = "{Id:999,Name:Dogwei}";

            var jsonFormatter = new JsonFormatter();

            jsonFormatter.DeserializeTo(json, obj);

            Console.WriteLine(jsonFormatter.Serialize(obj));

            while (true)
            {
                var stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < 1000000; i++)
                {
                    var fast = FastObjectRW<Demo>.Create();

                    fast.Initialize(obj);

                    jsonFormatter.DeserializeTo(json, fast);
                }

                Console.WriteLine(stopwatch.ElapsedMilliseconds);
            }

        }
    }
}