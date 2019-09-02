

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

            return auxiliary.GetDataReader() ?? (throwException ? throw new NotSupportedException($"Unable create data reader of '{typeof(T)}'.") : default(IDataReader));
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

            return auxiliary.GetDataRW() ?? (throwException ? throw new NotSupportedException($"Unable create data reader-writer of '{typeof(T)}'.") : default(IDataRW));
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

            return auxiliary.GetDataWriter() ?? (throwException ? throw new NotSupportedException($"Unable create data writer of '{typeof(T)}'.") : default(IDataWriter));
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
            catch (Exception)
            {
            }

            var writer = auxiliary.GetDataWriter();

            if (writer != null)
            {
                return writer;
            }

            ValueInterface<T>.ReadValue(auxiliary);

            writer = auxiliary.GetDataWriter();

            if (writer == null)
            {
                return auxiliary.GetDataWriter() ?? (throwException ? throw new NotSupportedException($"Unable create data writer of  '{typeof(T)}'.") : default(IDataWriter));
            }

            SetContent(writer, obj);

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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataReader() ?? (throwException ? throw new NotSupportedException($"Unable create data reader of '{obj.GetType()}'.") : default(IDataReader));
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
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var auxiliary = new AuxiliaryValueRW();

            ValueInterface.GetInterface(obj).Write(auxiliary, obj);

            return auxiliary.GetDataRW() ?? (throwException ? throw new NotSupportedException($"Unable create data reader-writer of '{obj.GetType()}'.") : default(IDataRW));
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
            if (type == null)
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
            if (obj == null)
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

            if (writer == null)
            {
                return auxiliary.GetDataWriter() ?? (throwException ? throw new NotSupportedException($"Unable create data writer of  '{obj.GetType()}'.") : default(IDataWriter));
            }

            SetContent(writer, obj);

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

            if (typeof(TKey) == typeof(long) && ((dataReader as IAsDataReader)?.Content ?? dataReader) is IId64DataRW id64DataRW)
            {
                id64DataRW.OnReadValue(Unsafe.As<TKey, long>(ref key), auxiliary);
            }
            else
            {
                dataReader.OnReadValue(key, auxiliary);
            }

            return auxiliary.GetDataReader() ?? (throwException ? throw new NotSupportedException($"Failed to create data reader of field : '{key}' in {dataReader}.") : default(IDataReader));
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

            if (typeof(TKey) == typeof(long) && ((dataReader as IAsDataReader)?.Content ?? dataReader) is IId64DataRW id64DataRW)
            {
                id64DataRW.OnReadValue(Unsafe.As<TKey, long>(ref key), auxiliary);
            }
            else
            {
                dataReader.OnReadValue(key, auxiliary);
            }

            return auxiliary.GetDataRW() ?? (throwException ? throw new NotSupportedException($"Failed to create data reader-writer of field : '{key}' in {dataReader}.") : default(IDataRW));
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter<T> dataWriter)
        {
            dataReader.OnReadAll(dataWriter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter dataWriter)
        {
            Copy(dataReader, dataWriter.As<T>());
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader dataReader, IDataWriter<T> dataWriter)
        {
            Copy(dataReader.As<T>(), dataWriter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter<T> dataWriter, IValueFilter<T> valueFilter)
        {
            dataReader.OnReadAll(dataWriter, valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader<T> dataReader, IDataWriter dataWriter, IValueFilter<T> valueFilter)
        {
            Copy(dataReader, dataWriter.As<T>(), valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <typeparam name="T">键类型</typeparam>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        /// <param name="valueFilter">筛选器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(IDataReader dataReader, IDataWriter<T> dataWriter, IValueFilter<T> valueFilter)
        {
            Copy(dataReader.As<T>(), dataWriter, valueFilter);
        }

        /// <summary>
        /// Copy 数据内容。
        /// </summary>
        /// <param name="dataReader">数据源</param>
        /// <param name="dataWriter">目标</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy(IDataReader dataReader, IDataWriter dataWriter)
        {
            Copy(dataReader.As<object>(), dataWriter.As<object>());
        }

        /// <summary>
        /// 获取数据读取或写入器的数据源。
        /// </summary>
        /// <typeparam name="T">数据源类型</typeparam>
        /// <param name="dataRW">数据读取器</param>
        /// <returns>返回该数据源</returns>
        public static T GetContent<T>(object dataRW)
        {
            if (dataRW is IInitialize<T> initialize)
            {
                return initialize.Content;
            }

            if (dataRW is IDirectContent directContent)
            {
                return (T)directContent.DirectContent;
            }

            throw new NotSupportedException($"Unable {"get"} content by '{dataRW.GetType().FullName}' (read)writer.");
        }
        /// <summary>
        /// 设置数据读取或写入器的数据源。
        /// </summary>
        /// <typeparam name="T">数据源类型</typeparam>
        /// <param name="dataRW">数据读取或写入器</param>
        /// <param name="content">数据源</param>
        public static void SetContent<T>(object dataRW, T content)
        {
            if (dataRW is IInitialize<T> initialize)
            {
                initialize.Initialize(content);

                return;
            }

            if (dataRW is IDirectContent directContent)
            {
                directContent.DirectContent = content;

                return;
            }

            throw new NotSupportedException($"Unable {"set"} content by '{dataRW.GetType().FullName}' (read)writer.");
        }

        /// <summary>
        /// 将数据读取器转换为具有键的类型的具体数据读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据读取器</returns>
        public static IDataReader<T> As<T>(this IDataReader dataReader)
        {
            if (dataReader is IDataReader<T> t)
            {
                return t;
            }

            if (dataReader is IAsDataReader a)
            {
                return As<T>(a.Content);
            }

            if (dataReader is IDataReader<string> s)
            {
                return new AsDataReader<string, T>(s);
            }

            if (dataReader is IDataReader<int> i)
            {
                return new AsDataReader<int, T>(i);
            }

            return AsHelper.GetInstance(dataReader).As<T>(dataReader);
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataWriter<T> As<T>(this IDataWriter dataWriter)
        {
            if (dataWriter is IDataWriter<T> t)
            {
                return t;
            }

            if (dataWriter is IAsDataWriter a)
            {
                return As<T>(a.Content);
            }

            if (dataWriter is IDataWriter<string> s)
            {
                return new AsDataWriter<string, T>(s);
            }

            if (dataWriter is IDataWriter<int> i)
            {
                return new AsDataWriter<int, T>(i);
            }

            return AsHelper.GetInstance(dataWriter).As<T>(dataWriter);
        }

        /// <summary>
        /// 将数据写入器转换为具有键的类型的具体数据写入器。
        /// </summary>
        /// <param name="dataRW">数据写入器</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <returns>返回具体数据写入器</returns>
        public static IDataRW<T> As<T>(this IDataRW dataRW)
        {
            if (dataRW is IDataRW<T> t)
            {
                return t;
            }

            if (dataRW is IAsDataRW a)
            {
                return As<T>(a.Content);
            }

            if (dataRW is IDataRW<string> s)
            {
                return new AsDataRW<string, T>(s);
            }

            if (dataRW is IDataRW<int> i)
            {
                return new AsDataRW<int, T>(i);
            }

            return AsHelper.GetInstance(dataRW).As<T>(dataRW);
        }

        /// <summary>
        /// 计算 Id64 值。
        /// </summary>
        /// <param name="dataRW">数据读写器</param>
        /// <param name="name">键</param>
        /// <returns>返回 Id64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe long GetId64Ex(this IId64DataRW<char> dataRW, string name)
        {
            fixed (char* pName = name)
            {
                return dataRW.GetId64(ref pName[0], name.Length);
            }
        }

        /// <summary>
        /// 创建授权代理的数据读取器。
        /// </summary>
        /// <typeparam name="TKey">字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataReader">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据读取器</returns>
        public static IDataReader<TKey> CreateAuthorizedKeysReader<TKey, TValue>(IDataReader<TKey> dataReader, Dictionary<TKey, TValue> authorizedKeys)
        {
            return new AuthorizedKeysRW<TKey, TKey, TValue>(dataReader, authorizedKeys);
        }

        /// <summary>
        /// 创建授权代理的数据写入器。
        /// </summary>
        /// <typeparam name="TKey">字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataWriter">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据写入器</returns>
        public static IDataWriter<TKey> CreateAuthorizedKeysWriter<TKey, TValue>(IDataWriter<TKey> dataWriter, Dictionary<TKey, TValue> authorizedKeys)
        {
            return new AuthorizedKeysRW<TKey, TKey, TValue>(dataWriter, authorizedKeys);
        }

        /// <summary>
        /// 创建授权代理的数据读取器。
        /// </summary>
        /// <typeparam name="TInput">原始数据的字段类型</typeparam>
        /// <typeparam name="TOutput">授权的字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataReader">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据读取器</returns>
        public static IDataReader<TInput> CreateAuthorizedKeysReader<TInput, TOutput, TValue>(IDataReader<TInput> dataReader, Dictionary<TOutput, TValue> authorizedKeys)
        {
            return new AuthorizedKeysRW<TInput, TOutput, TValue>(dataReader, authorizedKeys);
        }

        /// <summary>
        /// 创建授权代理的数据写入器。
        /// </summary>
        /// <typeparam name="TInput">原始数据的字段类型</typeparam>
        /// <typeparam name="TOutput">授权的字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataWriter">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据写入器</returns>
        public static IDataWriter<TInput> CreateAuthorizedKeysWriter<TInput, TOutput, TValue>(IDataWriter<TInput> dataWriter, Dictionary<TOutput, TValue> authorizedKeys)
        {
            return new AuthorizedKeysRW<TInput, TOutput, TValue>(dataWriter, authorizedKeys);
        }

        /// <summary>
        /// 创建授权代理的数据读取器。
        /// </summary>
        /// <typeparam name="TOutput">授权的字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataReader">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据读取器</returns>
        public static IDataReader CreateAuthorizedKeysReader<TOutput, TValue>(IDataReader dataReader, Dictionary<TOutput, TValue> authorizedKeys)
        {
            var invoker = new CreateAuthorizedKeysRWInvoker<TOutput, TValue>(dataReader, authorizedKeys);

            AsHelper.GetInstance(dataReader).Invoke(invoker);

            return (IDataReader)invoker.AuthorizedKeysRW;
        }

        /// <summary>
        /// 创建授权代理的数据写入器。
        /// </summary>
        /// <typeparam name="TOutput">授权的字段类型</typeparam>
        /// <typeparam name="TValue">字典的 Value 类型，此功能无需用到该值</typeparam>
        /// <param name="dataWriter">原始数据</param>
        /// <param name="authorizedKeys">一个标识允许访问的字段列表</param>
        /// <returns>返回一个已代理的数据写入器</returns>
        public static IDataWriter CreateAuthorizedKeysWriter<TOutput, TValue>(IDataWriter dataWriter, Dictionary<TOutput, TValue> authorizedKeys)
        {
            var invoker = new CreateAuthorizedKeysRWInvoker<TOutput, TValue>(dataWriter, authorizedKeys);

            AsHelper.GetInstance(dataWriter).Invoke(invoker);

            return (IDataWriter)invoker.AuthorizedKeysRW;
        }

        /// <summary>
        /// 获取一个 键类型 是否可作为数组索引。
        /// </summary>
        /// <typeparam name="TKey">键类型</typeparam>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsArray<TKey>()
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
    }
}