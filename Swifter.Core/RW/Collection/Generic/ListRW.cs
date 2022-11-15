using Swifter.Tools;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 列表读写器。
    /// </summary>
    /// <typeparam name="T">列表类型</typeparam>
    /// <typeparam name="TValue">元素类型</typeparam>
    public sealed class ListRW<T, TValue> : IArrayRW, IFastArrayRW where T : IList<TValue?>
    {
        /// <summary>
        /// 默认容量。
        /// </summary>
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromList = typeof(T).IsAssignableFrom(typeof(List<TValue?>));

        static ArrayAppendingInfo appendingInfo = new ArrayAppendingInfo() { MostClosestMeanCommonlyUsedLength = DefaultCapacity };
        static ArrayAppendingInfo readFromAppendingInfo = new ArrayAppendingInfo() { MostClosestMeanCommonlyUsedLength = DefaultCapacity };

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
            get
            {
                if (content is not null)
                {
                    appendingInfo.AddUsedLength(content.Count);
                }
                return content;
            }
            set => content = value;
        }

        /// <summary>
        /// 获取数据源类型。
        /// </summary>
        public Type ContentType => typeof(T);

        /// <summary>
        /// 获取元素类型。
        /// </summary>
        public Type ValueType => typeof(TValue);

        /// <summary>
        /// 初始化一个具有默认容量的列表。
        /// </summary>
        public void Initialize()
        {
            Initialize(appendingInfo.MostClosestMeanCommonlyUsedLength);
        }

        /// <summary>
        /// 初始化一个指定容量的数组。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        [MemberNotNull(nameof(content))]
        public void Initialize(int capacity)
        {
            if (IsAssignableFromList)
            {
                content = TypeHelper.As<List<TValue?>, T>(new List<TValue?>(capacity));
            }
            else
            {
                content = Activator.CreateInstance<T>();

                if (content is List<TValue?> list)
                {
                    list.Capacity = capacity;
                }

                // TODO: Other Capacity
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
                throw new NullReferenceException(nameof(content));
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

                    ValueInterface<TValue>.WriteValue(dataWriter[i], content[i]);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    ValueInterface<TValue>.WriteValue(dataWriter[i], content[i]);
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
                throw new NullReferenceException(nameof(content));
            }

            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
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
                throw new NullReferenceException(nameof(content));
            }

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
        /// <param name="stopToken">取消令牌</param>
        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
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

                    content[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    content[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                }
            }
        }

        /// <summary>
        /// 将当前数据源写入到数组值写入器中。
        /// </summary>
        public unsafe void WriteTo(IFastArrayValueWriter writer)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            int length = content.Count;

            if (length > 0)
            {
                if (content is List<TValue?> list)
                {
                    var rawData = ArrayHelper.GetRawData(list) ?? throw new NullReferenceException();

                    writer.WriteArray(ref rawData[0], length);
                }
                else
                {
                    var temp = new TValue?[length];

                    content.CopyTo(temp, 0);

                    writer.WriteArray(ref temp[0], length);
                }
            }
            else
            {
                writer.WriteEmptyArray<TValue>();
            }
        }

        /// <summary>
        /// 从数组值读取器中读取数组到数据源中。
        /// </summary>
        public void ReadFrom(IFastArrayValueReader valueReader)
        {
            var array = valueReader.ReadArray<TValue>(readFromAppendingInfo.MostClosestMeanCommonlyUsedLength, out var count);

            if (IsAssignableFromList)
            {
                content = TypeHelper.As<List<TValue?>, T>(ArrayHelper.CreateList(array, count));
            }
            else
            {
                Initialize(count);

                if (content is List<TValue?> list && array.Length == count)
                {
                    list.AddRange(array);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        content.Add(array[i]);
                    }
                }
            }

            readFromAppendingInfo.AddUsedLength(count);
        }

        object? IDataRW.Content { get => Content; set => Content = (T?)value; }

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (T?)value; }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        sealed class ValueRW : BaseGenericRW<TValue>
        {
            readonly ListRW<T, TValue> ListRW;
            readonly int Index;

            public ValueRW(ListRW<T, TValue> listRW, int index)
            {
                ListRW = listRW;
                Index = index;
            }

            public override TValue? ReadValue()
            {
                if (ListRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                return ListRW.content[Index];
            }

            public override void WriteValue(TValue? value)
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