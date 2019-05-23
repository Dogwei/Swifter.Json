using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Swifter
{
    class Program
    {
        static async void Main(string[] args)
        {

        Applier:

            Console.WriteLine("Fuck");


            Stream stream = default;

            int num = await stream.ReadAsync(null, 0, 0);

            Console.WriteLine(num);

            goto Applier;

        }
    }
}