using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class DataSetRW<T> : IArrayRW where T : DataSet
    {
        const int DefaultCapacity = 1;

        public T? content;

        public IValueRW this[int key] => new ValueRW(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        public int Count => content?.Tables.Count ?? -1;

        public object? Content
        {
            get => content;
            set => content = (T?)value;
        }

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(DataTable);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T) == typeof(DataSet))
            {
                content = Unsafe.As<T>(new DataSet());
            }
            else
            {
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            int length = content.Tables.Count;
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

                    ValueInterface<DataTable>.WriteValue(dataWriter[i], content.Tables[i]);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    ValueInterface<DataTable>.WriteValue(dataWriter[i], content.Tables[i]);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            ValueInterface<DataTable>.WriteValue(valueWriter, content.Tables[key]);
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            int length = content.Tables.Count;
            int i = 0;

            var canBeStopped = stopToken.CanBeStopped;

            if (canBeStopped && stopToken.PopState() is ValueTuple<int, int> state)
            {
                (length, i) = state;
            }
            else
            {
                content.Clear();
            }

            for (; i < length; i++)
            {
                if (canBeStopped && stopToken.IsStopRequested)
                {
                    stopToken.SetState((length, i));

                    return;
                }

                content.Tables.Add(ValueInterface<DataTable>.ReadValue(dataReader[i]) ?? throw new NullReferenceException());
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            if (key == Count)
            {
                content.Tables.Add(ValueInterface<DataTable>.ReadValue(valueReader) ?? throw new NullReferenceException());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        sealed class ValueRW : BaseGenericRW<DataTable>, IValueRW<DataTable>
        {
            public readonly DataSetRW<T> BaseRW;
            public readonly int Index;

            public ValueRW(DataSetRW<T> baseRW, int index)
            {
                BaseRW = baseRW;
                Index = index;
            }

            public override DataTable? ReadValue()
            {
                if (BaseRW.content is null)
                {
                    throw new NullReferenceException();
                }

                return BaseRW.content.Tables[Index];
            }

            public override void WriteValue(DataTable? value)
            {
                if (BaseRW.content is null)
                {
                    throw new NullReferenceException();
                }

                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (Index == BaseRW.Count)
                {
                    BaseRW.content.Tables.Add(value);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}