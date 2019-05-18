using Swifter.Readers;
using System;
using System.Collections.Generic;

namespace Swifter.Writers
{
    /// <summary>
    /// 数据筛选的辅助数据写入器。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public sealed class DataFilterWriter<TKey> : IDataWriter<TKey>, IValueWriter
    {
        readonly ValueFilterInfo<TKey> ValueInfo;
        readonly IDataWriter<TKey> dataWriter;
        readonly IValueFilter<TKey> ValueFilter;

        /// <summary>
        /// 初始化辅助数据写入器。
        /// </summary>
        /// <param name="dataWriter">原始数据写入器</param>
        /// <param name="valueFilter">数据筛选器</param>
        public DataFilterWriter(IDataWriter<TKey> dataWriter, IValueFilter<TKey> valueFilter)
        {
            this.dataWriter = dataWriter;
            ValueFilter = valueFilter;

            ValueInfo = new ValueFilterInfo<TKey>();
        }

        /// <summary>
        /// 获取指定键对应的值写入器。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>返回值写入器</returns>
        public IValueWriter this[TKey key]
        {
            get
            {
                ValueInfo.Key = key;

                return this;
            }
        }

        /// <summary>
        /// 获取原始数据写入器的键集合。
        /// </summary>
        public IEnumerable<TKey> Keys => dataWriter.Keys;

        /// <summary>
        /// 获取原始数据写入器的键的数量。
        /// </summary>
        public int Count => dataWriter.Count;

        /// <summary>
        /// 初始化原始写入器。
        /// </summary>
        public void Initialize()
        {
            dataWriter.Initialize();
        }

        /// <summary>
        /// 初始化具有指定容量的原始写入器。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        public void Initialize(int capacity)
        {
            dataWriter.Initialize(capacity);
        }

        /// <summary>
        /// 从值读取器中读取一个值设置到原始写入器的指定键的值中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(TKey key, IValueReader valueReader)
        {
            dataWriter.OnWriteValue(key, valueReader);
        }

        private void OnFilter()
        {
            if (ValueFilter.Filter(ValueInfo))
            {
                ValueInfo.ValueCopyer.WriteTo(dataWriter[ValueInfo.Key]);
            }
        }

        void IValueWriter.DirectWrite(object value)
        {
            ValueInfo.ValueCopyer.DirectWrite(value);

            OnFilter();
        }

        void IValueWriter.WriteArray(IDataReader<int> dataReader)
        {
            ValueInfo.ValueCopyer.WriteArray(dataReader);

            OnFilter();
        }

        void IValueWriter.WriteBoolean(bool value)
        {
            ValueInfo.ValueCopyer.WriteBoolean(value);

            OnFilter();
        }

        void IValueWriter.WriteByte(byte value)
        {
            ValueInfo.ValueCopyer.WriteByte(value);

            OnFilter();
        }

        void IValueWriter.WriteChar(char value)
        {
            ValueInfo.ValueCopyer.WriteChar(value);

            OnFilter();
        }

        void IValueWriter.WriteDateTime(DateTime value)
        {
            ValueInfo.ValueCopyer.WriteDateTime(value);

            OnFilter();
        }

        void IValueWriter.WriteDecimal(decimal value)
        {
            ValueInfo.ValueCopyer.WriteDecimal(value);

            OnFilter();
        }

        void IValueWriter.WriteDouble(double value)
        {
            ValueInfo.ValueCopyer.WriteDouble(value);

            OnFilter();
        }

        void IValueWriter.WriteInt16(short value)
        {
            ValueInfo.ValueCopyer.WriteInt16(value);

            OnFilter();
        }

        void IValueWriter.WriteInt32(int value)
        {
            ValueInfo.ValueCopyer.WriteInt32(value);

            OnFilter();
        }

        void IValueWriter.WriteInt64(long value)
        {
            ValueInfo.ValueCopyer.WriteInt64(value);

            OnFilter();
        }

        void IValueWriter.WriteObject(IDataReader<string> dataReader)
        {
            ValueInfo.ValueCopyer.WriteObject(dataReader);

            OnFilter();
        }

        void IValueWriter.WriteSByte(sbyte value)
        {
            ValueInfo.ValueCopyer.WriteSByte(value);

            OnFilter();
        }

        void IValueWriter.WriteSingle(float value)
        {
            ValueInfo.ValueCopyer.WriteSingle(value);

            OnFilter();
        }

        void IValueWriter.WriteString(string value)
        {
            ValueInfo.ValueCopyer.WriteString(value);

            OnFilter();
        }

        void IValueWriter.WriteUInt16(ushort value)
        {
            ValueInfo.ValueCopyer.WriteUInt16(value);

            OnFilter();
        }

        void IValueWriter.WriteUInt32(uint value)
        {
            ValueInfo.ValueCopyer.WriteUInt32(value);

            OnFilter();
        }

        void IValueWriter.WriteUInt64(ulong value)
        {
            ValueInfo.ValueCopyer.WriteUInt64(value);

            OnFilter();
        }

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<TKey> dataReader)
        {
            dataWriter.OnWriteAll(dataReader);
        }
    }
}
