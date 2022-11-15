using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 文档格式化器。
    /// 此类所有的静态方法和实例方法都是线程安全的。
    /// </summary>
    public sealed unsafe partial class JsonFormatter : IBinaryFormatter, ITextFormatter, ITargetableValueRWSource
    {
#if !NO_OPTIONS
        internal const JsonFormatterOptions ComplexOptions =
            JsonFormatterOptions.IgnoreEmptyString |
            JsonFormatterOptions.IgnoreNull |
            JsonFormatterOptions.IgnoreZero |
            JsonFormatterOptions.OnFilter |
            JsonFormatterOptions.ArrayOnFilter |
            JsonFormatterOptions.Indented;

        internal const JsonFormatterOptions ReferenceOptions =
            JsonFormatterOptions.LoopReferencingException |
            JsonFormatterOptions.LoopReferencingNull |
            JsonFormatterOptions.MultiReferencingNull |
            JsonFormatterOptions.MultiReferencingReference;

        internal const JsonFormatterOptions ModeOptions =
            JsonFormatterOptions.MultiReferencingReference |
            JsonFormatterOptions.DeflateDeserialize |
            JsonFormatterOptions.StandardDeserialize |
            JsonFormatterOptions.VerifiedDeserialize;
#endif

        /// <summary>
        /// JsonFormatter 的全局针对目标的 Id。
        /// </summary>
        public const long GlobalTargetedId = unchecked((long)0x96F9A1AB7FE15EE6);

        /// <summary>
        /// 获取 JsonFormatter 使用的全局缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<char> CharsPool = HGlobalCacheExtensions.CharsPool;

        /// <summary>
        /// 获取或设置默认对象读写接口。
        /// 设置此值可以改变反序列化时的默认对象类型。
        /// </summary>
        public static ValueInterface DefaultObjectValueInterface = ValueInterface<Dictionary<string, object>>.Instance;

        /// <summary>
        /// 获取或设置默认数组读写接口。
        /// 设置此值可以改变反序列化时的默认数组类型。
        /// </summary>
        public static ValueInterface DefaultArrayValueInterface = ValueInterface<List<object>>.Instance;

#if !NO_OPTIONS
        /// <summary>
        /// 默认最大结构深度。
        /// 可以通过枚举 <see cref="JsonFormatterOptions.OutOfDepthException"/> 来配置序列化或反序列化时 Json 结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
#else
        /// <summary>
        /// 默认最大结构深度。
        /// </summary>
#endif
        public const int DefaultMaxDepth = 20;

#if !NO_OPTIONS
        /// <summary>
        /// 默认缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultIndentedChars = "  ";

        /// <summary>
        /// 默认换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultLineBreakChars = "\n";

        /// <summary>
        /// 默认 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public const string DefaultMiddleChars = " ";
#endif

#if !NO_OPTIONS
        /// <summary>
        /// 读取或设置最大结构深度。
        /// 可以通过枚举 <see cref="JsonFormatterOptions.OutOfDepthException"/> 来配置序列化或反序列化时 Json 结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
#else
        /// <summary>
        /// 读取或设置最大结构深度。
        /// </summary>
#endif
        public int MaxDepth { get; set; } = DefaultMaxDepth;

#if !NO_OPTIONS

        /// <summary>
        /// 读取或设置缩进符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string IndentedChars { get; set; } = DefaultIndentedChars;

        /// <summary>
        /// 读取或设置换行符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string LineBreakChars { get; set; } = DefaultLineBreakChars;

        /// <summary>
        /// 读取或设置 Key 与 Value 之间的分隔符，仅在枚举 JsonFormatterOptions 配置为 Indented (缩进美化) 时有效。
        /// </summary>
        public string MiddleChars { get; set; } = DefaultMiddleChars;

#endif
        /// <summary>
        /// 获取或设置对象读写接口。
        /// </summary>
        public ValueInterface? ObjectValueInterface;

        /// <summary>
        /// 获取或设置数组读写接口。
        /// </summary>
        public ValueInterface? ArrayValueInterface;

        /// <summary>
        /// 获取或设置字符编码。
        /// </summary>
        public Encoding Encoding { get; set; }

#if !NO_OPTIONS
        /// <summary>
        /// JSON 格式化器配置项。
        /// </summary>
        public JsonFormatterOptions Options { get; set; }

        JsonEventHandler<JsonFilteringEventArgs<string>>? objectFiltering;
        JsonEventHandler<JsonFilteringEventArgs<int>>? arrayFiltering;

        /// <summary>
        /// 当序列化对象字段时触发。
        /// </summary>
        public event JsonEventHandler<JsonFilteringEventArgs<string>> ObjectFiltering
        {
            add
            {

                Options |= JsonFormatterOptions.OnFilter;

            Loop:

                var comparand = Volatile.Read(ref objectFiltering);

                if (!ReferenceEquals(Interlocked.CompareExchange(ref objectFiltering, comparand + value, comparand), comparand))
                {
                    goto Loop;
                }
            }

            remove
            {

            Loop:

                var comparand = Volatile.Read(ref objectFiltering);
                var newValue = comparand - value;

                if (!ReferenceEquals(Interlocked.CompareExchange(ref objectFiltering, newValue, comparand), comparand))
                {
                    goto Loop;
                }

                if (newValue is null)
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

        internal bool OnObjectFilter(JsonSerializer jsonWriter, ValueFilterInfo<string> valueInfo, bool result)
        {
            if (objectFiltering is null)
            {
                return result;
            }

            var args = new JsonFilteringEventArgs<string>(jsonWriter, valueInfo, result);

            objectFiltering(this, ref args);

            return args.Result;
        }


        internal bool OnArrayFilter(JsonSerializer jsonWriter, ValueFilterInfo<int> valueInfo, bool result)
        {
            if (arrayFiltering is null)
            {
                return result;
            }

            var args = new JsonFilteringEventArgs<int>(jsonWriter, valueInfo, result);

            arrayFiltering(this, ref args);

            return args.Result;
        }
        
#endif

#if !NO_OPTIONS
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
#else
        /// <summary>
        /// 初始化具有指定编码和指定配置项的 Json 格式化器。
        /// </summary>
        /// <param name="encoding">指定编码</param>
        public JsonFormatter(Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        /// <summary>
        /// 初始化默认编码 (UTF-8) 和指定配置项的 Json 格式化器。
        /// </summary>
        public JsonFormatter() : this(Encoding.UTF8)
        {

        }
#endif


        /// <summary>
        /// 将指定类型的实例序列化到 Json 缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, HGlobalCache<char> hGCache)
        {
            var jsonSerializer = new JsonSerializer(hGCache);

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
        public static void SerializeObject<T>(T? value, TextWriter textWriter)
        {
            var hGChars = CharsPool.Rent();

            var jsonSerializer = new JsonSerializer(
                new JsonSegmentedContent(textWriter, hGChars, false)
                );

            ValueInterface.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T? value)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars);

            var ret = hGChars.ToStringEx();

            CharsPool.Return(hGChars);

            return ret;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 Json 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="encoding">指定编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T? value, Encoding encoding)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars);

            var ret = hGChars.ToBytes(encoding);

            CharsPool.Return(hGChars);

            return ret;
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, HGlobalCache<byte> hGCache, Encoding encoding)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars);

            hGCache.ReadFrom(hGChars, encoding);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">Json 输出流</param>
        /// <param name="encoding">指定编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, Stream stream, Encoding encoding)
        {
            var streamWriter = new StreamWriter(stream, encoding);

            SerializeObject(value, streamWriter);

            streamWriter.Flush();
        }


        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string Serialize<T>(T? value)
        {
            var hGChars = CharsPool.Rent();

            Serialize(value, hGChars);

            var ret = hGChars.ToStringEx();

            CharsPool.Return(hGChars);

            return ret;
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T? value, HGlobalCache<char> hGCache)
        {
            var jsonSerializer = new JsonSerializer(this, hGCache);

            ValueInterface.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T? value, TextWriter textWriter)
        {
            var hGChars = CharsPool.Rent();

            var jsonSerializer = new JsonSerializer(this, new JsonSegmentedContent(textWriter, hGChars, false));

            ValueInterface.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 字节数组中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="bytes">Json 字节数组</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T? value, out byte[] bytes)
        {
            var hGChars = CharsPool.Rent();

            Serialize(value, hGChars);

            bytes = hGChars.ToBytes(Encoding);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 字节缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T? value, HGlobalCache<byte> hGCache)
        {
            var hGChars = CharsPool.Rent();

            Serialize(value, hGChars);

            hGCache.ReadFrom(hGChars, Encoding);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">Json 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T? value, Stream stream)
        {
            var streamWriter = new StreamWriter(stream, Encoding);

            Serialize(value, streamWriter);

            streamWriter.Flush();
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe T? DeserializeObject<T>(string text)
        {
            fixed (char* chars = text)
            {
                return ValueInterface<T>.ReadValue(
                    new JsonDeserializer(chars, text.Length)
                    );
            }
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe object? DeserializeObject(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.ReadValue(
                    new JsonDeserializer(chars, text.Length),
                    type
                    );
            }
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(HGlobalCache<char> hGCache, Type type)
        {
            return ValueInterface.ReadValue(
                new JsonDeserializer(hGCache.First, hGCache.Count),
                type
                );
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(HGlobalCache<char> hGCache)
        {
            return ValueInterface<T>.ReadValue(
                new JsonDeserializer(hGCache.First, hGCache.Count)
                );
        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(TextReader textReader)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface<T>.ReadValue(
                new JsonDeserializer(
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars)
                    )
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(TextReader textReader, Type type)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface.ReadValue(
                new JsonDeserializer(
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars)
                    ),
                type
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(ArraySegment<byte> bytes, Encoding encoding, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject(hGChars, type);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(byte[] bytes, Encoding encoding, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject(hGChars, type);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(HGlobalCache<byte> hGCache, Encoding encoding, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, encoding);

            var value = DeserializeObject(hGChars, type);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(Stream stream, Encoding encoding, Type type)
        {
            return DeserializeObject(new StreamReader(stream, encoding), type);
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(ArraySegment<byte> bytes, Encoding encoding)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(byte[] bytes, Encoding encoding)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(HGlobalCache<byte> hGCache, Encoding encoding)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, encoding);

            var value = DeserializeObject<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(Stream stream, Encoding encoding)
        {
            return DeserializeObject<T>(new StreamReader(stream, encoding));
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(HGlobalCache<char> hGCache, Type type)
        {
            return ValueInterface.ReadValue(
                new JsonDeserializer(this, hGCache.First, hGCache.Count),
                type
                );

        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(TextReader textReader, Type type)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface.ReadValue(
                new JsonDeserializer(
                    this,
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars)
                    ),
                type
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(string text)
        {
            fixed (char* chars = text)
            {
                return ValueInterface<T>.ReadValue(
                    new JsonDeserializer(this, chars, text.Length)
                    );
            }
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(HGlobalCache<char> hGCache)
        {
            return ValueInterface<T>.ReadValue(
                new JsonDeserializer(this, hGCache.First, hGCache.Count)
                );

        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(TextReader textReader)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface<T>.ReadValue(
                new JsonDeserializer(
                    this, 
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars)
                    )
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(string text, Type type)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.ReadValue(
                    new JsonDeserializer(this, chars, text.Length),
                    type
                    );
            }
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(ArraySegment<byte> bytes, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, Encoding);

            var value = Deserialize(hGChars, type);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(byte[] bytes, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, Encoding);

            var value = Deserialize(hGChars, type);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(HGlobalCache<byte> hGCache, Type type)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, Encoding);

            var value = Deserialize(hGChars, type);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? Deserialize(Stream stream, Type type)
        {
            return Deserialize(new StreamReader(stream, Encoding), type);
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(ArraySegment<byte> bytes)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, Encoding);

            var value = Deserialize<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(byte[] bytes)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, Encoding);

            var value = Deserialize<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(HGlobalCache<byte> hGCache)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, Encoding);

            var value = Deserialize<T>(hGChars);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? Deserialize<T>(Stream stream)
        {
            return Deserialize<T>(new StreamReader(stream, Encoding));

        }

        /// <summary>
        /// 将 Json 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(string text, IDataWriter dataWriter)
        {
            fixed (char* chars = text)
            {
                new JsonDeserializer(this, chars, text.Length)
                    .DeserializeTo(dataWriter);
            }
        }

        /// <summary>
        /// 将 Json 缓存反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(HGlobalCache<char> hGCache, IDataWriter dataWriter)
        {
            new JsonDeserializer(this, hGCache.First, hGCache.Count)
                .DeserializeTo(dataWriter);
        }

        /// <summary>
        /// 将 Json 读取器反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(TextReader textReader, IDataWriter dataWriter)
        {
            var hGChars = CharsPool.Rent();

            new JsonDeserializer(
                this,
                JsonSegmentedContent.CreateAndInitialize(textReader, hGChars)
                )
                .DeserializeTo(dataWriter);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将 Json 字节数组反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(ArraySegment<byte> bytes, IDataWriter dataWriter)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, Encoding);

            DeserializeTo(hGChars, dataWriter);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将 Json 字节缓存反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(HGlobalCache<byte> hGCache, IDataWriter dataWriter)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, Encoding);

            DeserializeTo(hGChars, dataWriter);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将 Json 输入流反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(Stream stream, IDataWriter dataWriter)
        {
            DeserializeTo(new StreamReader(stream, Encoding), dataWriter);
        }

#if !NO_OPTIONS

        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">Json 写入器</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, TextWriter textWriter, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            var jsonSerializer = new JsonSerializer(
                new JsonSegmentedContent(textWriter, hGChars, false),
                options
                );

            ValueInterface.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T? value, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars, options);

            var ret = hGChars.ToStringEx();

            CharsPool.Return(hGChars);

            return ret;
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, HGlobalCache<char> hGCache, JsonFormatterOptions options)
        {
            var jsonSerializer = new JsonSerializer(hGCache, options);

            ValueInterface.WriteValue(jsonSerializer, value);

            jsonSerializer.Flush();
        }

        /// <summary>
        /// 将指定类型的实例序列化为 Json 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T? value, Encoding encoding, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars, options);

            var ret = hGChars.ToBytes(encoding);

            CharsPool.Return(hGChars);

            return ret;
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, HGlobalCache<byte> hGCache, Encoding encoding, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            SerializeObject(value, hGChars, options);

            hGCache.ReadFrom(hGChars, encoding);

            CharsPool.Return(hGChars);
        }

        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">Json 输出流</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T? value, Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            var streamWriter = new StreamWriter(stream, encoding);

            SerializeObject(value, streamWriter, options);

            streamWriter.Flush();
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(string text, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return ValueInterface<T>.ReadValue(
                    new JsonDeserializer(chars, text.Length, options)
                    );
            }
        }

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(string text, Type type, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return ValueInterface.ReadValue(
                    new JsonDeserializer(chars, text.Length, options),
                    type
                    );
            }
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(HGlobalCache<char> hGCache, Type type, JsonFormatterOptions options)
        {
            return ValueInterface.ReadValue(
                new JsonDeserializer(hGCache.First, hGCache.Count, options),
                type
                );
        }

        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(HGlobalCache<char> hGCache, JsonFormatterOptions options)
        {
            return ValueInterface<T>.ReadValue(
                new JsonDeserializer(hGCache.First, hGCache.Count, options)
                );

        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(TextReader textReader, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface<T>.ReadValue(
                new JsonDeserializer(
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars),
                    options
                    )
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface.ReadValue(
                new JsonDeserializer(
                    JsonSegmentedContent.CreateAndInitialize(textReader, hGChars),
                    options
                    ),
                type
                );

            CharsPool.Return(hGChars);

            return value;
        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(ArraySegment<byte> bytes, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject(hGChars, type, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(byte[] bytes, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject(hGChars, type, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(HGlobalCache<byte> hGCache, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, encoding);

            var value = DeserializeObject(hGChars, type, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="encoding">指定编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? DeserializeObject(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            return DeserializeObject(new StreamReader(stream, encoding), type, options);

        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(ArraySegment<byte> bytes, Encoding encoding, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject<T>(hGChars, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(byte[] bytes, Encoding encoding, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(bytes, encoding);

            var value = DeserializeObject<T>(hGChars, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 字节缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(HGlobalCache<byte> hGCache, Encoding encoding, JsonFormatterOptions options)
        {
            var hGChars = CharsPool.Rent();

            hGChars.ReadFrom(hGCache, encoding);

            var value = DeserializeObject<T>(hGChars, options);

            CharsPool.Return(hGChars);

            return value;

        }

        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="encoding">指定编码</param>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? DeserializeObject<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            return DeserializeObject<T>(new StreamReader(stream, encoding), options);
        }
#endif

        #region -- 接口实现 --

        TargetableValueInterfaceMapSource ITargetableValueRWSource.ValueInterfaceMapSource { get; } = new TargetableValueInterfaceMapSource();

#pragma warning disable
        byte[] IBinaryFormatter.Serialize<T>(T value)
        {
            Serialize(value, out var bytes);

            return bytes;
        }
#pragma warning restor

#endregion
    }
}