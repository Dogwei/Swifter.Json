using Swifter.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace Swifter.Test.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {

                var json = JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" });

                using (StringReader sr = new StringReader(json))
                {
                    var jsonFormatter = new JsonFormatter();

                    var obj = jsonFormatter.DeserializeAsync(sr, typeof(Dictionary<string, object>));

                    obj.Wait();

                    System.Console.WriteLine(jsonFormatter.Serialize(obj.Result));
                }

                GC.Collect();
            }
        }
    }
}
