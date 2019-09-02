using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    sealed class AuthorizedKeysRW<TInput, TOutput, TValue> : IDataReader<TInput>, IDataWriter<TInput>, IValueReader, IValueWriter, IMapValueReader, IMapValueWriter
    {
        readonly Dictionary<TOutput, TValue> AuthorizedKeys;

        readonly object DataRW;
        object ValueRW;

        IDataReader<TInput> DataReader => Unsafe.As<IDataReader<TInput>>(DataRW);
        IDataWriter<TInput> DataWriter => Unsafe.As<IDataWriter<TInput>>(DataRW);
        ref IValueReader ValueReader => ref Unsafe.As<object, IValueReader>(ref ValueRW);
        ref IValueWriter ValueWriter => ref Unsafe.As<object, IValueWriter>(ref ValueRW);

        public AuthorizedKeysRW(object dataRW, Dictionary<TOutput, TValue> authorizedKeys)
        {
            DataRW = dataRW;
            AuthorizedKeys = authorizedKeys;
        }

        IValueReader IDataReader<TInput>.this[TInput key]
        {
            get
            {
                if (typeof(TInput) == typeof(TOutput) && !AuthorizedKeys.ContainsKey(Unsafe.As<TInput, TOutput>(ref key)))
                {
                    /* 未授权的字段 */

                    return RWHelper.DefaultValueRW;
                }

                ValueReader = DataReader[key];

                return this;
            }
        }

        IValueWriter IDataWriter<TInput>.this[TInput key]
        {
            get
            {
                if (typeof(TInput) == typeof(TOutput) && !AuthorizedKeys.ContainsKey(Unsafe.As<TInput, TOutput>(ref key)))
                {
                    /* 未授权的字段 */

                    return RWHelper.DefaultValueRW;
                }

                ValueWriter = DataWriter[key];

                return this;
            }
        }


        IEnumerable<TInput> GetContainsKeys(IEnumerable<TInput> keys)
        {
            foreach (var item in keys)
            {
                if (AuthorizedKeys.ContainsKey(Unsafe.As<TInput, TOutput>(ref Unsafe.AsRef(item))))
                {
                    yield return item;
                }
            }
        }

        IEnumerable<TInput> IDataReader<TInput>.Keys
        {
            get
            {
                if (typeof(TInput) == typeof(TOutput))
                {
                    return GetContainsKeys(DataReader.Keys);
                }
                else
                {
                    return DataReader.Keys;
                }
            }
        }

        IEnumerable<TInput> IDataWriter<TInput>.Keys
        {
            get
            {
                if (typeof(TInput) == typeof(TOutput))
                {
                    return GetContainsKeys(DataWriter.Keys);
                }
                else
                {
                    return DataWriter.Keys;
                }
            }
        }

        int IDataReader.Count => DataReader.Count;

        int IDataWriter.Count => DataWriter.Count;

        object IDataReader.ReferenceToken => DataReader.ReferenceToken;

        void IDataWriter.Initialize()
        {
            DataWriter.Initialize();
        }

        void IDataWriter.Initialize(int capacity)
        {
            DataWriter.Initialize(capacity);
        }

        public void OnReadAll(IDataWriter<TInput> dataWriter)
        {
            DataReader.OnReadAll(new AuthorizedKeysRW<TInput, TOutput, TValue>(dataWriter, AuthorizedKeys));
        }

        public void OnReadAll(IDataWriter<TInput> dataWriter, IValueFilter<TInput> valueFilter)
        {
            DataReader.OnReadAll(new AuthorizedKeysRW<TInput, TOutput, TValue>(dataWriter, AuthorizedKeys), valueFilter);
        }

        public void OnWriteAll(IDataReader<TInput> dataReader)
        {
            DataWriter.OnWriteAll(new AuthorizedKeysRW<TInput, TOutput, TValue>(dataReader, AuthorizedKeys));
        }

        public void OnReadValue(TInput key, IValueWriter valueWriter)
        {
            if (typeof(TInput) == typeof(TOutput) && !AuthorizedKeys.ContainsKey(Unsafe.As<TInput, TOutput>(ref key)))
            {
                /* 未授权的字段 */

                DataReader[key].DirectRead();
                valueWriter.DirectWrite(null);

                return;
            }

            DataReader.OnReadValue(key, valueWriter);
        }

        public void OnWriteValue(TInput key, IValueReader valueReader)
        {
            if (typeof(TInput) == typeof(TOutput) && !AuthorizedKeys.ContainsKey(Unsafe.As<TInput, TOutput>(ref key)))
            {
                /* 未授权的字段 */

                DataReader[key].DirectRead();
                valueReader.DirectRead();

                return;
            }

            DataWriter.OnWriteValue(key, valueReader);
        }

        void IValueReader.ReadObject(IDataWriter<string> valueWriter)
        {
            ValueReader.ReadObject(new AuthorizedKeysRW<string, TOutput, TValue>(valueWriter, AuthorizedKeys));
        }

        void IValueWriter.WriteObject(IDataReader<string> dataReader)
        {
            ValueWriter.WriteObject(new AuthorizedKeysRW<string, TOutput, TValue>(dataReader, AuthorizedKeys));
        }

        void IValueReader.ReadArray(IDataWriter<int> valueWriter)
        {
            ValueReader.ReadArray(new AuthorizedKeysRW<int, TOutput, TValue>(valueWriter, AuthorizedKeys));
        }

        void IValueWriter.WriteArray(IDataReader<int> dataReader)
        {
            ValueWriter.WriteArray(new AuthorizedKeysRW<int, TOutput, TValue>(dataReader, AuthorizedKeys));
        }

        void IMapValueReader.ReadMap<TKey>(IDataWriter<TKey> mapWriter)
        {
            mapWriter = new AuthorizedKeysRW<TKey, TOutput, TValue>(mapWriter, AuthorizedKeys);

            if (ValueReader is IMapValueReader mapReader)
            {
                mapReader.ReadMap(mapWriter);
            }
            else
            {
                ValueReader.ReadObject(mapWriter.As<string>());
            }
        }

        void IMapValueWriter.WriteMap<TKey>(IDataReader<TKey> mapReader)
        {
            mapReader = new AuthorizedKeysRW<TKey, TOutput, TValue>(mapReader, AuthorizedKeys);

            if (ValueWriter is IMapValueWriter mapWriter)
            {
                mapWriter.WriteMap(mapReader);
            }
            else
            {
                ValueWriter.WriteObject(mapReader.As<string>());
            }
        }

        long IValueReader.ReadInt64() => ValueReader.ReadInt64();

        double IValueReader.ReadDouble() => ValueReader.ReadDouble();

        string IValueReader.ReadString() => ValueReader.ReadString();

        bool IValueReader.ReadBoolean() => ValueReader.ReadBoolean();

        byte IValueReader.ReadByte() => ValueReader.ReadByte();

        char IValueReader.ReadChar() => ValueReader.ReadChar();

        DateTime IValueReader.ReadDateTime() => ValueReader.ReadDateTime();

        decimal IValueReader.ReadDecimal() => ValueReader.ReadDecimal();

        short IValueReader.ReadInt16() => ValueReader.ReadInt16();

        int IValueReader.ReadInt32() => ValueReader.ReadInt32();

        sbyte IValueReader.ReadSByte() => ValueReader.ReadSByte();

        float IValueReader.ReadSingle() => ValueReader.ReadSingle();

        ushort IValueReader.ReadUInt16() => ValueReader.ReadUInt16();

        uint IValueReader.ReadUInt32() => ValueReader.ReadUInt32();

        ulong IValueReader.ReadUInt64() => ValueReader.ReadUInt64();

        object IValueReader.DirectRead() => ValueReader.DirectRead();

        T? IValueReader.ReadNullable<T>() => ValueReader.ReadNullable<T>();

        void IValueWriter.WriteBoolean(bool value) => ValueWriter.WriteBoolean(value);

        void IValueWriter.WriteByte(byte value) => ValueWriter.WriteByte(value);

        void IValueWriter.WriteSByte(sbyte value) => ValueWriter.WriteSByte(value);

        void IValueWriter.WriteInt16(short value) => ValueWriter.WriteInt16(value);

        void IValueWriter.WriteChar(char value) => ValueWriter.WriteChar(value);

        void IValueWriter.WriteUInt16(ushort value) => ValueWriter.WriteUInt16(value);

        void IValueWriter.WriteInt32(int value) => ValueWriter.WriteInt32(value);

        void IValueWriter.WriteSingle(float value) => ValueWriter.WriteSingle(value);

        void IValueWriter.WriteUInt32(uint value) => ValueWriter.WriteUInt32(value);

        void IValueWriter.WriteInt64(long value) => ValueWriter.WriteInt64(value);

        void IValueWriter.WriteDouble(double value) => ValueWriter.WriteDouble(value);

        void IValueWriter.WriteUInt64(ulong value) => ValueWriter.WriteUInt64(value);

        void IValueWriter.WriteString(string value) => ValueWriter.WriteString(value);

        void IValueWriter.WriteDateTime(DateTime value) => ValueWriter.WriteDateTime(value);

        void IValueWriter.WriteDecimal(decimal value) => ValueWriter.WriteDecimal(value);

        void IValueWriter.DirectWrite(object value) => ValueWriter.DirectWrite(value);

    }
}