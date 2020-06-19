



using Swifter.RW;
using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

#if Async

using System.Threading.Tasks;

#endif

namespace Swifter.Json
{

	partial class JsonFormatter
	{



        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T value)
        {
			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars);
			
			var ret = hGChars.ToStringEx();

			CharsPool.Return(hGChars);

			return ret;
        }





        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string SerializeObject<T>(T value, JsonFormatterOptions options)
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
        public static void SerializeObject<T>(T value, HGlobalCache<char> hGCache, JsonFormatterOptions options)
        {


            if ((options & ReferenceOptions) != 0)
            {
                SerializeObject<T, JsonSerializeModes.ReferenceMode>(value, hGCache, options);
            }
            else if ((options & ComplexOptions) != 0)
            {
                SerializeObject<T, JsonSerializeModes.ComplexMode>(value, hGCache, options);
            }
            else
            {
                SerializeObject<T, JsonSerializeModes.SimpleMode>(value, hGCache, options);
            }
        }





        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SerializeObject<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
        {


			var hGChars = CharsPool.Rent();

            if ((options & ReferenceOptions) != 0)
            {
                SerializeObject<T, JsonSerializeModes.ReferenceMode>(value, hGChars, textWriter, options);
            }
            else if ((options & ComplexOptions) != 0)
            {
                SerializeObject<T, JsonSerializeModes.ComplexMode>(value, hGChars, textWriter, options);
            }
            else
            {
                SerializeObject<T, JsonSerializeModes.SimpleMode>(value, hGChars, textWriter, options);
            }

			hGChars.WriteTo(textWriter);

			CharsPool.Return(hGChars);
			
        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, TextWriter textWriter)
        {

			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars);

			var chars = hGChars.ToArray();
			
			CharsPool.Return(hGChars);

			await textWriter.WriteAsync(chars, 0, chars.Length);

        }

#endif
#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
        {

			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars, options);

			var chars = hGChars.ToArray();
			
			CharsPool.Return(hGChars);

			await textWriter.WriteAsync(chars, 0, chars.Length);

        }

#endif



        /// <summary>
        /// 将指定类型的实例序列化为 Json 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="encoding">指定编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T value, Encoding encoding)
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
        public static void SerializeObject<T>(T value, HGlobalCache<byte> hGCache, Encoding encoding)
        {
			
			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars);

			hGChars.WriteTo(hGCache, encoding);

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
        public static void SerializeObject<T>(T value, Stream stream, Encoding encoding)
        {
			
			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars);

			hGChars.WriteTo(stream, encoding);

			CharsPool.Return(hGChars);
			
        }





        /// <summary>
        /// 将指定类型的实例序列化为 Json 字节数组。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] SerializeObject<T>(T value, Encoding encoding, JsonFormatterOptions options)
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
        public static void SerializeObject<T>(T value, HGlobalCache<byte> hGCache, Encoding encoding, JsonFormatterOptions options)
        {
			
			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars, options);

			hGChars.WriteTo(hGCache, encoding);

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
        public static void SerializeObject<T>(T value, Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
			
			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars, options);

			hGChars.WriteTo(stream, encoding);

			CharsPool.Return(hGChars);
			
        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">Json 输出流</param>
		/// <param name="encoding">指定编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, Stream stream, Encoding encoding)
        {

			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars);

			var bytes = hGChars.ToBytes(encoding);
			
			CharsPool.Return(hGChars);

			await stream.WriteAsync(bytes, 0, bytes.Length);

        }

#endif
#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">Json 输出流</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="options">指定配置项</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeObjectAsync<T>(T value, Stream stream, Encoding encoding, JsonFormatterOptions options)
        {

			var hGChars = CharsPool.Rent();

			SerializeObject(value, hGChars, options);

			var bytes = hGChars.ToBytes(encoding);
			
			CharsPool.Return(hGChars);

			await stream.WriteAsync(bytes, 0, bytes.Length);

        }

