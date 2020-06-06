
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 数组（向量）读写器。
    /// </summary>
    /// <typeparam name="TElement">数组元素类型</typeparam>
    public sealed class ArrayRW<TElement> : IDataRW<int>, IArrayCollectionRW
    {
        static ArrayAppendingInfo appendingInfo = new ArrayAppendingInfo() { MostClosestMeanCommonlyUsedLength = DefaultCapacity };

        /// <summary>
        /// 默认容量。
        /// </summary>
        public const int DefaultCapacity = 3;

        TElement[] array;
        int count;

        /// <summary>
        /// 创建数组（向量）读写器并初始化。
        /// </summary>
        /// <param name="obj">数组</param>
        public ArrayRW(TElement[] obj)
        {
            Initialize(obj);
        }

        /// <summary>
        /// 创建数组（向量）读写器。
        /// </summary>
        public ArrayRW()
        {
        }

        /// <summary>
        /// 获取指定索引器的值读写器。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        /// <summary>
        /// 获取所有索引。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        /// <summary>
        /// 获取数组长度。
        /// </summary>
        public int Count => count;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public TElement[] GetContent()
        {
            if (array != null && array.Length != count)
            {
                Array.Resize(ref array, count);
            }

            appendingInfo.AddUsedLength(count);

            return array;
        }

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Content
        {
            get => GetContent();
            set => Initialize((TElement[])value);
        }

        /// <summary>
        /// 获取数据类型。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type ContentType => typeof(TElement[]);

        /// <summary>
        /// 初始化一个具有默认容量的数组。
        /// </summary>
        public void Initialize()
        {
            Initialize(appendingInfo.MostClosestMeanCommonlyUsedLength);
        }

        /// <summary>
        /// 初始化一个指定容量的数组。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        public void Initialize(int capacity)
        {
            if (array is null || capacity > array.Length)
            {
                array = new TElement[capacity];
            }

            count = 0;
        }

        /// <summary>
        /// 设置数据源。
        /// </summary>
        /// <param name="obj">数据源</param>
        public void Initialize(TElement[] obj)
        {
            array = obj;

            count = obj?.Length ?? 0;
        }

        /// <summary>
        /// 将所有元素写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            for (int i = 0; i < count; i++)
            {
                ValueInterface.WriteValue(dataWriter[i], array[i]);
            }
        }

        /// <summary>
        /// 将指定索引处的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var value = array[key];

            ValueInterface.WriteValue(valueWriter, value);
        }

        /// <summary>
        /// 在数据读取器中读取所有元素。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<int> dataReader)
        {
            for (int i = 0; i < count; i++)
            {
                array[i] = ValueInterface.ReadValue<TElement>(dataReader[i]);
            }
        }

        /// <summary>
        /// 扩容数组。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand()
        {
            Array.Resize(ref array, count * 2 + 1);
        }

        /// <summary>
        /// 读取值读取器的值到指定索引处。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == count && key == array.Length)
            {
                Expand();
            }

            if (key >= array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            array[key] = ValueInterface.ReadValue<TElement>(valueReader);

            if (key == count)
            {
                ++count;
            }
        }

        void IArrayCollectionRW.InvokeElementType(IGenericInvoker invoker)
        {
            invoker.Invoke<TElement>();
        }
    }
}