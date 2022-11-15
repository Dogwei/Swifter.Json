#if Async
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swifter.Json
{
    partial class JsonSegmentedContent
    {
        /// <summary>
        /// 创建并初始化。
        /// </summary>
        public static async ValueTask<JsonSegmentedContent> CreateAndInitializeAsync(TextReader textReader, HGlobalCache<char> hGCache)
        {
            var result = new JsonSegmentedContent(textReader, hGCache, true);

            await result.ReadSegmentAsync(0);

            return result;
        }

        /// <summary>
        /// 将缓存中的分段写入到 IO 对象。
        /// </summary>
        public async ValueTask WriteSegmentAsync()
        {
            var textWriter = (TextWriter)ioObject;

            await textWriter
                .WriteAsync(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 从 IO 对象中读取分段到缓存中。
        /// </summary>
        /// <param name="retain">需要保留的字符数</param>
        /// <returns>返回新读取的字符数</returns>
        public async ValueTask<int> ReadSegmentAsync(int retain)
        {
            var textReader = (TextReader)ioObject;

            int result = 0;

            if (retain > 0)
            {
                Array.Copy(
                    hGCache.Context,
                    hGCache.Offset + hGCache.Count - retain,
                    hGCache.Context,
                    hGCache.Offset,
                    retain);
            }

            hGCache.Count = retain;

        Loop:

            var readCount = await textReader.ReadAsync(hGCache.Context, hGCache.Offset + hGCache.Count, hGCache.Rest);

            await Task.Yield();

            if (readCount > 0)
            {
                hGCache.Count += readCount;
                result += readCount;

                if (hGCache.Rest >= readCount)
                {
                    goto Loop;
                }
            }

            return result;
        }
    }
}
#endif