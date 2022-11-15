using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    static class InternalXConvert<TSource, TDestination>
    {

        public static readonly InternalXConverter Converter;
        public static readonly XConvertFunc<TSource, TDestination>? Func;

        static InternalXConvert()
        {
            Converter = InternalXConvertFactories.GetConverter<TSource, TDestination>();

            if (Converter.Method is not null)
            {
                SystemConvertFactory.GetParametersTypes(Converter.Method, out var parameterTypes, out var returnType);

                if (parameterTypes.Length == 1 && parameterTypes[0].CanBeGenericParameter() && returnType.CanBeGenericParameter())
                {
                    var parameterType = parameterTypes[0];

                    if (parameterType == typeof(TSource) && returnType == typeof(TDestination))
                    {
                        Func = SystemConvertFactory.CreateDelegate<TSource, TDestination>(Converter.Method);
                    }
                    else
                    {
                        Func = (XConvertFunc<TSource, TDestination>)typeof(InternalXConvert<TSource, TDestination>)
                            .GetMethod(nameof(CreateIndirectFunc), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!
                            .MakeGenericMethod(parameterType, returnType)
                            .Invoke(null, new object[] { Converter.Method })!;
                    }
                }
                else
                {
                    var method = Converter.Method;

                    Converter.Method = null;

                    Func = _ => throw new InvalidOperationException($"Invalid conversion method : {method}.");
                }
            }
        }

        static XConvertFunc<TSource, TDestination> CreateIndirectFunc<TParameter, TReturn>(MethodBase method)
        {
            var Func = SystemConvertFactory.CreateDelegate<TParameter, TReturn>(method);

            if (Func is null)
            {
                throw new NotSupportedException();
            }

            TDestination? IndirectFunc(TSource? value)
            {
                var parameter = InternalXConvert<TSource, TParameter>.Convert(value);

                if (XConvert.IsNull(parameter))
                {
                    return XConvert.OfNull<TDestination>();
                }

                return XConvert.Convert<TReturn, TDestination>(Func(parameter));
            }

            return IndirectFunc;
        }

        public static TDestination? Convert(TSource? value)
        {
            if (Func is not null)
            {
                return Func(value);
            }

            throw GetInvalidCastException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidCastException GetInvalidCastException()
        {
            throw new InvalidCastException($"Cannot convert instance from \"{typeof(TSource)}\" to \"{typeof(TDestination)}\".");
        }
    }
}