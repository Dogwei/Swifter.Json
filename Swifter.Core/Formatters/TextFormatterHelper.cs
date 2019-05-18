using Swifter.RW;
using Swifter.Writers;
using System;
using System.IO;

namespace Swifter.Formatters
{
    /// <summary>
    /// 提供字符类文档的格式的扩展方法。
    /// </summary>
    public static class TextFormatterHelper
    {

        /// <summary>
        /// 将文档字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <typeparam name="Key">数据写入器键的类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="text">文档字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public static void DeserializeToWriter<Key>(this ITextFormatter textFormatter, string text, IDataWriter<Key> dataWriter)
        {
            AuxiliaryWriter<Key>.ThreadDataWriter = dataWriter;

            textFormatter.Deserialize<AuxiliaryWriter<Key>>(text);

            AuxiliaryWriter<Key>.ThreadDataWriter = null;
        }

        /// <summary>
        /// 将文档字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <typeparam name="Key">数据写入器键的类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public static void DeserializeToWriter<Key>(this ITextFormatter textFormatter, TextReader textReader, IDataWriter<Key> dataWriter)
        {
            AuxiliaryWriter<Key>.ThreadDataWriter = dataWriter;

            textFormatter.Deserialize<AuxiliaryWriter<Key>>(textReader);

            AuxiliaryWriter<Key>.ThreadDataWriter = null;
        }

        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="text">文档字符串</param>
        /// <param name="obj">对象</param>
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, string text, T obj)
        {
            IDataWriter dataWriter;

            try
            {
                dataWriter = RWHelper.CreateRW(obj);
            }
            catch (NotSupportedException)
            {
                dataWriter = RWHelper.CreateWriter<T>();

                RWHelper.SetContent(dataWriter, obj);
            }

            if (dataWriter is IDataWriter<string> nameWriter)
            {
                DeserializeToWriter(textFormatter, text, nameWriter);
            }
            else if (dataWriter is IDataWriter<int> indexWriter)
            {
                DeserializeToWriter(textFormatter, text, indexWriter);
            }
            else
            {
                var invoker = new DeserializeToAsInvoker(textFormatter, text, dataWriter);

                AsHelper.GetInstance(dataWriter).Invoke(invoker);
            }
        }

        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="obj">对象</param>
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, TextReader textReader, T obj)
        {
            IDataWriter dataWriter;

            try
            {
                dataWriter = RWHelper.CreateRW(obj);
            }
            catch (NotSupportedException)
            {
                dataWriter = RWHelper.CreateWriter<T>();

                RWHelper.SetContent(dataWriter, obj);
            }

            if (dataWriter is IDataWriter<string> nameWriter)
            {
                DeserializeToWriter(textFormatter, textReader, nameWriter);
            }
            else if (dataWriter is IDataWriter<int> indexWriter)
            {
                DeserializeToWriter(textFormatter, textReader, indexWriter);
            }
            else
            {
                var invoker = new DeserializeToAsInvoker(textFormatter, textReader, dataWriter);

                AsHelper.GetInstance(dataWriter).Invoke(invoker);
            }
        }
    }
}
