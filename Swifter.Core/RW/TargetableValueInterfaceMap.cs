using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    public readonly struct TargetableValueInterfaceMap
    {
        private readonly InternalTargetableValueInterfaceMap? internalMap;

        internal TargetableValueInterfaceMap(InternalTargetableValueInterfaceMap? internalMap)
        {
            this.internalMap = internalMap;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal IValueInterface<T>? GetValueInterface<T>()
        {
            if (internalMap == null)
            {
                return null;
            }

            if (internalMap.TryGetValue<T>(out var valueInterface))
            {
                return valueInterface;
            }

            return null;
        }

        public static TargetableValueInterfaceMap FromSource(TargetableValueInterfaceMapSource? source)
        {
            if (source != null)
            {
                return new TargetableValueInterfaceMap(source.internalMap);
            }

            return default;
        }

        public static TargetableValueInterfaceMap FromSource(ITargetableValueRWSource? source)
        {
            if (source != null)
            {
                return FromSource(source.ValueInterfaceMapSource);
            }

            return default;
        }
    }
}