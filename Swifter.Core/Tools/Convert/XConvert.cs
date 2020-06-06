using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 高级类型转换静态工具类。
    /// </summary>
    public static partial class XConvert
    {
        /// <summary>
        /// 添加一个类型转换函数实现类工厂。
        /// </summary>
        /// <param name="factory">转换函数实现类工厂</param>
        public static void AddImplFactory(IConverterFactory factory)
        {
            lock (InternalConvert.factories)
            {
                InternalConvert.factories.Add(factory);
            }
        }

        /// <summary>
        /// 将指定原类型的实例转换为指定的目标类型的实例。转换失败将引发异常。
        /// </summary>
        /// <typeparam name="TSource">指定原类型</typeparam>
        /// <typeparam name="TDestination">指定目标类型</typeparam>
        /// <param name="value">原类型的实例</param>
        /// <returns>返回目标类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TSource, TDestination>(TSource value)
        {
            if (typeof(TSource) == typeof(TDestination))
            {
                return Underlying.As<TSource, TDestination>(ref value);
            }

            return InternalConvert<TSource, TDestination>.Instance.Convert(value);
        }

        /// <summary>
        /// 判断源类型是否可以隐式转换为目标类型。
        /// 隐式转换包括：
        /// 从小范围的基础类型转换为兼容的大范围基础基础类型；
        /// 从子类型转换为基类型；
        /// 已定义从源类型隐式转换为目标类型函数的类型。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public static bool IsImplicitConvert(Type sourceType, Type destinationType)
        {
            return CastImpl.GetImpl(sourceType, destinationType).IsImplicitConvert;
        }

        /// <summary>
        /// 判断源类型是否可以显式转换为目标类型。
        /// 显式转换包括：
        /// 隐式转换；
        /// 从大范围的基础类型转换为兼容的小范围基础基础类型；
        /// 从基类型转换为子类型；
        /// 已定义从源类型显式转换为目标类型函数的类型。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public static bool IsExplicitConvert(Type sourceType, Type destinationType)
        {
            return CastImpl.GetImpl(sourceType, destinationType).IsExplicitConvert;
        }

        /// <summary>
        /// 判断源类型和目标类型是否为基础类型，并且可以相互转换。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public static bool IsBasicConvert(Type sourceType, Type destinationType)
        {
            return CastImpl.GetImpl(sourceType, destinationType).IsBasicConvert;
        }

        /// <summary>
        /// 判断源类型是否可以通过自定义方式转换为目标类型。
        /// 自定义方式包括：
        /// 使用 <see cref="AddImplFactory(IConverterFactory)"/> 方法添加的转换方式；
        /// 已定义从源类型转换为目标类型的 Parse, ValueOf To 函数。
        /// 目标类型已定义从源类型构造的构造函数。
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public static bool IsCustomConvert(Type sourceType, Type destinationType)
        {
            return CastImpl.GetImpl(sourceType, destinationType).IsCustomConvert;
        }

        /// <summary>
        /// 将指定原类型的实例转换为指定的目标类型的实例。转换失败将引发异常。
        /// </summary>
        /// <typeparam name="TSource">指定原类型</typeparam>
        /// <param name="value">原类型的实例</param>
        /// <param name="outType">指定目标类型</param>
        /// <returns>返回目标类型的实例</returns>
        public static object ToObject<TSource>(TSource value, Type outType) => ToObjectImpl.ToObject(value, outType);

        /// <summary>
        /// 将一个任意实例转换为指定类型的实例。转换失败将引发异常。
        /// </summary>
        /// <typeparam name="TDestination">指定目标类型</typeparam>
        /// <param name="value">任意实例</param>
        /// <returns>返回目标类型的实例</returns>
        public static TDestination FromObject<TDestination>(object value) => FromObjectImpl.FromObject<TDestination>(value);

        /// <summary>
        /// 将一个任意实例转换为指定类型的实例。转换失败将引发异常。
        /// </summary>
        /// <param name="value">任意实例</param>
        /// <param name="outType">指定目标类型</param>
        /// <returns>返回目标类型的实例</returns>
        public static object Cast(object value, Type outType) => CastImpl.Cast(value, outType);
    }

    /// <summary>
    /// 指定目标类型的高级类型转换工具。
    /// </summary>
    /// <typeparam name="TDestination">指定目标类型</typeparam>
    public static class XConvert<TDestination>
    {
        /// <summary>
        /// 将指定原类型的实例转换为目标类型的实例。
        /// </summary>
        /// <typeparam name="TSource">指定原类型</typeparam>
        /// <param name="value">原类型的实例</param>
        /// <returns>返回目标类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TSource>(TSource value) => XConvert.Convert<TSource, TDestination>(value);

        /// <summary>
        /// 将任意类型的实例转换为目标类型的实例。
        /// </summary>
        /// <param name="obj">任意类型的实例</param>
        /// <returns>返回目标类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination FromObject(object obj) => XConvert.FromObject<TDestination>(obj);
    }
}