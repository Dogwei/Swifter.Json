using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Swifter.RW
{
    /// <summary>
    /// 列表读写器。
    /// </summary>
    /// <typeparam name="T">列表类型</typeparam>
    /// <typeparam name="TValue">元素类型</typeparam>
    public sealed class ListRW<T, TValue> : IDataRW<int>, IArrayCollectionRW where T : IList<TValue>
    {
        /// <summary>
        /// 默认容量。
        /// </summary>
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromList = typeof(T).IsAssignableFrom(typeof(List<TValue>));

        /// <summary>
        /// 列表实例。
        /// </summary>
        public T content;

        /// <summary>
        /// 获取指定索引处值的读写器。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        /// <summary>
        /// 获取所有索引。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<int> Keys => Enumerable.Range(0, Count);

        /// <summary>
        /// 获取列表长度。
        /// </summary>
        public int Count => content.Count;

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        /// <summary>
        /// 获取数据源类型。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type ContentType => typeof(T);

        IValueRW IDataRW<int>.this[int key] => this[key];


        /// <summary>
        /// 初始化一个具有默认容量的列表。
        /// </summary>
        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        /// <summary>
        /// 初始化一个指定容量的数组。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        public void Initialize(int capacity)
        {
            if (IsAssignableFromList)
            {
                Underlying.As<T, List<TValue>>(ref content) = new List<TValue>(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
        }

        /// <summary>
        /// 将所有元素写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = content.Count;

            for (int i = 0; i < length; i++)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[i], content[i]);
            }
        }

        /// <summary>
        /// 将指定索引处的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
        }

        /// <summary>
        /// 读取值读取器的值到指定索引处。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == Count)
            {
                content.Add(ValueInterface<TValue>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<TValue>.ReadValue(valueReader);
            }
        }

        /// <summary>
        /// 在数据读取器中读取所有元素。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                content[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
            }
        }

        void IArrayCollectionRW.InvokeElementType(IGenericInvoker invoker)
        {
            invoker.Invoke<TValue>();
        }
    }
}