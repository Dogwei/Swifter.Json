
using Swifter.RW;
using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// 表示 MessagePack 的扩展对象。
    /// </summary>
    [Serializable]
    public sealed class MessagePackExtension
    {
        /// <summary>
        /// 扩展对象代码。
        /// </summary>
        public readonly sbyte Code;

        /// <summary>
        /// 扩展对象字节内容。
        /// </summary>
        public readonly byte[] Binary;

        /// <summary>
        /// 构建 MessagePack 的扩展对象实例。
        /// </summary>
        /// <param name="code">扩展对象代码</param>
        /// <param name="binary">扩展对象字节内容</param>
        public MessagePackExtension(sbyte code, byte[] binary)
        {
            Code = code;
            Binary = binary;
        }

        sealed class ValueInterface : IValueInterface<MessagePackExtension>
        {
            public MessagePackExtension ReadValue(IValueReader valueReader)
            {
                if (valueReader is IValueReader<MessagePackExtension> reader)
                {
                    return reader.ReadValue();
                }

                throw new NotSupportedException();
            }

            public void WriteValue(IValueWriter valueWriter, MessagePackExtension value)
            {
                if (valueWriter is IValueWriter<MessagePackExtension> writer)
                {
                    writer.WriteValue(value);

                    return;
                }

                throw new NotSupportedException();
            }
        }
    }
}