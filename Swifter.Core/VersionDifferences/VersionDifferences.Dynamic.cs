#if NET35 || NET30 || NET20

#pragma warning disable 1591

using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    public sealed class DynamicAttribute : Attribute
    {
        private readonly bool[] _transformFlags;

        public DynamicAttribute()
        {
            _transformFlags = new bool[] { true };
        }

        public DynamicAttribute(bool[] transformFlags)
        {
            _transformFlags = transformFlags ?? throw new ArgumentNullException(nameof(transformFlags));
        }

        public IList<bool> TransformFlags => _transformFlags;
    }
}
#endif