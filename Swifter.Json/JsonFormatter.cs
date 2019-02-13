using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

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

        private long Id;

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
        public string LineBreak { get; set; } = DefaultLineBreak;

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
                if ( Id == 0)
                {
                    Id = (long)Pointer.UnBox(this);
                }

                ValueInterface<T>.SetTargetedInterface(Id, valueInterface);
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
            if (Id != 0)
            {
                ValueInterface.RemoveTargetedInterface(Id);

                Id = 0;
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
            var options = Options;
            var id = Id;

            fixed (char* chars = text)
            {
                if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
                {
                    var jsonDeserializer = new JsonReferenceDeserializer(chars, 0, text.Length);

                    if (id != 0)
                    {
                        jsonDeserializer.id = id;
                    }

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
                else
                {
                    var jsonDeserializer = new JsonDeserializer(chars, 0, text.Length);

                    if (id != 0)
                    {
                        jsonDeserializer.id = id;
                    }

                    return ValueInterface<T>.Content.ReadValue(jsonDeserializer);
                }
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(TextReader textReader)
        {
            return Deserialize<T>(textReader.ReadToEnd());
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(string text, Type type)
        {
            var options = Options;
            var id = Id;

            fixed (char* chars = text)
            {
                if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
                {
                    var jsonDeserializer = new JsonReferenceDeserializer(chars, 0, text.Length);

                    if (id != 0)
                    {
                        jsonDeserializer.id = id;
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
                    var jsonDeserializer = new JsonDeserializer(chars, 0, text.Length);

                    if (id != 0)
                    {
                        jsonDeserializer.id = id;
                    }

                    return ValueInterface.GetInterface(type).Read(jsonDeserializer);
                }
            }
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(TextReader textReader, Type type)
        {
            return Deserialize(textReader.ReadToEnd(), type);
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
            var options = Options;
            var id = Id;

            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonReferenceSerializer(options);

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = IndentedChars;
                    jsonSerializer.lineBreak = LineBreak;
                    jsonSerializer.middleChars = MiddleChars;
                }

                if (id != 0)
                {
                    jsonSerializer.id = id;
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
                var jsonSerializer = new JsonDefaultSerializer(MaxDepth);

                if (id != 0)
                {
                    jsonSerializer.id = id;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);
                
                return jsonSerializer.ToString();
            }
            else
            {
                var jsonSerializer = new JsonSerializer(options, MaxDepth);

                if (id != 0)
                {
                    jsonSerializer.id = id;
                }

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = IndentedChars;
                    jsonSerializer.lineBreak = LineBreak;
                    jsonSerializer.middleChars = MiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);
                
                return jsonSerializer.ToString();
            }
        }

        private void Fixed()
        {

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
            var options = Options;
            var id = Id;

            if ((options & ReferenceOptions) != 0)
            {
                var jsonSerializer = new JsonReferenceSerializer(options);

                if (id != 0)
                {
                    jsonSerializer.id = id;
                }

                jsonSerializer.textWriter = textWriter;

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = IndentedChars;
                    jsonSerializer.lineBreak = LineBreak;
                    jsonSerializer.middleChars = MiddleChars;
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
                var jsonSerializer = new JsonDefaultSerializer(MaxDepth);

                if (id != 0)
                {
                    jsonSerializer.id = id;
                }

                jsonSerializer.textWriter = textWriter;

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                jsonSerializer.WriteTo(textWriter);
            }
            else
            {
                var jsonSerializer = new JsonSerializer(options, MaxDepth);

                if (id != 0)
                {
                    jsonSerializer.id = id;
                }

                jsonSerializer.textWriter = textWriter;

                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.indentedChars = IndentedChars;
                    jsonSerializer.lineBreak = LineBreak;
                    jsonSerializer.middleChars = MiddleChars;
                }

                ValueInterface<T>.Content.WriteValue(jsonSerializer, value);

                jsonSerializer.WriteTo(textWriter);
            }
        }
    }
}