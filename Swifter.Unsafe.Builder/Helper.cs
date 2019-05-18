using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter
{
    public static class Helper
    {
        public static void Deconstruct<T>(this T[] array, out T t0, out int length)
        {
            t0 = array[0];

            length = array.Length;
        }

        public static void Deconstruct<T>(this T[] array, out T t0, out T t1, out int length)
        {
            t0 = array[0];
            t1 = array[1];

            length = array.Length;
        }
    }
}