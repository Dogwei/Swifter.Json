using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class DataRowRW<T> : IObjectRW, IArrayRW where T : DataRow
    {
        const int DefaultCapacity = 3;

        public readonly DataTable BaseDataTable;
        public readonly T? FixedContent;

        public T? content;

        public DataRowRW(DataTable baseDataTable, T fixedContent)
        {
            BaseDataTable = baseDataTable;
            FixedContent = fixedContent;
        }

        public DataRowRW(DataTable baseDataTable)
        {
            BaseDataTable = baseDataTable;
        }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        public IValueRW this[string key] => new ValueRW(this, key);

        public IValueRW this[int key] => new ValueRW(this, key);

        public int Count => BaseDataTable.Columns.Count;

        public Type ContentType => typeof(T);

        public Type? ValueType => null;

        public IEnumerable<string> Keys
        {
            get
            {
                if (content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                throw new NotImplementedException();
            }
        }

        public object? Content
        {
            get => content;
            set
            {
                if (value is DataRow dataRowValue && dataRowValue.Table != BaseDataTable)
                {
                    if (content is null)
                    {
                        Initialize();
                    }

                    dataRowValue.CopyTo(content, true);
                }
                else
                {
                    content = (T?)value;
                }
            }
        }

        [MemberNotNull(nameof(content))]
        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        [MemberNotNull(nameof(content))]
        public void Initialize(int capacity)
        {
            if (FixedContent is not null)
            {
                content = FixedContent;
            }
            else
            {
                content = Unsafe.As<T>(BaseDataTable.NewRow());
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var columns = content.Table.Columns;

            int length = columns.Count;
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

                    var column = columns[i];

                    ValueInterface.WriteValue(dataWriter[column.ColumnName], content[column].AsNullIfDBNull());
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    var column = columns[i];

                    ValueInterface.WriteValue(dataWriter[column.ColumnName], content[column].AsNullIfDBNull());
                }
            }
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            ValueInterface.WriteValue(valueWriter, content[key].AsNullIfDBNull());
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var column = content.Table.Columns[key];

            // 此方法允许列不存在时新增列。
            if (column is null)
            {
                column = content.Table.Columns.Add(key, typeof(object));
            }

            if (column.DataType == typeof(object))
            {
                content[column] = valueReader.DirectRead();
            }
            else
            {
                content[column] = ValueInterface.GetInterface(column.DataType).Read(valueReader);
            }
        }

        public void OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var columns = content.Table.Columns;

            int length = columns.Count;
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

                    var column = columns[i];

                    content[column] = ValueInterface.ReadValue(dataReader[column.ColumnName], column.DataType);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    var column = columns[i];

                    content[column] = ValueInterface.ReadValue(dataReader[column.ColumnName], column.DataType);
                }
            }

        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            ValueInterface.WriteValue(valueWriter, content[key].AsNullIfDBNull());
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var columns = content.Table.Columns;

            int length = columns.Count;
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

                    var column = columns[i];

                    ValueInterface.WriteValue(dataWriter[i], content[column].AsNullIfDBNull());
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    var column = columns[i];

                    ValueInterface.WriteValue(dataWriter[i], content[column].AsNullIfDBNull());
                }
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var column = content.Table.Columns[key];

            if (column.DataType == typeof(object))
            {
                content[column] = valueReader.DirectRead();
            }
            else
            {
                content[column] = ValueInterface.GetInterface(column.DataType).Read(valueReader);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var columns = content.Table.Columns;

            int length = columns.Count;
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

                    var column = columns[i];

                    content[column] = ValueInterface.ReadValue(dataReader[i], column.DataType);
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    var column = columns[i];

                    content[column] = ValueInterface.ReadValue(dataReader[i], column.DataType);
                }
            }
        }

        sealed class ValueRW : BaseDirectRW
        {
            public readonly DataRowRW<T> BaseRW;
            public readonly object Key; // ColumnName or Index

            public ValueRW(DataRowRW<T> baseRW, string columnName)
            {
                BaseRW = baseRW;
                Key = columnName;
            }

            public ValueRW(DataRowRW<T> baseRW, int index)
            {
                BaseRW = baseRW;
                Key = index;
            }

            public override Type? ValueType
            {
                get
                {
                    if (BaseRW.content is null)
                    {
                        return null;
                    }

                    if (Key is int index)
                    {
                        if (index >= 0 && index < BaseRW.content.Table.Columns.Count)
                        {
                            return BaseRW.content[index]?.GetType();
                        }
                    }
                    else
                    {
                        index = BaseRW.content.Table.Columns.IndexOf(Unsafe.As<string>(Key));

                        if (index >= 0)
                        {
                            return BaseRW.content[index]?.GetType();
                        }
                    }

                    return null;
                }
            }

            public override object? DirectRead()
            {
                if (BaseRW.content is null)
                {
                    throw new NullReferenceException();
                }

                if (Key is int index)
                {
                    return BaseRW.content[index];
                }
                else
                {
                    return BaseRW.content[Unsafe.As<string>(Key)];
                }
            }

            public override void DirectWrite(object? value)
            {
                if (BaseRW.content is null)
                {
                    throw new NullReferenceException();
                }

                if (Key is int index)
                {
                    BaseRW.content[index] = value;
                }
                else
                {
                    var column = BaseRW.content.Table.Columns[Unsafe.As<string>(Key)];

                    // 此方法允许列不存在时新增列。
                    if (column is null)
                    {
                        column = BaseRW.content.Table.Columns.Add(Unsafe.As<string>(Key), typeof(object));
                    }

                    BaseRW.content[column] = value;
                }
            }
        }
    }
}