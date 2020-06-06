using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 读写器帮助类。
    /// </summary>
    public static class RWHelper
    {
        /// <summary>
        /// 获取一个所有方法均为获取 default 值的值读取器。
        /// </summary>
        public static readonly IValueReader DefaultValueReader = new DefaultValueReader();

        /// <summary>
        /// 获取一个所有方法均为获取 default 值或空实现的值读写器。
        /// </summary>
        public static readonly IValueRW DefaultValueRW = new AuxiliaryValueRW();

        /// <summary>
        /// 为实例创建读取器。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="throwException">是否抛出异常</param>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader CreateReader<T>(T obj, bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.WriteValue(auxiliary, obj);

            return auxiliary.GetDataReader() ??
                (throwException ? throw new NotSupportedException($"Unable create data reader of '{typeof(T)}'.") : default(IDataReader));
        }

        /// <summary>
        /// 为实例创建读写器。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="throwException">是否抛出异常</param>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW CreateRW<T>(T obj, bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.WriteValue(auxiliary, obj);

            return auxiliary.GetDataRW() ??
                (throwException ? throw new NotSupportedException($"Unable create data reader-writer of '{typeof(T)}'.") : default(IDataRW));
        }

        /// <summary>
        /// 为类型创建一个写入器。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter CreateWriter<T>(bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.ReadValue(auxiliary);

            return auxiliary.GetDataWriter() ??
                (throwException ? throw new NotSupportedException($"Unable create data writer of '{typeof(T)}'.") : default(IDataWriter));
        }

        /// <summary>
        /// 为一个实例创建数据写入器。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="throwException">是否抛出异常</param>
        /// <param name="obj">实例</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter CreateWriter<T>(T obj, bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            try
            {
                ValueInterface<T>.WriteValue(auxiliary, obj);
            }
            catch
            {
            }

            var writer = auxiliary.GetDataWriter();

            if (writer != null)
            {
                return writer;
            }

            ValueInterface<T>.ReadValue(auxiliary);

            writer = auxiliary.GetDataWriter();

            if (writer is null)
            {
                return auxiliary.GetDataWriter() ??
                    (throwException ? throw new NotSupportedException($"Unable create data writer of  '{typeof(T)}'.") : default(IDataWriter));
            }

            writer.Content = obj;

            return writer;
        }

        /// <summary>
        /// 为实例创建一个读取器。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader CreateReader(object obj, bool throwException = true)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataReader() ??
                (throwException ? throw new NotSupportedException($"Unable create data reader of '{obj.GetType()}'.") : default(IDataReader));
        }

        /// <summary>
        /// 为实例创建一个读写器。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW CreateRW(object obj, bool throwException = true)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataRW() ??
                (throwException ? throw new NotSupportedException($"Unable create data reader-writer of '{obj.GetType()}'.") : default(IDataRW));
        }

        /// <summary>
        /// 为类型创建一个写入器。
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter CreateWriter(Type type, bool throwException = true)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(type).Read(auxiliary);

            return auxiliary.GetDataWriter() ?? (throwException ? throw new NotSupportedException($"Unable create data writer of '{type}'.") : default(IDataRW));
        }

        /// <summary>
        /// 为一个实例创建数据写入器。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter CreateWriter(object obj, bool throwException = true)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var auxiliary = new AuxiliaryValueRW();

            var @interface = ValueInterface.GetInterface(obj);

            @interface.Write(auxiliary, obj);

            var writer = auxiliary.GetDataWriter();

            if (writer != null)
            {
                return writer;
            }

            @interface.Read(auxiliary);

            writer = auxiliary.GetDataWriter();

            if (writer is null)
            {
                return auxiliary.GetDataWriter() ??
                    (throwException ? throw new NotSupportedException($"Unable create data writer of  '{obj.GetType()}'.") : default(IDataWriter));
            }

            writer.Content = obj;

            return writer;
        }

        /// <summary>
        /// 为读取器中的字段创建数据读取器。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="key">字段的键</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader CreateItemReader<TKey>(IDataReader<TKey> dataReader, TKey key, bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            dataReader.OnReadValue(key, auxiliary);

            return auxiliary.GetDataReader() ??
                (throwException ? throw new NotSupportedException($"Failed to create data reader of field : '{key}' in {dataReader}.") : default(IDataReader));
        }

        /// <summary>
        /// 为读取器中的字段创建数据读写器。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="key">字段的键</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW CreateItemRW<TKey>(IDataReader<TKey> dataReader, TKey key, bool throwException = true)
        {
            var auxiliary = new AuxiliaryValueRW();

            dataReader.OnReadValue(key, auxiliary);

            return auxiliary.GetDataRW() ??
                (throwException ? throw new NotSupportedException($"Failed to create data reader-writer of field : '{key}' in {dataReader}.") : default(IDataRW));
        }


        /// <summary>
        /// 获取数据读写器中指定键的值的类型。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="dataRW">数据读写器</param>
        /// <param name="key">指定键</param>
        /// <returns>返回一个类型</returns>
        public static Type GetItemType<TKey>(IDataRW<TKey> dataRW, TKey key)
        {
            var auxiliary = new AuxiliaryValueRW();

            dataRW.OnReadValue(key, auxiliary);

            return auxiliary.GetValueType();
        }

        /// <summary>
        /// 获取数据读取器中指定键的值的类型。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="key">指定键</param>
        /// <returns>返回一个类型</returns>
        public static Type GetItemType<TKey>(IDataReader<TKey> dataReader, TKey key)
        {
            var auxiliary = new AuxiliaryValueRW();

            dataReader.OnReadValue(key, auxiliary);

            return auxiliary.GetValueType();
        }

        /// <summary>
        /// 获取数据写入器中指定键的值的类型。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="key">指定键</param>
        /// <returns>返回一个类型</returns>
        public static Type GetItemType<TKey>(IDataWriter<TKey> dataWriter, TKey key)
        {
            var auxiliary = new AuxiliaryValueRW();

            dataWriter.OnWriteValue(key, auxiliary);

            return auxiliary.GetValueType();
        }

        /// <summary>
        /// 将数据读取器转换为具有键的类型的具体数据读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据读取器</returns>
        public static IDataReader<T> As<T>(this IDataReader dataReader)
        {
            if (dataReader is IDataReader<T> tReader)
            {
                return tReader;
            }

            if (dataReader is IAsDataReader @asReader)
            {
                return As<T>(@asReader.Original);
            }

            if (dataReader is IDataReader<string> strReader)
            {
                return new AsDataReader<string, T>(strReader);
            }

            if (dataReader is IDataReader<int> intReader)
            {
                return new AsDataReader<int, T>(intReader);
            }

            if (AsHelper.GetInstance(dataReader) is AsHelper asHelper)
            {
                return asHelper.As<T>(dataReader);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataWriter<T> As<T>(this IDataWriter dataWriter)
        {
            if (dataWriter is IDataWriter<T> tWriter)
            {
                return tWriter;
            }

            if (dataWriter is IAsDataWriter asWriter)
            {
                return As<T>(asWriter.Original);
            }

            if (dataWriter is IDataWriter<string> strWriter)
            {
                return new AsDataWriter<string, T>(strWriter);
            }

            if (dataWriter is IDataWriter<int> intWriter)
            {
                return new AsDataWriter<int, T>(intWriter);
            }

            if (AsHelper.GetInstance(dataWriter) is AsHelper asHelper)
            {
                return asHelper.As<T>(dataWriter);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataRW">数据写入器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataRW<T> As<T>(this IDataRW dataRW)
        {
            if (dataRW is IDataRW<T> tRW)
            {
                return tRW;
            }

            if (dataRW is IAsDataRW asRW)
            {
                return As<T>(asRW.Original);
            }

            if (dataRW is IDataRW<string> strRW)
            {
                return new AsDataRW<string, T>(strRW);
            }

            if (dataRW is IDataRW<int> intRW)
            {
                return new AsDataRW<int, T>(intRW);
            }

            if (AsHelper.GetInstance(dataRW) is AsHelper asHelper)
            {
                return asHelper.As<T>(dataRW);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 获取一个 键类型 是否可作为数组索引。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsArrayKey<TKey>()
        {
            if (typeof(TKey).IsEnum)
            {
                return false;
            }

            switch (Type.GetTypeCode(typeof(TKey)))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 从值读取器中读取一个字典。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个字典</returns>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this IValueReader valueReader)
        {
            var rw = new DictionaryRW<Dictionary<TKey, TValue>, TKey, TValue>();

            valueReader.ReadObject(rw.As<string>());

            return rw.content;
        }

        /// <summary>
        /// 往值写入器中写入一个字典。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueWriter">值读取器</param>
        /// <param name="dictionary">字典</param>
        public static void WriteDictionary<TKey, TValue>(this IValueWriter valueWriter, Dictionary<TKey, TValue> dictionary)
        {
            valueWriter.WriteObject(new DictionaryRW<Dictionary<TKey, TValue>, TKey, TValue>
            {
                content = dictionary
            }.As<string>());
        }

        /// <summary>
        /// 从值读取器中读取一个列表。
        /// </summary>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个列表</returns>
        public static List<TValue> ReadList<TValue>(this IValueReader valueReader)
        {
            var rw = new ListRW<List<TValue>, TValue>();

            valueReader.ReadArray(rw);

            return rw.content;
        }

        /// <summary>
        /// 往值写入器中写入一个列表。
        /// </summary>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueWriter">值读取器</param>
        /// <param name="list">列表</param>
        public static void WriteList<TValue>(this IValueWriter valueWriter, List<TValue> list)
        {
            valueWriter.WriteArray(new ListRW<List<TValue>, TValue>
            {
                content = list
            });
        }

        /// <summary>
        /// 从值读取器中读取一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个数组</returns>
        public static T[] ReadArray<T>(this IValueReader valueReader)
        {
            var rw = new ArrayRW<T>();

            valueReader.ReadArray(rw);

            return rw.GetContent();
        }

        /// <summary>
        /// 往值写入器中写入一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="array">数组</param>
        public static void WriteArray<T>(this IValueWriter valueWriter, T[] array)
        {
            valueWriter.WriteArray(new ArrayRW<T>(array));
        }

        /// <summary>
        /// 使用 <see cref="FastObjectRW{T}"/> 从值读取器中读取一个对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个对象</returns>
        public static T FastReadObject<T>(this IValueReader valueReader)
        {
            var rw = FastObjectRW<T>.Create();

            valueReader.ReadObject(rw);

            return rw.content;
        }

        /// <summary>
        /// 使用 <see cref="FastObjectRW{T}"/> 往值写入器中写入一个对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="obj">对象</param>
        public static void FastWriteObject<T>(this IValueWriter valueWriter, T obj)
        {
            var rw = FastObjectRW<T>.Create();

            rw.content = obj;

            valueWriter.WriteObject(rw);
        }

        /// <summary>
        /// 使用 <see cref="XObjectRW"/> 从值读取器中读取一个对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <param name="flags">创建 <see cref="XObjectRW"/> 的标识符</param>
        /// <returns>返回对象</returns>
        public static T XReadObject<T>(this IValueReader valueReader, XBindingFlags flags = XBindingFlags.UseDefault)
        {
            var rw = XObjectRW.Create<T>(flags);

            valueReader.ReadObject(rw);

            return (T)rw.Content;
        }

        /// <summary>
        /// 使用 <see cref="XObjectRW"/> 往值写入器中写入一个对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="obj">对象</param>
        /// <param name="flags">创建 <see cref="XObjectRW"/> 的标识符</param>
        public static void XWriteObject<T>(this IValueWriter valueWriter, T obj, XBindingFlags flags = XBindingFlags.UseDefault)
        {
            var rw = XObjectRW.Create<T>(flags);

            rw.Content = obj;

            valueWriter.WriteObject(rw);
        }
    }
}