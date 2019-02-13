using Swifter.RW;
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
                return ValueInterface<T>.Content.ReadValue(new JsonDeserializer(chars, 0, text.Length));
            }
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
            return DeserializeObject<T>(textReader.ReadToEnd());
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.GetInterface(type).Read(new JsonDeserializer(chars, 0, text.Length));
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type)
        {
            return DeserializeObject(textReader.ReadToEnd(), type);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference 。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(string text, JsonFormatterOptions options)
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                fixed (char* chars = text)
                {
                    var jsonDeserializer = new JsonReferenceDeserializer(chars, 0, text.Length);

                    var result = ValueInterface<T>.Content.ReadValue(jsonDeserializer);

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
            }

            return DeserializeObject<T>(text);
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference 。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(TextReader textReader, JsonFormatterOptions options)
        {
            return DeserializeObject<T>(textReader.ReadToEnd(), options);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference 。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(string text, Type type, JsonFormatterOptions options)
        {
            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                fixed (char* chars = text)
                {
                    var jsonDeserializer = new JsonReferenceDeserializer(chars, 0, text.Length);

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
            }

            return DeserializeObject(text, type);
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference 。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options)
        {
            return DeserializeObject(textReader.ReadToEnd(), type);
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
            var serializer = new JsonDefaultSerializer(DefaultMaxDepth);

            ValueInterface<T>.Content.WriteValue(serializer, value);

            return serializer.ToString();
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
            var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth);

            jsonSerializer.textWriter = textWriter;

            ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

            jsonSerializer.WriteTo(textWriter);
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
            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonReferenceSerializer(options);

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = DefaultIndentedChars;
                    jsonSerializer.lineBreak = DefaultLineBreak;
                    jsonSerializer.middleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                return jsonSerializer.ToString();
            }

            if ((options & JsonFormatterOptions.PriorCheckReferences) != 0)
            {
                options ^= JsonFormatterOptions.PriorCheckReferences;
            }

            if (options == JsonFormatterOptions.Default)
            {
                var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth);

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                return jsonSerializer.ToString();
            }
            else
            {
                var jsonSerializer = new JsonSerializer(options, DefaultMaxDepth);

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = DefaultIndentedChars;
                    jsonSerializer.lineBreak = DefaultLineBreak;
                    jsonSerializer.middleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                return jsonSerializer.ToString();
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
            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonReferenceSerializer(options);

                jsonSerializer.textWriter = textWriter;

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = DefaultIndentedChars;
                    jsonSerializer.lineBreak = DefaultLineBreak;
                    jsonSerializer.middleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                jsonSerializer.WriteTo(textWriter);

                return;
            }

            if ((options & JsonFormatterOptions.PriorCheckReferences) != 0)
            {
                options ^= JsonFormatterOptions.PriorCheckReferences;
            }

            if (options == JsonFormatterOptions.Default)
            {
                var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth);

                jsonSerializer.textWriter = textWriter;

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                jsonSerializer.WriteTo(textWriter);
            }
            else
            {
                var jsonSerializer = new JsonSerializer(options, DefaultMaxDepth);

                jsonSerializer.textWriter = textWriter;

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = DefaultIndentedChars;
                    jsonSerializer.lineBreak = DefaultLineBreak;
                    jsonSerializer.middleChars = DefaultMiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                jsonSerializer.WriteTo(textWriter);
            }
        }
    }
}
