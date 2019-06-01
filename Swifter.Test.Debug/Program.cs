using Swifter.Json;
using Swifter.Reflection;
using Swifter.RW;
using System;

namespace Swifter.Test.Debug
{
    internal class Demo
    {
        public static void Main()
        {
            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

            Console.WriteLine(JsonFormatter.SerializeObject(new { Id = 1, Name = "Dogwei" }));
        }
    }
}
