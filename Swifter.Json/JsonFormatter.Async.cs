#if Async

using Swifter.RW;
using Swifter.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swifter.Json
{
	partial class JsonFormatter
	{
		/// <summary>
		/// 将指定类型的实例序列化到 Json 写入器中。
		/// </summary>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async ValueTask SerializeObjectAsync<T>(T value, TextWriter textWriter)
        {
			var hGChars = CharsPool.Rent();

			var jsonSerializer = new JsonSerializer(
				new JsonSegmentedContent(textWriter, hGChars, true)
				);

			ValueInterface.WriteValue(jsonSerializer, value);

			await jsonSerializer.FlushAsync();

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
        public static async ValueTask SerializeObjectAsync<T>(T value, Stream stream, Encoding encoding)
        {
			var streamWriter = new StreamWriter(stream, encoding);

			await SerializeObjectAsync(value, streamWriter);

			await streamWriter.FlushAsync();
		}

        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async ValueTask SerializeAsync<T>(T? value, TextWriter textWriter)
        {
			var hGChars = CharsPool.Rent();

			var jsonSerializer = new JsonSerializer(
				this,
				new JsonSegmentedContent(textWriter, hGChars, true)
				);

			ValueInterface.WriteValue(jsonSerializer, value);

			 await jsonSerializer.FlushAsync();

			CharsPool.Return(hGChars);
		}

		/// <summary>
		/// 将指定类型的实例序列化到 Json 输出流中。
		/// </summary>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="value">指定类型的实例</param>
		/// <param name="stream">Json 输出流</param>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
        public async ValueTask SerializeAsync<T>(T? value, Stream stream)
		{
			var streamWriter = new StreamWriter(stream, Encoding);

			await SerializeAsync(value, streamWriter);

			await streamWriter.FlushAsync();

		}

		/// <summary>
		/// 将 Json 读取器反序列化为指定类型的实例。
		/// </summary>
		/// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async ValueTask<T?> DeserializeObjectAsync<T>(TextReader textReader)
        {
            var hGChars = CharsPool.Rent();

            var value = ValueInterface.ReadValue<T>(
                new JsonDeserializer(
                    await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars)
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
		public static async ValueTask<object?> DeserializeObjectAsync(TextReader textReader, Type type)
		{
			var hGChars = CharsPool.Rent();

			var value = ValueInterface.ReadValue(
				new JsonDeserializer(
					await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars)
					),
				type
				);

			CharsPool.Return(hGChars);

			return value;
		}
		
		/// <summary>
		/// 将 Json 读取器反序列化为指定类型的实例。
		/// </summary>
		/// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public async ValueTask<T?> DeserializeAsync<T>(TextReader textReader)
		{
			var hGChars = CharsPool.Rent();

			var value = ValueInterface<T>.ReadValue(
				new JsonDeserializer(
					this, 
					await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars)
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
		public async ValueTask<object?> DeserializeAsync(TextReader textReader, Type type)
		{
			var hGChars = CharsPool.Rent();

			var value = ValueInterface.ReadValue(
				new JsonDeserializer(
					this,
					await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars)
					),
				type
				);

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
		public static async ValueTask<T?> DeserializeObjectAsync<T>(Stream stream, Encoding encoding)
		{
			return await DeserializeObjectAsync<T>(new StreamReader(stream, encoding));
		}
		


        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async ValueTask<object?> DeserializeObjectAsync(Stream stream, Encoding encoding, Type type)
		{
			return await DeserializeObjectAsync(new StreamReader(stream, encoding), type);
		}
		
		/// <summary>
		/// 将 Json 输入流反序列化为指定类型的实例。
		/// </summary>
		/// <param name="stream">Json 输入流</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public async ValueTask<T?> DeserializeAsync<T>(Stream stream)
		{
			return await DeserializeAsync<T>(new StreamReader(stream, Encoding));
		}
		
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public async ValueTask<object?> DeserializeAsync(Stream stream, Type type)
		{
			return await DeserializeAsync(new StreamReader(stream, Encoding), type);
		}

		/// <summary>
		/// 将 Json 读取器反序列化到指定的数据写入器中。
		/// </summary>
		/// <param name="textReader">Json 读取器</param>
		/// <param name="dataWriter">数据写入器</param>
		public async ValueTask DeserializeToAsync(TextReader textReader, IDataWriter dataWriter)
		{
			var hGChars = CharsPool.Rent();

			new JsonDeserializer(
				this,
				await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars)
				)
				.DeserializeTo(dataWriter);

			CharsPool.Return(hGChars);
		}

		/// <summary>
		/// 将 Json 输入流反序列化到指定的数据写入器中。
		/// </summary>
		/// <param name="stream">Json 输入流</param>
		/// <param name="dataWriter">数据写入器</param>
		public async ValueTask DeserializeToAsync(Stream stream, IDataWriter dataWriter)
        {
			await DeserializeToAsync(new StreamReader(stream, Encoding), dataWriter);
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
		public static async ValueTask SerializeObjectAsync<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			var jsonSerializer = new JsonSerializer(
				new JsonSegmentedContent(textWriter, hGChars, true),
				options
				);

			ValueInterface.WriteValue(jsonSerializer, value);

			await jsonSerializer.FlushAsync();

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
		public static async ValueTask SerializeObjectAsync<T>(T? value, Stream stream, Encoding encoding, JsonFormatterOptions options)
		{
			var streamWriter = new StreamWriter(stream, encoding);

			await SerializeObjectAsync(value, streamWriter, options);

			await streamWriter.FlushAsync();
		}

		/// <summary>
		/// 将 Json 读取器反序列化为指定类型的实例。
		/// </summary>
		/// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
		/// <returns>返回指定类型的实例</returns>
		[MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async ValueTask<T?> DeserializeObjectAsync<T>(TextReader textReader, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			var value = ValueInterface<T>.ReadValue(
				new JsonDeserializer(
					await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars), options)
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
		public static async ValueTask<object?> DeserializeObjectAsync(TextReader textReader, Type type, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			var value = ValueInterface.ReadValue(
				new JsonDeserializer(
					await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGChars),
					options
					),
				type
				);

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
		public static async ValueTask<T?> DeserializeObjectAsync<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
		{
			return await DeserializeObjectAsync<T>(new StreamReader(stream, encoding), options);
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
		public static async ValueTask<object?> DeserializeObjectAsync(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
		{
			return await DeserializeObjectAsync(new StreamReader(stream, encoding), type, options);
		}
#endif
	}
}


#endif

namespace System.Threading.Tasks
{

}