using System;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class BasicImplicitFactory : IInternalXConverterFactory
    {
        static int GetCode(Type type)
        {
            const int
                BooleanCode = 0x1,
                ByteCode = 0x2,
                SByteCode = 0x4,
                Int16Code = 0x8 | ByteCode | SByteCode,
                UInt16Code = 0x10 | ByteCode,
                Int32Code = 0x20 | UInt16Code | Int16Code,
                UInt32Code = 0x40 | UInt16Code,
                Int64Code = 0x80 | UInt32Code | Int32Code,
                UInt64Code = 0x100 | UInt32Code,
                CharCode = 0x200,
                SingleCode = 0x400 | Int64Code | UInt64Code,
                DoubleCode = 0x800 | SingleCode;

            // TODO: 应处理枚举类型。

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => BooleanCode,
                TypeCode.Byte => ByteCode,
                TypeCode.SByte => SByteCode,
                TypeCode.Int16 => Int16Code,
                TypeCode.UInt16 => UInt16Code,
                TypeCode.Int32 => Int32Code,
                TypeCode.UInt32 => UInt32Code,
                TypeCode.Int64 => Int64Code,
                TypeCode.UInt64 => UInt64Code,
                TypeCode.Char => CharCode,
                TypeCode.Single => SingleCode,
                TypeCode.Double => DoubleCode,
                _ => 0
            };
        }

        public static bool CanConvert(Type sourceType, Type destinationType)
        {
            var sourceCode = GetCode(sourceType);

            return sourceCode != 0 && (GetCode(destinationType) & sourceCode) == sourceCode;
        }

        public XConvertMode Mode => XConvertMode.BasicImplicit;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            if (CanConvert(typeof(TSource), typeof(TDestination)))
            {
                return SystemConvertFactory.GetMethod(typeof(TSource), typeof(TDestination));
            }

            return null;
        }
    }
}