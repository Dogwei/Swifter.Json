using InlineIL;
using Swifter.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对数组和集合操作的方法。
    /// </summary>
    public static unsafe class ArrayHelper
    {
        /// <summary>
        /// 获取数组最后一个元素的引用。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">数组</param>
        /// <returns>返回最后一个元素的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Last<T>(this T[] elements) => ref elements[elements.Length - 1];

        /// <summary>
        /// 获取数组第一个元素的引用。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="elements">数组</param>
        /// <returns>返回第一个元素的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T First<T>(this T[] elements) => ref elements[0];

        /// <summary>
        /// 获取数组的指定索引处的元素引用。可以是多维数组。
        /// </summary>
        /// <typeparam name="TElement">需要获取的引用的类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="index">指定索引</param>
        /// <returns>返回元素引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TElement? AddrOfArrayElement<TElement>(Array array, int index)
        {
            fixed (void* ptr = &TypeHelper.Unbox<byte>(array))
                return ref Unsafe.AsRef<TElement?>((void*)Marshal.UnsafeAddrOfPinnedArrayElement(array, index));
        }

        /// <summary>
        /// 将一个多维数组的元素复制到另一个相同维度的多维数组中。从每个维度 0 索引开始，复制这两个数组中该维度较小的长度数量的元素。
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="source">源数组</param>
        /// <param name="destination">目标数组</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe void Copy<TElement>(Array source, Array destination)
        {
            if (source.Rank != destination.Rank)
            {
                throw new InvalidOperationException();
            }

            var indices = stackalloc int[source.Rank];

            InteranlCopy(source, destination, indices, 0);

            static void InteranlCopy(Array source, Array destination, int* indices, int dimension)
            {
                var length = Math.Min(source.GetLength(dimension), destination.GetLength(dimension));

                ref int index = ref indices[dimension];

                ++dimension;

                if (dimension == source.Rank)
                {
                    ref var fSource = ref AddrOfArrayElement<TElement>(source, ComputeOffset(source, indices));
                    ref var fDestination = ref AddrOfArrayElement<TElement>(destination, ComputeOffset(destination, indices));

                    for (--length; length >= 0; --length)
                    {
                        Unsafe.Add(ref fDestination, length) = Unsafe.Add(ref fSource, length);
                    }
                }
                else
                {
                    for (index = length - 1; index >= 0; --index)
                    {
                        InteranlCopy(source, destination, indices, dimension);
                    }
                }
            }
        }

        /// <summary>
        /// 计算多维数组指定多维索引的元素偏移量。
        /// </summary>
        /// <param name="array">多维数组</param>
        /// <param name="indices">多维索引</param>
        /// <returns>返回相当于第一个元素的元素偏移量</returns>
        public static int ComputeOffset(Array array, int* indices)
        {
            var dim = array.Rank - 1;

            var offset = indices[dim];

            var len = array.GetLength(dim);

            if (dim >= 2)
            {
                do
                {
                    --dim;

                    offset += indices[dim] * len;

                    len *= array.GetLength(dim);

                } while (dim >= 2);
            }

            if (dim >= 1)
            {
                offset += indices[0] * len;
            }

            return offset;
        }

        /// <summary>
        /// 创建一个维度的多维数组。
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="length">数组长度</param>
        /// <returns>返回一个维度的多维数组</returns>
        /// <exception cref="PlatformNotSupportedException">平台不支持创建一个维度的多维数组</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Array CreateMultiDimArrayOfOneDim(Type elementType, int length)
        {
            const int exLowerBound = 1;

            if (length >= int.MaxValue)
            {
                throw new OutOfMemoryException();
            }

            var array = Array.CreateInstance(elementType, new int[] { length }, new int[] { exLowerBound });



            ref var first = ref AddrOfArrayElement<int>(array, 0);
            ref var last = ref AddrOfArrayElement<int>(array, length);

            if (TrySetLowerBound(array, ref Unsafe.Add(ref first, -1))) return array;
            if (TrySetLowerBound(array, ref Unsafe.Add(ref last, 1))) return array;
            if (TrySetLowerBound(array, ref Unsafe.Add(ref first, -2))) return array;
            if (TrySetLowerBound(array, ref Unsafe.Add(ref last, 0))) return array;


            throw new PlatformNotSupportedException();


            static bool TrySetLowerBound(Array array, ref int lowerBound)
            {
                if (lowerBound == exLowerBound)
                {
                    lowerBound = 0;

                    if (array.GetLowerBound(0) == 0)
                    {
                        return true;
                    }

                    lowerBound = exLowerBound;
                }

                return false;
            }
        }

        /// <summary>
        /// 创建多维数组。
        /// </summary>
        /// <param name="elementType">元素类型</param>
        /// <param name="lengths">多维数组每个维度的长度</param>
        /// <returns>返回一个多维数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Array CreateInstance(Type elementType, int[] lengths)
        {
            if (lengths.Length is 1)
            {
                return CreateMultiDimArrayOfOneDim(elementType, lengths[0]);
            }

            return Array.CreateInstance(elementType, lengths);
        }

        /// <summary>
        /// 重新分配多维数组每个维度的长度。如果每个维度新旧长度都一样，则返回原数组。
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="array">多维数组</param>
        /// <param name="lengths">多维数组每个维度新的长度</param>
        /// <returns>返回新的多维数组的原数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Resize<TElement>([NotNull] ref Array? array, int[] lengths)
        {
            if (array is null)
            {
                goto Resize;
            }

            VersionDifferences.Assert(array.Rank == lengths.Length);

            for (int i = 0; i < lengths.Length; i++)
            {
                if (array.GetLength(i) != lengths[i])
                {
                    goto Resize;
                }
            }

            return;

        Resize:

            var newArray = CreateInstance(typeof(TElement), lengths);

            if (array != null)
            {
                Copy<TElement>(array, newArray);
            }

            array = newArray;
        }

        /// <summary>
        /// 获取或添加一个键值。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <param name="valueFactory">生成值的方法</param>
        /// <returns>返回值</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = valueFactory(key);

            dictionary.Add(key, value);

            return value;
        }

        /// <summary>
        /// 获取列表的数据源。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T[]? GetRawData<T>(this List<T> list) =>
            ref ListRaw<T>.f_items.UnsafeGetReference(list);

        /// <summary>
        /// 获取列表的数据源。
        /// </summary>
        /// <param name="arrayList">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref object?[]? GetRawData(this ArrayList arrayList) =>
            ref ArrayListRaw.f_items.UnsafeGetReference(arrayList);

        /// <summary>
        /// 获取列表的长度。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref int GetCount<T>(this List<T> list) =>
            ref ListRaw<T>.f_count.UnsafeGetReference(list);

        /// <summary>
        /// 获取列表的长度。
        /// </summary>
        /// <param name="arrayList">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref int GetCount(this ArrayList arrayList) =>
            ref ArrayListRaw.f_count.UnsafeGetReference(arrayList);

        /// <summary>
        /// 创建一个列表。
        /// </summary>
        /// <typeparam name="T">列表类型</typeparam>
        /// <param name="array">列表数据源</param>
        /// <param name="count">列表数量</param>
        /// <returns>返回一个列表</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static List<T> CreateList<T>(T[]? array, int count)
        {
            var ret = new List<T>();

            ListRaw<T>.f_items.UnsafeGetReference(ret) = array;
            ListRaw<T>.f_count.UnsafeGetReference(ret) = count;

            return ret;
        }

        /// <summary>
        /// 创建一个列表。
        /// </summary>
        /// <param name="array">列表数据源</param>
        /// <param name="count">列表数量</param>
        /// <returns>返回一个列表</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayList CreateArrayList(object?[]? array, int count)
        {
            var ret = new ArrayList();

            ArrayListRaw.f_items.UnsafeGetReference(ret) = array;
            ArrayListRaw.f_count.UnsafeGetReference(ret) = count;

            return ret;
        }

        /// <summary>
        /// 返回一个新的数组，数组中的元素为原始数组元素调用函数处理后的值。
        /// </summary>
        /// <typeparam name="TInput">原始数组元素类型</typeparam>
        /// <typeparam name="TOutput">新数组元素类</typeparam>
        /// <param name="elements">新数组</param>
        /// <param name="selector">处理函数</param>
        /// <returns>返回一个数组</returns>
        public static TOutput[] Map<TInput, TOutput>(this TInput[] elements, Func<TInput, TOutput> selector)
        {
            var outputs = new TOutput[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
                outputs[i] = selector(elements[i]);
            }

            return outputs;
        }

        /// <summary>
        /// 内存移动
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="destination">目标地址</param>
        /// <param name="source">源地址</param>
        /// <param name="elementCount">元素数量</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Memmove<T>(ref T destination, ref T source, /*nuint*/int elementCount)
        {
#if Span
            CreateSpan(ref source, elementCount).CopyTo(CreateSpan(ref destination, elementCount));
#else
            while (elementCount > 0)
            {
                destination = source;

                destination = ref Unsafe.Add(ref destination, 1);
                source = ref Unsafe.Add(ref source, 1);

                --elementCount;
            }
#endif
        }

