


using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

#if Async

using System.Threading.Tasks;

#endif

namespace Swifter.MessagePack
{

	partial class MessagePackFormatter
	{



        /// <summary>
        /// 将指定类型的实例序列化为 MessagePack 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T value)
        {
			var hGBytes = BytesPool.Rent();

			SerializeObject(value, hGBytes);
			
			var ret = hGBytes.ToArray();

			BytesPool.Return(hGBytes);

			return ret;
        }





        /// <summary>
        /// 将指定类型的实例序列化为 MessagePack 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T value, MessagePackFormatterOptions options)
        {
			var hGBytes = BytesPool.Rent();

			SerializeObject(value, hGBytes, options);
			
			var ret = hGBytes.ToArray();

			BytesPool.Return(hGBytes);

			return ret;
        }





        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="hGCache">MessagePack 字节缓存</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, HGlobalCache<byte> hGCache, MessagePackFormatterOptions options)
        {


            if ((options & ReferenceSerializerOptions) != 0)
            {
                SerializeObject<T, MessagePackSerializeModes.ReferenceMode>(value, hGCache, options);
            }
            else
            {
                SerializeObject<T, MessagePackSerializeModes.StandardMode>(value, hGCache, options);
            }
        }





        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, Stream stream, MessagePackFormatterOptions options)
        {


			var hGBytes = BytesPool.Rent();

            if ((options & ReferenceSerializerOptions) != 0)
            {
                SerializeObject<T, MessagePackSerializeModes.ReferenceMode>(value, hGBytes, options);
            }
            else
            {
                SerializeObject<T, MessagePackSerializeModes.StandardMode>(value, hGBytes, options);
            }

			hGBytes.WriteTo(stream);

			BytesPool.Return(hGBytes);

        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, Stream stream)
        {
			
			var hGBytes = BytesPool.Rent();

			SerializeObject(value, hGBytes);

			var bytes = hGBytes.ToArray();
			
			BytesPool.Return(hGBytes);

			await stream.WriteAsync(bytes, 0, bytes.Length);
        }

#endif
#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, Stream stream, MessagePackFormatterOptions options)
        {
			
			var hGBytes = BytesPool.Rent();

			SerializeObject(value, hGBytes, options);

			var bytes = hGBytes.ToArray();
			
			BytesPool.Return(hGBytes);

			await stream.WriteAsync(bytes, 0, bytes.Length);
        }

