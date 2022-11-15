using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 分割连续内存信息的迭代器。
    /// </summary>
    public unsafe ref struct SplitEnumerator<T> where T : unmanaged
    {
        T* pointer;
        int length;

        readonly T separator;

        internal SplitEnumerator(T* pointer, int length, T separator)
        {
            this.pointer = pointer;
            this.length = length;

            this.separator = separator;

            Current = default;
        }

        /// <summary>
        /// 获取当前段落。
        /// </summary>
        public Ps<T> Current;

        /// <summary>
        /// 移动至下个段落。
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (length > 0)
            {
                var count = 0;

                while (count < length && !Equals(pointer[count], separator))
                {
                    ++count;
                }

                Current = new Ps<T>(pointer, count);

                ++count;

                pointer += count;
                length -= count;

                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static bool Equals(T x, T y)
        {
            if (typeof(T) == typeof(char)) return Unsafe.As<T, char>(ref x) == Unsafe.As<T, char>(ref y);
            else if (typeof(T) == typeof(byte) || typeof(T) == typeof(Utf8Byte)) return Unsafe.As<T, byte>(ref x) == Unsafe.As<T, byte>(ref y);
            else return EqualityComparer<T>.Default.Equals(x, y);
        }
    }

}