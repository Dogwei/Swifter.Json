
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Swifter.Data
{
    sealed class ReadScalarReader : BaseDirectRW,
        IValueReader<DataSet>,
        IValueReader<DataTable>,
        IValueReader<DbRowObject>,
        ITargetedBind
    {
        public const long TargetedId = unchecked((long)0xB42D701DD20187D8);

        enum ReadState
        {
            Table = 1,
            Row = 2,
            Value = 0,
            Finish = 4
        }

        readonly DbDataReader dbDataReader;

        DbRowObjectMap dbRowObjectMap;

        ReadState state;

        int ordinal;

        public ReadScalarReader(DbDataReader dbDataReader)
        {
            this.dbDataReader = dbDataReader;

            state = ReadState.Table;
        }

        public override void ReadArray(IDataWriter<int> valueWriter)
        {
            switch (state)
            {
                case ReadState.Table:

                    state = ReadState.Row;

                    if (!FastObjectArrayCollectionInvoker.TryReadTo(dbDataReader, valueWriter))
                    {
                        valueWriter.Initialize();

                        for (int i = 0; dbDataReader.Read(); i++)
                        {
                            valueWriter.OnWriteValue(i, this);
                        }
                    }

                    state = ReadState.Finish;

                    break;
                case ReadState.Row:
                    state = ReadState.Value;

                    valueWriter.Initialize();

                    for (ordinal = 0; ordinal < dbDataReader.FieldCount; ordinal++)
                    {
                        valueWriter.OnWriteValue(ordinal, this);
                    }

                    state = ReadState.Row;

                    break;
                case ReadState.Value:
                    base.ReadArray(valueWriter);

                    break;
                default:
                    if (dbDataReader.NextResult())
                    {
                        goto case ReadState.Table;
                    }

                    throw new ArgumentException("Finish");
            }
        }

        public override void ReadObject(IDataWriter<string> valueWriter)
        {
            switch (state)
            {
                case ReadState.Table:
                    state = ReadState.Value;

                    if (dbDataReader.Read())
                    {
                        valueWriter.Initialize();

                        for (ordinal = 0; ordinal < dbDataReader.FieldCount; ordinal++)
                        {
                            valueWriter.OnWriteValue(dbDataReader.GetName(ordinal), this);
                        }
                    }

                    state = ReadState.Finish;

                    break;
                case ReadState.Row:
                    state = ReadState.Value;

                    valueWriter.Initialize();

                    for (ordinal = 0; ordinal < dbDataReader.FieldCount; ordinal++)
                    {
                        valueWriter.OnWriteValue(dbDataReader.GetName(ordinal), this);
                    }

                    state = ReadState.Row;

                    break;
                case ReadState.Value:
                    base.ReadObject(valueWriter);

                    break;
                default:
                    if (dbDataReader.NextResult())
                    {
                        goto case ReadState.Table;
                    }

                    throw new ArgumentException("Finish");
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object DirectRead()
        {
            object value = null;

            switch (state)
            {
                case ReadState.Table:

                    if (dbDataReader.Read())
                    {
                        value = dbDataReader[0];
                    }

                    state = ReadState.Finish;

                    break;
                case ReadState.Row:
                    value = dbDataReader[0];

                    break;
                case ReadState.Value:
                    value = dbDataReader[ordinal];

                    break;
                default:
                    if (dbDataReader.NextResult())
                    {
                        goto case ReadState.Table;
                    }

                    throw new ArgumentException("Finish");
            }

            if (value == DBNull.Value)
            {
                return null;
            }

            return value;
        }

        public override void DirectWrite(object value)
        {
            throw new NotSupportedException();
        }

        public DbRowObject ReadDbRowObject()
        {
            switch (state)
            {
                case ReadState.Table:

                    state = ReadState.Finish;

                    if (dbDataReader.Read())
                    {
                        goto case ReadState.Row;
                    }
                    else
                    {
                        return null;
                    }

                case ReadState.Row:
                    dbRowObjectMap ??= DbRowObject.CreateMap(dbDataReader);

                    return DbRowObject.ValueOf(dbDataReader, dbRowObjectMap);
                case ReadState.Value:
                    throw new NotSupportedException("Dimension error.");
                default:
                    if (dbDataReader.NextResult())
                    {
                        goto case ReadState.Table;
                    }

                    throw new ArgumentException("Finish");
            }

        }

        public DataSet ReadDataSet()
        {
            switch (state)
            {
                case ReadState.Table:

                    var ds = new DataSet();

                    while (state != ReadState.Finish)
                    {
                        ds.Tables.Add(ReadDataTable());
                    }

                    return ds;
                case ReadState.Row:
                    throw new NotSupportedException();
                case ReadState.Value:
                    return XConvert<DataSet>.FromObject(DirectRead());
                default:
                    throw new ArgumentException("Finish");
            }
        }

        public DataTable ReadDataTable()
        {
            switch (state)
            {
                case ReadState.Table:

                    var dt = new DataTable();

                    for (int i = 0; i < dbDataReader.FieldCount; i++)
                    {
                        dt.Columns.Add(dbDataReader.GetName(i), dbDataReader.GetFieldType(i));
                    }

                    while (dbDataReader.Read())
                    {
                        var row = dt.NewRow();

                        for (int i = 0; i < dbDataReader.FieldCount; i++)
                        {
                            row[i] = dbDataReader[i];
                        }

                        dt.Rows.Add(row);
                    }

                    state = ReadState.Finish;

                    return dt;
                case ReadState.Row:
                    throw new NotSupportedException();
                case ReadState.Value:
                    return XConvert<DataTable>.FromObject(DirectRead());
                default:

                    if (dbDataReader.NextResult())
                    {
                        goto case ReadState.Table;
                    }

                    throw new ArgumentException("Finish");
            }
        }






        DataTable IValueReader<DataTable>.ReadValue() => ReadDataTable();

        DataSet IValueReader<DataSet>.ReadValue() => ReadDataSet();

        DbRowObject IValueReader<DbRowObject>.ReadValue() => ReadDbRowObject();

        long ITargetedBind.TargetedId => TargetedId;

        void ITargetedBind.MakeTargetedId()
        {

        }
    }
}