using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 列表读写器。
    /// </summary>
    /// <typeparam name="T">列表类型</typeparam>
    public sealed class ListRW<T> : IArrayRW where T : IList
    {
        /// <summary>
        /// 默认容量。
        /// </summary>
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromArrayList = typeof(T).IsAssignableFrom(typeof(ArrayList));

        /// <summary>
        /// 列表实例。
        /// </summary>
        T? content;

        /// <summary>
        /// 获取指定索引处值的读写器。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IValueRW this[int key] => new ValueRW(this, key);

        /// <summary>
        /// 获取列表长度。
        /// </summary>
        public int Count => content?.Count ?? -1;

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        public T? Content
        {
            get => content;
            set => content = value;
        }

        /// <summary>
        /// 获取数据源类型。
        /// </summary>
        public Type ContentType => typeof(T);

        /// <summary>
        /// 获取元素类型。
        /// </summary>
        public Type ValueType => typeof(object);

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
            if (IsAssignableFromArrayList)
            {
                Unsafe.As<T?, ArrayList?>(ref content) = new ArrayList(capacity);
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
        /// <param name="stopToken">停止令牌</param>
        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            int length = content.Count;
            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; i < length; i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    ValueInterface.WriteValue(dataWriter[i], content[i]);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], content[i]);
                }
            }
        }

        /// <summary>
        /// 将指定索引处的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            ValueInterface.WriteValue(valueWriter, content[key]);
        }

        /// <summary>
        /// 读取值读取器的值到指定索引处。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (key == Count)
            {
                content.Add(ValueInterface<object>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<object>.ReadValue(valueReader);
            }
        }

        /// <summary>
        /// 在数据读取器中读取所有元素。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            for (int i = 0; i < content.Count; i++)
            {
                content[i] = ValueInterface<object>.ReadValue(dataReader[i]);
            }
        }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        object? IDataRW.Content { get => Content; set => Content = (T?)value; }

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (T?)value; }

        sealed class ValueRW : BaseDirectRW
        {
            readonly ListRW<T> ListRW;
            readonly int Index;

            public ValueRW(ListRW<T> listRW, int index)
            {
                ListRW = listRW;
                Index = index;
            }

            public override Type ValueType => typeof(object);

            public override object? DirectRead()
            {
                if (ListRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                return ListRW.content[Index];
            }

            public override void DirectWrite(object? value)
            {
                if (ListRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                if (Index == ListRW.Count)
                {
                    ListRW.content.Add(value);
                }
                else
                {
                    ListRW.content[Index] = value;
                }
            }
        }
    }
}