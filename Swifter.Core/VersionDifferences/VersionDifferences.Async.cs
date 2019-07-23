#if NET45 || NET451 || NET47 || NET471 || NETSTANDARD || NETCOREAPP

using Swifter.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Swifter
{
    partial class VersionDifferences
    {
        /// <summary>
        /// 往字符串写入器中异步写入一个字符串。
        /// </summary>
        /// <param name="textWriter">字符串写入器</param>
        /// <param name="chars">字符串地址</param>
        /// <param name="length">字符串长度</param>
        [MethodImpl(AggressiveInlining)]
        public static async Task WriteCharsAsync(TextWriter textWriter, IntPtr chars, int length)
        {
            if (length > 128 && ArrayHelper.IsSupportedOneRankValueArrayInfo)
            {
                var ends = ArrayHelper.AsTempOneRankValueArray<char>(chars, length, out var starts);

                await textWriter.WriteAsync(starts);

                await textWriter.WriteAsync(ends);

                unsafe
                {
                    Unsafe.CopyBlock(
                        ref Unsafe.As<char, byte>(ref ((char*)chars)[0]),
                        ref Unsafe.As<char, byte>(ref starts[0]),
                        (uint)(starts.Length * sizeof(char)));
                }
            }
            else
            {
                const int bufferLength = 128;

                var buffer = new char[Math.Min(bufferLength, length)];

                for (int index = 0, count = buffer.Length;
                    index < length;
                    index += count, count = Math.Min(buffer.Length, length - index))
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(
                            ref Unsafe.As<char, byte>(ref buffer[0]),
                            ref Unsafe.As<char, byte>(ref ((char*)chars)[index]),
                            (uint)(count * sizeof(char)));
                    }

                    textWriter.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 在字符串读取器中异步读取一个字符串。
        /// </summary>
        /// <param name="textReader">字符串读取器</param>
        /// <param name="chars">字符串地址</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回读取的字符串长度</returns>
        [MethodImpl(AggressiveInlining)]
        public static async Task<int> ReadCharsAsync(TextReader textReader, IntPtr chars, int length)
        {
            if (length > 128 && ArrayHelper.IsSupportedOneRankValueArrayInfo)
            {
                var ends = ArrayHelper.AsTempOneRankValueArray<char>(chars, length, out var starts);

                var count = await textReader.ReadAsync(starts, 0, starts.Length);

                if (count == starts.Length)
                {
                    count += await textReader.ReadAsync(ends, 0, ends.Length);
                }

                unsafe
                {
                    Unsafe.CopyBlock(
                        ref Unsafe.As<char, byte>(ref ((char*)chars)[0]),
                        ref Unsafe.As<char, byte>(ref starts[0]),
                        (uint)(starts.Length * sizeof(char)));
                }

                return count;
            }
            else
            {
                const int bufferLength = 128;

                var buffer = new char[bufferLength];

                var total = 0;

                int readCount;

                while (total < length &&
                    (readCount = await textReader.ReadAsync(buffer, 0, Math.Min(bufferLength, length - total))) != 0)
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(
                            ref Unsafe.As<char, byte>(ref ((char*)chars)[total]),
                            ref Unsafe.As<char, byte>(ref buffer[0]),
                            (uint)readCount * sizeof(char));
                    }

                    total += readCount;
                }

                return total;
            }
        }

        /// <summary>
        /// 在流中异步写入一个内存块。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="bytes">内存块地址</param>
        /// <param name="length">内存块长度</param>
        /// <returns>返回一个异步操作</returns>
        [MethodImpl(AggressiveInlining)]
        public static async Task WriteBytesAsync(Stream stream, IntPtr bytes, int length)
        {
            if (length > 130 && ArrayHelper.IsSupportedOneRankValueArrayInfo)
            {
                var ends = ArrayHelper.AsTempOneRankValueArray<byte>(bytes, length, out var starts);

                await stream.WriteAsync(starts, 0, starts.Length);

                await stream.WriteAsync(ends, 0, ends.Length);

                unsafe
                {
                    Unsafe.CopyBlock(
                        ref ((byte*)bytes)[0],
                        ref starts[0],
                        (uint)starts.Length);
                }
            }
            else
            {
                const int bufferLength = 128;

                var buffer = new byte[Math.Min(bufferLength, length)];

                for (int index = 0, count = buffer.Length;
                    index < length;
                    index += count, count = Math.Min(buffer.Length, length - index))
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(
                            ref buffer[0],
                            ref ((byte*)bytes)[index],
                            (uint)count);
                    }

                    await stream.WriteAsync(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 在流中异步读取一个内存块。
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="bytes">内存块地址</param>
        /// <param name="length">内存块长度</param>
        /// <returns>返回一个 int 异步操作</returns>
        [MethodImpl(AggressiveInlining)]
        public static async Task<int> ReadBytesAsync(Stream stream, IntPtr bytes, int length)
        {
            if (length > 130 && ArrayHelper.IsSupportedOneRankValueArrayInfo)
            {
                var ends = ArrayHelper.AsTempOneRankValueArray<byte>(bytes, length, out var starts);

                var count = await stream.ReadAsync(starts, 0, starts.Length);

                if (count == starts.Length)
                {
                    count += await stream.ReadAsync(ends, 0, ends.Length);
                }

                unsafe
                {
                    Unsafe.CopyBlock(
                        ref ((byte*)bytes)[0],
                        ref starts[0],
                        (uint)starts.Length);
                }

                return count;
            }
            else
            {
                const int bufferLength = 128;

                var buffer = new byte[bufferLength];

                var total = 0;

                int readCount;

                while (total < length &&
                    (readCount = await stream.ReadAsync(buffer, 0, Math.Min(bufferLength, length - total))) != 0)
                {
                    unsafe
                    {
                        Unsafe.CopyBlock(
                            ref ((byte*)bytes)[total],
                            ref buffer[0],
                            (uint)readCount);
                    }

                    total += readCount;
                }

                return total;
            }
        }
    }
}

#endif