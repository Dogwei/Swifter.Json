using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Json
{
    public sealed unsafe partial class JsonFormatter
    {
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
        public static string DefaultLineBreak { get; set; } = "\n";

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
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(char* chars, int length)
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer(chars, length));
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return DeserializeObject<T>(hGCache.GetPointer(), length);
            }
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
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(char* chars, int length, Type type)
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer(chars, length));
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return DeserializeObject(hGCache.GetPointer(), length, type);
            }
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
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                var jsonDeserializer = new JsonReferenceDeserializer(chars, length);

                var result = ValueInterface<T>.ReadValue(jsonDeserializer);

                if (jsonDeserializer.references.Count != 0)
                {
                    ReferenceInfo.ProcessReference(result, jsonDeserializer.references);

                    if (jsonDeserializer.updateBase)
                    {
                        result = RWHelper.GetContent<T>(jsonDeserializer.writer);
                    }
                }

                return result;
            }

            return DeserializeObject<T>(chars, length);
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return DeserializeObject<T>(hGCache.GetPointer(), length, options);
            }
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
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(char* chars, int length, Type type, JsonFormatterOptions options)
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                var jsonDeserializer = new JsonReferenceDeserializer(chars, length);

                var result = ValueInterface.GetInterface(type).Read(jsonDeserializer);

                if (jsonDeserializer.references.Count != 0)
                {
                    ReferenceInfo.ProcessReference(result, jsonDeserializer.references);

                    if (jsonDeserializer.updateBase)
                    {
                        result = RWHelper.GetContent<object>(jsonDeserializer.writer);

                        result = Convert.ChangeType(result, type);
                    }
                }

                return result;
            }

            return DeserializeObject(chars, length, type);
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options)
        {
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return DeserializeObject(hGCache.GetPointer(), length, type, options);
            }
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
            using (var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth))
            {
                ValueInterface<T>.WriteValue(jsonSerializer, value);

                return new string(
                    jsonSerializer.hGlobal.GetPointer(),
                    0,
                    jsonSerializer.StringLength);
            }
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
            using (var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth) { textWriter = textWriter })
            {
                ValueInterface<T>.WriteValue(jsonSerializer, value);

                VersionDifferences.WriteChars(
                    textWriter,
                    jsonSerializer.hGlobal.GetPointer(),
                    jsonSerializer.StringLength);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static BaseJsonSerializer CreateJsonSerializer(JsonFormatterOptions options)
        {
            if ((options & ReferenceOptions) != 0)
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    return new JsonReferenceSerializer(options)
                    {
                        indentedChars = DefaultIndentedChars,
                        lineBreak = DefaultLineBreak,
                        middleChars = DefaultMiddleChars
                    };
                }

                return new JsonReferenceSerializer(options);
            }

            if ((options & ~JsonFormatterOptions.PriorCheckReferences) == JsonFormatterOptions.Default)
            {
                return new JsonDefaultSerializer(DefaultMaxDepth);
            }

            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                return new JsonSerializer(options, DefaultMaxDepth)
                {
                    indentedChars = DefaultIndentedChars,
                    lineBreak = DefaultLineBreak,
                    middleChars = DefaultMiddleChars
                };
            }

            return new JsonSerializer(options, DefaultMaxDepth);
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
            using (var jsonSerializer = CreateJsonSerializer(options))
            {
                ValueInterface<T>.WriteValue((IValueWriter)jsonSerializer, value);

                return new string(
                    jsonSerializer.hGlobal.GetPointer(),
                    0,
                    jsonSerializer.StringLength);
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
            using (var jsonSerializer = CreateJsonSerializer(options))
            {
                ValueInterface<T>.WriteValue((IValueWriter)jsonSerializer, value);

                VersionDifferences.WriteChars(
                    textWriter,
                    jsonSerializer.hGlobal.GetPointer(),
                    jsonSerializer.StringLength);
            }
        }
    }
}
