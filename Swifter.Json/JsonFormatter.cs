using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 文档格式化器。
    /// 此类所有的静态方法和实例方法都是线程安全的。
    /// </summary>
    public sealed unsafe partial class JsonFormatter : ITextFormatter, ITargetedBind
    {
        private const JsonFormatterOptions ComplexOptions = 
            JsonFormatterOptions.IgnoreEmptyString | 
            JsonFormatterOptions.IgnoreNull | 
            JsonFormatterOptions.IgnoreZero | 
            JsonFormatterOptions.Indented;

        private const JsonFormatterOptions ReferenceOptions =
            JsonFormatterOptions.LoopReferencingException |
            JsonFormatterOptions.LoopReferencingNull |
            JsonFormatterOptions.MultiReferencingNull |
            JsonFormatterOptions.MultiReferencingReference;


        static readonly HGlobalCachePool<char> CharsPool = new HGlobalCachePool<char>();

        /// <summary>
        /// 作为自定义值读写接口的 Id。
        /// </summary>
        internal long id;

        /// <summary>
        /// 读取或设置最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 JsonFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public int MaxDepth { get; set; } = DefaultMaxDepth;

        /// <summary>
        /// 读取或设置缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string IndentedChars { get; set; } = DefaultIndentedChars;

        /// <summary>
        /// 读取或设置换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string LineBreakChars { get; set; } = DefaultLineCharsBreak;

        /// <summary>
        /// 读取或设置默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string MiddleChars { get; set; } = DefaultMiddleChars;

        /// <summary>
        /// JSON 格式化器配置项。
        /// </summary>
        public JsonFormatterOptions Options { get; set; }

        long ITargetedBind.TargetedId => id;

        void ITargetedBind.MakeTargetedId()
        {
            if (id == 0)
            {
                id = (long)Unsafe.AsPointer(ref Unsafe.AsRef(this));
            }
        }

        /// <summary>
        /// 初始化具有默认配置的 JSON 格式化器。
        /// </summary>
        public JsonFormatter()
        {
            Options = JsonFormatterOptions.Default;
        }

        /// <summary>
        /// 释放对象时移除读写接口实例。
        /// </summary>
        ~JsonFormatter()
        {
            if (id != 0)
            {
                ValueInterface.RemoveTargetedInterface(this);

                id = 0;
            }
        }

        /// <summary>
        /// 初始化指定配置的 JSON 格式化器。
        /// </summary>
        /// <param name="options">指定配置</param>
        public JsonFormatter(JsonFormatterOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(string text)
        {
            fixed (char* chars = text)
            {
                return Deserialize<T>(chars, text.Length);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(HGlobalCache<char> hGCache)
        {
            return Deserialize<T>(hGCache.GetPointer(), hGCache.Count);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <returns>返回指定类型的值</returns>
        public T Deserialize<T>(char* chars, int length)
        {
            switch (Options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    return InternalDeserialize<T, JsonDeserializeModes.Reference>(chars, length);
                case JsonFormatterOptions.DeflateDeserialize:
                    return InternalDeserialize<T, JsonDeserializeModes.Deflate>(chars, length);
                case JsonFormatterOptions.StandardDeserialize:
                    return InternalDeserialize<T, JsonDeserializeModes.Standard>(chars, length);
                default:
                    return InternalDeserialize<T, JsonDeserializeModes.Verified>(chars, length);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private T InternalDeserialize<T, TMode>(char* chars, int length) where TMode : struct
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<TMode>(this, chars, length));
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return Deserialize(chars, text.Length, type);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(HGlobalCache<char> hGCache, Type type)
        {
            return Deserialize(hGCache.GetPointer(), hGCache.Count, type);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(char* chars, int length, Type type)
        {
            switch (Options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    return InternalDeserialize<JsonDeserializeModes.Reference>(chars, length, type);
                case JsonFormatterOptions.DeflateDeserialize:
                    return InternalDeserialize<JsonDeserializeModes.Deflate>(chars, length, type);
                case JsonFormatterOptions.StandardDeserialize:
                    return InternalDeserialize<JsonDeserializeModes.Standard>(chars, length, type);
                default:
                    return InternalDeserialize<JsonDeserializeModes.Verified>(chars, length, type);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private object InternalDeserialize<TMode>(char* chars, int length, Type type) where TMode : struct
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<TMode>(this, chars, length));
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(TextReader textReader)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = Deserialize<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(TextReader textReader, Type type)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = Deserialize(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = Deserialize<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(Stream stream, Encoding encoding, Type type)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = Deserialize(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <returns>返回 JSON 字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string Serialize<T>(T value)
        {
            var hGCache = CharsPool.Rent();

            Serialize(value, hGCache);

            var str = hGCache.ToStringEx();

            CharsPool.Return(hGCache);

            return str;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, TextWriter textWriter)
        {
            var hGCache = CharsPool.Rent();

            var options = Options;

            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ReferenceMode>(options, hGCache, MaxDepth)
                {
                    jsonFormatter = this,
                    textWriter = textWriter,
                    References = new JsonReferenceWriter()
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else if ((options & ComplexOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ComplexMode>(options, hGCache, MaxDepth)
                {
                    jsonFormatter = this,
                    textWriter = textWriter
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.SimpleMode>(options, hGCache, MaxDepth)
                {
                    jsonFormatter = this,
                    textWriter = textWriter
                };

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }

            hGCache.WriteTo(textWriter);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            Serialize(value, hGCache);

            hGCache.WriteTo(stream, encoding);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">HGlobalCache</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, HGlobalCache<char> hGCache)
        {
            var options = Options;

            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ReferenceMode>(options,hGCache, MaxDepth)
                {
                    jsonFormatter = this,
                    References = new JsonReferenceWriter()
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else if ((options & ComplexOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ComplexMode>(options, hGCache, MaxDepth)
                {
                    jsonFormatter = this
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.SimpleMode>(options, hGCache, MaxDepth)
                {
                    jsonFormatter = this
                };

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void InternalDeserializeTo<TMode>(char* chars, int length, IDataWriter dataWriter) where TMode : struct
        {
            var jsonDeserializer = new JsonDeserializer<TMode>(chars, length);

            if (jsonDeserializer.IsObject)
            {
                if (dataWriter is IId64DataRW<char> fastWriter)
                {
                    jsonDeserializer.FastReadObject(fastWriter);
                }
                else
                {
                    jsonDeserializer.SlowReadObject(dataWriter.As<string>());
                }
            }
            else if (jsonDeserializer.IsArray)
            {
                jsonDeserializer.NoInliningReadArray(dataWriter.As<int>());
            }
            else
            {
                RWHelper.SetContent(dataWriter, jsonDeserializer.DirectRead());
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="dataWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DeserializeTo(char* chars, int length, IDataWriter dataWriter)
        {
            switch (Options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    InternalDeserializeTo<JsonDeserializeModes.Reference>(chars, length, dataWriter);
                    break;
                case JsonFormatterOptions.DeflateDeserialize:
                    InternalDeserializeTo<JsonDeserializeModes.Deflate>(chars, length, dataWriter);
                    break;
                case JsonFormatterOptions.StandardDeserialize:
                    InternalDeserializeTo<JsonDeserializeModes.Standard>(chars, length, dataWriter);
                    break;
                default:
                    InternalDeserializeTo<JsonDeserializeModes.Verified>(chars, length, dataWriter);
                    break;
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(string text, IDataWriter dataWriter)
        {
            fixed(char* chars= text)
            {
                DeserializeTo(chars, text.Length, dataWriter);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(TextReader textReader, IDataWriter dataWriter)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            DeserializeTo(hGCache, dataWriter);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">JSON 字符串缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(HGlobalCache<char> hGCache, IDataWriter dataWriter)
        {
            DeserializeTo(hGCache.GetPointer(), hGCache.Count, dataWriter);
        }
    }
}