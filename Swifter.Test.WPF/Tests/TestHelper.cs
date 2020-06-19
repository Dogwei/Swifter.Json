using Swifter.MessagePack;
using Swifter.RW;
using System;
using System.Collections.Generic;

namespace Swifter.Test.WPF.Tests
{
    public static class TestHelper
    {
        public static bool Equals<TKey>(IDataReader<TKey> reader1, IDataReader<TKey> reader2)
        {
            try
            {
                using var keys1 = reader1.Keys.GetEnumerator();
                using var keys2 = reader2.Keys.GetEnumerator();

                Loop:

                var m1 = keys1.MoveNext();
                var m2 = keys2.MoveNext();

                if (m1 != m2)
                {
                    return false;
                }

                if (m1)
                {
                    var key1 = keys1.Current;
                    var key2 = keys1.Current;
                    var value1 = reader1[key1].DirectRead();
                    var value2 = reader2[key2].DirectRead();

                    if (Equals(key1, key2) && Equals(value1, value2))
                    {
                        goto Loop;
                    }

                    return false;
                }

                return true;
            }
            catch
            {
                var obj1 = MessagePackFormatter.DeserializeObject<object>(MessagePackFormatter.SerializeObject(reader1));
                var obj2 = MessagePackFormatter.DeserializeObject<object>(MessagePackFormatter.SerializeObject(reader2));

                return Equals(obj1, obj2);
            }
        }

        public static bool Almost(double x, double y)
        {
            if (Math.Abs(x / y - 1) > 1e-10)
            {
                return Equals(x, y);
            }

            return true;
        }

        public static bool Almost(float x, float y)
        {
            if (Math.Abs(x / y - 1) > 1e-2)
            {
                return Equals(x, y);
            }

            return true;
        }

        public static bool Equals<TValue>(TValue x, TValue y)
        {
            if (EqualityComparer<TValue>.Default.Equals(x, y))
            {
                return true;
            }

            if (x is double dx && y is double dy)
            {
                return Almost(dx, dy);
            }

            if (x is float fx && y is float fy)
            {
                return Almost(fx, fy);
            }

            if (x != null && y != null)
            {
                var rw1 = RWHelper.CreateReader(x, false);
                var rw2 = RWHelper.CreateReader(y, false);

                if (rw1 != null && rw2 != null)
                {
                    if (rw1.Count != rw2.Count)
                    {
                        return false;
                    }

                    if (rw1 is IDataReader<string> strRW1 && rw2 is IDataReader<string> strRW2)
                    {
                        return Equals(strRW1, strRW2);
                    }
                    if (rw1 is IDataReader<int> iRW1 && rw2 is IDataReader<int> iRW2)
                    {
                        return Equals(iRW1, iRW2);
                    }
                    else
                    {
                        return Equals(rw1.As<string>(), rw2.As<string>());
                    }
                }

                return rw1 == rw2;
            }

            return false;
        }

    }

}