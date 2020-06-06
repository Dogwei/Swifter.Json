using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
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
    public sealed unsafe partial class JsonFormatter : IBinaryFormatter, ITextFormatter, ITargetedBind
    {
        const JsonFormatterOptions ComplexOptions =
            JsonFormatterOptions.IgnoreEmptyString |
            JsonFormatterOptions.IgnoreNull |
            JsonFormatterOptions.IgnoreZero |
            JsonFormatterOptions.OnFilter |
            JsonFormatterOptions.ArrayOnFilter |
            JsonFormatterOptions.Indented;

        const JsonFormatterOptions ReferenceOptions =
            JsonFormatterOptions.LoopReferencingException |
            JsonFormatterOptions.LoopReferencingNull |
            JsonFormatterOptions.MultiReferencingNull |
            JsonFormatterOptions.MultiReferencingReference;

        const JsonFormatterOptions ModeOptions =
            JsonFormatterOptions.MultiReferencingReference |
            JsonFormatterOptions.DeflateDeserialize |
            JsonFormatterOptions.StandardDeserialize |
            JsonFormatterOptions.VerifiedDeserialize;

        /// <summary>
        /// JsonFormatter 的全局针对目标的 Id。
        /// </summary>
        public const long GlobalTargetedId = unchecked((long)0x96F9A1AB7FE15EE6);


        /// <summary>
        /// 获取 JsonFormatter 使用的全局缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<char> CharsPool = HGlobalCacheExtensions.CharsPool;

        /// <summary>
        /// 默认最大结构深度。
        /// 可以通过枚举 <see cref="JsonFormatterOptions.OutOfDepthException"/> 来配置序列化或反序列化时 Json 结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public const int DefaultMaxDepth = 20;

        /// <summary>
        /// 默认缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultIndentedChars = "  ";

        /// <summary>
        /// 默认换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultLineCharsBreak = "\n";

        /// <summary>
        /// 默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultMiddleChars = " ";

        /// <summary>
        /// 读取或设置最大结构深度。
        /// 可以通过枚举 <see cref="JsonFormatterOptions.OutOfDepthException"/> 来配置序列化或反序列化时 Json 结构深度超出该值时选择抛出异常还是不解析超出部分。
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

        /// <summary>
        /// 获取或设置字符编码。
        /// </summary>
        public Encoding Encoding { get; set; }

        JsonEventHandler<JsonFilteringEventArgs<string>> objectFiltering;
        JsonEventHandler<JsonFilteringEventArgs<int>> arrayFiltering;

        /// <summary>
        /// 当序列化对象字段时触发。
        /// </summary>
        public event JsonEventHandler<JsonFilteringEventArgs<string>> ObjectFiltering
        {
            add
            {
                Options |= JsonFormatterOptions.OnFilter;

                objectFiltering += value;
            }

            remove
            {
                objectFiltering -= value;

                if (objectFiltering is null)
                {
                    Options &= ~JsonFormatterOptions.OnFilter;
                }
            }
        }

        /// <summary>
        /// 当序列化数组元素时触发。
        /// </summary>
        public event JsonEventHandler<JsonFilteringEventArgs<int>> ArrayFiltering
        {
            add
            {
                Options |= JsonFormatterOptions.ArrayOnFilter;

                arrayFiltering += value;
            }

            remove
            {
                arrayFiltering -= value;

                if (arrayFiltering is null)
                {
                    Options &= ~JsonFormatterOptions.ArrayOnFilter;
                }
            }
        }

        internal bool OnObjectFilter(IJsonWriter jsonWriter, ValueFilterInfo<string> valueInfo, bool result)
        {
            if (objectFiltering is null)
            {
                return result;
            }

            var args = new JsonFilteringEventArgs<string>(jsonWriter, valueInfo, result);

            objectFiltering(this, ref args);

            return args.Result;
        }


        internal bool OnArrayFilter(IJsonWriter jsonWriter, ValueFilterInfo<int> valueInfo, bool result)
        {
            if (arrayFiltering is null)
            {
                return result;
            }

            var args = new JsonFilteringEventArgs<int>(jsonWriter, valueInfo, result);

            arrayFiltering(this, ref args);

            return args.Result;
        }

        /// <summary>
        /// 作为自定义值读写接口的 Id。
        /// </summary>
        internal long targeted_id;

        /// <summary>
        /// 初始化具有指定编码和指定配置项的 Json 格式化器。
        /// </summary>
        /// <param name="encoding">指定编码</param>
        /// <param name="options">指定配置项</param>
        public JsonFormatter(Encoding encoding, JsonFormatterOptions options = JsonFormatterOptions.Default)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            Options = options;
        }

        /// <summary>
        /// 初始化默认编码 (UTF-8) 和指定配置项的 Json 格式化器。
        /// </summary>
        public JsonFormatter(JsonFormatterOptions options = JsonFormatterOptions.Default) : this(Encoding.UTF8, options)
        {

        }

        /// <summary>
        /// 释放对象时移除读写接口实例。
        /// </summary>
        ~JsonFormatter()
        {
            if (targeted_id != 0)
            {
                ValueInterface.RemoveTargetedInterface(this);

                targeted_id = 0;
            }
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, HGlobalCache<char> hGCache)
        {
            var jsonSerializer = new JsonSerializer<JsonSerializeModes.SimpleMode>(hGCache, DefaultMaxDepth);

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter)
        {
            var hGChars = CharsPool.Rent();

            var jsonSerializer = new JsonSerializer<JsonSerializeModes.SimpleMode>(hGChars, DefaultMaxDepth, textWriter);

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();

            hGChars.WriteTo(textWriter);

            CharsPool.Return(hGChars);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static void SerializeObject<T, TMode>(T value, HGlobalCache<char> hGCache, JsonFormatterOptions options) where TMode : struct
        {
            var jsonSerializer = new JsonSerializer<TMode>(hGCache, DefaultMaxDepth, options);

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                jsonSerializer.References = new JsonReferenceWriter();
            }

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode) || typeof(TMode) == typeof(JsonSerializeModes.ComplexMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }
            }

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static void SerializeObject<T, TMode>(T value, HGlobalCache<char> hGCache, TextWriter textWriter, JsonFormatterOptions options) where TMode : struct
        {
            var jsonSerializer = new JsonSerializer<TMode>(hGCache, DefaultMaxDepth, textWriter, options);

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                jsonSerializer.References = new JsonReferenceWriter();
            }

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode) || typeof(TMode) == typeof(JsonSerializeModes.ComplexMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = DefaultIndentedChars;
                    jsonSerializer.LineBreakChars = DefaultLineCharsBreak;
                    jsonSerializer.MiddleChars = DefaultMiddleChars;
                }
            }

            ValueInterface<T>.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Serialize<T, TMode>(T value, HGlobalCache<char> hGCache, JsonFormatterOptions options) where TMode : struct
        {
            var jsonSerializer = new JsonSerializer<TMode>(this, hGCache, MaxDepth, options);

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                jsonSerializer.References = new JsonReferenceWriter();
            }

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode) || typeof(TMode) == typeof(JsonSerializeModes.ComplexMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }
            }

            ValueInterface<T>.WriteValue(jsonSerializer, value);


            jsonSerializer.Flush();
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Serialize<T, TMode>(T value, HGlobalCache<char> hGCache, TextWriter textWriter, JsonFormatterOptions options) where TMode : struct
        {
            var jsonSerializer = new JsonSerializer<TMode>(this, hGCache, MaxDepth, textWriter, options);

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                jsonSerializer.References = new JsonReferenceWriter();
            }

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode) || typeof(TMode) == typeof(JsonSerializeModes.ComplexMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    jsonSerializer.IndentedChars = IndentedChars;
                    jsonSerializer.LineBreakChars = LineBreakChars;
                    jsonSerializer.MiddleChars = MiddleChars;
                }
            }

            ValueInterface<T>.WriteValue(jsonSerializer, value);


            jsonSerializer.Flush();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T>(char* chars, int length)
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length, DefaultMaxDepth));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(Type type, char* chars, int length)
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length, DefaultMaxDepth));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T>(char* chars, int length, JsonFormatterOptions options)
        {
            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => DeserializeObject<T, JsonDeserializeModes.Reference>(chars, length, options),
                JsonFormatterOptions.DeflateDeserialize => DeserializeObject<T, JsonDeserializeModes.Deflate>(chars, length, options),
                JsonFormatterOptions.StandardDeserialize => DeserializeObject<T, JsonDeserializeModes.Standard>(chars, length, options),
                _ => DeserializeObject<T, JsonDeserializeModes.Verified>(chars, length, options),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(Type type, char* chars, int length, JsonFormatterOptions options)
        {
            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => DeserializeObject<JsonDeserializeModes.Reference>(type, chars, length, options),
                JsonFormatterOptions.DeflateDeserialize => DeserializeObject<JsonDeserializeModes.Deflate>(type, chars, length, options),
                JsonFormatterOptions.StandardDeserialize => DeserializeObject<JsonDeserializeModes.Standard>(type, chars, length, options),
                _ => DeserializeObject<JsonDeserializeModes.Verified>(type, chars, length, options),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T, TMode>(char* chars, int length, JsonFormatterOptions options) where TMode : struct
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<TMode>(chars, length, DefaultMaxDepth, options));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject<TMode>(Type type, char* chars, int length, JsonFormatterOptions options) where TMode : struct
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<TMode>(chars, length, DefaultMaxDepth, options));
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="chars">JSON 字符串</param>
        /// <param name="length">JSON 字符串长度</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(char* chars, int length)
        {
            return new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length, DefaultMaxDepth);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T>(char* chars, int length)
        {
            var options = Options;

            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => Deserialize<T, JsonDeserializeModes.Reference>(chars, length, options),
                JsonFormatterOptions.DeflateDeserialize => Deserialize<T, JsonDeserializeModes.Deflate>(chars, length, options),
                JsonFormatterOptions.StandardDeserialize => Deserialize<T, JsonDeserializeModes.Standard>(chars, length, options),
                _ => Deserialize<T, JsonDeserializeModes.Verified>(chars, length, options),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize(Type type, char* chars, int length)
        {
            var options = Options;

            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => Deserialize<JsonDeserializeModes.Reference>(type, chars, length, options),
                JsonFormatterOptions.DeflateDeserialize => Deserialize<JsonDeserializeModes.Deflate>(type, chars, length, options),
                JsonFormatterOptions.StandardDeserialize => Deserialize<JsonDeserializeModes.Standard>(type, chars, length, options),
                _ => Deserialize<JsonDeserializeModes.Verified>(type, chars, length, options),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T, TMode>(char* chars, int length, JsonFormatterOptions options) where TMode : struct
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<TMode>(this, chars, length, MaxDepth, options));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize<TMode>(Type type, char* chars, int length, JsonFormatterOptions options) where TMode : struct
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<TMode>(this, chars, length, MaxDepth, options));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo<TMode>(char* chars, int length, JsonFormatterOptions options, IDataWriter dataWriter) where TMode : struct
        {
            var jsonDeserializer = new JsonDeserializer<TMode>(this, chars, length, MaxDepth, options);

            switch (jsonDeserializer.GetToken())
            {
                case JsonToken.Object:
                    if (dataWriter is IDataWriter<string> && dataWriter is IDataWriter<Ps<char>> fastWriter)
                    {
                        jsonDeserializer.FastReadObject(fastWriter);
                    }
                    else
                    {
                        jsonDeserializer.SlowReadObject(dataWriter.As<string>());
                    }
                    break;
                case JsonToken.Array:
                    jsonDeserializer.SlowReadArray(dataWriter.As<int>());
                    break;
                default:
                    dataWriter.Content = XConvert.Cast(jsonDeserializer.DirectRead(), dataWriter.ContentType);
                    break;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo(char* chars, int length, IDataWriter dataWriter)
        {
            var options = Options;

            switch (options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    DeserializeTo<JsonDeserializeModes.Reference>(chars, length, options, dataWriter);
                    break;
                case JsonFormatterOptions.DeflateDeserialize:
                    DeserializeTo<JsonDeserializeModes.Deflate>(chars, length, options, dataWriter);
                    break;
                case JsonFormatterOptions.StandardDeserialize:
                    DeserializeTo<JsonDeserializeModes.Standard>(chars, length, options, dataWriter);
                    break;
                default:
                    DeserializeTo<JsonDeserializeModes.Verified>(chars, length, options, dataWriter);
                    break;
            }
        }

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="hGCache">JSON 内容缓存</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonReader CreateJsonReader(HGlobalCache<char> hGCache)
        {
            return CreateJsonReader(hGCache.First, hGCache.Count);
        }

        /// <summary>
        /// 创建 JSON 文档写入器。注意：在写入器中请遵守规则写入，否则生成的 JSON 将不正常。
        /// </summary>
        /// <returns>返回一个 JSON 文档写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IJsonWriter CreateJsonWriter(HGlobalCache<char> hGCache)
        {
            return new JsonSerializer<JsonSerializeModes.SimpleMode>(hGCache, DefaultMaxDepth);
        }

        long ITargetedBind.TargetedId => targeted_id;

        void ITargetedBind.MakeTargetedId()
        {
            if (targeted_id == 0)
            {
                targeted_id = (long)Underlying.AsPointer(ref Underlying.AsRef(this));
            }
        }

        byte[] IBinaryFormatter.Serialize<T>(T value)
        {
            Serialize(value, out var bytes);

            return bytes;
        }
    }
}