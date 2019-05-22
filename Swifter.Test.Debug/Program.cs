using System;

namespace Swifter.Test.Debug
{
    public class Demo
    {
        public string Name { get; set; }

        public static void Main()
        {
            var obj = new { Name = "Fuck", Id = 1 };

            Console.WriteLine(obj);

            // Console.WriteLine(JsonFormatter.SerializeObject(obj));
        }
    }
}
