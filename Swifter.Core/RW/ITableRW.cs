using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Data;
using System.Data.Common;

namespace Swifter.RW
{
    /// <summary>
    /// 表格数据读写器。
    /// </summary>
    public interface ITableRW : ITableReader, ITableWriter, IDataRW<string>, IDataRW<int>
    {
    }

    internal sealed class TableRWInternalMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            if (typeof(DbDataReader).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DbDataReaderInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataRow).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DataRowInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataTable).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DataTableInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataSet).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DataSetInterface<>).MakeGenericType(typeof(T)));
            }

            return null;
        }
    }
}