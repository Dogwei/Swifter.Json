using Swifter.Json;
using Swifter.RW;
using System;

namespace Swifter.Test.Debug
{
    public class Demo
    {
        public int TestZero { get; set; } = 0;
        public int TestNonZero { get; set; } = 1;

        public string TestEmptyString { get; set; } = "";
        public string TestNonEmptyString { get; set; } = "Dogwei";

        public object TestNull { get; set; } = null;
        public object TestNonNull { get; set; } = new object();

        public static void Main()
        {

            Console.WriteLine("Fuck123".GetHashCode());
        }
    }
}
