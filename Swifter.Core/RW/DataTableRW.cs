using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using static Swifter.RW.DataTableRW;
using IDataReader = Swifter.Readers.IDataReader;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataTable Reader impl.
    /// </summary>
    internal sealed class DataTableRW<T> : IDataRW<int>, IInitialize<T>, IDirectContent where T : DataTable
    {
        readonly DataTableRWOptions Options;

        readonly DataRowRW<DataRow> DataRowRW;

        T DataTable;

        bool Initialized;

        public DataTableRW(DataTableRWOptions options)
        {
            DataRowRW = new DataRowRW<DataRow>((options & DataTableRWOptions.SetFirstRowsTypeToColumnTypes) != 0);

            Options = options;
        }

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => Content.Columns.Count;

        object IDataReader.ReferenceToken => Content;

        object IDirectContent.DirectContent
        {
            get
            {
                return Content;
            }
            set
            {
                Initialize((T)value);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Fill()
        {
            if (DataTable != null)
            {
                return;
            }

            var firstRow = DataRowRW.Content;

            if (typeof(T) == typeof(DataTable))
            {
                if (firstRow == null)
                {
                    DataTable = Unsafe.As<T>(new DataTable());
                }
                else
                {
                    DataTable = Unsafe.As<T>(firstRow.Table);

                    DataTable.Rows.Add(firstRow);
                }
            }
            else
            {
                DataTable = Activator.CreateInstance<T>();

                if (firstRow != null)
                {
                    DataTable.ImportRow(firstRow);
                }
            }
        }

        public T Content
        {
            get
            {
                if (DataTable == null && Initialized)
                {
                    Fill();
                }

                return DataTable;
            }
        }

        public void Initialize()
        {
            DataRowRW.Clear();

            DataTable = null;

            Initialized = true;
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void Initialize(T obj)
        {
            DataRowRW.Clear();

            DataTable = obj;

            Initialized = true;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            var length = Content.Rows.Count;

            for (int i = 0; i < length; i++)
            {
                DataRowRW.Initialize(Content.Rows[i]);

                if (i != 0 && (Options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
                {
                    dataWriter[index].WriteArray(DataRowRW);
                }
                else
                {
                    dataWriter[index].WriteObject(DataRowRW);
                }
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            OnReadAll(new DataFilterWriter<int>(dataWriter, valueFilter));
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            DataRowRW<DataRow> reader;

            if (valueWriter is IUsePool)
            {
                reader = DataRowRW;
            }
            else
            {
                reader = new DataRowRW<DataRow>();
            }


            reader.Initialize(Content.Rows[key]);

            if (key != 0 && (Options & DataTableRWOptions.WriteToArrayFromBeginningSecondRows) != 0)
            {
                valueWriter.WriteArray(reader);
            }
            else
            {
                valueWriter.WriteObject(reader);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var index = 0;

            foreach (DataRow item in Content.Rows)
            {
                DataRowRW.Initialize(item);

                dataReader[index].ReadObject(DataRowRW);

                ++index;
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (DataTable == null && Initialized && key == 0)
            {
                DataRowRW.Clear();

                valueReader.ReadObject(DataRowRW);

                Fill();

                return;
            }

            DataRowRW<DataRow> writer;

            if (valueReader is IUsePool)
            {
                writer = DataRowRW;
            }
            else
            {
                writer = new DataRowRW<DataRow>();
            }

            writer.Clear();

            if (key == DataTable.Rows.Count)
            {
                writer.Initialize(DataTable.NewRow());

                valueReader.ReadObject(writer);

                DataTable.Rows.Add(writer.Content);
            }
            else
            {
                writer.Initialize(DataTable.Rows[key]);

                valueReader.ReadObject(writer);
            }
        }
    }

    /// <summary>
    /// DataTable 读写器的配置。
    /// </summary>
    public enum DataTableRWOptions
    {
        /// <summary>
        /// 默认配置项。
        /// </summary>
        None = 0,

        /// <summary>
        /// 设置第一行的数据类型为各个 Column 的类型。否则将设置 Object 为各个 Column 的类型。默认不开启。
        /// </summary>
        SetFirstRowsTypeToColumnTypes = 1,

        /// <summary>
        /// 设置第二行开始的数据写入为数组。
        /// </summary>
        WriteToArrayFromBeginningSecondRows = 2
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
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="options">默认配置项</param>
        public static void SetDataTableRWOptions(this ITargetedBind targeted, DataTableRWOptions options)
        {
            ValueInterface<XAssistant>.SetTargetedInterface(targeted, new XAssistant(options));
        }

        /// <summary>
        /// 获取一个支持针对性接口的 DataTableRW 默认配置项。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <returns>默认配置项</returns>
        public static DataTableRWOptions GetDataTableRWOptions(this ITargetedBind targeted)
        {
            return ValueInterface<XAssistant>.GetTargetedInterface(targeted) is XAssistant assistant ? assistant.Options : DefaultOptions;
        }

        sealed class XAssistant : IValueInterface<XAssistant>
        {
            public readonly DataTableRWOptions Options;

            public XAssistant(DataTableRWOptions options)
            {
                Options = options;
            }

            public XAssistant ReadValue(IValueReader valueReader) => default;

            public void WriteValue(IValueWriter valueWriter, XAssistant value)
            {
            }
        }
    }


    internal sealed class DataTableInterface<T> : IValueInterface<T> where T : DataTable
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var writer = new DataTableRW<T>(valueReader is ITargetedBind targeted && targeted.TargetedId != 0 ? targeted.GetDataTableRWOptions() : DefaultOptions);

            valueReader.ReadArray(writer);

            return writer.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> weiter)
            {
                weiter.WriteValue(value);

                return;
            }

            var reader = new DataTableRW<T>(valueWriter is ITargetedBind targeted && targeted.TargetedId != 0 ? targeted.GetDataTableRWOptions() : DefaultOptions);

            reader.Initialize(value);

            valueWriter.WriteArray(reader);
        }
    }
}