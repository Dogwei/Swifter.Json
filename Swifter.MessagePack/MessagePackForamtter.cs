using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.MessagePack
{
    public sealed unsafe partial class MessagePackForamtter : IBinaryFormatter, ITargetedBind
    {
        public static MessagePackFormatterOptions DefaultOptions =
            MessagePackFormatterOptions.UnknownTypeAsString |
            MessagePackFormatterOptions.OutOfDepthException |
            MessagePackFormatterOptions.UseTimestamp32;

        public static int DefaultMaxDepth = 12;


        internal long id;

        long ITargetedBind.TargetedId => id;

        void ITargetedBind.MakeTargetedId()
        {
            if (id == 0)
            {
                id = (long)Unsafe.AsPointer(ref Unsafe.AsRef(this));
            }
        }

        /// <summary>
        /// 释放对象时移除读写接口实例。
        /// </summary>
        ~MessagePackForamtter()
        {
            if (id != 0)
            {
                ValueInterface.RemoveTargetedInterface(this);

                id = 0;
            }
        }

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

                ValueInterface<T>.SetTargetedInterface(this, valueInterface);
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(byte[] text, Type type)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(Stream stream, Type type)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize<T>(T value)
        {
            throw new NotImplementedException();
        }

        public void Serialize<T>(T value, Stream stream)
        {
            throw new NotImplementedException();
        }



        public static byte[] SerializeObject<T>(T value)
        {
            var hGlobal = CacheHelper.RentBytes();

            try
            {
                var serializer = new MessagePackSerializer<MessagePackModes.Simple>(
                    DefaultOptions,
                    DefaultMaxDepth,
                    hGlobal
                );

                ValueInterface<T>.WriteValue(serializer, value);

                serializer.HGCache.Count = serializer.offset;

                return serializer.HGCache.ToBytes();
            }
            finally
            {
                CacheHelper.Return(hGlobal);
            }
        }

        public static unsafe T DeserializeObject<T>(byte[] bytes)
        {
            fixed (byte* pBytes = bytes)
            {
                return DeserializeObject<T>(pBytes, bytes.Length);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe T DeserializeObject<T>(byte* bytes, int length)
        {
            var hGlobal = CacheHelper.RentChars();

            try
            {
                var deserializer = new MessagePackDeserializer(
                    bytes,
                    length,
                    hGlobal);

                return ValueInterface<T>.ReadValue(deserializer);
            }
            finally
            {
                CacheHelper.Return(hGlobal);
            }
        }

        public T Deserialize<T>(HGlobalCache<byte> hGCache)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(HGlobalCache<byte> hGCache, Type type)
        {
            throw new NotImplementedException();
        }

        public void DeserializeTo(string text, IDataWriter dataWriter)
        {
            throw new NotImplementedException();
        }

        public void DeserializeTo(Stream stream, IDataWriter dataWriter)
        {
            throw new NotImplementedException();
        }

        public void DeserializeTo(HGlobalCache<byte> hGCache, IDataWriter dataWriter)
        {
            throw new NotImplementedException();
        }

        public void Serialize<T>(T value, HGlobalCache<byte> hGCache)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(ArraySegment<byte> bytes, Type type)
        {
            throw new NotImplementedException();
        }

        public void DeserializeTo(ArraySegment<byte> bytes, IDataWriter dataWriter)
        {
            throw new NotImplementedException();
        }
    }
}