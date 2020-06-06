using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.Tools
{
    internal static class InternalConvert
    {
        internal static readonly List<IConverterFactory> factories;

        static InternalConvert()
        {
            factories = new List<IConverterFactory>();
        }

        public static int GetImplicitCode(Type type)
        {
            const int
                Boolean = 0x1,
                Byte = 0x2,
                SByte = 0x4,
                Int16 = 0x8 | Byte | SByte,
                UInt16 = 0x10 | Byte,
                Int32 = 0x20 | UInt16 | Int16,
                UInt32 = 0x40 | UInt16,
                Int64 = 0x80 | UInt32 | Int32,
                UInt64 = 0x100 | UInt32,
                Char = 0x200,
                Single = 0x400 | Int64 | UInt64,
                Double = 0x800 | Single;

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => Boolean,
                TypeCode.Byte => Byte,
                TypeCode.SByte => SByte,
                TypeCode.Int16 => Int16,
                TypeCode.UInt16 => UInt16,
                TypeCode.Int32 => Int32,
                TypeCode.UInt32 => UInt32,
                TypeCode.Int64 => Int64,
                TypeCode.UInt64 => UInt64,
                TypeCode.Char => Char,
                TypeCode.Single => Single,
                TypeCode.Double => Double,
                _ => 0
            };
        }

        public static int GetExplicitCode(Type type)
        {
            const int
                Boolean = 0x1,
                Byte = 0x2,
                SByte = 0x4,
                Int16 = 0x8,
                UInt16 = 0x10,
                Int32 = 0x20,
                UInt32 = 0x40,
                Int64 = 0x80,
                UInt64 = 0x100,
                Char = 0x200,
                Single = 0x400,
                Double = 0x800;

            const int Number = Byte | SByte | Int16 | UInt16 | Int32 | UInt32 | Int64 | UInt64 | Char | Single | Double;

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => Boolean,
                TypeCode.Byte => Number,
                TypeCode.SByte => Number,
                TypeCode.Int16 => Number,
                TypeCode.UInt16 => Number,
                TypeCode.Int32 => Number,
                TypeCode.UInt32 => Number,
                TypeCode.Int64 => Number,
                TypeCode.UInt64 => Number,
                TypeCode.Char => Number,
                TypeCode.Single => Number,
                TypeCode.Double => Number,
                _ => 0
            };
        }

        public static bool IsImplicitConvert(Type sourceType, Type destinationType)
        {
            var sourceCode = GetImplicitCode(sourceType);

            if (sourceCode != 0 && (GetImplicitCode(destinationType) & sourceCode) == sourceCode)
            {
                return true;
            }

            if (destinationType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (ImplicitConvert.TryGetMathod(sourceType, destinationType, out _))
            {
                return true;
            }

            return false;
        }

        public static bool IsExplicitConvert(Type sourceType, Type destinationType)
        {
            if (IsImplicitConvert(sourceType, destinationType))
            {
                return true;
            }

            var sourceCode = GetImplicitCode(sourceType);

            if (sourceCode != 0 && (GetExplicitCode(destinationType) & sourceCode) == sourceCode)
            {
                return true;
            }

            if (sourceType.IsAssignableFrom(destinationType))
            {
                return true;
            }

            if (ExplicitConvert.TryGetMathod(sourceType, destinationType, out _))
            {
                return true;
            }

            return false;
        }

        public static Type GetConverterType(Type sourceType, Type destinationType)
        {
            return typeof(IXConverter<,>).MakeGenericType(sourceType, destinationType);
        }

        public static bool IsCustomConvert(Type sourceType, Type destinationType)
        {
            foreach (var factory in factories)
            {
                var converter = factory.GetConverter(sourceType, destinationType);

                if (converter != null && GetConverterType(sourceType, destinationType).IsInstanceOfType(converter))
                {
                    return true;
                }
            }

            if (destinationType == typeof(string))
            {
                return true;
            }

            if (sourceType == typeof(DBNull))
            {
                return true;
            }

            if (destinationType == typeof(DBNull))
            {
                return true;
            }


            if (sourceType == typeof(string) && destinationType.IsEnum)
            {
                return true;
            }


            if (ParseConvert.TryGetMathod(sourceType, destinationType, out _))
            {
                return true;
            }

            if (ToConvert.TryGetMathod(sourceType, destinationType, out _))
            {
                return true;
            }

            if (ConstructorConvert.TryGetMathod(sourceType, destinationType, out _))
            {
                return true;
            }

            return false;
        }

        public static bool IsBasicConvert(Type sourceType, Type destinationType)
        {
            return GetConverterType(sourceType, destinationType).IsInstanceOfType(BasicConvert.Instance);
        }
    }

    internal static class InternalConvert<TSource, TDestination>
    {
        public static readonly IXConverter<TSource, TDestination> Instance 
            = Underlying.As<IXConverter<TSource, TDestination>>(
                GetImpls().FirstOrDefault(item => item is IXConverter<TSource, TDestination>));

        private static IEnumerable<object> GetImpls()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            // Custom
            foreach (var factory in InternalConvert.factories)
            {
                yield return factory.GetConverter(sourceType, destinationType);
            }

            // Basic
            yield return BasicConvert.Instance;

            // Implicit
            if (sourceType == destinationType)
            {
                yield return new EqualsConvert<TSource>();
            }

            // Implicit
            if (Nullable.GetUnderlyingType(destinationType) == sourceType)
            {
                yield return Activator.CreateInstance(typeof(StructToNullableConvert<>).MakeGenericType(sourceType));
            }

            // Explicit
            if (Nullable.GetUnderlyingType(sourceType) == destinationType)
            {
                yield return Activator.CreateInstance(typeof(NullableToStructConvert<>).MakeGenericType(destinationType));
            }

            // Custom
            if (destinationType == typeof(string))
            {
                yield return new ToStringConvert<TSource>();
            }

            // Implicit
            if (destinationType.IsAssignableFrom(sourceType))
            {
                yield return Activator.CreateInstance(typeof(AssignableConvert<,>).MakeGenericType(sourceType, destinationType));
            }

            // Custom
            if (sourceType == typeof(DBNull))
            {
                yield return new FromDBNullConvert<TDestination>();
            }

            // Custom
            if (destinationType == typeof(DBNull))
            {
                yield return new ToDBNullConvert<TSource>();
            }

            // Implicit & XConvert
            {
                if (Nullable.GetUnderlyingType(sourceType) is Type underlyingType && underlyingType != sourceType)
                {
                    yield return Activator.CreateInstance(typeof(NullableToValueConvert<,>).MakeGenericType(underlyingType, destinationType));
                }
            }

            // XConvert & Implicit
            {
                if (Nullable.GetUnderlyingType(destinationType) is Type underlyingType && underlyingType != destinationType)
                {
                    yield return Activator.CreateInstance(typeof(ValueToNullableConvert<,>).MakeGenericType(sourceType, underlyingType));
                }
            }

            // Custom
            if (sourceType == typeof(string) && destinationType.IsEnum)
            {
                yield return Activator.CreateInstance(typeof(ParseEnum<>).MakeGenericType(destinationType));
            }

            // Implicit
            if (ImplicitConvert.TryGetMathod(sourceType, destinationType, out var method))
            {
                yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
            }

            // Explicit
            if (ExplicitConvert.TryGetMathod(sourceType, destinationType, out method))
            {
                yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
            }

            // Custom
            if (ParseConvert.TryGetMathod(sourceType, destinationType, out method))
            {
                yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
            }

            // Custom
            if (ToConvert.TryGetMathod(sourceType, destinationType, out method))
            {
                yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(method);
            }

            // Custom
            if (ConstructorConvert.TryGetMathod(sourceType, destinationType, out var constructor))
            {
                yield return BaseDynamicConvert.CreateInstanceByIL<TSource, TDestination>(constructor);
            }

            // Explicit
            if (sourceType.IsAssignableFrom(destinationType))
            {
                yield return Activator.CreateInstance(typeof(BaseConvert<,>).MakeGenericType(sourceType, destinationType));
            }

            // Error
            yield return new ForceConvert<TSource, TDestination>();
        }
    }
}