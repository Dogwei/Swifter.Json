using InlineIL;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class NonConvertibleClass : IInternalXConverterFactory
    {
        static void InvalidConverter()
        {
        }

        public XConvertMode Mode => XConvertMode.Custom;

        public MethodBase? GetConverter<TSource, TDestination>()
        {
            if (typeof(TSource) == typeof(NonConvertibleClass) 
                || typeof(TDestination) == typeof(NonConvertibleClass) 
                || typeof(TSource) == typeof(NonConvertibleStruct) 
                || typeof(TDestination) == typeof(NonConvertibleStruct))
            {
                return TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(NonConvertibleClass), nameof(InvalidConverter))));
            }

            return null;
        }
    }
}
