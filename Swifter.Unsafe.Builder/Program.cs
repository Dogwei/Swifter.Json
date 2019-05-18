using System;
using System.Runtime.InteropServices;

namespace Swifter
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var builder = new UnsafeBuilder();

            builder.BuildAll();

            builder.Save();

#if NET20 
            // Console.WriteLine(Unsafe.Equal(decimal.Zero, decimal.Zero));

#endif
        }
    }
}