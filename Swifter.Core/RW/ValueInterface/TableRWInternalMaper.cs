using Swifter.Readers;
using System;
using System.Data;

namespace Swifter.RW
{
    internal sealed class TableRWInternalMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            if (typeof(System.Data.IDataReader).IsAssignableFrom(type))
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