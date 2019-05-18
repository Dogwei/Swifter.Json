using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对数组和集合操作的方法。
    /// </summary>
    public static unsafe class ArrayHelper
    {
        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="input">输入数组</param>
        /// <param name="filter">输入数组筛选器</param>
        /// <param name="asFunc">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(TIn[] input, Func<TIn, bool> filter, Func<TIn, TOut> asFunc)
        {
            var array = new TOut[input.Length];

            int length = 0;

            foreach (var Item in input)
            {
                if (filter(Item))
                {
                    array[length] = asFunc(Item);

                    ++length;
                }
            }

            if (length != input.Length)
            {
                Array.Resize(ref array, length);
            }

            return array;
        }

        /// <summary>
        /// 筛选数组元素
        /// </summary>
        /// <typeparam name="TIn">输入数组类型</typeparam>
        /// <typeparam name="TOut">输出数组类型</typeparam>
        /// <param name="input">输入源</param>
        /// <param name="filter">输入数组筛选器</param>
        /// <param name="asFunc">输入数组元素转输出数组元素委托</param>
        /// <returns>返回一个新的数组</returns>
        public static TOut[] Filter<TIn, TOut>(IEnumerable<TIn> input, Func<TIn, bool> filter, Func<TIn, TOut> asFunc)
        {
            var list = new List<TOut>();

            foreach (var item in input)
            {
                if (filter(item))
                {
                    list.Add(asFunc(item));
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 复制集合元素到数组中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">集合</param>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组起始索引</param>
        public static void CopyTo<T>(IEnumerable<T> collection, T[] array, int arrayIndex)
        {
            foreach (var item in collection)
            {
                array[arrayIndex] = item;

                ++arrayIndex;
            }
        }

        /// <summary>
        /// 复制集合元素到数组中。
        /// </summary>
        /// <param name="collection">集合</param>
        /// <param name="array">数组</param>
        /// <param name="arrayIndex">数组起始索引</param>
        public static void CopyTo<T>(IEnumerable<T> collection, Array array, int arrayIndex)
        {
            if (array is T[] tArray)
            {
                CopyTo(collection, tArray, arrayIndex);

                return;
            }

            foreach (var item in collection)
            {
                array.SetValue(item, arrayIndex);

                ++arrayIndex;
            }
        }

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
        /// 创建 String 系统数据读取器的字段名称迭代器。
        /// </summary>
        /// <param name="dbDataReader">系统数据读取器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<string> CreateNamesIterator(DbDataReader dbDataReader)
        {
            var length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                yield return dbDataReader.GetName(i);
            }
        }

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
        /// 创建数组的迭代器。
        /// </summary>
        /// <typeparam name="T">数组类型</typeparam>
        /// <param name="array">数组</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<T> CreateArrayIterator<T>(T[] array)
        {
            var length = array.Length;

            for (int i = 0; i < length; i++)
            {
                yield return array[i];
            }
        }

        /// <summary>
        /// 创建 XConvert 类型转换迭代器。
        /// </summary>
        /// <typeparam name="TIn">输入类型</typeparam>
        /// <typeparam name="TOut">输出类型</typeparam>
        /// <param name="input">输入迭代器</param>
        /// <returns>返回一个 yield 关键字实现的迭代器</returns>
        public static IEnumerable<TOut> CreateAsIterator<TIn, TOut>(IEnumerable<TIn> input)
        {
            foreach (var item in input)
            {
                yield return XConvert<TOut>.Convert(item);
            }
        }

        /// <summary>
        /// 判断当前平台是否支持一维数组信息结构。
        /// </summary>
        public static bool IsSupportedOneRankValueArrayInfo => OneRankValueArrayInfo<byte>.Available && OneRankValueArrayInfo<char>.Available;

        /// <summary>
        /// 将一块内存转换为临时数组对象（非托管）。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="address">内存地址</param>
        /// <param name="length">内存元素数量</param>
        /// <param name="starts">返回一个起始位置的内容备份，此实例长度加临时数组长度刚好等于元素数量</param>
        /// <returns>返回一个临时数组</returns>
        public static T[] AsTempOneRankValueArray<T>(void* address, int length, out T[] starts) where T : struct
        {
            if (!(IsSupportedOneRankValueArrayInfo && OneRankValueArrayInfo<T>.Available))
            {
                throw new PlatformNotSupportedException("This operation is not supported by the current platform.");
            }

            var startsLength = (OneRankValueArrayInfo<T>.ElementOffset + Unsafe.SizeOf<T>() - 1) / Unsafe.SizeOf<T>();

            if (length < startsLength)
            {
                throw new ArgumentException("Not enough length to store array information.", nameof(length));
            }

            starts = new T[startsLength];

            Unsafe.CopyBlock(
                ref Unsafe.As<T, byte>(ref starts[0]),
                ref Unsafe.AsRef<byte>(address),
                (uint)(startsLength * Unsafe.SizeOf<T>()));

            ref var pArray = ref Unsafe.AddByteOffset(
                ref Unsafe.AsRef<OneRankValueArrayInfo<T>>(address),
                (startsLength * Unsafe.SizeOf<T>()) - OneRankValueArrayInfo<T>.ElementOffset);

            pArray.TypeHandle = OneRankValueArrayInfo<T>.ObjectTypeHandle;
            pArray.Length = (IntPtr)(length - startsLength);

            return Unsafe.As<T[]>(Unsafe.AsObject(ref pArray));
        }

        /// <summary>
        /// 将一块内存转换为临时数组对象（非托管）。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="address">内存地址</param>
        /// <param name="length">内存元素数量</param>
        /// <param name="starts">返回一个起始位置的内容备份，此实例长度加临时数组长度刚好等于元素数量</param>
        /// <returns>返回一个临时数组</returns>
        public static unsafe T[] AsTempOneRankValueArray<T>(IntPtr address, int length, out T[] starts) where T : struct => AsTempOneRankValueArray((void*)address, length, out starts);
    }
}