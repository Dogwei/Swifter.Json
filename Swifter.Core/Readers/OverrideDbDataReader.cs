using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Swifter.Readers
{
    /// <summary>
    /// 重写数据库读取器，使它成为表格读取器。
    /// </summary>
    public sealed class OverrideDbDataReader : ITableReader
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        public readonly DbDataReader dbDataReader;

        /// <summary>
        /// 初始化数据读取器。
        /// </summary>
        /// <param name="dbDataReader">数据源</param>
        public OverrideDbDataReader(DbDataReader dbDataReader)
        {
            this.dbDataReader = dbDataReader;
        }

        /// <summary>
        /// 获取位于指定索引处的值读取器。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[int key]=> new ValueReader(dbDataReader, key);


        /// <summary>
        /// 获取位于指定名称的值读取器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[string key]=> this[dbDataReader.GetOrdinal(key)];

        IEnumerable<int> IDataReader<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        /// <summary>
        /// 获取表格列的数量。
        /// </summary>
        public int Count => dbDataReader.FieldCount;

        /// <summary>
        /// 获取表格列的名称集合。
        /// </summary>
        public IEnumerable<string> Keys => ArrayHelper.CreateNamesIterator(dbDataReader);

        /// <summary>
        /// 获取数据源的 Id。
        /// </summary>
        public object ReferenceToken => null;

        /// <summary>
        /// 读取所有值当前行的所有值，然后写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        /// <summary>
        /// 读取所有值当前行的所有值，然后写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            int length = dbDataReader.FieldCount;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[dbDataReader.GetName(i)]);
            }
        }

        /// <summary>
        /// 读取指定位置的值，然后写入到值写入器中。
        /// </summary>
        /// <param name="key">指定位置</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var value = dbDataReader[key];

            ValueInterface.GetInterface(value).Write(valueWriter, value);
        }

        /// <summary>
        /// 读取指定名称的值，然后写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            OnReadValue(dbDataReader.GetOrdinal(key), valueWriter);
        }

        /// <summary>
        /// 读取下一行数据。
        /// </summary>
        /// <returns>返回是否有下一行数据</returns>
        public bool Read()
        {
            return dbDataReader.Read();
        }

        /// <summary>
        /// 读取当前行的所有数据并进行筛选，然后将筛选结果写入器数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            int length = dbDataReader.FieldCount;

            var valueInfo = new ValueFilterInfo<string>();

            for (int i = 0; i < length; i++)
            {
                var value = dbDataReader[i];

                ValueInterface.GetInterface(value).Write(valueInfo.ValueCopyer, value);

                valueInfo.Key = dbDataReader.GetName(i);
                valueInfo.Type = dbDataReader.GetFieldType(i);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        /// <summary>
        /// 读取当前行的所有数据并进行筛选，然后将筛选结果写入器数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = dbDataReader.FieldCount;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; i++)
            {
                var value = dbDataReader[i];

                ValueInterface.GetInterface(value).Write(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = dbDataReader.GetFieldType(i);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        private sealed class ValueReader : IValueReader
        {
            public readonly DbDataReader dbDataReader;
            public readonly int ordinal;

            public ValueReader(DbDataReader dbDataReader, int ordinal)
            {
                this.dbDataReader = dbDataReader;
                this.ordinal = ordinal;
            }

            public void ReadArray(IDataWriter<int> valueWriter)
            {
                throw new NotSupportedException($"Type '{nameof(OverrideDbDataReader)}' not support '{nameof(ReadArray)}'.");
            }

            public bool ReadBoolean() => Convert.ToBoolean(dbDataReader[ordinal]);

            public byte ReadByte() => Convert.ToByte(dbDataReader[ordinal]);

            public char ReadChar() => Convert.ToChar(dbDataReader[ordinal]);

            public DateTime ReadDateTime() => Convert.ToDateTime(dbDataReader[ordinal]);

            public decimal ReadDecimal() => Convert.ToDecimal(dbDataReader[ordinal]);

            public object DirectRead()
            {
                var value = dbDataReader[ordinal];

                if (value == DBNull.Value)
                {
                    return null;
                }

                return value;
            }

            public double ReadDouble() => Convert.ToDouble(dbDataReader[ordinal]);

            public short ReadInt16() => Convert.ToInt16(dbDataReader[ordinal]);

            public int ReadInt32() => Convert.ToInt32(dbDataReader[ordinal]);

            public long ReadInt64() => Convert.ToInt64(dbDataReader[ordinal]);

            public void ReadObject(IDataWriter<string> valueWriter)
            {
                throw new NotSupportedException($"Type '{nameof(OverrideDbDataReader)}' not support '{nameof(ReadObject)}'.");
            }

            public sbyte ReadSByte() => Convert.ToSByte(dbDataReader[ordinal]);

            public float ReadSingle() => Convert.ToSingle(dbDataReader[ordinal]);

            public string ReadString() => Convert.ToString(dbDataReader[ordinal]);

            public ushort ReadUInt16() => Convert.ToUInt16(dbDataReader[ordinal]);

            public uint ReadUInt32() => Convert.ToUInt32(dbDataReader[ordinal]);

            public ulong ReadUInt64() => Convert.ToUInt64(dbDataReader[ordinal]);

            public T? ReadNullable<T>() where T : struct => XConvert.FromObject<T>(DirectRead());
        }
    }

    internal sealed class DbDataReaderInterface<T> : IValueInterface<T> where T : DbDataReader
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            return XConvert.FromObject<T>(valueReader.DirectRead());
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var dataReader = new OverrideDbDataReader(value);
            var toArrayReader = new TableToArrayReader(dataReader);

            valueWriter.WriteArray(toArrayReader);
        }
    }
}