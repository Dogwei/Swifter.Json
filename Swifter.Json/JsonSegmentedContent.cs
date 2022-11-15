using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Swifter.Json
{
    /// <summary>
    /// 输入输出分段器
    /// </summary>
    public sealed partial class JsonSegmentedContent
    {
        /// <summary>
        /// 创建并初始化。
        /// </summary>
        public static JsonSegmentedContent CreateAndInitialize(TextReader textReader, HGlobalCache<char> hGCache)
        {
            var result = new JsonSegmentedContent(textReader, hGCache, false);

            result.ReadSegment(0);

            return result;
        }

        private readonly object ioObject;

        internal readonly HGlobalCache<char> hGCache;
        internal readonly bool isAsync;

        internal bool IsFinalBlock
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 初始化输入分段器。
        /// </summary>
        public JsonSegmentedContent(TextReader textReader, HGlobalCache<char> hGCache, bool isAsync)
        {
            ioObject = textReader;

            this.hGCache = hGCache;
            this.isAsync = isAsync;
        }

        /// <summary>
        /// 初始化输出分段器。
        /// </summary>
        public JsonSegmentedContent(TextWriter textWriter, HGlobalCache<char> hGCache, bool isAsync)
        {
            ioObject = textWriter;

            this.hGCache = hGCache;
            this.isAsync = isAsync;
        }

        /// <summary>
        /// 将缓存中的分段写入到 IO 对象。
        /// </summary>
        public void WriteSegment()
        {
            var textWriter = (TextWriter)ioObject;

            textWriter
                .Write(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 从 IO 对象中读取分段到缓存中。
        /// </summary>
        /// <param name="retain">需要保留的字符数</param>
        /// <returns>返回新读取的字符数</returns>
        public int ReadSegment(int retain)
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

            var readCount = textReader.Read(hGCache.Context, hGCache.Offset + hGCache.Count, hGCache.Rest);

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