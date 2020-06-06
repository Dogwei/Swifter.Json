using Swifter.Reflection;
using Swifter.RW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Swifter.Underlying;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对数组和集合操作的方法。
    /// </summary>
    public static unsafe class ArrayHelper
    {
        /// <summary>
        /// 创建 Int32 范围迭代器。
        /// </summary>
        /// <param name="start">起始值（包含）</param>
        /// <param name="end">结束值（不包含）</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<int> CreateRangeIterator(int start, int end)
        {
            while (start < end)
            {
                yield return start;

                ++start;
            }
        }

        /// <summary>
        /// 创建 Int32 长度迭代器。
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<int> CreateLengthIterator(int length) => CreateRangeIterator(0, length);

        /// <summary>
        /// 创建 String 表格的字段名称迭代器。
        /// </summary>
        /// <param name="dataTable">表格</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<string> CreateNamesIterator(DataTable dataTable)
        {
            foreach (DataColumn item in dataTable.Columns)
            {
                yield return item.ColumnName;
            }
        }

        /// <summary>
        /// 创建 String 系统数据读取器的字段名称迭代器。
        /// </summary>
        /// <param name="dbDataReader">系统数据读取器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<string> CreateNamesIterator(System.Data.IDataReader dbDataReader)
        {
            var length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                yield return dbDataReader.GetName(i);
            }
        }

        /// <summary>
        /// 创建 XConvert 类型转换迭代器。
        /// </summary>
        /// <typeparam name="TIn">输入类型</typeparam>
        /// <typeparam name="TOut">输出类型</typeparam>
        /// <param name="input">输入迭代器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<TOut> CreateConvertIterator<TIn, TOut>(IEnumerable<TIn> input)
        {
            foreach (var item in input)
            {
                yield return XConvert<TOut>.Convert(item);
            }
        }

        /// <summary>
        /// 合并一个数组和一个元素。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="tail">尾部元素</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T[] Merge<T>(T[] array, T tail)
        {
            var right = array != null ? array.Length : 0;

            Array.Resize(ref array, right + 1);

            array[right] = tail;

            return array;
        }

        /// <summary>
        /// 合并一个头部元素和一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="head">头部元素</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T[] Merge<T>(T head, T[] array)
        {
            if (array != null && array.Length != 0)
            {
                var ret = new T[array.Length + 1];

                ret[0] = head;

                CopyBlock(
                    ref As<T, byte>(ref ret[1]),
                    ref As<T, byte>(ref array[0]),
                    checked((uint)array.Length * (uint)SizeOf<T>())
                    );

                return ret;
            }
            else
            {
                return new T[] { head };
            }
        }

        /// <summary>
        /// 合并一个头部元素和一个数组和一个尾部元素。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="head">头部元素</param>
        /// <param name="tail">尾部元素</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T[] Merge<T>(T head, T[] array, T tail)
        {
            if (array != null && array.Length != 0)
            {
                var ret = new T[array.Length + 2];

                ret[0] = head;
                ret[ret.Length - 1] = tail;

                CopyBlock(
                    ref As<T, byte>(ref ret[1]),
                    ref As<T, byte>(ref array[0]),
                    checked((uint)array.Length * (uint)SizeOf<T>())
                    );

                return ret;
            }
            else
            {
                return new T[] { head, tail };
            }
        }

        /// <summary>
        /// 获取数组的指定索引处的元素引用。可以是多维数组。
        /// </summary>
        /// <typeparam name="TElement">需要获取的引用的类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="index">指定索引</param>
        /// <returns>返回元素引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TElement AddrOfArrayElement<TElement>(Array array, int index)
        {
            fixed (void* ptr = &TypeHelper.Unbox<byte>(array))
                return ref AsRef<TElement>((void*)Marshal.UnsafeAddrOfPinnedArrayElement(array, index));
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
            if (TypeHelper.GetMethodTablePointer(source) != TypeHelper.GetMethodTablePointer(destination))
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
                        Add(ref fDestination, length) = Add(ref fSource, length);
                    }
                }
                else
                {
                    for (index = length - 1; index >= 0; --index)
                    {
                        InteranlCopy(source, destination,indices, dimension);
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

            if (TrySetLowerBound(array, ref Add(ref first, -1))) return array;
            if (TrySetLowerBound(array, ref Add(ref last, 1))) return array;
            if (TrySetLowerBound(array, ref Add(ref first, -2))) return array;
            if (TrySetLowerBound(array, ref Add(ref last, 0))) return array;


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
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="lengths">多维数组每个维度的长度</param>
        /// <returns>返回一个多维数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Array CreateInstance<TElement>(int[] lengths)
        {
            return lengths.Length switch
            {
                01 => CreateMultiDimArrayOfOneDim(typeof(TElement), lengths[0]),
                02 => new TElement[lengths[0], lengths[1]],
                03 => new TElement[lengths[0], lengths[1], lengths[2]],
                04 => new TElement[lengths[0], lengths[1], lengths[2], lengths[3]],
                05 => new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4]],
                _ => Array.CreateInstance(typeof(TElement), lengths)
            };
        }

        /// <summary>
        /// 重新分配多维数组每个维度的长度。如果每个维度新旧长度都一样，则返回原数组。
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="array">多维数组</param>
        /// <param name="lengths">多维数组每个维度新的长度</param>
        /// <returns>返回新的多维数组的原数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Array Resize<TElement>(Array array, int[] lengths)
        {
            if (IsResize())
            {
                var newArray = CreateInstance<TElement>(lengths);

                if (array != null)
                {
                    Copy<TElement>(array, newArray);
                }

                return newArray;
            }

            return array;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            bool IsResize()
            {
                if (array is null)
                {
                    return true;
                }

                for (int dim = array.Rank - 1; dim >= 0; --dim)
                {
                    if (array.GetLength(dim) != lengths[dim])
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 将迭代器转换为可迭代器。
        /// </summary>
        /// <param name="enumerator">迭代器</param>
        /// <returns>返回一个可迭代器</returns>
        public static IEnumerable AsEnumerable(this IEnumerator enumerator)
        {
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// 将迭代器转换为可迭代器。
        /// </summary>
        /// <param name="enumerator">迭代器</param>
        /// <returns>返回一个可迭代器</returns>
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
        {
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// 获取列表的数据源。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T[] GetRawData<T>(this List<T> list) =>
            ref ListRaw<T>.f_items.GetReference(list);

        /// <summary>
        /// 获取列表的数据源。
        /// </summary>
        /// <param name="arrayList">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref object[] GetRawData(this ArrayList arrayList) =>
            ref ArrayListRaw.f_items.GetReference(arrayList);

        /// <summary>
        /// 获取或添加一个键值。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <param name="func">生成值的方法</param>
        /// <returns>返回值</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> func)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            value = func(key);

            dictionary.Add(key, value);

            return value;
        }

        /// <summary>
        /// 尝试添加一个键值。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="dictionary">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);

            return true;
        }

        /// <summary>
        /// 获取列表的长度。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref int GetCount<T>(this List<T> list) =>
            ref ListRaw<T>.f_count.GetReference(list);

        /// <summary>
        /// 获取列表的长度。
        /// </summary>
        /// <param name="arrayList">列表</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref int GetCount(this ArrayList arrayList) =>
            ref ArrayListRaw.f_count.GetReference(arrayList);

        /// <summary>
        /// 创建一个列表。
        /// </summary>
        /// <typeparam name="T">列表类型</typeparam>
        /// <param name="array">列表数据源</param>
        /// <param name="count">列表数量</param>
        /// <returns>返回一个列表</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static List<T> CreateList<T>(T[] array, int count)
        {
            var ret = new List<T>(0);

            ListRaw<T>.f_items.SetValue(ret, array);
            ListRaw<T>.f_count.SetValue(ret, count);

            return ret;
        }

        /// <summary>
        /// 创建一个列表。
        /// </summary>
        /// <param name="array">列表数据源</param>
        /// <param name="count">列表数量</param>
        /// <returns>返回一个列表</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayList CreateArrayList(object[] array, int count)
        {
            var ret = new ArrayList(0);

            ArrayListRaw.f_items.SetValue(ret, array);
            ArrayListRaw.f_count.SetValue(ret, count);

            return ret;
        }

        /// <summary>
        /// 归并集合。
        /// </summary>
        /// <typeparam name="TInput">集合元素类型</typeparam>
        /// <typeparam name="TOutput">结果类型</typeparam>
        /// <param name="source">集合</param>
        /// <param name="selector">选择器</param>
        /// <param name="output">初始值</param>
        /// <returns>返回结果值</returns>
        public static TOutput Merge<TInput, TOutput>(this IEnumerable<TInput> source, Func<TInput, TOutput, TOutput> selector, TOutput output = default)
        {
            foreach (var item in source)
            {
                output = selector(item, output);
            }

            return output;
        }

        /// <summary>
        /// 将一个二维集合转换为一维集合。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="tss">二维集合</param>
        /// <returns>返回一维集合</returns>
        public static IEnumerable<T> AsOneDim<T>(this IEnumerable<IEnumerable<T>> tss)
        {
            foreach (var ts in tss)
            {
                foreach (var t in ts)
                {
                    yield return t;
                }
            }
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

        static class ListRaw<T>
        {
            public static readonly XClassFieldInfo<T[]> f_items;
            public static readonly XClassFieldInfo<int> f_count;

            static ListRaw()
            {
                var fields = typeof(List<T>).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var t_items = fields.Where(item => item.FieldType == typeof(T[])).First();

                if (!(typeof(List<T>).GetProperty(nameof(List<T>.Count)) is PropertyInfo p_count && TypeHelper.IsAutoProperty(p_count, out var t_count)))
                {
                    var temp = new List<T>
                    {
                        default,
                        default,
                        default
                    };

                    temp.RemoveAt(2);

                    t_count = fields.Where(item => item.FieldType == typeof(int) && item.GetValue(temp) is int count && count == 2).First();
                }

                f_items = XFieldInfo.Create(t_items, XBindingFlags.NonPublic) as XClassFieldInfo<T[]>;
                f_count = XFieldInfo.Create(t_count, XBindingFlags.NonPublic) as XClassFieldInfo<int>;
            }
        }

        static class ArrayListRaw
        {
            public static readonly XClassFieldInfo<object[]> f_items;
            public static readonly XClassFieldInfo<int> f_count;

            static ArrayListRaw()
            {
                var fields = typeof(ArrayList).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var t_items = fields.Where(item => item.FieldType == typeof(object[])).First();

                if (!(typeof(ArrayList).GetProperty(nameof(ArrayList.Count)) is PropertyInfo p_count && TypeHelper.IsAutoProperty(p_count, out var t_count)))
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

                f_items = XFieldInfo.Create(t_items, XBindingFlags.NonPublic) as XClassFieldInfo<object[]>;
                f_count = XFieldInfo.Create(t_count, XBindingFlags.NonPublic) as XClassFieldInfo<int>;
            }
        }
    }
}