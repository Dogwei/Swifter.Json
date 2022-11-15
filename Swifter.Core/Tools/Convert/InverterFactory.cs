using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class InverterFactory : IInternalXConverterFactory
    {
        public static bool CanConvert(Type sourceType, Type destinationType)
        {
            return destinationType.IsAssignableFrom(sourceType);
        }

        public static TDestination? Convert<TSource, TDestination>(TSource value) where TDestination : TSource
        {
            return (TDestination?)value;
        }

        public XConvertMode Mode => XConvertMode.Inverter;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            if (CanConvert(typeof(TSource), typeof(TDestination)))
            {
                return typeof(CovariantFactory)
                   .GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!
                   .MakeGenericMethod(typeof(TSource), typeof(TDestination));
            }

            return null;
        }
    }
}