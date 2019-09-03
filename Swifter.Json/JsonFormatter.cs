using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Swifter.Formatters;

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
        /// 获取 JsonFormatter 使用的 HGlobalCache&lt;char&gt; 池。
        /// </summary>
        public static readonly HGlobalCachePool<char> CharsPool = new HGlobalCachePool<char>();

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

        /// <summary>
        /// 获取或设置字符编码。
        /// </summary>
        public Encoding Encoding { get; set; }

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
            return ValueInterface<T>.ReadValue(new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(char* chars, int length, Type type)
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T>(char* chars, int length, JsonFormatterOptions options)
        {
            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => DeserializeObject<T, JsonDeserializeModes.Reference>(chars, length),
                JsonFormatterOptions.DeflateDeserialize => DeserializeObject<T, JsonDeserializeModes.Deflate>(chars, length),
                JsonFormatterOptions.StandardDeserialize => DeserializeObject<T, JsonDeserializeModes.Standard>(chars, length),
                _ => DeserializeObject<T, JsonDeserializeModes.Verified>(chars, length),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(char* chars, int length, Type type, JsonFormatterOptions options)
        {
            return (options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => DeserializeObject<JsonDeserializeModes.Reference>(chars, length, type),
                JsonFormatterOptions.DeflateDeserialize => DeserializeObject<JsonDeserializeModes.Deflate>(chars, length, type),
                JsonFormatterOptions.StandardDeserialize => DeserializeObject<JsonDeserializeModes.Standard>(chars, length, type),
                _ => DeserializeObject<JsonDeserializeModes.Verified>(chars, length, type),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T, TMode>(char* chars, int length) where TMode : struct
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<TMode>(chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject<TMode>(char* chars, int length, Type type) where TMode : struct
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<TMode>(chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static IJsonReader CreateJsonReader(char* chars, int length)
        {
            return new JsonDeserializer<JsonDeserializeModes.Verified>(chars, length);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T>(char* chars, int length)
        {
            return (Options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => Deserialize<T, JsonDeserializeModes.Reference>(chars, length),
                JsonFormatterOptions.DeflateDeserialize => Deserialize<T, JsonDeserializeModes.Deflate>(chars, length),
                JsonFormatterOptions.StandardDeserialize => Deserialize<T, JsonDeserializeModes.Standard>(chars, length),
                _ => Deserialize<T, JsonDeserializeModes.Verified>(chars, length),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize(char* chars, int length, Type type)
        {
            return (Options & ModeOptions) switch
            {
                JsonFormatterOptions.MultiReferencingReference => Deserialize<JsonDeserializeModes.Reference>(chars, length, type),
                JsonFormatterOptions.DeflateDeserialize => Deserialize<JsonDeserializeModes.Deflate>(chars, length, type),
                JsonFormatterOptions.StandardDeserialize => Deserialize<JsonDeserializeModes.Standard>(chars, length, type),
                _ => Deserialize<JsonDeserializeModes.Verified>(chars, length, type),
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T, TMode>(char* chars, int length) where TMode : struct
        {
            return ValueInterface<T>.ReadValue(new JsonDeserializer<TMode>(this, chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize<TMode>(char* chars, int length, Type type) where TMode : struct
        {
            return ValueInterface.GetInterface(type).Read(new JsonDeserializer<TMode>(this, chars, length));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo<TMode>(char* chars, int length, IDataWriter dataWriter) where TMode : struct
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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo(char* chars, int length, IDataWriter dataWriter)
        {
            switch (Options & ModeOptions)
            {
                case JsonFormatterOptions.MultiReferencingReference:
                    DeserializeTo<JsonDeserializeModes.Reference>(chars, length, dataWriter);
                    break;
                case JsonFormatterOptions.DeflateDeserialize:
                    DeserializeTo<JsonDeserializeModes.Deflate>(chars, length, dataWriter);
                    break;
                case JsonFormatterOptions.StandardDeserialize:
                    DeserializeTo<JsonDeserializeModes.Standard>(chars, length, dataWriter);
                    break;
                default:
                    DeserializeTo<JsonDeserializeModes.Verified>(chars, length, dataWriter);
                    break;
            }
        }

        long ITargetedBind.TargetedId => targeted_id;

        void ITargetedBind.MakeTargetedId()
        {
            if (targeted_id == 0)
            {
                targeted_id = (long)Unsafe.AsPointer(ref Unsafe.AsRef(this));
            }
        }

        byte[] IBinaryFormatter.Serialize<T>(T value)
        {
            Serialize(value, out var bytes);
            return bytes;
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
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        public static IJsonReader CreateJsonReader(string text)
        {
            fixed (char* pText = text)
            {
                return CreateJsonReader(pText, text.Length);
            }
        }

#if NETCOREAPP && !NETCOREAPP2_0

        /// <summary>
        /// 创建 JSON 文档读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回一个 JSON 文档读取器</returns>
        public static IJsonReader CreateJsonReader(ReadOnlySpan<char> text)
        {
            fixed (char* pText = text)
            {
                return CreateJsonReader(pText, text.Length);
            }
        }

#endif

        /// <summary>
        /// 创建 JSON 文档写入器。注意：在写入器中请遵守规则写入，否则生成的 JSON 将不正常。
        /// </summary>
        /// <returns>返回一个 JSON 文档写入器</returns>
        public static IJsonWriter CreateJsonWriter(HGlobalCache<char> hGCache)
        {
            return new JsonSerializer<JsonSerializeModes.SimpleMode>(hGCache, DefaultMaxDepth);
        }
    }
}