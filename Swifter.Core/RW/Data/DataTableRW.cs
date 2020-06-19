
using Swifter.Reflection;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataTable Reader impl.
    /// </summary>
    internal sealed class DataTableRW<T> : IDataRW<int> where T : DataTable
    {
        const int DefaultCapacity = 3;

        readonly DataTableRWOptions options;

        public T datatable;

        public DataTableRW(DataTableRWOptions options)
        {
            this.options = options;
        }

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => Enumerable.Range(0, Count);

        public int Count => datatable?.Rows.Count ?? -1;

        public object Content
        {
            get => datatable;
            set => datatable = (T)value;
        }

        public Type ContentType => typeof(T);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T) == typeof(DataTable))
            {
                datatable = Underlying.As<T>(new DataTable());
            }
            else
            {
                datatable = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var rows = datatable.Rows;

            var length = rows.Count;

            var rw = new DataRowRW<DataRow>();

            for (int i = 0; i < length; i++)
            {
                rw.datarow = rows[i];

                if (i != 0 && (options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
                {
                    dataWriter[i].WriteArray(rw);
                }
                else
                {
                    dataWriter[i].WriteObject(rw);
                }
            }
        }


        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var rows = datatable.Rows;

            var rw = new DataRowRW<DataRow>
            {
                datarow = rows[key]
            };

            if (key != 0 && (options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
            {
                valueWriter.WriteArray(rw);
            }
            else
            {
                valueWriter.WriteObject(rw);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var rows = datatable.Rows;

            var length = rows.Count;

            var rw = new DataRowRW<DataRow>();

            for (int i = 0; i < length; i++)
            {
                rw.datarow = rows[i];

                if (i != 0 && (options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
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
            var rows = datatable.Rows;
            var columns = datatable.Columns;

            var length = rows.Count;

            if (key == length && key == 0)
            {
                var dictionary = valueReader.ReadDictionary<string, object>();

                if ((options & DataTableRWOptions.SetFirstRowsTypeToColumnTypes) != 0)
                {
                    foreach (var item in dictionary)
                    {
                        columns.Add(item.Key, item.Value?.GetType() ?? typeof(object));
                    }
                }
                else
                {
                    foreach (var item in dictionary)
                    {
                        columns.Add(item.Key, typeof(object));
                    }
                }

                var datarow = datatable.NewRow();

                foreach (var item in dictionary)
                {
                    // TODO: 使用序号优化性能。
                    datarow[item.Key] = item.Value;
                }

                rows.Add(datarow);
            }
            else
            {
                var rw = new DataRowRW<DataRow>
                {
                    datarow = key == length ? datatable.NewRow() : rows[key]
                };

                if ((options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
                {
                    valueReader.ReadArray(rw);
                }
                else
                {
                    valueReader.ReadObject(rw);
                }

                if (key == length)
                {
                    rows.Add(rw.datarow);
                }
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

        static readonly XFieldInfo DataColumnDataTypeFieldInfo = GetDataColumnDataTypeFieldInfo();

        static XFieldInfo GetDataColumnDataTypeFieldInfo()
        {
            try
            {
                if (TypeHelper.IsAutoGetMethod(typeof(DataColumn).GetProperty(nameof(DataColumn.DataType)).GetGetMethod(true), out var fieldMetadataToken) 
                    && TypeHelper.GetMemberByMetadataToken<FieldInfo>(typeof(DataColumn), fieldMetadataToken) is FieldInfo fieldInfo)
                {
                    return XFieldInfo.Create(fieldInfo, XBindingFlags.NonPublic);
                }

                var dataColumn = new DataColumn(nameof(GetDataColumnDataTypeFieldInfo), typeof(DataTableRWOptions));

                foreach (var field in typeof(DataColumn).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.FieldType == typeof(Type) && typeof(DataTableRWOptions).Equals(field.GetValue(dataColumn)))
                    {
                        field.SetValue(dataColumn, typeof(int));

                        if (dataColumn.DataType == typeof(int))
                        {
                            return XFieldInfo.Create(field, XBindingFlags.NonPublic);
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// 设置一个支持针对性接口的 DataTableRW 默认配置项。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="options">默认配置项</param>
        public static void SetDataTableRWOptions(this ITargetedBind targeted, DataTableRWOptions options)
        {
            ValueInterface<SetDataTableRWOptionsAssistant>.SetTargetedInterface(targeted, new SetDataTableRWOptionsAssistant(options));
        }

        /// <summary>
        /// 读取一个数据表。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <param name="options">配置项</param>
        /// <returns>返回一个数据表</returns>
        public static DataTable ReadDataTable(this IValueReader valueReader, DataTableRWOptions options = DataTableRWOptions.None)
        {
            var rw = new DataTableRW<DataTable>(options);

            valueReader.ReadArray(rw);

            return rw.datatable;
        }

        /// <summary>
        /// 写入一个数据表。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="dataTable">数据表</param>
        /// <param name="options">配置项</param>
        public static void WriteDataTable(this IValueWriter valueWriter, DataTable dataTable, DataTableRWOptions options = DataTableRWOptions.None)
        {
            valueWriter.WriteArray(new DataTableRW<DataTable>(options) { datatable = dataTable });
        }



        /// <summary>
        /// 识别数据表的列类型。
        /// </summary>
        /// <param name="dataTable">要识别的数据表</param>
        /// <param name="anyType">当识别为 <see cref="object"/> 类型时的类型，默认为 <see cref="object"/></param>
        /// <returns></returns>
        public static DataTable IdentifyColumnTypes(this DataTable dataTable, Type anyType = null)
        {
            if (anyType is null)
            {
                anyType = typeof(object);
            }

            DataTable newDataTable = null;

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                Type dataType = null;

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    ChooseType(ref dataType, dataRow[dataColumn]);
                }

                if (IsAnyType(dataType))
                {
                    dataType = anyType;
                }

                if (newDataTable != null)
                {
                    newDataTable.Columns.Add(dataColumn.ColumnName, dataType);
                }
                else if (dataType != dataColumn.DataType)
                {
                    if (DataColumnDataTypeFieldInfo != null)
                    {
                        DataColumnDataTypeFieldInfo.SetValue(dataColumn, dataType);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            dataRow[dataColumn] = XConvert.Cast(dataRow[dataColumn], dataType);
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
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    var newDataRow = newDataTable.NewRow();

                    foreach (DataColumn dataColumn in newDataTable.Columns)
                    {
                        newDataRow[dataColumn] = XConvert.Cast(dataRow[/* dataColumn.ColumnName */dataColumn.Ordinal], dataColumn.DataType);
                    }

                    newDataTable.Rows.Add(newDataRow);
                }

                return newDataTable;
            }

            return dataTable;

            static void ChooseType(ref Type type, object value)
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
                            valueType = valueType.BaseType;
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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static DataTableRWOptions GetDataTableRWOptions(object valueRW)
        {
            if (valueRW is ITargetedBind targeted && ValueInterface<SetDataTableRWOptionsAssistant>.GetTargetedInterface(targeted) is SetDataTableRWOptionsAssistant assistant)
            {
                return assistant.Options;
            }

            return DefaultOptions;
        }

        sealed class SetDataTableRWOptionsAssistant : IValueInterface<SetDataTableRWOptionsAssistant>
        {
            public readonly DataTableRWOptions Options;

            public SetDataTableRWOptionsAssistant(DataTableRWOptions options) => Options = options;

            public SetDataTableRWOptionsAssistant ReadValue(IValueReader valueReader) => throw new NotSupportedException();

            public void WriteValue(IValueWriter valueWriter, SetDataTableRWOptionsAssistant value) => throw new NotSupportedException();
        }
    }
}