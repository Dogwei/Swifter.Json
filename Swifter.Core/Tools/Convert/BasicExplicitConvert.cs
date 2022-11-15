using System;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class BasicExplicitConvert : IInternalXConverterFactory
    {
        static int GetCode(Type type)
        {
            const int
                BooleanCode = 1,
                NumberCode = 2;

            // TODO: 应处理枚举类型。

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => BooleanCode,
                TypeCode.Byte => NumberCode,
                TypeCode.SByte => NumberCode,
                TypeCode.Int16 => NumberCode,
                TypeCode.UInt16 => NumberCode,
                TypeCode.Int32 => NumberCode,
                TypeCode.UInt32 => NumberCode,
                TypeCode.Int64 => NumberCode,
                TypeCode.UInt64 => NumberCode,
                TypeCode.Char => NumberCode,
                TypeCode.Single => NumberCode,
                TypeCode.Double => NumberCode,
                _ => 0
            };
        }

        public static bool CanConvert(Type sourceType, Type destinationType)
        {
            var sourceCode = GetCode(sourceType);

            return sourceCode != 0 && (GetCode(destinationType) & sourceCode) == sourceCode;
        }

        public XConvertMode Mode => XConvertMode.BasicExplicit;

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