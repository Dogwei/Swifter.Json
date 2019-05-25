using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 文档格式化器。
    /// 此类所有的静态方法和实例方法都是线程安全的。
    /// </summary>
    public sealed unsafe partial class JsonFormatter : ITextFormatter
    {
        private const JsonFormatterOptions ReferenceOptions =
            JsonFormatterOptions.LoopReferencingException |
            JsonFormatterOptions.LoopReferencingNull |
            JsonFormatterOptions.MultiReferencingNull |
            JsonFormatterOptions.MultiReferencingReference;

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
        /// 设置当前实例指定类型的值读写器。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="valueInterface">值读写器</param>
        public void SetValueInterface<T>(IValueInterface<T> valueInterface)
        {
            if (valueInterface == null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            lock (this)
            {
                if (id == 0)
                {
                    id = (long)Unsafe.AsPointer(ref Unsafe.AsRef(this));
                }

                ValueInterface<T>.SetTargetedInterface(id, valueInterface);
            }
        }

        /// <summary>
        /// JSON 格式化器配置项。
        /// </summary>
        public JsonFormatterOptions Options { get; set; }

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
                ValueInterface.RemoveTargetedInterface(id);

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
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <returns>返回指定类型的值</returns>
        public T Deserialize<T>(char* chars, int length)
        {
            var options = Options;

            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                var jsonDeserializer = new JsonReferenceDeserializer(chars, length);

                if (id != 0)
                {
                    jsonDeserializer.jsonFormatter = this;
                }

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
            else
            {
                var jsonDeserializer = new JsonDeserializer(chars, length);

                if (id != 0)
                {
                    jsonDeserializer.jsonFormatter = this;
                }

                return ValueInterface<T>.ReadValue(jsonDeserializer);
            }
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
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(char* chars, int length, Type type)
        {
            var options = Options;

            if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
            {
                var jsonDeserializer = new JsonReferenceDeserializer(chars, length);

                if (id != 0)
                {
                    jsonDeserializer.jsonFormatter = this;
                }

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
            else
            {
                var jsonDeserializer = new JsonDeserializer(chars, length);

                if (id != 0)
                {
                    jsonDeserializer.jsonFormatter = this;
                }

                return ValueInterface.GetInterface(type).Read(jsonDeserializer);
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return Deserialize<T>(hGCache.GetPointer(), length);
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = hGCache.Buffer(textReader);

                return Deserialize(hGCache.GetPointer(), length, type);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        BaseJsonSerializer CreateJsonSerializer()
        {
            var options = Options;

            if ((options & ReferenceOptions) != 0)
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    return new JsonReferenceSerializer(options)
                    {
                        indentedChars = IndentedChars,
                        lineBreakChars = LineBreakChars,
                        middleChars = MiddleChars
                    };
                }

                return new JsonReferenceSerializer(options);
            }
            else if ((options & ~JsonFormatterOptions.PriorCheckReferences) == JsonFormatterOptions.Default)
            {
                return new JsonDefaultSerializer(MaxDepth);
            }
            else
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    return new JsonSerializer(options, MaxDepth)
                    {
                        indentedChars = IndentedChars,
                        lineBreakChars = LineBreakChars,
                        middleChars = MiddleChars
                    };
                }
                
                return new JsonSerializer(options, MaxDepth); ;
            }
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
            using (var jsonSerializer = CreateJsonSerializer())
            {
                if (id != 0)
                {
                    jsonSerializer.jsonFormatter = this;
                }

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
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, TextWriter textWriter)
        {
            using (var jsonSerializer = CreateJsonSerializer())
            {
                if (id != 0)
                {
                    jsonSerializer.jsonFormatter = this;
                }

                ValueInterface<T>.WriteValue((IValueWriter)jsonSerializer, value);

                VersionDifferences.WriteChars(
                    textWriter,
                    jsonSerializer.hGlobal.GetPointer(),
                    jsonSerializer.StringLength);
            }
        }
    }
}