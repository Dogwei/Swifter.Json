using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER

#pragma warning disable

namespace System.Collections.Generic
{
    public sealed class ReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
    {
        public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

        private ReferenceEqualityComparer()
        {

        }

        public new bool Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

#endif