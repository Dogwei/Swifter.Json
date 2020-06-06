#if NET20 || NET35

#pragma warning disable 1591

using System.Collections.Generic;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 编译器必要的动态类型特性。
    /// </summary>
    public sealed class DynamicAttribute : Attribute
    {
        public DynamicAttribute()
        {

        }

        public DynamicAttribute(bool[] transformFlags)
        {
            TransformFlags = transformFlags;
        }

        public IList<bool> TransformFlags { get; set; }
    }
}

#endif