using Newtonsoft.Json;
using Swifter.Json;
using Swifter.RW;
using Swifter.Test.WPF.Models;
using Swifter.Test.WPF.Models.Polymorphism;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Documents;

namespace Swifter.Test.WPF.Tests
{
    public interface ITest
    {
        string TestName { get; }
        Type ObjectType { get; }
    }

    public static class TestHelper
    {
        public static bool Equals<TKey>(IDataReader<TKey> reader1, IDataReader<TKey> reader2)
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

    public abstract class BaseTest<TObject> : ITest
    {
        public virtual Type ObjectType => typeof(TObject);

        public virtual string TestName => GetType().Name;

        public abstract TObject GetObject();

        public virtual bool Equals(TObject obj1, TObject obj2)
        {
            return TestHelper.Equals(obj1, obj2);
        }
    }

    public class Int32Array : BaseTest<int[]>
    {
        public override int[] GetObject()
        {
            return Enumerable.Range(1, 9999).ToArray();
        }
    }

    public class Int64Array : BaseTest<long[]>
    {
        public override long[] GetObject()
        {
            return Enumerable.Range(9999, 99999).Select(i=>(long)i).ToArray();
        }
    }

    public class DoubleArray : BaseTest<double[]>
    {
        public override double[] GetObject()
        {
            return Enumerable.Range(0, 99999).Select(i => Math.Pow(i, 12)).ToArray();
        }
    }

    public class DecimalArray : BaseTest<decimal[]>
    {
        public override decimal[] GetObject()
        {
            return new RandomValueReader(1812).ReadArray<decimal>();
        }
    }

    public class SingleArray : BaseTest<float[]>
    {
        public override float[] GetObject()
        {
            return Enumerable.Range(0, 1630).Select(i => (float)Math.Pow(i, 12)).ToArray();
        }
    }

    public class DeviceModel : BaseTest<Device>
    {
        public override string TestName => "Model";

        public override Device GetObject()
        {
            return new RandomValueReader(1218).FastReadObject<Device>();
        }
    }

    public class CatalogModel : BaseTest<Catalog>
    {
        public override string TestName => "Catalog";

        public override Catalog GetObject()
        {
            return JsonFormatter.DeserializeObject<Catalog>(File.ReadAllText(@"..\..\..\..\Swifter.Test\Resources\catalog.json"));
        }
    }

    public class DevicesDataTable : BaseTest<DataTable>
    {
        public override string TestName => "DataTable";

        public override DataTable GetObject()
        {
            return ValueCopyer.ValueOf(new RandomValueReader(1218).ReadList<Device>()).ReadDataTable().IdentifyColumnTypes();
        }
    }

    public class Devices : BaseTest<List<Device>>
    {
        public override string TestName => "List<Model>";

        public override List<Device> GetObject()
        {
            return new RandomValueReader(1218).ReadList<Device>();
        }
    }

    public class DeviceDictionary : BaseTest<Dictionary<string,object>>
    {
        public override string TestName => "Dictionary<string,object>";

        public override Dictionary<string, object> GetObject()
        {
            return ValueCopyer.ValueOf(new RandomValueReader(1218).FastReadObject<Device>()).ReadDictionary<string, object>();
        }
    }

    public class StringTest : BaseTest<string>
    {
        public override string GetObject()
        {
            return new RandomValueReader(1218).ReadString();
        }
    }

    public class PolymorphismTest : BaseTest<Polymorphism>
    {
        public override Polymorphism GetObject()
        {
            return new RandomValueReader(1218).FastReadObject<Polymorphism>();
        }
    }

    public class TwoDimArray : BaseTest<int[,]>
    {
        public override int[,] GetObject()
        {
            return ValueInterface<int[,]>.ReadValue(new RandomValueReader(1218));
        }
    }

    public class ThreeDimArray : BaseTest<int[,,]>
    {
        public override int[,,] GetObject()
        {
            return ValueInterface<int[,,]>.ReadValue(new RandomValueReader(1218));
        }
    }

}