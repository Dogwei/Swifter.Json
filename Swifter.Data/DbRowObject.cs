using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Swifter.Data
{
    /// <summary>
    /// 表示一个数据库结果集的行对象。
    /// </summary>
    public sealed partial class DbRowObject : IDataReader<string>
    {
        internal static DbRowObjectMap CreateMap(DbDataReader dbDataReader)
        {
            var map = new DbRowObjectMap { Capacity = dbDataReader.FieldCount };

            for (int i = 0; i < dbDataReader.FieldCount; i++)
            {
                map.Add(dbDataReader.GetName(i), RW.ValueInterface.GetInterface(dbDataReader.GetFieldType(i)));
            }

            return map;
        }

        internal static DbRowObject ValueOf(DbDataReader dbDataReader, DbRowObjectMap dbRowObjectMap)
        {
            var values = new object[dbRowObjectMap.Count];

            for (int i = 0; i < values.Length; i++)
            {
                var value = dbDataReader[i];

                if (value == DBNull.Value)
                {
                    value = null;
                }

                values[i] = value;
            }

            return new DbRowObject(dbRowObjectMap, values);
        }

        internal DbRowObjectMap Map;

        internal object[] Values;

        internal DbRowObject(DbRowObjectMap map, object[] values)
        {
            Map = map;

            Values = values;
        }

        /// <summary>
        /// 尝试获取指定列名的值。
        /// </summary>
        /// <param name="name">指定列名</param>
        /// <param name="value">返回值</param>
        /// <returns>返回是否存在该列名</returns>
        public bool TryGetValue(string name, out object value)
        {
            var index = Map.FindIndex(name);

            if (index >= 0)
            {
                value = Values[index];

                return true;
            }

            value = null;

            return false;
        }

        /// <summary>
        /// 获取指定列名的索引，不存在返回 -1。
        /// </summary>
        /// <param name="name">列名</param>
        /// <returns>返回索引</returns>
        public int GetOrdinal(string name)
        {
            return Map.FindIndex(name);
        }

        /// <summary>
        /// 获取指定索引处的列的类型。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回列的类型</returns>
        public Type GetColumnType(int index)
        {
            return Map[index].Value.Type;
        }

        /// <summary>
        /// 获取指定列名的值引用。
        /// </summary>
        /// <param name="name">指定列名</param>
        /// <returns>返回该值的引用</returns>
        public object this[string name]
        {
            get
            {
                return this[Map.FindIndex(name)];
            }
            set
            {
                this[Map.FindIndex(name)] = value;
            }
        }

        /// <summary>
        /// 获取指定索引处的值引用。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回该值的引用</returns>
        public object this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = DbHelper.ToValue(value, Map[index].Value.Type);
            }
        }

        /// <summary>
        /// 结果集的列名集合。
        /// </summary>
        public IEnumerable<string> Keys => Map.Keys;

        /// <summary>
        /// 结果集中列数量。
        /// </summary>
        public int Count => Map.Count;

        Type IDataReader.ContentType => typeof(DbRowObject);

        object IDataReader.Content
        {
            get => this;
            set => throw new NotSupportedException();
        }

        IValueReader IDataReader<string>.this[string key]
        {
            get
            {
                var index = Map.FindIndex(key);

                if (index >= 0)
                {
                    return ValueCopyer.ValueOf(Values[index]);
                }

                return null;
            }
        }

        void IDataReader<string>.OnReadValue(string key, IValueWriter valueWriter)
        {
            RW.ValueInterface.WriteValue(valueWriter, Values[Map.FindIndex(key)]);
        }

        void IDataReader<string>.OnReadAll(IDataWriter<string> dataWriter)
        {
            if ((Map.flags & DbRowsFlags.SkipDefault) != 0)
            {
                for (int i = 0; i < Map.Count; i++)
                {
                    var key = Map[i].Key;
                    var value = Values[i];

                    if (!TypeHelper.IsEmptyValue(value))
                    {
                        var valueInterface = Map[i].Value;

                        valueInterface.Write(dataWriter[key], value);
                    }
                }
            }
            else if ((Map.flags & DbRowsFlags.SkipNull) != 0)
            {
                for (int i = 0; i < Map.Count; i++)
                {
                    var key = Map[i].Key;
                    var value = Values[i];

                    if (value != null)
                    {
                        var valueInterface = Map[i].Value;

                        valueInterface.Write(dataWriter[key], value);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Map.Count; i++)
                {
                    var key = Map[i].Key;
                    var value = Values[i];

                    if (value is null)
                    {
                        dataWriter[key].DirectWrite(null);
                    }
                    else
                    {
                        var valueInterface = Map[i].Value;

                        valueInterface.Write(dataWriter[key], value);
                    }
                }
            }
        }

        sealed class ValueInterface : IValueInterface<DbRowObject>
        {
            public DbRowObject ReadValue(IValueReader valueReader)
            {
                if (valueReader is IValueReader<DbRowObject> reader)
                {
                    return reader.ReadValue();
                }

                throw new NotSupportedException();
            }

            public void WriteValue(IValueWriter valueWriter, DbRowObject value)
            {
                valueWriter.WriteObject(value);
            }
        }
    }
}