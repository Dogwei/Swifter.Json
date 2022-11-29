
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class DataInterfaceMaper : IValueInterfaceMaper
    {
#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DbDataReaderInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataRowInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataTableInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataSetInterface<>))]
#endif
        public IValueInterface<T>? TryMap<T>()
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
                return (IValueInterface<T>)Activator.CreateInstance(type)!;
            }
        }
    }
}