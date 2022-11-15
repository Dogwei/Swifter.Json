using Swifter.Tools;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 数组（向量）读写器。
    /// </summary>
    /// <typeparam name="TElement">数组元素类型</typeparam>
    public sealed class ArrayRW<TElement> : IArrayRW, IFastArrayRW
    {
        /// <summary>
        /// 默认容量。
        /// </summary>
        public const int DefaultCapacity = 3;

        static ArrayAppendingInfo appendingInfo = new ArrayAppendingInfo() { MostClosestMeanCommonlyUsedLength = DefaultCapacity };

        TElement?[]? array;
        int count;

        /// <summary>
        /// 获取指定索引器的值读写器。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IValueRW this[int key] => new ValueRW(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        /// <summary>
        /// 获取数组长度。
        /// </summary>
        public int Count => count;

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        public TElement?[]? Content
        {
            get
            {
                if (array != null && array.Length != count)
                {
                    Array.Resize(ref array, count);
                }

                appendingInfo.AddUsedLength(count);

                return array;
            }
            set
            {
                array = value;
                
                count = value?.Length ?? 0;
            }
        }

        /// <summary>
        /// 获取数据类型。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type ContentType => typeof(TElement[]);

        /// <summary>
        /// 获取元素类型。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type ValueType => typeof(TElement);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Expand()
        {
            Array.Resize(ref array, count * 2 + 1);
        }

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
            array = new TElement[capacity];

            count = 0;
        }

        /// <summary>
        /// 将所有元素写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; i < count; i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    ValueInterface.WriteValue(dataWriter[i], array[i]);
                }
            }
            else
            {
                for (; i < count; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], array[i]);
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
            if (array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (key >= count)
            {
                throw new IndexOutOfRangeException();
            }

            ValueInterface.WriteValue(valueWriter, array[key]);
        }

        /// <summary>
        /// 在数据读取器中读取所有元素。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            if (array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; i < count; i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    array[i] = ValueInterface.ReadValue<TElement>(dataReader[i])!;
                }
            }
            else
            {
                for (; i < count; i++)
                {
                    array[i] = ValueInterface.ReadValue<TElement>(dataReader[i])!;
                }
            }
        }

        /// <summary>
        /// 读取值读取器的值到指定索引处。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (key == count && key == array.Length)
            {
                Expand();
            }

            if (key > count)
            {
                throw new IndexOutOfRangeException();
            }

            array[key] = ValueInterface.ReadValue<TElement>(valueReader);

            if (key == count)
            {
                ++count;
            }
        }

        /// <summary>
        /// 将当前数据源写入到数组值写入器中。
        /// </summary>
        public void WriteTo(IFastArrayValueWriter writer)
        {
            if (array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (count > 0)
            {
                writer.WriteArray(ref array[0], count);
            }
            else
            {
                writer.WriteEmptyArray<TElement>();
            }
        }

        /// <summary>
        /// 从数组值读取器中读取数组到数据源中。
        /// </summary>
        public void ReadFrom(IFastArrayValueReader valueReader)
        {
            array = valueReader.ReadArray<TElement>(appendingInfo.MostClosestMeanCommonlyUsedLength, out count);
        }

        object? IDataRW.Content { get => Content; set => Content = (TElement[]?)value; }

        object? IDataReader.Content { get => Content; set => Content = (TElement[]?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (TElement[]?)value; }

        sealed class ValueRW : BaseGenericRW<TElement>, IValueRW<TElement>
        {
            readonly ArrayRW<TElement> BaseRW;
            readonly int Index;

            public ValueRW(ArrayRW<TElement> baseRW, int index)
            {
                BaseRW = baseRW;
                Index = index;
            }

            public override TElement? ReadValue()
            {
                if (BaseRW.array is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                if (Index >= BaseRW.count)
                {
                    throw new IndexOutOfRangeException();
                }

                return BaseRW.array[Index];
            }

            public override void WriteValue(TElement? value)
            {
                ref var count = ref BaseRW.count;
                ref var array = ref BaseRW.array;

                if (array is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                if (Index == count && Index == array.Length)
                {
                    BaseRW.Expand();
                }

                if (Index > count)
                {
                    throw new IndexOutOfRangeException();
                }

                array[Index] = value;

                if (Index == count)
                {
                    ++count;
                }
            }
        }
    }

    /// <summary>
    /// 表示连续内存的快速数组读写器。
    /// </summary>
    public interface IFastArrayRW
    {
        /// <summary>
        /// 将当前数据源写入到数组值写入器中。
        /// </summary>
        void WriteTo(IFastArrayValueWriter valueWriter);

        /// <summary>
        /// 从数组值读取器中读取数组到数据源中。
        /// </summary>
        void ReadFrom(IFastArrayValueReader valueReader);
    }

    /// <summary>
    /// 连续内存的快速数组值写入器。
    /// </summary>
    public interface IFastArrayValueWriter
    {
        /// <summary>
        /// 写入一个数组。
        /// </summary>
        /// <param name="firstElement">数组首个元素引用</param>
        /// <param name="length">数组长度</param>
        void WriteArray<T>(ref T? firstElement, int length);
    }

    /// <summary>
    /// 连续内存的快速数组值读取器。
    /// </summary>
    public interface IFastArrayValueReader
    {
        /// <summary>
        /// 读取一个数组。
        /// </summary>
        /// <param name="defaultCapacity">数组默认容量</param>
        /// <param name="length">返回数组长度</param>
        T?[] ReadArray<T>(int defaultCapacity, out int length);
    }
}