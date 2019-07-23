using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using static Swifter.RW.DataTableRW;

namespace Swifter.Readers
{
    /// <summary>
    /// 重写数据库读取器，使它成为表格读取器。
    /// </summary>
    sealed class OverrideDbDataReader : IDataReader<int>
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        public readonly System.Data.IDataReader dbDataReader;

        public readonly RowReader Reader;

        public readonly DataTableRWOptions Options;

        /// <summary>
        /// 初始化数据读取器。
        /// </summary>
        /// <param name="dbDataReader">数据源</param>
        /// <param name="options">配置项</param>
        public OverrideDbDataReader(System.Data.IDataReader dbDataReader, DataTableRWOptions options = DataTableRWOptions.None)
        {
            this.dbDataReader = dbDataReader;

            Options = options;

            Reader = new RowReader(dbDataReader);
        }

        /// <summary>
        /// 获取位于指定索引处的值读取器。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[int key] => new ReadCopyer<int>(this, key);

        /// <summary>
        /// 获取表格列的数量。
        /// </summary>
        public int Count => 0;

        /// <summary>
        /// 获取表格列的名称集合。
        /// </summary>
        public IEnumerable<int> Keys => null;

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
            for (int i = 0; dbDataReader.Read(); i++)
            {
                if (i != 0 && (Options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
                {
                    dataWriter[i].WriteArray(Reader);
                }
                else
                {
                    dataWriter[i].WriteObject(Reader);
                }
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
            if (dbDataReader.Read())
            {
                valueWriter.WriteObject(Reader);
            }
            else
            {
                valueWriter.DirectWrite(null);
            }
        }

        /// <summary>
        /// 读取当前行的所有数据并进行筛选，然后将筛选结果写入器数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            OnReadAll(new DataFilterWriter<int>(dataWriter, valueFilter));
        }

        public sealed class RowReader : IDataReader<string>, IDataReader<int>
        {
            /// <summary>
            /// 数据源。
            /// </summary>
            public readonly System.Data.IDataReader dbDataReader;

            public RowReader(System.Data.IDataReader dbDataReader)
            {
                this.dbDataReader = dbDataReader;
            }

            public IValueReader this[string key] => new ValueReader(dbDataReader, dbDataReader.GetOrdinal(key));

            public IValueReader this[int key] => new ValueReader(dbDataReader, key);

            public IEnumerable<string> Keys => ArrayHelper.CreateNamesIterator(dbDataReader);

            IEnumerable<int> IDataReader<int>.Keys => null;

            public int Count => dbDataReader.FieldCount;

            public object ReferenceToken => dbDataReader;

            public void OnReadAll(IDataWriter<string> dataWriter)
            {
                for (int i = 0; i < dbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[dbDataReader.GetName(i)], dbDataReader[i]);
                }
            }

            public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
            {
                OnReadAll(new DataFilterWriter<string>(dataWriter, valueFilter));
            }

            public void OnReadAll(IDataWriter<int> dataWriter)
            {
                for (int i = 0; i < dbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], dbDataReader[i]);
                }
            }

            public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
            {
                OnReadAll(new DataFilterWriter<int>(dataWriter, valueFilter));
            }

            public void OnReadValue(string key, IValueWriter valueWriter)
            {
                ValueInterface.WriteValue(valueWriter, dbDataReader[key]);
            }

            public void OnReadValue(int key, IValueWriter valueWriter)
            {
                ValueInterface.WriteValue(valueWriter, dbDataReader[key]);
            }
        }

        public sealed class ValueReader : IValueReader
        {
            public readonly System.Data.IDataReader dbDataReader;
            public readonly int ordinal;

            public ValueReader(System.Data.IDataReader dbDataReader, int ordinal)
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

    internal sealed class DbDataReaderInterface<T> : IValueInterface<T> where T : class, System.Data.IDataReader
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            if (typeof(T).IsAssignableFrom(typeof(DataTableReader)))
            {
                return Unsafe.As<T>(ValueInterface<DataTable>.ReadValue(valueReader)?.CreateDataReader());
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var reader = new OverrideDbDataReader(value, valueWriter is ITargetedBind targeted && targeted.TargetedId != 0 ? targeted.GetDataTableRWOptions() : DefaultOptions);

            valueWriter.WriteArray(reader);
        }
    }
}