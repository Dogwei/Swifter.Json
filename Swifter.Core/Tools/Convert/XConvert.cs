using Swifter.RW;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    delegate TDestination? XConvertFunc<TSource, TDestination>(TSource? value);

    /// <summary>
    /// 高级类型转换工具。
    /// </summary>
    public static class XConvert
    {
        /// <summary>
        /// 添加类型转换器工厂。
        /// </summary>
        /// <param name="factory">类型转换器工厂</param>
        public static void AddFactory(IXConverterFactory factory)
        {
            InternalXConvertFactories.Add(factory);
        }

        internal static TDestination? OfNull<TDestination>()
        {
            if (typeof(TDestination) == typeof(DBNull))
            {
                return TypeHelper.As<DBNull, TDestination>(DBNull.Value);
            }

            return default;
        }

        internal static object? OfNull(Type destinationType)
        {
            if (destinationType == typeof(DBNull))
            {
                return DBNull.Value;
            }

            if (destinationType.IsValueType)
            {
                return TypeHelper.GetDefaultValue(destinationType);
            }

            return null;
        }

        internal static bool IsNull<T>([NotNullWhen(false)] T? value)
        {
            return value == null || value is DBNull;
        }

        /// <summary>
        /// 将原类型的值转换为目标类型的值。
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="value">原类型的值</param>
        /// <returns>返回目标类型的值</returns>
        /// <exception cref="InvalidCastException">没有从原类型转换到目标类型的转换方式</exception>
        public static TDestination? Convert<TSource, TDestination>(TSource? value)
        {
            if (typeof(TSource) == typeof(TDestination))
            {
                return Unsafe.As<TSource?, TDestination?>(ref value);
            }

            if (IsNull(value))
            {
                return OfNull<TDestination>();
            }

            if (!ValueInterface<TSource>.IsFinalType && value.GetType() != typeof(TSource))
            {
                return TypeHelper.GenericHelper.GetOrCreate(value).XConvertTo<TDestination>(value);
            }

            return InternalXConvert<TSource, TDestination>.Convert(value);
        }

        /// <summary>
        /// 将原类型的值转换为目标类型的值。
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <param name="value">原类型的值</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回目标类型的值</returns>
        /// <exception cref="InvalidCastException">没有从原类型转换到目标类型的转换方式</exception>
        public static object? Convert<TSource>(TSource? value, Type destinationType)
        {
            if (IsNull(value))
            {
                return OfNull(destinationType);
            }

            if (!ValueInterface<TSource>.IsFinalType && value.GetType() != typeof(TSource))
            {
                return InternalNonGenericXConvert.Convert(value, destinationType);
            }

            return TypeHelper.GenericHelper.GetOrCreate(destinationType).XConvertFrom(value);
        }

        /// <summary>
        /// 将一个对象转换为目标类型的值。
        /// </summary>
        /// <param name="value">对象</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回目标类型的值</returns>
        /// <exception cref="InvalidCastException">没有从原类型转换到目标类型的转换方式</exception>
        public static object? Convert(object? value, Type destinationType)
        {
            if (IsNull(value))
            {
                return OfNull(destinationType);
            }

            return InternalNonGenericXConvert.Convert(value, destinationType);
        }

        /// <summary>
        /// 将一个对象转换为目标类型的值。
        /// </summary>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <param name="value">对象</param>
        /// <returns>返回目标类型的值</returns>
        /// <exception cref="InvalidCastException">没有从原类型转换到目标类型的转换方式</exception>
        public static TDestination? Convert<TDestination>(object? value)
        {
            if (IsNull(value))
            {
                return OfNull<TDestination>();
            }

            return TypeHelper.GenericHelper.GetOrCreate(value).XConvertTo<TDestination>(value);
        }

        /// <summary>
        /// 判断原类型能否隐式转换为目标类型。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个布尔值</returns>
        public static bool IsImplicitConvert(Type sourceType, Type destinationType)
        {
            return InternalNonGenericXConvert.GetConverter(sourceType, destinationType).Mode switch
            {
                XConvertMode.BasicImplicit or XConvertMode.Covariant or XConvertMode.Implicit => true,
                _ => false,
            };
        }

        /// <summary>
        /// 判断原类型能否显示转换为目标类型。注意：如果可以隐式转换，那么也将允许显示转换。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个布尔值</returns>
        public static bool IsExplicitConvert(Type sourceType, Type destinationType)
        {
            return InternalNonGenericXConvert.GetConverter(sourceType, destinationType).Mode switch
            {
                XConvertMode.BasicImplicit or XConvertMode.Covariant or XConvertMode.Implicit or XConvertMode.BasicExplicit or XConvertMode.Explicit => true,
                _ => false,
            };
        }

        /// <summary>
        /// 判断源类型是否能有效的转换为目标类型。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个布尔值</returns>
        public static bool IsEffectiveConvert(Type sourceType, Type destinationType)
        {
            var converter = InternalNonGenericXConvert.GetConverter(sourceType, destinationType);

            return converter.Mode switch
            {
                XConvertMode.BasicImplicit or XConvertMode.Covariant or XConvertMode.Implicit or XConvertMode.BasicExplicit or XConvertMode.Explicit or XConvertMode.Extended => true,
                XConvertMode.Custom => converter.Method is not null,
                _ => false,
            };
        }

        /// <summary>
        /// 获取源类型到目标类型的转换方式。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个可能为空的转换方式枚举</returns>
        public static XConvertMode? GetConvertMode(Type sourceType, Type destinationType)
        {
            var converter = InternalNonGenericXConvert.GetConverter(sourceType, destinationType);

            if (converter.Mode == XConvertMode.Custom && converter.Method is null)
            {
                return null;
            }

            return converter.Mode;
        }
    }
}