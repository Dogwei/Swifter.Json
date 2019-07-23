using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Swifter
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new UnsafeBuilder();

            builder.BuildAll();

            builder.Save();
        }
    }
}