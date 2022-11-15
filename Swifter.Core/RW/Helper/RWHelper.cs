using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 读写器帮助类。
    /// </summary>
    public static class RWHelper
    {
        /// <summary>
        /// 获取一个所有方法均为获取 <see langword="default"/> 值或空实现的值读写器。
        /// </summary>
        public static readonly IValueRW DefaultValueRW = new AuxiliaryValueRW();

        /// <summary>
        /// 为实例创建读取器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader? CreateReader<T>(T obj)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.WriteValue(auxiliary, obj);

            return auxiliary.GetDataReader();
        }

        /// <summary>
        /// 为实例创建读写器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW? CreateRW<T>(T obj)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.WriteValue(auxiliary, obj);

            return auxiliary.GetDataRW();
        }

        /// <summary>
        /// 为类型创建一个写入器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter? CreateWriter<T>()
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.ReadValue(auxiliary);

            return auxiliary.GetDataWriter();
        }

        /// <summary>
        /// 为一个实例创建数据写入器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="obj">实例</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter? CreateWriter<T>(T obj)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface<T>.WriteValue(auxiliary, obj);

            var writer = auxiliary.GetDataWriter();

            if (writer != null)
            {
                return writer;
            }

            ValueInterface<T>.ReadValue(auxiliary);

            writer = auxiliary.GetDataWriter();

            if (writer != null)
            {
                writer.Content = obj;

                return writer;
            }

            return null;
        }

        /// <summary>
        /// 为实例创建一个读取器。无法创建则返回 null。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader? CreateReader(object obj)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataReader();
        }

        /// <summary>
        /// 为实例创建一个读写器。无法创建则返回 null。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW? CreateRW(object obj)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataRW();
        }

        /// <summary>
        /// 为类型创建一个写入器。无法创建则返回 null。
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter? CreateWriter(Type type)
        {
            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(type).Read(auxiliary);

            return auxiliary.GetDataWriter();
        }

        /// <summary>
        /// 为一个实例创建数据写入器。无法创建则返回 null。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter? CreateWriter(object obj)
        {
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

            if (writer != null)
            {
                writer.Content = obj;

                return writer;
            }

            return null;
        }

        /// <summary>
        /// 为读取器中的字段创建数据读取器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="key">字段的键</param>
        /// <returns>返回一个读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataReader? CreateItemReader<TKey>(IDataReader<TKey> dataReader, TKey key) where TKey : notnull
        {
            var auxiliary = new AuxiliaryValueRW();

            dataReader.OnReadValue(key, auxiliary);

            return auxiliary.GetDataReader();
        }

        /// <summary>
        /// 为写入器中的字段创建数据写入器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <param name="dataWriter">数据读取器</param>
        /// <param name="key">字段的键</param>
        /// <returns>返回一个写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataWriter? CreateItemWriter<TKey>(IDataWriter<TKey> dataWriter, TKey key) where TKey : notnull
        {
            var dataReader = dataWriter as IDataReader;

            if (dataReader is null && dataWriter.ContentType is not null && dataWriter.Content is object content)
            {
                dataReader = CreateReader(content);
            }

            if (dataReader is null)
            {
                return null;
            }

            var auxiliary = new AuxiliaryValueRW();

            dataReader.As<TKey>().OnReadValue(key, auxiliary);

            return auxiliary.GetDataWriter();
        }

        /// <summary>
        /// 为读取器中的字段创建数据读写器。无法创建则返回 null。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="key">字段的键</param>
        /// <returns>返回一个读写器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IDataRW? CreateItemRW<TKey>(IDataReader<TKey> dataReader, TKey key) where TKey : notnull
        {
            var auxiliary = new AuxiliaryValueRW();

            dataReader.OnReadValue(key, auxiliary);

            return auxiliary.GetDataRW();
        }


        /// <summary>
        /// 尝试设置数据写入器的数据源。
        /// </summary>
        /// <param name="dataWriter">对象写入器</param>
        /// <param name="content">数据源</param>
        internal static bool TrySetContent(this IDataWriter dataWriter, object content)
        {
            if (dataWriter.ContentType is Type contentType)
            {
                if (contentType.IsInstanceOfType(content))
                {
                    dataWriter.Content = content;

                    return true;
                }
                else if (XConvert.IsEffectiveConvert(content.GetType(), contentType))
                {
                    dataWriter.Content = XConvert.Convert(content, contentType);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试将数据读取器的数据源复制到数据写入器。
        /// </summary>
        /// <param name="destinationWriter">对象写入器</param>
        /// <param name="sourceReader">对象读取器</param>
        internal static bool TryCopyFromContent(this IDataWriter destinationWriter, IDataReader sourceReader)
        {
            if (destinationWriter.ContentType is Type destinationContentType && sourceReader.ContentType is Type sourceContentType)
            {
                if (destinationContentType.IsAssignableFrom(sourceContentType))
                {
                    destinationWriter.Content = sourceReader.Content;

                    return true;
                }
                else if (XConvert.IsEffectiveConvert(sourceContentType, destinationContentType))
                {
                    destinationWriter.Content = XConvert.Convert(sourceReader.Content, destinationContentType);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 将数据读取器转换为具有键的类型的具体数据读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <returns>返回具体数据读取器</returns>
        public static IDataReader<TKey> As<TKey>(this IDataReader dataReader) where TKey : notnull
        {
            if (dataReader is IDataReader<TKey> tReader)
            {
                return tReader;
            }

            if (dataReader is IAsDataReader @asReader)
            {
                return As<TKey>(@asReader.Original);
            }

            if (dataReader is IDataReader<string> strReader)
            {
                return new AsDataReader<string, TKey>(strReader);
            }

            if (dataReader is IDataReader<int> intReader)
            {
                return new AsDataReader<int, TKey>(intReader);
            }

            if (AsHelper.GetInstance(dataReader) is AsHelper asHelper)
            {
                return asHelper.As<TKey>(dataReader);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataWriter<TKey> As<TKey>(this IDataWriter dataWriter) where TKey : notnull
        {
            if (dataWriter is IDataWriter<TKey> tWriter)
            {
                return tWriter;
            }

            if (dataWriter is IAsDataWriter asWriter)
            {
                return As<TKey>(asWriter.Original);
            }

            if (dataWriter is IDataWriter<string> strWriter)
            {
                return new AsDataWriter<string, TKey>(strWriter);
            }

            if (dataWriter is IDataWriter<int> intWriter)
            {
                return new AsDataWriter<int, TKey>(intWriter);
            }

            if (AsHelper.GetInstance(dataWriter) is AsHelper asHelper)
            {
                return asHelper.As<TKey>(dataWriter);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataRW">数据写入器</param>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataRW<TKey> As<TKey>(this IDataRW dataRW) where TKey : notnull
        {
            if (dataRW is IDataRW<TKey> tRW)
            {
                return tRW;
            }

            if (dataRW is IAsDataRW asRW)
            {
                return As<TKey>(asRW.Original);
            }

            if (dataRW is IDataRW<string> strRW)
            {
                return new AsDataRW<string, TKey>(strRW);
            }

            if (dataRW is IDataRW<int> intRW)
            {
                return new AsDataRW<int, TKey>(intRW);
            }

            if (dataRW is IDataRW<object> objectRW)
            {
                return new AsDataRW<object, TKey>(objectRW);
            }

            if (AsHelper.GetInstance(dataRW) is AsHelper asHelper)
            {
                return asHelper.As<TKey>(dataRW);
            }

            throw new NotSupportedException("Unsupported object.");
        }

        /// <summary>
        /// 从值读取器中读取一个字典。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个字典</returns>
        public static Dictionary<TKey, TValue?>? ReadDictionary<TKey, TValue>(this IValueReader valueReader) where TKey : notnull
        {
            var rw = new DictionaryRW<Dictionary<TKey, TValue?>, TKey, TValue>();

            valueReader.ReadObject(rw.As<string>());

            return rw.Content;
        }

        /// <summary>
        /// 往值写入器中写入一个字典。
        /// </summary>
        /// <typeparam name="TKey">键的类型</typeparam>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueWriter">值读取器</param>
        /// <param name="dictionary">字典</param>
        public static void WriteDictionary<TKey, TValue>(this IValueWriter valueWriter, Dictionary<TKey, TValue?> dictionary) where TKey : notnull
        {
            valueWriter.WriteObject(new DictionaryRW<Dictionary<TKey, TValue?>, TKey, TValue> { Content = dictionary }.As<string>());
        }

        /// <summary>
        /// 从值读取器中读取一个列表。
        /// </summary>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个列表</returns>
        public static List<TValue?>? ReadList<TValue>(this IValueReader valueReader)
        {
            var rw = new ListRW<List<TValue?>, TValue>();

            valueReader.ReadArray(rw);

            return rw.Content;
        }

        /// <summary>
        /// 往值写入器中写入一个列表。
        /// </summary>
        /// <typeparam name="TValue">值的类型</typeparam>
        /// <param name="valueWriter">值读取器</param>
        /// <param name="list">列表</param>
        public static void WriteList<TValue>(this IValueWriter valueWriter, List<TValue?> list)
        {
            valueWriter.WriteArray(new ListRW<List<TValue?>, TValue> { Content = list });
        }

        /// <summary>
        /// 从值读取器中读取一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个数组</returns>
        public static T?[]? ReadArray<T>(this IValueReader valueReader)
        {
            var rw = new ArrayRW<T>();

            valueReader.ReadArray(rw);

            return rw.Content;
        }

        /// <summary>
        /// 往值写入器中写入一个数组。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="array">数组</param>
        public static void WriteArray<T>(this IValueWriter valueWriter, T?[] array)
        {
            valueWriter.WriteArray(new ArrayRW<T> { Content = array });
        }

        /// <summary>
        /// 使用 <see cref="FastObjectRW{T}"/> 从值读取器中读取一个对象。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个对象</returns>
        public static T? FastReadObject<T>(this IValueReader valueReader)
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
        public static T? XReadObject<T>(this IValueReader valueReader, XBindingFlags flags = XBindingFlags.UseDefault)
        {
            var rw = XObjectRW.Create<T>(flags);

            valueReader.ReadObject(rw);

            return (T?)rw.Content;
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

        /// <summary>
        /// 写入一个空数组。
        /// </summary>
        public static unsafe void WriteEmptyArray<T>(this IFastArrayValueWriter valueWriter)
        {
            valueWriter.WriteArray(ref Unsafe.AsRef<T?>(null), 0);
        }
    }
}