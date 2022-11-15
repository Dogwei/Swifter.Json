using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class ConstructorFactory : IInternalXConverterFactory
    {
        const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        static IEnumerable<ConstructorInfo> GetMethods(Type type)
        {
            return type.GetConstructors(BindingFlag).Where(construstor => construstor.GetParameters().Length == 1);
        }

        public XConvertMode Mode => XConvertMode.Extended;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            return GetMethods(typeof(TDestination))
                .Where(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)) <= 2)
                .OrderBy(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)))
                .FirstOrDefault();
        }
    }
}