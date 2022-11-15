using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    static class InternalNonGenericXConvert
    {
        static readonly Dictionary<ulong, InternalXConverter> converters;

        static InternalNonGenericXConvert()
        {
            converters = new();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static unsafe ulong AsHigh(IntPtr value) => ((ulong)value) << 32;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static unsafe ulong AsLow(IntPtr value) => ((ulong)value) & 0xffffffff;

        static object NotSupportedConvert(object source) => throw new NotSupportedException(/*TODO*/);

        public static InternalXConverter GetConverter(Type sourceType, Type destinationType)
        {
            if (converters.TryGetValue(AsHigh(TypeHelper.GetTypeHandle(sourceType)) | AsLow(TypeHelper.GetTypeHandle(destinationType)), out var converter))
            {
                return converter;
            }

            return InternalGetConverter(sourceType, destinationType);

            static InternalXConverter InternalGetConverter(Type sourceType, Type destinationType)
            {
                lock (converters)
                {
                    var key = AsHigh(TypeHelper.GetTypeHandle(sourceType)) | AsLow(TypeHelper.GetTypeHandle(destinationType));

                    if (!converters.TryGetValue(key, out var converter))
                    {
                        if (sourceType.CanBeGenericParameter() && destinationType.CanBeGenericParameter())
                        {
                            converter = (InternalXConverter)typeof(InternalXConvert<,>)
                                .MakeGenericType(sourceType, destinationType)
                                .GetField(nameof(InternalXConvert<object, object>.Converter), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!
                                .GetValue(null)!;
                        }
                        else
                        {
                            converter = new InternalXConverter(null, XConvertMode.Custom, NotSupportedConvert);
                        }

                        converters.Add(key, converter);
                    }

                    return converter;
                }
            }
        }

        public static object? Convert(object value, Type destinationType)
        {
            // TODO: MONO 下检查委托执行成功与否。
            return GetConverter(value.GetType(), destinationType).Convert(value);
        }
    }
}