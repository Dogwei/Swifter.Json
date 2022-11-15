using Swifter.Tools;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using DbDataReader = System.Data.IDataReader;

namespace Swifter.RW
{
    sealed class DbDataReaderReader : IArrayReader
    {
        readonly DbDataReader dbDataReader;
        readonly RowReader rowReader;
        readonly DataTableRWOptions options;

        public DbDataReaderReader(DbDataReader dbDataReader, DataTableRWOptions options = DataTableRWOptions.None)
        {
            this.options = options;
            this.dbDataReader = dbDataReader;

            rowReader = new RowReader(dbDataReader);
        }

        public IValueReader this[int key] => throw new NotSupportedException();

        public int Count => -1;

        public Type? ContentType => null;

        public Type? ValueType => null;

        public object? Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            for (int i = 0; dbDataReader.Read(); i++)
            {
                if (i != 0 && options.On(DataTableRWOptions.WriteToArrayFromBeginningSecondRows))
                {
                    dataWriter[i].WriteArray(rowReader);
                }
                else
                {
                    dataWriter[i].WriteObject(rowReader);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter) => throw new NotSupportedException();

        sealed class RowReader : IDataReader<string>, IArrayReader
        {
            /// <summary>
            /// 数据源。
            /// </summary>
            public readonly DbDataReader DbDataReader;

            public RowReader(DbDataReader dbDataReader)
            {
                DbDataReader = dbDataReader;
            }

            public IValueReader this[string key] => new ValueReader(DbDataReader, DbDataReader.GetOrdinal(key));

            public IValueReader this[int key] => new ValueReader(DbDataReader, key);

            public int Count => DbDataReader.FieldCount;

            public Type? ContentType => null;

            public Type? ValueType => null;

            public object? Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default)
            {
                for (int i = 0; i < DbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[DbDataReader.GetName(i)], DbDataReader[i]);
                }
            }

            public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
            {
                for (int i = 0; i < DbDataReader.FieldCount; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], DbDataReader[i]);
                }
            }

            public void OnReadValue(string key, IValueWriter valueWriter)
            {
                ValueInterface.WriteValue(valueWriter, DbDataReader[key]);
            }

            public void OnReadValue(int key, IValueWriter valueWriter)
            {
                ValueInterface.WriteValue(valueWriter, DbDataReader[key]);
            }
        }

        sealed class ValueReader : BaseDirectRW
        {
            public readonly DbDataReader DbDataReader;
            public readonly int Ordinal;

            public ValueReader(DbDataReader dbDataReader, int ordinal)
            {
                DbDataReader = dbDataReader;
                Ordinal = ordinal;
            }

            public override Type ValueType => DbDataReader.GetFieldType(Ordinal);

            public override object? DirectRead()
            {
                return DbDataReader[Ordinal].AsNullIfDBNull();
            }

            public override void DirectWrite(object? value)
            {
                throw new NotSupportedException();
            }
        }
    }
}