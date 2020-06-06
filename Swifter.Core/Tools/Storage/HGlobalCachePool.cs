using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 全局缓存的对象池。
    /// </summary>
    /// <typeparam name="T">缓存类型</typeparam>
    public sealed class HGlobalCachePool<T> : BaseObjectPool<HGlobalCache<T>> where T : unmanaged
    {
        /// <summary>
        /// 指示回收内存大小与平均大小的比例，超过该比例的对象将会被释放（即：不回收）。
        /// 此值越大，回收率越低；当值小于等于 0 时，所有的对象都会被回收至池中。
        /// 单位：千分之(‰)
        /// </summary>
        public int Ratio = 800;

        long average = 0;
        long heft = 0;

        /// <summary>
        /// 创建全局缓存实例。
        /// </summary>
        /// <returns>返回一个实例</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override HGlobalCache<T> CreateInstance()
        {
            return new HGlobalCache<T>();
        }

        /// <summary>
        /// 归还全局缓存。
        /// </summary>
        /// <param name="hGCache">全局缓存实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public new void Return(HGlobalCache<T> hGCache)
        {
            if (hGCache.Offset != 0)
            {
                hGCache.Offset = 0;
            }

            ++heft;
            average += (hGCache.Available * 1000 - average) / heft;

            if (hGCache.Available * Ratio <= average)
            {
                hGCache.Offset = 0;
                hGCache.Count = 0;

                base.Return(hGCache);
            }
        }

        /// <summary>
        /// 获取当前线程的全局缓存。
        /// </summary>
        /// <returns>返回全局缓存实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public HGlobalCache<T> Current()
        {
            ref var thread_static = ref ThreadStatic;

            return thread_static ??= CreateInstance();
        }
    }
}