#endif



        /// <summary>
        /// 将指定类型的实例序列化为 Json 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string Serialize<T>(T value)
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
        public void Serialize<T>(T value, HGlobalCache<char> hGCache)
        {

			var options = Options;

            if ((options & ReferenceOptions) != 0)
            {
                Serialize<T, JsonSerializeModes.ReferenceMode>(value, hGCache, options);
            }
            else if ((options & ComplexOptions) != 0)
            {
                Serialize<T, JsonSerializeModes.ComplexMode>(value, hGCache, options);
            }
            else
            {
                Serialize<T, JsonSerializeModes.SimpleMode>(value, hGCache, options);
            }
        }





        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, TextWriter textWriter)
        {

			var options = Options;

			var hGChars = CharsPool.Rent();

            if ((options & ReferenceOptions) != 0)
            {
                Serialize<T, JsonSerializeModes.ReferenceMode>(value, hGChars, textWriter, options);
            }
            else if ((options & ComplexOptions) != 0)
            {
                Serialize<T, JsonSerializeModes.ComplexMode>(value, hGChars, textWriter, options);
            }
            else
            {
                Serialize<T, JsonSerializeModes.SimpleMode>(value, hGChars, textWriter, options);
            }

			hGChars.WriteTo(textWriter);

			CharsPool.Return(hGChars);
			
        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 写入器中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="textWriter">Json 写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeAsync<T>(T value, TextWriter textWriter)
        {

			var hGChars = CharsPool.Rent();

			Serialize(value, hGChars);

			var chars = hGChars.ToArray();
			
			CharsPool.Return(hGChars);

			await textWriter.WriteAsync(chars, 0, chars.Length);

        }

#endif



        /// <summary>
        /// 将指定类型的实例序列化到 Json 字节数组中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="bytes">Json 字节数组</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, out byte[] bytes)
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
        public void Serialize<T>(T value, HGlobalCache<byte> hGCache)
        {
			
			var hGChars = CharsPool.Rent();

			Serialize(value, hGChars);

			hGChars.WriteTo(hGCache, Encoding);

			CharsPool.Return(hGChars);
			
        }





        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">Json 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Serialize<T>(T value, Stream stream)
        {
			
			var hGChars = CharsPool.Rent();

			Serialize(value, hGChars);

			hGChars.WriteTo(stream, Encoding);

			CharsPool.Return(hGChars);
			
        }


#if Async


        /// <summary>
        /// 将指定类型的实例序列化到 Json 输出流中。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
		/// <param name="stream">Json 输出流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
        SerializeAsync<T>(T value, Stream stream)
        {

			var hGChars = CharsPool.Rent();

			Serialize(value, hGChars);

			var bytes = hGChars.ToBytes(Encoding);
			
			CharsPool.Return(hGChars);

			await stream.WriteAsync(bytes, 0, bytes.Length);

        }

#endif





        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(string text, Type type)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject(type, chars, text.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(HGlobalCache<char> hGCache, Type type)
		{
			unsafe
			{
				return DeserializeObject(type, hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(TextReader textReader, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = DeserializeObject(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<char> text, Type type)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject(type, chars, text.Length);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(string text)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject<T>(chars, text.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(HGlobalCache<char> hGCache)
		{
			unsafe
			{
				return DeserializeObject<T>(hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(TextReader textReader)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = DeserializeObject<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<char> text)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject<T>(chars, text.Length);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeObjectAsync<T>(TextReader textReader)
		{

			return DeserializeObject<T>(await textReader.ReadToEndAsync());


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeObjectAsync(TextReader textReader, Type type)
		{

			return DeserializeObject(await textReader.ReadToEndAsync(), type);


		}
		
#endif



        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(string text, Type type, JsonFormatterOptions options)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject(type, chars, text.Length, options);
				}
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
		public static object DeserializeObject(HGlobalCache<char> hGCache, Type type, JsonFormatterOptions options)
		{
			unsafe
			{
				return DeserializeObject(type, hGCache.First, hGCache.Count, options);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(TextReader textReader, Type type, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = DeserializeObject(hGChars, type, options);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<char> text, Type type, JsonFormatterOptions options)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject(type, chars, text.Length, options);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(string text, JsonFormatterOptions options)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject<T>(chars, text.Length, options);
				}
			}

		}
		




        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(HGlobalCache<char> hGCache, JsonFormatterOptions options)
		{
			unsafe
			{
				return DeserializeObject<T>(hGCache.First, hGCache.Count, options);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(TextReader textReader, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = DeserializeObject<T>(hGChars, options);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<char> text, JsonFormatterOptions options)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return DeserializeObject<T>(chars, text.Length, options);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
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
        DeserializeObjectAsync<T>(TextReader textReader, JsonFormatterOptions options)
		{

			return DeserializeObject<T>(await textReader.ReadToEndAsync(), options);


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
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
        DeserializeObjectAsync(TextReader textReader, Type type, JsonFormatterOptions options)
		{

			return DeserializeObject(await textReader.ReadToEndAsync(), type, options);


		}
		
#endif



        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(string text, Type type)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return Deserialize(type, chars, text.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(HGlobalCache<char> hGCache, Type type)
		{
			unsafe
			{
				return Deserialize(type, hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(TextReader textReader, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = Deserialize(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(ReadOnlySpan<char> text, Type type)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return Deserialize(type, chars, text.Length);
				}
			}

		}
		

#endif


        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(string text)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return Deserialize<T>(chars, text.Length);
				}
			}

		}
		




        /// <summary>
        /// 将 Json 缓存反序列化为指定类型的实例。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(HGlobalCache<char> hGCache)
		{
			unsafe
			{
				return Deserialize<T>(hGCache.First, hGCache.Count);
			}

		}
		




        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(TextReader textReader)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			var value = Deserialize<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字符串反序列化为指定类型的实例。
        /// </summary>
        /// <param name="text">Json 字符串</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(ReadOnlySpan<char> text)
		{

			unsafe
			{
				fixed (char* chars = text)
				{
					return Deserialize<T>(chars, text.Length);
				}
			}

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeAsync<T>(TextReader textReader)
		{

			return Deserialize<T>(await textReader.ReadToEndAsync());


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 读取器反序列化为指定类型的实例。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeAsync(TextReader textReader, Type type)
		{

			return Deserialize(await textReader.ReadToEndAsync(), type);


		}
		
#endif



        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ArraySegment<byte> bytes, Encoding encoding, Type type)
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
		public static object DeserializeObject(byte[] bytes, Encoding encoding, Type type)
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
		public static object DeserializeObject(HGlobalCache<byte> hGCache, Encoding encoding, Type type)
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
		public static object DeserializeObject(Stream stream, Encoding encoding, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, encoding);

			var value = DeserializeObject(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<byte> bytes, Encoding encoding, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, encoding);

			var value = DeserializeObject(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif


        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ArraySegment<byte> bytes, Encoding encoding)
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
		public static T DeserializeObject<T>(byte[] bytes, Encoding encoding)
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
		public static T DeserializeObject<T>(HGlobalCache<byte> hGCache, Encoding encoding)
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
		public static T DeserializeObject<T>(Stream stream, Encoding encoding)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, encoding);

			var value = DeserializeObject<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<byte> bytes, Encoding encoding)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, encoding);

			var value = DeserializeObject<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="encoding">指定编码</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        DeserializeObjectAsync<T>(Stream stream, Encoding encoding)
		{

			return DeserializeObject<T>(await new StreamReader(stream, encoding).ReadToEndAsync());


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        DeserializeObjectAsync(Stream stream, Encoding encoding, Type type)
		{

			return DeserializeObject(await new StreamReader(stream, encoding).ReadToEndAsync(), type);


		}
		
#endif



        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ArraySegment<byte> bytes, Encoding encoding, Type type, JsonFormatterOptions options)
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
		public static object DeserializeObject(byte[] bytes, Encoding encoding, Type type, JsonFormatterOptions options)
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
		public static object DeserializeObject(HGlobalCache<byte> hGCache, Encoding encoding, Type type, JsonFormatterOptions options)
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
		public static object DeserializeObject(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, encoding);

			var value = DeserializeObject(hGChars, type, options);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <param name="type">指定类型</param>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static object DeserializeObject(ReadOnlySpan<byte> bytes, Encoding encoding, Type type, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, encoding);

			var value = DeserializeObject(hGChars, type, options);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif


        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ArraySegment<byte> bytes, Encoding encoding, JsonFormatterOptions options)
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
		public static T DeserializeObject<T>(byte[] bytes, Encoding encoding, JsonFormatterOptions options)
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
		public static T DeserializeObject<T>(HGlobalCache<byte> hGCache, Encoding encoding, JsonFormatterOptions options)
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
		public static T DeserializeObject<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, encoding);

			var value = DeserializeObject<T>(hGChars, options);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="encoding">指定编码</param>
		/// <typeparam name="T">指定类型</typeparam>
		/// <param name="options">指定配置项</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public static T DeserializeObject<T>(ReadOnlySpan<byte> bytes, Encoding encoding, JsonFormatterOptions options)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, encoding);

			var value = DeserializeObject<T>(hGChars, options);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="encoding">指定编码</param>
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
        DeserializeObjectAsync<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
		{

			return DeserializeObject<T>(await new StreamReader(stream, encoding).ReadToEndAsync(), options);


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
		/// <param name="encoding">指定编码</param>
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
        DeserializeObjectAsync(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
		{

			return DeserializeObject(await new StreamReader(stream, encoding).ReadToEndAsync(), type, options);


		}
		
#endif



        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(ArraySegment<byte> bytes, Type type)
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
		public object Deserialize(byte[] bytes, Type type)
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
		public object Deserialize(HGlobalCache<byte> hGCache, Type type)
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
		public object Deserialize(Stream stream, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, Encoding);

			var value = Deserialize(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public object Deserialize(ReadOnlySpan<byte> bytes, Type type)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, Encoding);

			var value = Deserialize(hGChars, type);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif


        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(ArraySegment<byte> bytes)
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
		public T Deserialize<T>(byte[] bytes)
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
		public T Deserialize<T>(HGlobalCache<byte> hGCache)
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
		public T Deserialize<T>(Stream stream)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, Encoding);

			var value = Deserialize<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		


#if Span

        /// <summary>
        /// 将 Json 字节数组反序列化为指定类型的实例。
        /// </summary>
        /// <param name="bytes">Json 字节数组</param>
		/// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
		public T Deserialize<T>(ReadOnlySpan<byte> bytes)
		{
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(bytes, Encoding);

			var value = Deserialize<T>(hGChars);

			CharsPool.Return(hGChars);

			return value;

		}
		

#endif

#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
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

			return Deserialize<T>(await new StreamReader(stream, Encoding).ReadToEndAsync());


		}
		
#endif


#if Async
        /// <summary>
        /// 将 Json 输入流反序列化为指定类型的实例。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
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

			return Deserialize(await new StreamReader(stream, Encoding).ReadToEndAsync(), type);


		}
		
#endif

	










        /// <summary>
        /// 将 Json 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(string text, IDataWriter dataWriter)
        {
			unsafe
			{
				fixed (char* chars = text)
				{
					DeserializeTo(chars, text.Length, dataWriter);
				}
			}
        }
		






        /// <summary>
        /// 将 Json 缓存反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="hGCache">Json 缓存</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(HGlobalCache<char> hGCache, IDataWriter dataWriter)
        {
			unsafe
			{
				DeserializeTo(hGCache.First, hGCache.Count, dataWriter);
			}
        }
		






        /// <summary>
        /// 将 Json 读取器反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(TextReader textReader, IDataWriter dataWriter)
        {
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(textReader);

			DeserializeTo(hGChars, dataWriter);

			CharsPool.Return(hGChars);
        }
		


#if Span



        /// <summary>
        /// 将 Json 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">Json 字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(ReadOnlySpan<char> text, IDataWriter dataWriter)
        {
			unsafe
			{
				fixed (char* chars = text)
				{
					DeserializeTo(chars, text.Length, dataWriter);
				}
			}
        }
		

#endif

#if Async


        /// <summary>
        /// 将 Json 读取器反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="textReader">Json 读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
	    DeserializeToAsync(TextReader textReader, IDataWriter dataWriter)
        {

			DeserializeTo(await textReader.ReadToEndAsync(), dataWriter);

        }
		
#endif





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
			var hGChars = CharsPool.Rent();

			hGChars.ReadFrom(stream, Encoding);

			DeserializeTo(hGChars, dataWriter);

			CharsPool.Return(hGChars);
        }
		


#if Span



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
		

#endif

#if Async


        /// <summary>
        /// 将 Json 输入流反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="stream">Json 输入流</param>
        /// <param name="dataWriter">数据写入器</param>
        public async
#if ValueTask
        ValueTask
#else
        Task
#endif
	    DeserializeToAsync(Stream stream, IDataWriter dataWriter)
        {

			DeserializeTo(await new StreamReader(stream, Encoding).ReadToEndAsync(), dataWriter);

        }
		
#endif

	}
}