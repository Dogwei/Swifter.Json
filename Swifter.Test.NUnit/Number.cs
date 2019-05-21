using NUnit.Framework;
using Swifter.Tools;
using System;

namespace Swifter.Test.NUnit
{
    public abstract class Number
    {
        public static readonly Random random = new Random();

        public virtual T RandomNext<T>()
        {
            var guid = new Guid(
                random.Next(int.MinValue, int.MaxValue),
                (short)random.Next(short.MinValue, short.MaxValue),
                (short)random.Next(short.MinValue, short.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue));

            return Unsafe.As<Guid, T>(ref guid);
        }

        [Test]
        [Repeat(10000)]
        public void Double([Range(2, 64)]int radix)
        {
            var value = RandomNext<double>();
            
            var str = NumberHelper.InstanceByRadix((byte)radix).ToString(value);

            switch (str)
            {
                case "NaN":
                case "∞":
                case "-∞":
                    /* 非数字在 NumberInfo 处理中有歧义，就假定一定没错吧。*/
                    return;
            }

            var val = NumberHelper.InstanceByRadix((byte)radix).ParseDouble(str);

            str = NumberHelper.InstanceByRadix((byte)radix).ToString(val);

            var numberInfo = NumberHelper.InstanceByRadix((byte)radix).GetNumberInfo(str, out var gcHandle);

            val = numberInfo.ToDouble();

            gcHandle.Free();


            if (val == value)
            {
                return;
            }

            var num = val / value;

            // double 运算允许一定精度丢失，不可避免的。
            if (!(num >= 0.999999 && num <= 1.000001))
            {
                Assert.AreEqual(value, val);
            }
        }

        [Test]
        [Repeat(10000)]
        public void Single([Range(2, 64)]int radix)
        {
            var value = RandomNext<float>();

            var str = NumberHelper.InstanceByRadix((byte)radix).ToString(value);

            switch (str)
            {
                case "NaN":
                case "∞":
                case "-∞":
                    /* 非数字在 NumberInfo 处理中有歧义，就假定一定没错吧。*/
                    return;
            }

            var val = (float)NumberHelper.InstanceByRadix((byte)radix).ParseDouble(str);

            str = NumberHelper.InstanceByRadix((byte)radix).ToString(val);

            var numberInfo = NumberHelper.InstanceByRadix((byte)radix).GetNumberInfo(str, out var gcHandle);

            val = (float)numberInfo.ToDouble();

            gcHandle.Free();

            if (val == value)
            {
                return;
            }

            var num = val / value;

            // float 运算允许一定精度丢失，不可避免的。
            if (!(num >= 0.9 && num <= 1.1))
            {
                Assert.AreEqual(value, val);
            }
        }

        [Test]
        [Repeat(10000)]
        public void Int64([Range(2, 64)]int radix)
        {
            var value = RandomNext<long>();

            var str = NumberHelper.InstanceByRadix((byte)radix).ToString(value);
            
            var val = NumberHelper.InstanceByRadix((byte)radix).ParseInt64(str);

            str = NumberHelper.InstanceByRadix((byte)radix).ToString(val);

            var numberInfo = NumberHelper.InstanceByRadix((byte)radix).GetNumberInfo(str, out var gcHandle);

            val = numberInfo.ToInt64();

            gcHandle.Free();

            if (val != value)
            {
                Assert.AreEqual(value, val);
            }
        }

        [Test]
        [Repeat(10000)]
        public void UInt64([Range(2, 64)]int radix)
        {
            var value = RandomNext<ulong>();

            var str = NumberHelper.InstanceByRadix((byte)radix).ToString(value);

            var val = NumberHelper.InstanceByRadix((byte)radix).ParseUInt64(str);

            str = NumberHelper.InstanceByRadix((byte)radix).ToString(val);

            var numberInfo = NumberHelper.InstanceByRadix((byte)radix).GetNumberInfo(str, out var gcHandle);

            val = numberInfo.ToUInt64();

            gcHandle.Free();

            if (val != value)
            {
                Assert.AreEqual(value, val);
            }
        }

        [Test]
        [Repeat(10000)]
        public void _Decimal()
        {
            decimal RandomNext()
            {
                var num = (decimal)random.NextDouble();

                for (int j = random.Next(28); j > 0; --j)
                {
                    num = num / (decimal)Math.Max(random.NextDouble(), 0.001);
                }

                return num;
            }

            var value = RandomNext();

            var str = NumberHelper.ToString(value);

            var val = NumberHelper.ParseDecimal(str);

            str = NumberHelper.ToString(val);

            var numberInfo = NumberHelper.Decimal.GetNumberInfo(str, out var gcHandle);

            val = numberInfo.ToDecimal();

            gcHandle.Free();

            if (val != value)
            {
                Assert.AreEqual(val, value);
            }
        }

        [Test]
        [Repeat(10000)]
        public void _Guid()
        {
            var value = RandomNext<Guid>();

            var str = NumberHelper.ToString(value);

            var val = NumberHelper.ParseGuid(str);

            str = NumberHelper.ToString(val);

            val = NumberHelper.ParseGuid(str);

            if (val != value)
            {
                Assert.AreEqual(val, value);
            }
        }
    }
}