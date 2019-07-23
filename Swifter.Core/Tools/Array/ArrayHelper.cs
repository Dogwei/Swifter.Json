using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

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
        public static IEnumerable<string> CreateNamesIterator(IDataReader dbDataReader)
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

        /// <summary>
        /// 合并一个数组和一个元素。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="tail">尾部元素</param>
        /// <returns>返回一个新的数组</returns>
        public static T[] Merge<T>(T[] array, T tail)
        {
            var result = new T[array.Length + 1];

            Unsafe.CopyBlock(ref Unsafe.As<T, byte>(ref result[0]), ref Unsafe.As<T, byte>(ref array[0]), (uint)array.Length * (uint)Unsafe.SizeOf<T>());

            result[array.Length] = tail;

            return result;
        }

        /// <summary>
        /// 合并一个头部元素和一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="head">头部元素</param>
        /// <returns>返回一个新的数组</returns>
        public static T[] Merge<T>(T head, T[] array)
        {
            var result = new T[array.Length + 1];

            result[0] = head;

            Unsafe.CopyBlock(ref Unsafe.As<T, byte>(ref result[1]), ref Unsafe.As<T, byte>(ref array[0]), (uint)array.Length * (uint)Unsafe.SizeOf<T>());

            return result;
        }

        /// <summary>
        /// 合并一个头部元素和一个数组和一个尾部元素。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="head">头部元素</param>
        /// <param name="tail">尾部元素</param>
        /// <returns>返回一个新的数组</returns>
        public static T[] Merge<T>(T head, T[] array, T tail)
        {
            var result = new T[array.Length + 2];

            result[0] = head;

            Unsafe.CopyBlock(ref Unsafe.As<T, byte>(ref result[1]), ref Unsafe.As<T, byte>(ref array[0]), (uint)array.Length * (uint)Unsafe.SizeOf<T>());

            result[result.Length - 1] = tail;

            return result;
        }

        /// <summary>
        /// 获取数组指定索引处的引用。（只支持 16 维度以内的数组）
        /// </summary>
        /// <typeparam name="TArray">数组类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="indices">索引</param>
        /// <returns>返回一个引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TElement Ref<TArray, TElement>(TArray array, int[] indices) where TArray : class
        {
            switch (indices.Length)
            {
                case 1: return ref Unsafe.As<TElement[]>(array)[indices[0]];
                case 2: return ref Unsafe.As<TElement[,]>(array)[indices[0], indices[1]];
                case 3: return ref Unsafe.As<TElement[,,]>(array)[indices[0], indices[1], indices[2]];
                case 4: return ref Unsafe.As<TElement[,,,]>(array)[indices[0], indices[1], indices[2], indices[3]];
                case 5: return ref Unsafe.As<TElement[,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4]];
                case 6: return ref Unsafe.As<TElement[,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]];
                case 7: return ref Unsafe.As<TElement[,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6]];
                case 8: return ref Unsafe.As<TElement[,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7]];
                case 9: return ref Unsafe.As<TElement[,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8]];
                case 10: return ref Unsafe.As<TElement[,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9]];
                case 11: return ref Unsafe.As<TElement[,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10]];
                case 12: return ref Unsafe.As<TElement[,,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10], indices[11]];
                case 13: return ref Unsafe.As<TElement[,,,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10], indices[11], indices[12]];
                case 14: return ref Unsafe.As<TElement[,,,,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10], indices[11], indices[12], indices[13]];
                case 15: return ref Unsafe.As<TElement[,,,,,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10], indices[11], indices[12], indices[13], indices[14]];
                case 16: return ref Unsafe.As<TElement[,,,,,,,,,,,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5], indices[6], indices[7], indices[8], indices[9], indices[10], indices[11], indices[12], indices[13], indices[14], indices[15]];
            }


            throw new NotSupportedException("Dimension exceeds maximum limit.");
        }

        /// <summary>
        /// 创建数组实例。（只支持 16 维度以内的数组）
        /// </summary>
        /// <typeparam name="TArray">数组类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="lengths">数组各个维度的长度</param>
        /// <returns>返回一个数组</returns>
        public static TArray CreateInstance<TArray, TElement>(int[] lengths) where TArray : class
        {
            switch (lengths.Length)
            {
                case 1: return Unsafe.As<TArray>(new TElement[lengths[0]]);
                case 2: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1]]);
                case 3: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2]]);
                case 4: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3]]);
                case 5: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4]]);
                case 6: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5]]);
                case 7: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6]]);
                case 8: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7]]);
                case 9: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8]]);
                case 10: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9]]);
                case 11: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10]]);
                case 12: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11]]);
                case 13: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12]]);
                case 14: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13]]);
                case 15: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14]]);
                case 16: return Unsafe.As<TArray>(new TElement[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], lengths[6], lengths[7], lengths[8], lengths[9], lengths[10], lengths[11], lengths[12], lengths[13], lengths[14], lengths[15]]);
            }


            throw new NotSupportedException("Dimension exceeds maximum limit.");
        }

        /// <summary>
        /// 重写分配数组的大小。（只支持 16 维度以内的数组）
        /// </summary>
        /// <typeparam name="TArray">数组类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="array">数组</param>
        /// <param name="lengths">新的长度</param>
        public static void Resize<TArray, TElement>(ref TArray array, int[] lengths) where TArray : class
        {
            var temp = Unsafe.As<Array>(array);

            for (int i = 0; i < lengths.Length; i++)
            {
                if (lengths[i] != temp.GetLength(i))
                {
                    if (array == temp)
                    {
                        array = CreateInstance<TArray, TElement>(lengths);
                    }

                    if (lengths[i] > temp.GetLength(i))
                    {
                        lengths[i] = temp.GetLength(i);
                    }
                }
            }

            if (array != temp)
            {
                Copy<TArray, TElement>(Unsafe.As<TArray>(temp), array, lengths);
            }
        }

        /// <summary>
        /// 从 0 索引处开始复制数组内容。（只支持 16 维度以内的数组）
        /// </summary>
        /// <typeparam name="TArray">数组类型</typeparam>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="source">源数组</param>
        /// <param name="destination">目标数组</param>
        /// <param name="lengths">各个维度的长度</param>
        public static void Copy<TArray, TElement>(TArray source, TArray destination, int[] lengths) where TArray : class
        {
            switch (lengths.Length)
            {
                case 1: Copy1D();return;
                case 2: Copy2D(); return;
                case 3: Copy3D(); return;
                case 4: Copy4D(); return;
                case 5: Copy5D(); return;
                case 6: Copy6D(); return;
                default: CopyND(new int[lengths.Length], 0); return;
            }

            void Copy1D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                        Unsafe.As<TElement[]>(destination)[i1] = Unsafe.As<TElement[]>(source)[i1];
            }
            void Copy2D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                    for (int i2 = lengths[1] - 1; i2 >= 0; --i2)
                        Unsafe.As<TElement[,]>(destination)[i1, i2] = Unsafe.As<TElement[,]>(source)[i1, i2];
            }
            void Copy3D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                    for (int i2 = lengths[1] - 1; i2 >= 0; --i2)
                        for (int i3 = lengths[2] - 1; i3 >= 0; --i3)
                            Unsafe.As<TElement[,,]>(destination)[i1, i2, i3] = Unsafe.As<TElement[,,]>(source)[i1, i2, i3];
            }
            void Copy4D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                    for (int i2 = lengths[1] - 1; i2 >= 0; --i2)
                        for (int i3 = lengths[2] - 1; i3 >= 0; --i3)
                            for (int i4 = lengths[3] - 1; i4 >= 0; --i4)
                                Unsafe.As<TElement[,,,]>(destination)[i1, i2, i3, i4] = Unsafe.As<TElement[,,,]>(source)[i1, i2, i3, i4];
            }
            void Copy5D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                    for (int i2 = lengths[1] - 1; i2 >= 0; --i2)
                        for (int i3 = lengths[2] - 1; i3 >= 0; --i3)
                            for (int i4 = lengths[3] - 1; i4 >= 0; --i4)
                                for (int i5 = lengths[4] - 1; i5 >= 0; --i5)
                                    Unsafe.As<TElement[,,,,]>(destination)[i1, i2, i3, i4, i5] = Unsafe.As<TElement[,,,,]>(source)[i1, i2, i3, i4, i5];
            }
            void Copy6D()
            {
                for (int i1 = lengths[0] - 1; i1 >= 0; --i1)
                    for (int i2 = lengths[1] - 1; i2 >= 0; --i2)
                        for (int i3 = lengths[2] - 1; i3 >= 0; --i3)
                            for (int i4 = lengths[3] - 1; i4 >= 0; --i4)
                                for (int i5 = lengths[4] - 1; i5 >= 0; --i5)
                                    for (int i6 = lengths[5] - 1; i6 >= 0; --i6)
                                        Unsafe.As<TElement[,,,,,]>(destination)[i1, i2, i3, i4, i5, i6] = Unsafe.As<TElement[,,,,,]>(source)[i1, i2, i3, i4, i5, i6];
            }

            void CopyND(int[] indices, int dimension)
            {
                var length = lengths[dimension];

                ref int index = ref indices[dimension];

                ++dimension;

                if (dimension == lengths.Length)
                {
                    for (index = 0; index < length; ++index)
                    {
                        Ref<TArray, TElement>(destination, indices) = Ref<TArray, TElement>(source, indices);
                    }
                }
                else
                {
                    for (index = 0; index < length; ++index)
                    {
                        CopyND(indices, dimension);
                    }
                }
            }
        }
    }
}