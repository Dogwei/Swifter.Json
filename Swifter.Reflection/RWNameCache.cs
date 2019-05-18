using Swifter.Tools;

namespace Swifter.Reflection
{
    internal sealed class RWNameCache<T> : BaseCache<string, T>
    {
        readonly XBindingFlags flags;

        public RWNameCache(XBindingFlags flags) : base(0)
        {
            this.flags = flags;
        }

        protected override int ComputeHashCode(string key)
        {
            if ((flags & XBindingFlags.RWIgnoreCase) != 0)
            {
                return StringHelper.GetUpperedHashCode(key);
            }

            return StringHelper.GetHashCode(key);
        }

        protected override bool Equals(string key1, string key2)
        {
            if ((flags & XBindingFlags.RWIgnoreCase) != 0)
            {
                return StringHelper.IgnoreCaseEqualsByLower(key1, key2);
            }

            return StringHelper.Equals(key1, key2);
        }
    }
}