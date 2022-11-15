using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XTypeInfoCollection
    {
        public static readonly Dictionary<IntPtr, XTypeInfoCollection> Collections = new();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XTypeInfoCollection GetInstance<T>()
        {
            return GenericCache<T>.Instance;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XTypeInfoCollection GetInstance(Type type)
        {
            if (Collections.TryGetValue(type.TypeHandle.Value, out var value))
            {
                return value;
            }

            return InternalGetInstance(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static XTypeInfoCollection InternalGetInstance(Type type)
        {
            lock (Collections)
            {
                if (Collections.TryGetValue(type.TypeHandle.Value, out var value))
                {
                    return value;
                }

                var instance = new XTypeInfoCollection(type);

                Collections.Add(type.TypeHandle.Value, instance);

                return instance;
            }
        }

        public readonly Type Type;

        public SinglyLinkedList<KeyValuePair<XBindingFlags, XTypeInfo>> Nodes;

        public XTypeInfoCollection(Type type)
        {
            Type = type;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public XTypeInfo GetOrCreateXTypeInfo(XBindingFlags flags)
        {
            for (var node = Nodes.FirstNode; node is not null; node = node.Next)
            {
                if (node.Value.Key == flags)
                {
                    return node.Value.Value;
                }
            }

            return InternalGetOrCreateXTypeInfo(flags);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        XTypeInfo InternalGetOrCreateXTypeInfo(XBindingFlags flags)
        {
            lock (this)
            {
                for (var node = Nodes.FirstNode; node is not null; node = node.Next)
                {
                    if (node.Value.Key == flags)
                    {
                        return node.Value.Value;
                    }
                }

                var result = new XTypeInfo(Type, flags);

                Nodes.AddLast(new(flags, result));

                return result;
            }
        }

        static class GenericCache<T>
        {
            public static readonly XTypeInfoCollection Instance = GetInstance(typeof(T));
        }
    }
}
