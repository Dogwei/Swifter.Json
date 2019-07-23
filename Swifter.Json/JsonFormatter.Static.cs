using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using DefaultDeserializeMode = Swifter.Json.JsonDeserializeModes.Verified;
using DefaultSerializeMode = Swifter.Json.JsonSerializeModes.SimpleMode;

namespace Swifter.Json
{
    public sealed unsafe partial class JsonFormatter
    {
        const JsonFormatterOptions ModeOptions =
            JsonFormatterOptions.MultiReferencingReference |
            JsonFormatterOptions.DeflateDeserialize |
            JsonFormatterOptions.StandardDeserialize |
            JsonFormatterOptions.VerifiedDeserialize;


        /// <summary>
        /// 读取或设置默认最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 JsonFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public static int DefaultMaxDepth { get; set; } = 20;

        /// <summary>
        /// 读取或设置默认缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultIndentedChars { get; set; } = "  ";

        /// <summary>
        /// 读取或设置默认换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultLineCharsBreak { get; set; } = "\n";

        /// <summary>
        /// 读取或设置默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public static string DefaultMiddleChars { get; set; } = " ";

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(string text)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject<T>(chars, text.Length);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(HGlobalCache<char> hGCache)
        {
            return DeserializeObject<T>(hGCache.GetPointer(), hGCache.Count);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(char* chars, int length)
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<DefaultDeserializeMode>(chars, length));
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(TextReader textReader)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = DeserializeObject<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = DeserializeObject<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject(chars, text.Length, type);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(HGlobalCache<char> hGCache, Type type)
        {
            return DeserializeObject(hGCache.GetPointer(), hGCache.Count, type);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(char* chars, int length, Type type)
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<DefaultDeserializeMode>(chars, length));
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = DeserializeObject(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(Stream stream, Encoding encoding, Type type)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = DeserializeObject(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(HGlobalCache<char> hGCache, JsonFormatterOptions options)
        {
            return DeserializeObject<T>(hGCache.GetPointer(), hGCache.Count, options);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(string text, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject<T>(chars, text.Length, options);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(char* chars, int length, JsonFormatterOptions options)
        {
            switch (options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    return ValueInterface<T>.ReadValue(new JsonDeserializer<JsonDeserializeModes.Reference>(chars, length));
                case JsonFormatterOptions.DeflateDeserialize:
                    return ValueInterface<T>.ReadValue(new JsonDeserializer<JsonDeserializeModes.Deflate>(chars, length));
                case JsonFormatterOptions.StandardDeserialize:
                    return ValueInterface<T>.ReadValue(new JsonDeserializer<JsonDeserializeModes.Standard>(chars, length));
                default:
                    return ValueInterface<T>.ReadValue(new JsonDeserializer<DefaultDeserializeMode>(chars, length));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(TextReader textReader, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = DeserializeObject<T>(hGCache, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = DeserializeObject<T>(hGCache, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(string text, Type type, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject(chars, text.Length, type, options);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="hGCache">JSON HGlobalCache</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(HGlobalCache<char> hGCache, Type type, JsonFormatterOptions options)
        {
            return DeserializeObject(hGCache.GetPointer(), hGCache.Count, type, options);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(char* chars, int length, Type type, JsonFormatterOptions options)
        {
            switch (options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    return ValueInterface.GetInterface(type).Read(new JsonDeserializer<JsonDeserializeModes.Reference>(chars, length));
                case JsonFormatterOptions.DeflateDeserialize:
                    return ValueInterface.GetInterface(type).Read(new JsonDeserializer<JsonDeserializeModes.Deflate>(chars, length));
                case JsonFormatterOptions.StandardDeserialize:
                    return ValueInterface.GetInterface(type).Read(new JsonDeserializer<JsonDeserializeModes.Standard>(chars, length));
                default:
                    return ValueInterface.GetInterface(type).Read(new JsonDeserializer<DefaultDeserializeMode>(chars, length));
            }
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var value = DeserializeObject(hGCache, type, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var value = DeserializeObject(hGCache, type, options);

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
        public static string SerializeObject<T>(T value)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache);

            var str = hGCache.ToStringEx();

            CharsPool.Return(hGCache);

            return str;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">HGlobalCache</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, HGlobalCache<char> hGCache)
        {
            var jsonSerializer = new JsonSerializer<DefaultSerializeMode>(hGCache, DefaultMaxDepth);

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter)
        {
            var hGCache = CharsPool.Rent();

            var jsonSerializer = new JsonSerializer<DefaultSerializeMode>(hGCache, DefaultMaxDepth)
            {
                textWriter = textWriter
            };

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();

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
        public static void SerializeObject<T>(T value, Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache);

            hGCache.WriteTo(stream, encoding);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="options">序列化配置</param>
        /// <returns>返回 JSON 字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T value, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache, options);

            var str = hGCache.ToStringEx();

            CharsPool.Return(hGCache);

            return str;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, HGlobalCache<char> hGCache, JsonFormatterOptions options)
        {
            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ReferenceMode>(options, hGCache, DefaultMaxDepth)
                {
                    References = new JsonReferenceWriter()
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else if ((options & ComplexOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ComplexMode>(options, hGCache, DefaultMaxDepth);

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else
            {
                var jsonSerializer = new JsonSerializer<DefaultSerializeMode>(options, hGCache, DefaultMaxDepth);

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ReferenceMode>(options, hGCache, DefaultMaxDepth)
                {
                    References = new JsonReferenceWriter()
                };

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else if ((options & ComplexOptions) != 0)
            {
                var jsonSerializer = new JsonSerializer<JsonSerializeModes.ComplexMode>(options, hGCache, DefaultMaxDepth);

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.WriteValue(jsonSerializer, value);

                jsonSerializer.Flush();
            }
            else
            {
                var jsonSerializer = new JsonSerializer<DefaultSerializeMode>(options, hGCache, DefaultMaxDepth);

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
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache, options);

            hGCache.WriteTo(stream, encoding);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="chars">JSON 内容</param>
        /// <param name="length">JSON 内容长度</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(char* chars, int length)
        {
            return new JsonDeserializer<DefaultDeserializeMode>(chars, length);
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="hGCache">JSON 内容缓存</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(HGlobalCache<char> hGCache)
        {
            return CreateJsonReader(hGCache.GetPointer(), hGCache.Count);
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="textReader">JSON 内容读取器</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(TextReader textReader)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(textReader);

            var jsonReader = CreateJsonReader(hGCache);

            CharsPool.Return(hGCache);

            return jsonReader;
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="stream">JSON 内容流</param>
        /// <param name="encoding">JSON 内容编码</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            hGCache.ReadFrom(stream, encoding);

            var jsonReader = CreateJsonReader(hGCache);

            CharsPool.Return(hGCache);

            return jsonReader;
        }

        /// <summary>
        /// 创建 JSON 文档写入器。注意：在写入器中请遵守规则写入，否则生成的 JSON 将不正常。
        /// </summary>
        /// <returns>返回一个 JSON 文档写入器</returns>
        public static IJsonWriter CreateJsonWriter()
        {
            return new JsonSerializer<DefaultSerializeMode>();
        }
    }
}