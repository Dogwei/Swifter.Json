#if Span


using Swifter.RW;
using System;

namespace Swifter.Formatters
{
    partial interface IBinaryFormatter
    {
        /// <summary>
        /// 将字节码数据源反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="bytes">字节码数据源</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(ReadOnlySpan<byte> bytes);

        /// <summary>
        /// 将字节码数据源反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">字节码数据源</param>
        /// <param name="dataWriter">数据写入器</param>
        void DeserializeTo(ReadOnlySpan<byte> bytes, IDataWriter dataWriter);

        /// <summary>
        /// 将字节码数据源反序列化为指定类型的值。
        /// </summary>
        /// <param name="bytes">字节码数据源</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(ReadOnlySpan<byte> bytes, Type type);
    }
}

#endif
