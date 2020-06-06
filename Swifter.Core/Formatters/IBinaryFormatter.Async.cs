#if Async


using Swifter.RW;
using System;
using System.IO;
using System.Threading.Tasks;

#if ValueTask

using Task = System.Threading.Tasks.ValueTask;

#endif

namespace Swifter.Formatters
{
    partial interface IBinaryFormatter
    {
        /// <summary>
        /// 异步将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">字节数据源读取流</param>
        /// <returns>指定类型的值</returns>
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeAsync<T>(Stream stream);


        /// <summary>
        /// 异步将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">字节码数据源读取流</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeAsync(Stream stream, Type type);

        /// <summary>
        /// 异步将字节数据源反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">字节码数据源读取流</param>
        /// <param name="dataWriter">数据写入器</param>
        Task DeserializeToAsync(Stream stream, IDataWriter dataWriter);


        /// <summary>
        /// 异步将指定类型的实例序列化为字节数据格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <param name="stream">字节数据源写入流</param>
        Task SerializeAsync<T>(T value, Stream stream);
    }
}
#endif