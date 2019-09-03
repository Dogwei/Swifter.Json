
using System;
using System.Data;

namespace Swifter.RW
{
    internal sealed class DataInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            if (typeof(System.Data.IDataReader).IsAssignableFrom(type))
            {
                return CreateInstance(typeof(DbDataReaderInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataRow).IsAssignableFrom(type))
            {
                return CreateInstance(typeof(DataRowInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataTable).IsAssignableFrom(type))
            {
                return CreateInstance(typeof(DataTableInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(DataSet).IsAssignableFrom(type))
            {
                return CreateInstance(typeof(DataSetInterface<>).MakeGenericType(typeof(T)));
            }

            return null;

            static IValueInterface<T> CreateInstance(Type type)
            {
                return (IValueInterface<T>)Activator.CreateInstance(type);
            }
        }
    }
}