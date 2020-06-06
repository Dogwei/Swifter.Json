using NUnit.Framework;
using Swifter.Tools;
using System;
using System.Numerics;
using System.Reflection;
using System.Text;
using static NUnit.Framework.Assert;

namespace Swifter.Test
{
    public class XConvertTest
    {
        [Test]
        public void ConvertTest()
        {
            const string str = "JXU3MkQ3JXU0RjFGJXU2NjJGJXU0RTE2JXU3NTRDJXU0RTBBJXU2NzAwJXU1RTA1JXU3Njg0JXU0RUJB";

            AreEqual(XConvert<string>.Convert(XConvert<byte[]>.Convert(str)), str);

            AreEqual(XConvert<int>.Convert(XConvert<BigInteger>.Convert(int.MinValue)), int.MinValue);

            AreEqual(XConvert<BigInteger>.Convert(XConvert<byte[]>.Convert((BigInteger)ulong.MaxValue)), (BigInteger)ulong.MaxValue);


            AreEqual(XConvert<string>.FromObject(XConvert.ToObject(str, typeof(byte[]))), str);

            AreEqual(XConvert<int>.FromObject(XConvert.Cast(int.MinValue, typeof(BigInteger))), int.MinValue);

            AreEqual(XConvert<BigInteger>.FromObject(XConvert<byte[]>.FromObject((BigInteger)ulong.MaxValue)), (BigInteger)ulong.MaxValue);
        }

        [Test]
        public void AddImplTest()
        {
            XConvert.AddImplFactory(new MyImplFactory());

            AreEqual("2", XConvert.Convert<string, StringBuilder>("2").ToString());
            AreEqual("2", XConvert.Convert<int, StringBuilder>(2).ToString());
        }

        class MyImplFactory : IConverterFactory, IXConverter<int, StringBuilder>
        {
            public StringBuilder Convert(int value)
            {
                return new StringBuilder().Append(value);
            }

            public object GetConverter(Type sourceType, Type destinationType)
            {
                return this;
            }
        }
    }
}