#if Span
        /// <summary>
        /// 创建一个 Span。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="reference">元素引用</param>
        /// <param name="length">长度</param>
        /// <returns>返回一个 Span</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Span<T> CreateSpan<T>(ref T reference, int length)
        {
#if NativeSpan
            return MemoryMarshal.CreateSpan<T>(ref reference, length);
#else
            IL.DeclareLocals(new LocalVar(typeof(Span<T>)));

            IL.MarkLabel("Loop");

            IL.Emit.Ldloca(0);
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();

            IL.Emit.Call(MethodRef.Constructor(typeof(Span<byte>), typeof(void*), typeof(int)));

            IL.Emit.Ldloca(0);
            IL.Emit.Call(MethodRef.Method(typeof(Span<T>), nameof(Span<T>.GetPinnableReference)));
            IL.Emit.Ldarg_0();

            IL.Emit.Bne_Un("Loop");

            IL.Emit.Ldloc(0);
            IL.Emit.Ret();

            throw IL.Unreachable();
#endif
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstByte"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool IsEmpty(ref byte firstByte, int length)
        {
            while (length >= 8)
            {
                if (Unsafe.As<byte, ulong>(ref firstByte) != 0)
                {
                    return false;
                }

                firstByte = ref TypeHelper.AddByteOffset(ref firstByte, 8);
                length -= 8;
            }

            return (Unsafe.As<byte, ulong>(ref firstByte) & (~(ulong.MaxValue << (length * 8)))) == 0;
        }

        static class ListRaw<T>
        {
            public static readonly XInstanceFieldInfo<T[]> f_items;
            public static readonly XInstanceFieldInfo<int> f_count;

            static ListRaw()
            {
                var fields = typeof(List<T>).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var t_items = fields.Where(item => item.FieldType == typeof(T[])).First();

                if (TypeHelper.IsAutoGetMethod(
                    TypeHelper.GetMethodFromHandle(
                        IL.Ldtoken(MethodRef.PropertyGet(typeof(List<T>), nameof(List<T>.Count))),
                        typeof(List<T>).TypeHandle
                        ), out var t_count))
                {

                }
                else
                {
                    var temp = new List<T?>(3);

                    temp.Add(default);
                    temp.Add(default);
                    temp.Add(default);

                    temp.RemoveAt(2);

                    t_count = fields.Where(item => item.FieldType == typeof(int) && item.GetValue(temp) is int count && count == 2).First();
                }

                f_items = new(t_items, XBindingFlags.NonPublic);
                f_count = new(t_count, XBindingFlags.NonPublic);
            }
        }

        static class ArrayListRaw
        {
            public static readonly XInstanceFieldInfo<object?[]> f_items;
            public static readonly XInstanceFieldInfo<int> f_count;

            static ArrayListRaw()
            {
                var fields = typeof(ArrayList).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var t_items = fields.Where(item => item.FieldType == typeof(object[])).First();

                if (TypeHelper.IsAutoGetMethod(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.PropertyGet(typeof(ArrayList), nameof(ArrayList.Count)))), out var t_count))
                {

                }
                else
                {
                    var temp = new ArrayList
                    {
                        default,
                        default,
                        default
                    };

                    temp.RemoveAt(2);

                    t_count = fields.Where(item => item.FieldType == typeof(int) && item.GetValue(temp) is int count && count == 2).First();
                }

                f_items = new(t_items, XBindingFlags.NonPublic);
                f_count = new(t_count, XBindingFlags.NonPublic);
            }
        }
    }
}