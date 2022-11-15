using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class ExplicitFactory : IInternalXConverterFactory
    {
        const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
        const string MethodName = "op_Explicit";

        static IEnumerable<MethodInfo> GetMethods(Type type)
        {
            return type.GetMember(MethodName, BindingFlag).OfType<MethodInfo>();
        }

        public XConvertMode Mode => XConvertMode.Explicit;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            return GetMethods(typeof(TSource)).Concat(GetMethods(typeof(TDestination)))
                .Where(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)) <= 2)
                .OrderBy(method => SystemConvertFactory.GetComparison(method, typeof(TSource), typeof(TDestination)))
                .FirstOrDefault();
        }
    }
}