#endif



        /// <summary>
        /// 将指定类型的实例序列化为 MessagePack 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte[] Serialize<T>(T value)
        {
			var hGBytes = BytesPool.Rent();

			Serialize(value, hGBytes);
			
			var ret = hGBytes.ToArray();

			BytesPool.Return(hGBytes);

			return ret;
        }





        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 字节缓存中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="hGCache">MessagePack 字节缓存</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, HGlobalCache<byte> hGCache)
        {

			var options = Options;

            if ((options & ReferenceSerializerOptions) != 0)
            {
                Serialize<T, MessagePackSerializeModes.ReferenceMode>(value, hGCache, options);
            }
            else
            {
                Serialize<T, MessagePackSerializeModes.StandardMode>(value, hGCache, options);
            }
        }





        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, Stream stream)
        {

			var options = Options;

			var hGBytes = BytesPool.Rent();

            if ((options & ReferenceSerializerOptions) != 0)
            {
                Serialize<T, MessagePackSerializeModes.ReferenceMode>(value, hGBytes, options);
            }
            else
            {
                Serialize<T, MessagePackSerializeModes.StandardMode>(value, hGBytes, options);
            }

			hGBytes.WriteTo(stream);

			BytesPool.Return(hGBytes);

        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 MessagePack 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">MessagePack 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeAsync<T>(T value, Stream stream)
        {
			
			var hGBytes = BytesPool.Rent();

			Serialize(value, hGBytes);

			var bytes = hGBytes.ToArray();
			
			BytesPool.Return(hGBytes);

			await stream.WriteAsync(bytes, 0, bytes.Length);
        }

#endif





        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ArraySegment<byte> bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return DeserializeObject(pBytes, bytes.Count, type);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(byte[] bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject(pBytes, bytes.Length, type);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(HGlobalCache<byte> hGCache, Type type)
		{
			unsafe
			{
				return DeserializeObject(hGCache.First, hGCache.Count, type);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(Stream stream, Type type)
		{
			return DeserializeObject(stream.ReadToEnd(), type);

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<byte> bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject(pBytes, bytes.Length, type);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ArraySegment<byte> bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return DeserializeObject<T>(pBytes, bytes.Count);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(byte[] bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject<T>(pBytes, bytes.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(HGlobalCache<byte> hGCache)
		{
			unsafe
			{
				return DeserializeObject<T>(hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(Stream stream)
		{
			return DeserializeObject<T>(stream.ReadToEnd());

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<byte> bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject<T>(pBytes, bytes.Length);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeObjectAsync<T>(Stream stream)
		{
			return DeserializeObject<T>(await stream.ReadToEndAsync());

		}
		
#endif


#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeObjectAsync(Stream stream, Type type)
		{
			return DeserializeObject(await stream.ReadToEndAsync(), type);

		}
		
#endif



        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ArraySegment<byte> bytes, Type type, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return DeserializeObject(pBytes, bytes.Count, type, options);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(byte[] bytes, Type type, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject(pBytes, bytes.Length, type, options);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(HGlobalCache<byte> hGCache, Type type, MessagePackFormatterOptions options)
		{
			unsafe
			{
				return DeserializeObject(hGCache.First, hGCache.Count, type, options);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(Stream stream, Type type, MessagePackFormatterOptions options)
		{
			return DeserializeObject(stream.ReadToEnd(), type, options);

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<byte> bytes, Type type, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject(pBytes, bytes.Length, type, options);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ArraySegment<byte> bytes, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return DeserializeObject<T>(pBytes, bytes.Count, options);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(byte[] bytes, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject<T>(pBytes, bytes.Length, options);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(HGlobalCache<byte> hGCache, MessagePackFormatterOptions options)
		{
			unsafe
			{
				return DeserializeObject<T>(hGCache.First, hGCache.Count, options);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(Stream stream, MessagePackFormatterOptions options)
		{
			return DeserializeObject<T>(stream.ReadToEnd(), options);

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<byte> bytes, MessagePackFormatterOptions options)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return DeserializeObject<T>(pBytes, bytes.Length, options);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeObjectAsync<T>(Stream stream, MessagePackFormatterOptions options)
		{
			return DeserializeObject<T>(await stream.ReadToEndAsync(), options);

		}
		
#endif


#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeObjectAsync(Stream stream, Type type, MessagePackFormatterOptions options)
		{
			return DeserializeObject(await stream.ReadToEndAsync(), type, options);

		}
		
#endif



        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(ArraySegment<byte> bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return Deserialize(pBytes, bytes.Count, type);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(byte[] bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return Deserialize(pBytes, bytes.Length, type);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(HGlobalCache<byte> hGCache, Type type)
		{
			unsafe
			{
				return Deserialize(hGCache.First, hGCache.Count, type);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(Stream stream, Type type)
		{
			return Deserialize(stream.ReadToEnd(), type);

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(ReadOnlySpan<byte> bytes, Type type)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return Deserialize(pBytes, bytes.Length, type);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(ArraySegment<byte> bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					return Deserialize<T>(pBytes, bytes.Count);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(byte[] bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return Deserialize<T>(pBytes, bytes.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 MessagePack 字节缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(HGlobalCache<byte> hGCache)
		{
			unsafe
			{
				return Deserialize<T>(hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(Stream stream)
		{
			return Deserialize<T>(stream.ReadToEnd());

		}
		


#if Span

        /// <summary>
        /// 将 MessagePack 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(ReadOnlySpan<byte> bytes)
		{
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					return Deserialize<T>(pBytes, bytes.Length);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeAsync<T>(Stream stream)
		{
			return Deserialize<T>(await stream.ReadToEndAsync());

		}
		
#endif


#if Async
        /// <summary>
        /// 将 MessagePack 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeAsync(Stream stream, Type type)
		{
			return Deserialize(await stream.ReadToEndAsync(), type);

		}
		
#endif

	










        /// <summary>
        /// 将 MessagePack 字节数组反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(ArraySegment<byte> bytes, IDataWriter dataWriter)
        {
			unsafe
			{
				fixed (byte* pBytes = &bytes.Array[bytes.Offset])
				{
					DeserializeTo(pBytes, bytes.Count, dataWriter);
				}
			}
        }
		






        /// <summary>
        /// 将 MessagePack 字节数组反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(byte[] bytes, IDataWriter dataWriter)
        {
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					DeserializeTo(pBytes, bytes.Length, dataWriter);
				}
			}
        }
		






        /// <summary>
        /// 将 MessagePack 字节缓存反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">MessagePack 字节缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(HGlobalCache<byte> hGCache, IDataWriter dataWriter)
        {
			unsafe
			{
				DeserializeTo(hGCache.First, hGCache.Count, dataWriter);
			}
        }
		






        /// <summary>
        /// 将 MessagePack 输入流反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(Stream stream, IDataWriter dataWriter)
        {
			DeserializeTo(stream.ReadToEnd(), dataWriter);
        }
		


#if Span



        /// <summary>
        /// 将 MessagePack 字节数组反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="bytes">MessagePack 字节数组</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(ReadOnlySpan<byte> bytes, IDataWriter dataWriter)
        {
			unsafe
			{
				fixed (byte* pBytes = bytes)
				{
					DeserializeTo(pBytes, bytes.Length, dataWriter);
				}
			}
        }
		

#endif

#if Async


        /// <summary>
        /// 将 MessagePack 输入流反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">MessagePack 输入流</param>
        /// <param name="dataWriter">数据写入器</param>
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
	    DeserializeToAsync(Stream stream, IDataWriter dataWriter)
        {
			DeserializeTo(await stream.ReadToEndAsync(), dataWriter);
        }
		
#endif

	}
}