using Swifter.RW;
using Swifter.Tools;

using System;
using System.IO;

namespace Swifter.Formatters
{
    /// <summary>
    /// 提供字节类数据格式的反序列化和序列化接口。
    /// </summary>
    public partial interface IBinaryFormatter
    {
        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="bytes">字节数据源</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(ArraySegment<byte> bytes);

        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">字节数据源读取流</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(Stream stream);

        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="hGCache">字节数据源缓存</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(HGlobalCache<byte> hGCache);

        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <param name="bytes">文档字符串</param>
        /// <param name="type">字节数据源</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(ArraySegment<byte> bytes, Type type);

        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">字节数据源读取流</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(Stream stream, Type type);

        /// <summary>
        /// 将字节数据源反序列化为指定类型的值。
        /// </summary>
        /// <param name="hGCache">字节数据源缓存</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(HGlobalCache<byte> hGCache, Type type);

        /// <summary>
        /// 将字节数据源反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">字节数据源</param>
        /// <param name="dataWriter">数据写入器</param>
        void DeserializeTo(ArraySegment<byte> bytes, IDataWriter dataWriter);

        /// <summary>
        /// 将字节数据源反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">字节数据源读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        void DeserializeTo(Stream stream, IDataWriter dataWriter);

        /// <summary>
        /// 将字节数据源反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">字节数据源缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        void DeserializeTo(HGlobalCache<byte> hGCache, IDataWriter dataWriter);

        /// <summary>
        /// 将指定类型的实例序列化为字节码数据格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <returns>返回当前文档字符串</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// 将指定类型的实例序列化为字节码数据格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <param name="stream">字节数据源写入流</param>
        void Serialize<T>(T value, Stream stream);

        /// <summary>
        /// 将指定类型的实例序列化为字节码数据格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <param name="hGCache">字节码数据缓存</param>
        void Serialize<T>(T value, HGlobalCache<byte> hGCache);
    }
}
