using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class ParseFactory : IInternalXConverterFactory
    {
        const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        static readonly string[] MethodNames = { "Parse", "ValueOf" };

        static IEnumerable<MethodInfo> GetMethods(Type type)
        {
            return MethodNames
                .SelectMany(methodName => type.GetMember(methodName, BindingFlag).OfType<MethodInfo>())
                .Where(method => !method.IsGenericMethodDefinition && method.ReturnType != typeof(void) && method.GetParameters().Length == 1)
                ;
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