
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Swifter.Data
{
    unsafe sealed class FastObjectArrayCollectionInvoker : BaseDirectRW, IGenericInvoker, IDataReader<int>
    {
        static class GenericAppendingInfo<T>
        {
            public static ArrayAppendingInfo ArrayAppendingInfo = new ArrayAppendingInfo { MostClosestMeanCommonlyUsedLength = 3 };
            public static ArrayAppendingInfo OtherAppendingInfo = new ArrayAppendingInfo { MostClosestMeanCommonlyUsedLength = 3 };
        }


        public static bool TryReadTo(DbDataReader dbDataReader, IDataWriter<int> dataWriter)
        {
            if (dataWriter is IArrayCollectionRW acrw)
            {
                var invoker = new FastObjectArrayCollectionInvoker(dbDataReader, dataWriter);

                acrw.InvokeElementType(invoker);

                return invoker.Result;
            }

            return false;
        }

        readonly DbDataReader DbDataReader;
        readonly IDataWriter<int> DataWriter;

        int* Map;

        int index;

        public bool Result;

        FastObjectArrayCollectionInvoker(DbDataReader dbDataReader, IDataWriter<int> dataWriter)
        {
            DbDataReader = dbDataReader;
            DataWriter = dataWriter;
        }

        public void Invoke<TElement>()
        {
            if (FastObjectRW<TElement>.IsFastObjectInterface)
            {
                var objectRW = FastObjectRW<TElement>.Create();

                var map = stackalloc int[objectRW.Count];

                Map = map;

                for (int i = 0; i < DbDataReader.FieldCount; i++)
                {
                    var index = objectRW.GetOrdinal(DbDataReader.GetName(i));

                    if (index >= 0)
                    {
                        map[index] = i + 1;
                    }
                }

                if (DataWriter.ContentType == typeof(TElement[]))
                {
                    ref var appendingInfo = ref GenericAppendingInfo<TElement>.ArrayAppendingInfo;

                    var elements = new TElement[appendingInfo.MostClosestMeanCommonlyUsedLength];

                    int offset = 0;

                    while (DbDataReader.Read())
                    {
                        if (offset >= elements.Length)
                        {
                            Array.Resize(ref elements, offset * 2 + 1);
                        }

                        objectRW.Initialize();

                        objectRW.OnWriteAll(this);

                        elements[offset] = objectRW.content;

                        ++offset;
                    }

                    if (elements.Length != offset)
                    {
                        Array.Resize(ref elements, offset);
                    }

                    appendingInfo.AddUsedLength(offset);

                    DataWriter.Content = elements;

                    Result = true;
                }
                else
                {
                    ref var appendingInfo = ref GenericAppendingInfo<TElement>.OtherAppendingInfo;

                    var copyer = new ValueCopyer();

                    DataWriter.Initialize(appendingInfo.MostClosestMeanCommonlyUsedLength);

                    int offset = 0;

                    while (DbDataReader.Read())
                    {
                        objectRW.Initialize();

                        objectRW.OnWriteAll(this);

                        copyer.DirectWrite(objectRW.content);

                        DataWriter.OnWriteValue(offset, copyer);

                        ++offset;
                    }

                    appendingInfo.AddUsedLength(offset);

                    Result = true;
                }

            }
        }

        public IValueReader this[int key]
        {
            get
            {
                index = Map[key];

                return this;
            }
        }

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => throw new NotSupportedException();

        public Type ContentType => throw new NotSupportedException();

        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            throw new NotSupportedException();
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object DirectRead()
        {
            if (index > 0)
            {
                var obj = DbDataReader[index - 1];

                if (obj != DBNull.Value)
                {
                    return obj;
                }
            }

            return null;
        }

        public override void DirectWrite(object value)
        {
            throw new NotSupportedException();
        }
    }
}