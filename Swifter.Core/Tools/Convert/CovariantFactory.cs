using System;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class CovariantFactory : IInternalXConverterFactory
    {
        public static bool CanConvert(Type sourceType, Type destinationType)
        {
            return destinationType.IsAssignableFrom(sourceType) && Nullable.GetUnderlyingType(destinationType) != sourceType;
        }

        public static TDestination Convert<TSource, TDestination>(TSource value) where TSource : TDestination
        {
            return value;
        }

        public XConvertMode Mode => XConvertMode.Covariant;

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