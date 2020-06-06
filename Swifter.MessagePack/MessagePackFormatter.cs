using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 文档格式化器。
    /// 此类所有的静态方法和实例方法都是线程安全的。
    /// </summary>
    public sealed unsafe partial class MessagePackFormatter : IBinaryFormatter, ITargetedBind
    {
        /// <summary>
        /// 全局针对目标的 Id。
        /// </summary>
        public const long GlobalTargetedId = unchecked((long)0x96F9A1AB7FE15EE6);

        const MessagePackFormatterOptions ReferenceSerializerOptions =
            MessagePackFormatterOptions.LoopReferencingException |
            MessagePackFormatterOptions.LoopReferencingNull |
            MessagePackFormatterOptions.MultiReferencingNull |
            MessagePackFormatterOptions.MultiReferencingReference;

        const MessagePackFormatterOptions ReferenceDeserializerOptions =
            MessagePackFormatterOptions.MultiReferencingReference;

        /// <summary>
        /// 获取 JsonFormatter 使用的全局缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<byte> BytesPool = HGlobalCacheExtensions.BytesPool;

        /// <summary>
        /// 读取或设置默认最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 MessagePackFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public static int DefaultMaxDepth { get; set; } = 20;

        /// <summary>
        /// 读取或设置最大结构深度。
        /// 此值只在序列化时有效。
        /// 可以通过枚举 MessagePackFormatterOptions 来配置序列化 (Serialize) 时结构深度超出该值时选择抛出异常还是不解析超出部分。
        /// </summary>
        public int MaxDepth { get; set; } = DefaultMaxDepth;

        /// <summary>
        /// MessagePack 格式化器配置项。
        /// </summary>
        public MessagePackFormatterOptions Options { get; set; }

        /// <summary>
        /// 作为自定义值读写接口的 Id。
        /// </summary>
        internal long targeted_id;


        /// <summary>
        /// 初始化具有指定配置项的 MessagePack 格式化器。
        /// </summary>
        /// <param name="options">指定配置项</param>
        public MessagePackFormatter(MessagePackFormatterOptions options = MessagePackFormatterOptions.Default)
        {
            Options = options;
        }

        /// <summary>
        /// 释放对象时移除读写接口实例。
        /// </summary>
        ~MessagePackFormatter()
        {
            if (targeted_id != 0)
            {
                ValueInterface.RemoveTargetedInterface(this);

                targeted_id = 0;
            }
        }

        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="hGCache">MessagePack 字节缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, HGlobalCache<byte> hGCache)
        {
            var messagePackSerializer = new MessagePackSerializer<MessagePackSerializeModes.StandardMode>(hGCache, DefaultMaxDepth);

            ValueInterface<T>.WriteValue(messagePackSerializer, value);

            messagePackSerializer.Flush();
        }





        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, Stream stream)
        {
            var hGBytes = BytesPool.Rent();

            var messagePackSerializer = new MessagePackSerializer<MessagePackSerializeModes.StandardMode>(hGBytes, DefaultMaxDepth);

            ValueInterface<T>.WriteValue(messagePackSerializer, value);

            messagePackSerializer.Flush();

            hGBytes.WriteTo(stream);

            BytesPool.Return(hGBytes);

        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static void SerializeObject<T, TMode>(T value, HGlobalCache<byte> hGCache, MessagePackFormatterOptions options) where TMode : struct
        {
            var messagePackSerializer = new MessagePackSerializer<TMode>(hGCache, DefaultMaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackSerializeModes.ReferenceMode))
            {
                messagePackSerializer.InitReferences();
            }

            ValueInterface<T>.WriteValue(messagePackSerializer, value);

            messagePackSerializer.Flush();
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Serialize<T, TMode>(T value, HGlobalCache<byte> hGCache, MessagePackFormatterOptions options) where TMode : struct
        {
            var messagePackSerializer = new MessagePackSerializer<TMode>(this, hGCache, MaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackSerializeModes.ReferenceMode))
            {
                messagePackSerializer.InitReferences();
            }

            ValueInterface<T>.WriteValue(messagePackSerializer, value);

            messagePackSerializer.Flush();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T>(byte* bytes, int length)
        {
            return ValueInterface<T>.ReadValue(new MessagePackDeserializer<MessagePackDeserializeModes.StandardMode>(bytes, length, DefaultMaxDepth));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(byte* bytes, int length, Type type)
        {
            return ValueInterface.GetInterface(type).Read(new MessagePackDeserializer<MessagePackDeserializeModes.StandardMode>(bytes, length, DefaultMaxDepth));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T>(byte* bytes, int length, MessagePackFormatterOptions options)
        {
            if ((options & ReferenceDeserializerOptions) != 0)
            {
                return DeserializeObject<T, MessagePackDeserializeModes.ReferenceMode>(bytes, length, options);
            }
            else
            {
                return DeserializeObject<T, MessagePackDeserializeModes.StandardMode>(bytes, length, options);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject(byte* bytes, int length, Type type, MessagePackFormatterOptions options)
        {
            if ((options & ReferenceDeserializerOptions) != 0)
            {
                return DeserializeObject<MessagePackDeserializeModes.ReferenceMode>(bytes, length, type, options);
            }
            else
            {
                return DeserializeObject<MessagePackDeserializeModes.StandardMode>(bytes, length, type, options);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static T DeserializeObject<T, TMode>(byte* bytes, int length, MessagePackFormatterOptions options) where TMode : struct
        {
            var deserializer = new MessagePackDeserializer<TMode>(bytes, length, DefaultMaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackDeserializeModes.ReferenceMode))
            {
                deserializer.InitReferences();
            }
            
            return ValueInterface<T>.ReadValue(deserializer);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static object DeserializeObject<TMode>(byte* bytes, int length, Type type, MessagePackFormatterOptions options) where TMode : struct
        {
            var deserializer = new MessagePackDeserializer<TMode>(bytes, length, DefaultMaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackDeserializeModes.ReferenceMode))
            {
                deserializer.InitReferences();
            }

            return ValueInterface.GetInterface(type).Read(deserializer);
        }

        /// <summary>
        /// 创建 MessagePack 内容读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="bytes">MessagePack 内容</param>
        /// <param name="length">MessagePack 内容长度</param>
        /// <returns>返回一个 MessagePack 内容读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IMessagePackReader CreateMessagePackReader(byte* bytes, int length)
        {
            return new MessagePackDeserializer<MessagePackDeserializeModes.StandardMode>(bytes, length, DefaultMaxDepth);
        }

        /// <summary>
        /// 创建 MessagePack 内容读取器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <param name="hGCache">MessagePack 内容缓存</param>
        /// <returns>返回一个 MessagePack 内容读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IMessagePackReader CreateMessagePackReader(HGlobalCache<byte> hGCache)
        {
            return CreateMessagePackReader(hGCache.First, hGCache.Count);
        }

        /// <summary>
        /// 创建 MessagePack 内容写入器。注意：在写入器中请遵守规则写入，否则生成的 MessagePack 内容将不正常。
        /// </summary>
        /// <returns>返回一个 MessagePack 内容写入器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IMessagePackWriter CreateMessagePackWriter(HGlobalCache<byte> hGCache)
        {
            return new MessagePackSerializer<MessagePackSerializeModes.StandardMode>(hGCache, DefaultMaxDepth);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T>(byte* bytes, int length)
        {
            var options = Options;

            if ((options & ReferenceDeserializerOptions) != 0)
            {
                return Deserialize<T, MessagePackDeserializeModes.ReferenceMode>(bytes, length, options);
            }
            else
            {
                return Deserialize<T, MessagePackDeserializeModes.StandardMode>(bytes, length, options);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize(byte* bytes, int length, Type type)
        {
            var options = Options;

            if ((options & ReferenceDeserializerOptions) != 0)
            {
                return Deserialize<MessagePackDeserializeModes.ReferenceMode>(bytes, length, type, options);
            }
            else
            {
                return Deserialize<MessagePackDeserializeModes.StandardMode>(bytes, length, type, options);
            }
        }

        MessagePackEventHandler<MessagePackFilteringEventArgs<string>> objectFiltering;
        MessagePackEventHandler<MessagePackFilteringEventArgs<int>> arrayFiltering;

        /// <summary>
        /// 当序列化对象字段时触发。
        /// </summary>
        public event MessagePackEventHandler<MessagePackFilteringEventArgs<string>> ObjectFiltering
        {
            add
            {
                Options |= MessagePackFormatterOptions.OnFilter;

                objectFiltering += value;
            }

            remove
            {
                objectFiltering -= value;

                if (objectFiltering is null)
                {
                    Options &= ~MessagePackFormatterOptions.OnFilter;
                }
            }
        }

        /// <summary>
        /// 当序列化数组元素时触发。
        /// </summary>
        public event MessagePackEventHandler<MessagePackFilteringEventArgs<int>> ArrayFiltering
        {
            add
            {
                Options |= MessagePackFormatterOptions.ArrayOnFilter;

                arrayFiltering += value;
            }

            remove
            {
                arrayFiltering -= value;

                if (arrayFiltering is null)
                {
                    Options &= ~MessagePackFormatterOptions.ArrayOnFilter;
                }
            }
        }

        internal bool OnObjectFilter(IMessagePackWriter messagePackWriter, ValueFilterInfo<string> valueInfo, bool result) 
        {
            if (objectFiltering is null)
            {
                return result;
            }

            var args = new MessagePackFilteringEventArgs<string>(messagePackWriter, valueInfo, result);

            objectFiltering(this, ref args);

            return args.Result;
        }

        internal bool OnArrayFilter(IMessagePackWriter messagePackWriter, ValueFilterInfo<int> valueInfo, bool result)
        {
            if (arrayFiltering is null)
            {
                return result;
            }

            var args = new MessagePackFilteringEventArgs<int>(messagePackWriter, valueInfo, result);

            arrayFiltering(this, ref args);

            return args.Result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T Deserialize<T, TMode>(byte* bytes, int length, MessagePackFormatterOptions options) where TMode : struct
        {
            var deserializer = new MessagePackDeserializer<TMode>(this, bytes, length, MaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackDeserializeModes.ReferenceMode))
            {
                deserializer.InitReferences();
            }

            return ValueInterface<T>.ReadValue(deserializer);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        object Deserialize<TMode>(byte* bytes, int length, Type type, MessagePackFormatterOptions options) where TMode : struct
        {
            var deserializer = new MessagePackDeserializer<TMode>(this, bytes, length, MaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackDeserializeModes.ReferenceMode))
            {
                deserializer.InitReferences();
            }

            return ValueInterface.GetInterface(type).Read(deserializer);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo<TMode>(byte* bytes, int length, IDataWriter dataWriter, MessagePackFormatterOptions options) where TMode : struct
        {
            var deserializer = new MessagePackDeserializer<TMode>(this, bytes, length, MaxDepth, options);

            if (typeof(TMode) == typeof(MessagePackDeserializeModes.ReferenceMode))
            {
                deserializer.InitReferences();
            }

            switch (deserializer.GetToken())
            {
                case MessagePackToken.Map:
                    if (dataWriter is IDataWriter<string> objectWriter)
                    {
                        deserializer.InternalReadObject(objectWriter);
                    }
                    else
                    {
                        ((IAsDataWriter)dataWriter.As<string>()).InvokeTIn(new MessagePackDeserializer<TMode>.InternalWriteMapInvoker(deserializer, dataWriter));
                    }
                    break;
                case MessagePackToken.Array:
                    deserializer.InternalReadArray(dataWriter.As<int>());
                    break;
                default:
                    dataWriter.Content = XConvert.Cast(deserializer.DirectRead(), dataWriter.ContentType);
                    break;
            }
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void DeserializeTo(byte* bytes, int length, IDataWriter dataWriter)
        {
            var options = Options;

            if ((options & ReferenceDeserializerOptions) != 0)
            {
                DeserializeTo<MessagePackDeserializeModes.ReferenceMode>(bytes, length, dataWriter, options);
            }
            else
            {
                DeserializeTo<MessagePackDeserializeModes.StandardMode>(bytes, length, dataWriter, options);
            }
        }

        long ITargetedBind.TargetedId => targeted_id;

        void ITargetedBind.MakeTargetedId()
        {
            if (targeted_id == 0)
            {
                targeted_id = (long)Underlying.AsPointer(ref Underlying.AsRef(this));
            }
        }
    }
}