#if Span

using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Json
{
    unsafe partial class JsonFormatter
	{
		/// <summary>
		/// 将 Json 字符串反序列化为指定类型的实例。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object? DeserializeObject(ReadOnlySpan<char> text, Type type)
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
		/// 将 Json 字符串反序列化为指定类型的实例。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T? DeserializeObject<T>(ReadOnlySpan<char> text)
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
		public object? Deserialize(ReadOnlySpan<char> text, Type type)
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
		/// 将 Json 字符串反序列化为指定类型的实例。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public T? Deserialize<T>(ReadOnlySpan<char> text)
		{
			fixed (char* chars = text)
			{
				return ValueInterface<T>.ReadValue(
					new JsonDeserializer(this, chars, text.Length)
					);
			}
		}

		/// <summary>
		/// 将 Json 字节数组反序列化为指定类型的实例。
		/// </summary>
		/// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object? DeserializeObject(ReadOnlySpan<byte> bytes, Encoding encoding, Type type)
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
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T? DeserializeObject<T>(ReadOnlySpan<byte> bytes, Encoding encoding)
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
		/// <param name="type">指定类型</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public object? Deserialize(ReadOnlySpan<byte> bytes, Type type)
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
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public T? Deserialize<T>(ReadOnlySpan<byte> bytes)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, Encoding);

			var value = Deserialize<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;
		}

		/// <summary>
		/// 将 Json 字符串反序列化到指定的数据写入器中。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <param name="dataWriter">数据写入器</param>
		public void DeserializeTo(ReadOnlySpan<char> text, IDataWriter dataWriter)
		{
			fixed (char* chars = text)
			{
				new JsonDeserializer(this, chars, text.Length)
					.DeserializeTo(dataWriter);
			}
		}

		/// <summary>
		/// 将 Json 字节数组反序列化到指定的数据写入器中。
		/// </summary>
		/// <param name="bytes">Json 字节数组</param>
		/// <param name="dataWriter">数据写入器</param>
		public void DeserializeTo(ReadOnlySpan<byte> bytes, IDataWriter dataWriter)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, Encoding);

			DeserializeTo(hGChars, dataWriter);

			CharsPool.Return(hGChars);
		}

#if !NO_OPTIONS

		/// <summary>
		/// 将 Json 字符串反序列化为指定类型的实例。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object? DeserializeObject(ReadOnlySpan<char> text, Type type, JsonFormatterOptions options)
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
		/// 将 Json 字符串反序列化为指定类型的实例。
		/// </summary>
		/// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T? DeserializeObject<T>(ReadOnlySpan<char> text, JsonFormatterOptions options)
		{
			fixed (char* chars = text)
			{
				return ValueInterface<T>.ReadValue(
					new JsonDeserializer(chars, text.Length, options)
					);
			}
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
		public static object? DeserializeObject(ReadOnlySpan<byte> bytes, Encoding encoding, Type type, JsonFormatterOptions options)
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
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T? DeserializeObject<T>(ReadOnlySpan<byte> bytes, Encoding encoding, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, encoding);

			var value = DeserializeObject<T>(hGChars, options);

			CharsPool.Return(hGChars);

			return value;
		}
#endif
	}
}

#endif