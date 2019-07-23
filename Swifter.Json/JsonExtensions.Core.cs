
#if NETCOREAPP && !NETCOREAPP2_0

using System;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    partial class JsonExtensions
    {
        /// <summary>
        /// 读取 JSON 对象中字段的名称。
        /// </summary>
        /// <param name="jsonReader">Json Reader</param>
        /// <returns>返回一个只读的 Span</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static unsafe ReadOnlySpan<char> ReadPropertyNameSpan(this IJsonReader jsonReader)
        {
            return new ReadOnlySpan<char>(Unsafe.AsPointer(ref Unsafe.AsRef(jsonReader.ReadPropertyName(out var length))), length);
        }

        /// <summary>
        /// 读取 字符串 值。
        /// </summary>
        /// <param name="jsonReader">Json Reader</param>
        /// <returns>返回一个只读的 Span</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static unsafe ReadOnlySpan<char> ReadStringSpan(this IJsonReader jsonReader)
        {
            return new ReadOnlySpan<char>(Unsafe.AsPointer(ref Unsafe.AsRef(jsonReader.ReadString(out var length))), length);
        }
    }
}

#endif