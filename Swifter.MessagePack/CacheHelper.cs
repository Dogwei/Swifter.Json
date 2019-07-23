using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.MessagePack
{
    static class CacheHelper
    {
        [ThreadStatic]
        static Internal ThreadInternal;

        static HGlobalCachePool<byte> bytesPool;
        static HGlobalCachePool<char> charsPool;
        
        static HGlobalCachePool<byte> BytesPool => bytesPool ?? (bytesPool = new HGlobalCachePool<byte>());
        
        static HGlobalCachePool<char> CharsPool => charsPool ?? (charsPool = new HGlobalCachePool<char>());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<byte> RentBytes()
        {
            ref var @internal = ref ThreadInternal;

            if (@internal.BytesIsUsed)
            {
                return InternalRentBytes();
            }

            @internal.BytesIsUsed = true;

            return @internal.Bytes;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Return(HGlobalCache<byte> hGlobal)
        {
            ref var @internal = ref ThreadInternal;

            if (@internal.BytesIsUsed && @internal.Bytes == hGlobal)
            {
                @internal.BytesIsUsed = false;
            }
            else
            {
                InternalReturn(hGlobal);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> RentChars()
        {
            ref var @internal = ref ThreadInternal;

            if (@internal.CharsIsUsed)
            {
                return InternalRentChars();
            }

            @internal.CharsIsUsed = true;

            return @internal.Chars;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Return(HGlobalCache<char> hGlobal)
        {
            ref var @internal = ref ThreadInternal;

            if (@internal.CharsIsUsed && @internal.Chars == hGlobal)
            {
                @internal.CharsIsUsed = false;
            }
            else
            {
                InternalReturn(hGlobal);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static HGlobalCache<byte> InternalRentBytes()
        {
            return BytesPool.Rent();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InternalReturn(HGlobalCache<byte> hGlobal)
        {
            BytesPool.Return(hGlobal);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static HGlobalCache<char> InternalRentChars()
        {
            return CharsPool.Rent();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InternalReturn(HGlobalCache<char> hGlobal)
        {
            CharsPool.Return(hGlobal);
        }

        struct Internal
        {
            HGlobalCache<char> chars;
            HGlobalCache<byte> bytes;

            public HGlobalCache<char> Chars => chars ?? (chars = new HGlobalCache<char>());

            public HGlobalCache<byte> Bytes => bytes ?? (bytes = new HGlobalCache<byte>());

            public bool CharsIsUsed;
            public bool BytesIsUsed;
        }
    }
}