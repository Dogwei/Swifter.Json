using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DbDataReader = System.Data.IDataReader;

namespace Swifter.RW
{
    /// <summary>
    /// DB 读取器的读取器。
    /// </summary>
    sealed class DbDataReaderReader : IDataReader<int>
    {
        /// <summary>
        /// 数据源。
        /// </summary>
        readonly DbDataReader dbDataReader;

        readonly RowReader Reader;

        readonly DataTableRWOptions Options;

        /// <summary>
        /// 初始化数据读取器。
        /// </summary>
        /// <param name="dbDataReader">数据源</param>
        /// <param name="options">配置项</param>
        public DbDataReaderReader(DbDataReader dbDataReader, DataTableRWOptions options = DataTableRWOptions.None)
        {
            Options = options;

            this.dbDataReader = dbDataReader;

            Reader = new RowReader(dbDataReader);
        }

        /// <summary>
        /// 获取位于指定索引处的值读取器。
        /// </summary>
        /// <param name="key">指定索引</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[int key] => throw new NotSupportedException();

        /// <summary>
        /// 获取表格列的数量。
        /// </summary>
        public int Count => -1;

        /// <summary>
        /// 获取表格列的名称集合。
        /// </summary>
        public IEnumerable<int> Keys => throw new NotSupportedException();

        /// <summary>
        /// 获取数据源的类型。
        /// </summary>
        public Type ContentType => null;

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

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
        public void OnReadValue(int key, IValueWriter valueWriter) => throw new NotSupportedException();

        sealed class RowReader : IDataReader<string>, IDataReader<int>
        {
            /// <summary>
            /// 数据源。
            /// </summary>
            public DbDataReader dbDataReader;

            public RowReader(DbDataReader dbDataReader)
            {
                this.dbDataReader = dbDataReader;
            }

            public IValueReader this[string key] => new ValueReader(dbDataReader, dbDataReader.GetOrdinal(key));

            public IValueReader this[int key] => new ValueReader(dbDataReader, key);

            IEnumerable<string> IDataReader<string>.Keys => ArrayHelper.CreateNamesIterator(dbDataReader);

            IEnumerable<int> IDataReader<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

            public int Count => dbDataReader.FieldCount;

            public Type ContentType => null;

            public object Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void OnReadAll(IDataWriter<string> dataWriter)
            {
                for (int i = 0; i < dbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[dbDataReader.GetName(i)], dbDataReader[i]);
                }
            }

            public void OnReadAll(IDataWriter<int> dataWriter)
            {
                for (int i = 0; i < dbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], dbDataReader[i]);
                }
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

        sealed class ValueReader : IValueReader
        {
            public readonly DbDataReader dbDataReader;
            public readonly int ordinal;

            public ValueReader(DbDataReader dbDataReader, int ordinal)
            {
                this.dbDataReader = dbDataReader;
                this.ordinal = ordinal;
            }

            public void ReadArray(IDataWriter<int> valueWriter) => ValueCopyer.ValueOf(DirectRead()).ReadArray(valueWriter);

            public bool ReadBoolean() => Convert.ToBoolean(DirectRead());

            public byte ReadByte() => Convert.ToByte(DirectRead());

            public char ReadChar() => Convert.ToChar(DirectRead());

            public DateTime ReadDateTime() => Convert.ToDateTime(DirectRead());

            public decimal ReadDecimal() => Convert.ToDecimal(DirectRead());

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public object DirectRead()
            {
                var value = dbDataReader[ordinal];

                if (value == DBNull.Value)
                {
                    return null;
                }

                return value;
            }

            public double ReadDouble() => Convert.ToDouble(DirectRead());

            public short ReadInt16() => Convert.ToInt16(DirectRead());

            public int ReadInt32() => Convert.ToInt32(DirectRead());

            public long ReadInt64() => Convert.ToInt64(DirectRead());

            public void ReadObject(IDataWriter<string> valueWriter) => ValueCopyer.ValueOf(DirectRead()).ReadObject(valueWriter);

            public sbyte ReadSByte() => Convert.ToSByte(DirectRead());

            public float ReadSingle() => Convert.ToSingle(DirectRead());

            public string ReadString() => Convert.ToString(DirectRead());

            public ushort ReadUInt16() => Convert.ToUInt16(DirectRead());

            public uint ReadUInt32() => Convert.ToUInt32(DirectRead());

            public ulong ReadUInt64() => Convert.ToUInt64(DirectRead());

            public TValue? ReadNullable<TValue>() where TValue : struct => XConvert.FromObject<TValue?>(DirectRead());

            public TValue ReadEnum<TValue>() where TValue : struct, Enum => XConvert.FromObject<TValue>(DirectRead());
        }
    }
}