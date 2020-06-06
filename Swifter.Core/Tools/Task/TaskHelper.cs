#if Async

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供异步任务的工具函数。
    /// </summary>
    public static class TaskHelper
    {

        /// <summary>
        /// 不等待该任务执行完成。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void NoWait(this Task _)
        {
        }

        /// <summary>
        /// 不等待该任务执行完成。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void NoWait<T>(this Task<T> _)
        {
        }


#if ValueTask

        /// <summary>
        /// 不等待该任务执行完成。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void NoWait(this ValueTask _)
        {
        }

        /// <summary>
        /// 不等待该任务执行完成。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void NoWait<T>(this ValueTask<T> _)
        {
        }

#endif
    }
}

#endif