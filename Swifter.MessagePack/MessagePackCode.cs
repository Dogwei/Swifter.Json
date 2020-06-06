using System.Runtime.CompilerServices;

namespace Swifter.MessagePack
{

    static class MessagePackCode
    {
        public const byte FixInt = 0x00;
        public const byte FixIntMax = 0x7f;


        public const byte FixMap = 0x80;
        public const byte FixMapMax = 0x8f;


        public const byte FixArray = 0x90;
        public const byte FixArrayMax = 0x9f;


        public const byte FixStr = 0xa0;
        public const byte FixStrMax = 0xbf;


        public const byte Nil = 0xc0;


        public const byte NeverUsed = 0xc1;


        public const byte False = 0xc2;
        public const byte True = 0xc3;

        public const byte Bin8 = 0xc4;
        public const byte Bin16 = 0xc5;
        public const byte Bin32 = 0xc6;

        public const byte Ext8 = 0xc7;
        public const byte Ext16 = 0xc8;
        public const byte Ext32 = 0xc9;

        public const byte Float32 = 0xca;
        public const byte Float64 = 0xcb;


        public const byte UInt8 = 0xcc;
        public const byte UInt16 = 0xcd;
        public const byte UInt32 = 0xce;
        public const byte UInt64 = 0xcf;


        public const byte Int8 = 0xd0;
        public const byte Int16 = 0xd1;
        public const byte Int32 = 0xd2;
        public const byte Int64 = 0xd3;


        public const byte FixExt1 = 0xd4;
        public const byte FixExt2 = 0xd5;
        public const byte FixExt4 = 0xd6;
        public const byte FixExt8 = 0xd7;
        public const byte FixExt16 = 0xd8;

        public const byte Str8 = 0xd9;
        public const byte Str16 = 0xda;
        public const byte Str32 = 0xdb;


        public const byte Array16 = 0xdc;
        public const byte Array32 = 0xdd;


        public const byte Map16 = 0xde;
        public const byte Map32 = 0xdf;


        public const byte FixNegativeInt = 0xe0;
        public const byte FixNegativeIntMax = 0xff;


        public const int FixNegativeIntMinValue = -0x20;
        public const int Int8MinValue = -0x80;
        public const int Int16MinValue = -0x8000;
        public const int Int32MinValue = -0x80000000;


        public const int FixPositiveIntMaxValue = 0x7f;
        public const int UInt8MaxValue = 0xff;
        public const int UInt16MaxValue = 0xffff;
        public const uint UInt32MaxValue = 0xffffffff;
        public const long UInt34MaxValue = 0x3ffffffff;


        public const int FixStrMaxLength = 0x1f;
        public const int Str8MaxLength = 0xff;
        public const int Str16MaxLength = 0xffff;


        public const int FixMapMaxCount = 0xf;
        public const int Map16MaxCount = 0xffff;


        public const int FixArrayMaxCount = 0xf;
        public const int Array16MaxCount = 0xffff;



        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static uint CloseRight(ulong value)
            => LowRight((value | (value >> 8)) & 0x00007f7f00007f7fu);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static uint LowRight(ulong value)
            => (uint)(value | (value >> 16));
    }
}