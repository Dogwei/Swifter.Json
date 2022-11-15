using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    public sealed class TargetableValueInterfaceMapSource
    {
        internal InternalTargetableValueInterfaceMap? internalMap;

        internal void SetValueInterface<T>(IValueInterface<T> valueInterface)
        {
            lock (this)
            {
                if (internalMap == null)
                {
                    internalMap = new InternalTargetableValueInterfaceMap();
                }

                internalMap.Set<T>(valueInterface);
            }
        }

        internal bool RemoveValueInterface<T>()
        {
            if (internalMap == null)
            {
                return false;
            }

            lock (this)
            {
                if (internalMap == null)
                {
                    return false;
                }

                if (internalMap.Remove<T>())
                {
                    if (internalMap.Count == 0)
                    {
                        GC.SuppressFinalize(internalMap);

                        internalMap = null;
                    }

                    return true;
                }

                return false;
            }
        }

        internal void ClearValueInterfaces()
        {
            lock (this)
            {
                if (internalMap != null)
                {
                    internalMap.Clear();

                    GC.SuppressFinalize(internalMap);

                    internalMap = null;
                }
            }
        }

        internal IValueInterface<T>? GetValueInterface<T>()
        {
            var internalMap = this.internalMap;

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
    }
}