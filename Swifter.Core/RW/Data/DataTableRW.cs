using InlineIL;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    sealed class DataTableRW<T> : IArrayRW where T : DataTable
    {
        const int DefaultCapacity = 3;

        readonly DataTableRWOptions options;

        public T? content;

        public DataTableRW(DataTableRWOptions options)
        {
            this.options = options;
        }

        public IValueRW this[int key] => new ValueRW(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        public int Count => content?.Rows.Count ?? -1;

        public object? Content
        {
            get => content;
            set => content = (T?)value;
        }

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(DataRow);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T) == typeof(DataTable))
            {
                content = Unsafe.As<T>(new DataTable());
            }
            else
            {
                content = Activator.CreateInstance<T>();
            }

            content.MinimumCapacity = capacity;
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            var rows = content.Rows;

            int length = rows.Count;
            int i = 0;

            var canBeStopped = stopToken.CanBeStopped;

            if (canBeStopped && stopToken.PopState() is int index)
            {
                i = index;
            }

            for (; i < length; i++)
            {
                if (canBeStopped && stopToken.IsStopRequested)
                {
                    stopToken.SetState(i);

                    return;
                }

                var dataRowRW = new DataRowRW<DataRow>(content, rows[i]);

                dataRowRW.Initialize();

                if (i != 0 && options.On(DataTableRWOptions.WriteToArrayFromBeginningSecondRows))
                {
                    dataWriter[i].WriteArray(dataRowRW);
                }
                else
                {
                    dataWriter[i].WriteObject(dataRowRW);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            var rows = content.Rows;

            var rw = new DataRowRW<DataRow>(content, rows[key]);

            rw.Initialize();

            if (key != 0 && options.On(DataTableRWOptions.WriteToArrayFromBeginningSecondRows))
            {
                valueWriter.WriteArray(rw);
            }
            else
            {
                valueWriter.WriteObject(rw);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            var rows = content.Rows;

            int length = rows.Count;
            int i = 0;

            var canBeStopped = stopToken.CanBeStopped;

            if (canBeStopped && stopToken.PopState() is int index)
            {
                i = index;
            }

            for (; i < length; i++)
            {
                if (canBeStopped && stopToken.IsStopRequested)
                {
                    stopToken.SetState(i);

                    return;
                }

                var rw = new DataRowRW<DataRow>(content, rows[i]);

                rw.Initialize();

                if (i != 0 && options.On(DataTableRWOptions.WriteToArrayFromBeginningSecondRows))
                {
                    dataReader[i].ReadArray(rw);
                }
                else
                {
                    dataReader[i].ReadObject(rw);
                }
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            var rows = content.Rows;
            var columns = content.Columns;

            var length = rows.Count;

            var isAddNew = key == length;

            if (key == 0 && isAddNew)
            {
                var dictionary = valueReader.ReadDictionary<string, object>();

                if (dictionary is null)
                {
                    throw new InvalidOperationException("First row can't be null!");
                }

                var columnQueue = new Queue<int>(dictionary.Count);

                foreach (var item in dictionary)
                {
                    var column = columns[item.Key];

                    if (column is null)
                    {
                        var columnDataType = typeof(object);

                        if (options.On(DataTableRWOptions.SetFirstRowsTypeToColumnTypes) && item.Value is not null)
                        {
                            columnDataType = item.Value.GetType();
                        }

                        column = columns.Add(item.Key, columnDataType);
                    }

                    columnQueue.Enqueue(column.Ordinal);
                }

                var firstRow = content.NewRow();

                foreach (var item in dictionary)
                {
                    var ordinal = columnQueue.Dequeue();

                    firstRow[ordinal] = item.Value;
                }

                rows.Add(firstRow);
            }
            else
            {
                var rw = new DataRowRW<DataRow>(content, isAddNew ? content.AddNewRow() : rows[key]);

                rw.Initialize();

                if (key > 0 && options.On(DataTableRWOptions.WriteToArrayFromBeginningSecondRows))
                {
                    valueReader.ReadArray(rw);
                }
                else
                {
                    valueReader.ReadObject(rw);
                }
            }
        }

        sealed class ValueRW : BaseGenericRW<DataRow>
        {
            public readonly DataTableRW<T> BaseRW;
            public readonly int Index;

            public ValueRW(DataTableRW<T> baseRW, int index)
            {
                BaseRW = baseRW;
                Index = index;
            }

            public override DataRow? ReadValue()
            {
                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                return dataTable.Rows[Index];
            }

            public override void WriteValue(DataRow? value)
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                var isAddNew = Index == dataTable.Rows.Count;

                if (isAddNew && value.Table == dataTable)
                {
                    dataTable.Rows.Add(value);

                    return;
                }

                var rw = new DataRowRW<DataRow>(value.Table, value);

                rw.Initialize();

                WriteObject(rw);
            }

            public override void ReadArray(IDataWriter<int> valueWriter)
            {
                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                var rw = new DataRowRW<DataRow>(dataTable, dataTable.Rows[Index]);

                rw.Initialize();

                rw.OnReadAll(valueWriter);
            }

            public override void WriteArray(IDataReader<int> dataReader)
            {
                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                var isAddNew = Index == dataTable.Rows.Count;

                var rw = new DataRowRW<DataRow>(dataTable, isAddNew ? dataTable.NewRow() : dataTable.Rows[Index]);

                dataReader.OnReadAll(rw);
            }

            public override void ReadObject(IDataWriter<string> valueWriter)
            {
                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                var rw = new DataRowRW<DataRow>(dataTable, dataTable.Rows[Index]);

                rw.Initialize();

                rw.OnReadAll(valueWriter);
            }

            public override void WriteObject(IDataReader<string> dataReader)
            {
                var dataTable = BaseRW.content;

                if (dataTable is null)
                {
                    throw new NullReferenceException();
                }

                var isAddNew = Index == dataTable.Rows.Count;

                var rw = new DataRowRW<DataRow>(dataTable, isAddNew ? dataTable.AddNewRow() : dataTable.Rows[Index]);

                rw.Initialize();

                dataReader.OnReadAll(rw);
            }
        }
    }

    /// <summary>
    /// 提供 DataTable 读写器的扩展方法。
    /// </summary>
    public static class DataTableRW
    {
        /// <summary>
        /// 读取或设置 DataTableRW 默认配置项
        /// </summary>
        public static DataTableRWOptions DefaultOptions { get; set; } = DataTableRWOptions.None;

        /// <summary>
        /// 设置一个支持针对性接口的 DataTableRW 默认配置项。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <param name="options">默认配置项</param>
        public static void SetDataTableRWOptions(this ITargetableValueRWSource targetable, DataTableRWOptions options)
        {
            TargetableSetOptionsHelper<DataTableRWOptions>.SetOptions(targetable, options);
        }

        /// <summary>
        /// 获取一个支持针对性接口的 DataTableRW 默认配置项。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <returns>返回默认配置项</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DataTableRWOptions GetDataTableRWOptions(this ITargetableValueRWSource targetable)
        {
            if (TargetableSetOptionsHelper<DataTableRWOptions>.TryGetOptions(targetable, out var options))
            {
                return options;
            }

            return DefaultOptions;
        }

        /// <summary>
        /// 读取一个数据表。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <param name="options">配置项</param>
        /// <returns>返回一个数据表</returns>
        public static DataTable? ReadDataTable(this IValueReader valueReader, DataTableRWOptions options = DataTableRWOptions.None)
        {
            var rw = new DataTableRW<DataTable>(options);

            valueReader.ReadArray(rw);

            return rw.content;
        }

        /// <summary>
        /// 写入一个数据表。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="dataTable">数据表</param>
        /// <param name="options">配置项</param>
        public static void WriteDataTable(this IValueWriter valueWriter, DataTable? dataTable, DataTableRWOptions options = DataTableRWOptions.None)
        {
            if (dataTable is null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                valueWriter.WriteArray(new DataTableRW<DataTable>(options) { content = dataTable });
            }
        }

        /// <summary>
        /// 识别数据表的列类型。
        /// </summary>
        /// <param name="dataTable">要识别的数据表</param>
        /// <param name="anyType">当识别为 <see cref="object"/> 类型时的类型，默认为 <see cref="object"/></param>
        /// <returns></returns>
        public static DataTable IdentifyColumnTypes(this DataTable dataTable, Type? anyType = null)
        {
            if (anyType is null)
            {
                anyType = typeof(object);
            }

            DataTable? newDataTable = null;

            foreach (DataColumn? dataColumn in dataTable.Columns)
            {
                VersionDifferences.Assert(dataColumn != null);

                Type? dataType = null;

                foreach (DataRow? dataRow in dataTable.Rows)
                {
                    VersionDifferences.Assert(dataRow != null);

                    ChooseType(ref dataType, dataRow[dataColumn]);
                }

                if (dataType is null || IsAnyType(dataType))
                {
                    dataType = anyType;
                }

                if (newDataTable != null)
                {
                    newDataTable.Columns.Add(dataColumn.ColumnName, dataType);
                }
                else if (dataType != dataColumn.DataType)
                {
                    if (TypeHelper.IsAutoGetMethod(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.PropertyGet(typeof(DataColumn), nameof(DataColumn.DataType)))), out var fieldInfo))
                    {
                        fieldInfo.SetValue(dataColumn, dataType);

                        foreach (DataRow? dataRow in dataTable.Rows)
                        {
                            VersionDifferences.Assert(dataRow != null);

                            dataRow[dataColumn] = XConvert.Convert(dataRow[dataColumn], dataType);
                        }
                    }
                    else
                    {
                        newDataTable = new DataTable(dataTable.TableName, dataTable.Namespace);

                        newDataTable.Columns.Add(dataColumn.ColumnName, dataType);
                    }
                }
            }

            if (newDataTable != null)
            {
                foreach (DataRow? dataRow in dataTable.Rows)
                {
                    VersionDifferences.Assert(dataRow != null);

                    var newDataRow = newDataTable.NewRow();

                    foreach (DataColumn? dataColumn in newDataTable.Columns)
                    {
                        VersionDifferences.Assert(dataColumn != null);

                        newDataRow[dataColumn] = XConvert.Convert(dataRow[/* dataColumn.ColumnName */dataColumn.Ordinal], dataColumn.DataType);
                    }

                    newDataTable.Rows.Add(newDataRow);
                }

                return newDataTable;
            }

            return dataTable;

            static void ChooseType(ref Type? type, object value)
            {
                if (value != null)
                {
                    var valueType = value.GetType();

                    if (type is null)
                    {
                        type = valueType;
                    }
                    else if (!XConvert.IsImplicitConvert(valueType, type))
                    {
                        while (!XConvert.IsImplicitConvert(type, valueType))
                        {
                            valueType = valueType.BaseType!;
                        }

                        type = valueType;
                    }
                }
            }

            static bool IsAnyType(Type type)
            {
                return type == typeof(object) || type == typeof(ValueType);
            }
        }

        /// <summary>
        /// 为数据表添加一行新的行。
        /// </summary>
        /// <param name="dataTable">数据表</param>
        /// <returns>返回已经添加到数据表的新行</returns>
        public static DataRow AddNewRow(this DataTable dataTable)
        {
            var row = dataTable.NewRow();

            dataTable.Rows.Add(row);

            return row;
        }

        /// <summary>
        /// 如果值为 <see cref="DBNull.Value"/>，则返回 <see langword="null"/>。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static object? AsNullIfDBNull(this object? value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// 将源数据行复制到目标数据行。
        /// </summary>
        /// <param name="source">源数据行</param>
        /// <param name="destination">目标数据行</param>
        /// <param name="newColumnOnNotExists">当列在目标数据表中不存在时添加新的列</param>
        public static void CopyTo(this DataRow source, DataRow destination, bool newColumnOnNotExists = false)
        {
            var items = new List<KeyValuePair<int, object>>(source.Table.Columns.Count);

            foreach (DataColumn? sourceColumn in source.Table.Columns)
            {
                VersionDifferences.Assert(sourceColumn != null);

                var destinationColumn = destination.Table.Columns[sourceColumn.ColumnName];

                if (destinationColumn is null && newColumnOnNotExists)
                {
                    destinationColumn = destination.Table.Columns.Add(sourceColumn.ColumnName, sourceColumn.DataType);
                }

                if (destinationColumn is not null)
                {
                    items.Add(new KeyValuePair<int, object>(destinationColumn.Ordinal, source[sourceColumn]));
                }
            }

            foreach (var item in items)
            {
                destination[item.Key] = item.Value;
            }
        }
    }
}