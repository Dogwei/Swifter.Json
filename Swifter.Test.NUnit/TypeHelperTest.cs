using NUnit.Framework;
using Swifter.Tools;

using static NUnit.Framework.Assert;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Test
{
    public unsafe class TypeHelperTest
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void OffsetOfTest()
        {
            var obj = new Tester();

            var int64_offset = TypeHelper.OffsetOf(typeof(Tester).GetField(nameof(Tester.public_int64)));
            var string_offset = TypeHelper.OffsetOf(typeof(Tester).GetField(nameof(Tester.public_string)));

            var static_int_offset = TypeHelper.OffsetOf(typeof(Tester).GetField(nameof(Tester.static_int)));
            var static_string_offset = TypeHelper.OffsetOf(typeof(Tester).GetField(nameof(Tester.static_string)));
            var static_object_offset = TypeHelper.OffsetOf(typeof(Tester).GetField(nameof(Tester.static_object)));

            var struct_int64_offset = TypeHelper.OffsetOf(typeof(Tester_Struct).GetField(nameof(Tester_Struct.public_int64)));
            var struct_string_offset = TypeHelper.OffsetOf(typeof(Tester_Struct).GetField(nameof(Tester_Struct.public_string)));

            var struct_static_bool_offset = TypeHelper.OffsetOf(typeof(Tester_Struct).GetField(nameof(Tester_Struct.static_bool)));
            var struct_static_byte_offset = TypeHelper.OffsetOf(typeof(Tester_Struct).GetField(nameof(Tester_Struct.static_byte)));

            var gc_statics_base_pointer = (void*)TypeHelper.GetGCStaticsBasePointer(typeof(Tester));
            var non_gc_statics_base_pointer = (void*)TypeHelper.GetNonGCStaticsBasePointer(typeof(Tester));
            var struct_non_gc_statics_base_pointer = (void*)TypeHelper.GetNonGCStaticsBasePointer(typeof(Tester_Struct));

            Underlying.AddByteOffset(ref TypeHelper.Unbox<long>(obj), int64_offset) = 999;
            Underlying.AddByteOffset(ref TypeHelper.Unbox<string>(obj), string_offset) = "Fuck";

            AreEqual(obj.public_int64, 999);
            AreEqual(obj.public_string, "Fuck");

            Underlying.AddByteOffset(ref Underlying.AsRef<int>(non_gc_statics_base_pointer), static_int_offset) = 123;
            Underlying.AddByteOffset(ref Underlying.AsRef<string>(gc_statics_base_pointer), static_string_offset) = "Dogwei";
            Underlying.AddByteOffset(ref Underlying.AsRef<object>(gc_statics_base_pointer), static_object_offset) = obj;

            AreEqual(Tester.static_int, 123);
            AreEqual(Tester.static_string, "Dogwei");
            AreEqual(Tester.static_object, obj);


            var value = new Tester_Struct();


            Underlying.AddByteOffset(ref Underlying.As<Tester_Struct, long>(ref value), struct_int64_offset) = 999;
            Underlying.AddByteOffset(ref Underlying.As<Tester_Struct, string>(ref value), struct_string_offset) = "Fuck";

            AreEqual(value.public_int64, 999);
            AreEqual(value.public_string, "Fuck");

            Underlying.AddByteOffset(ref Underlying.AsRef<bool>(struct_non_gc_statics_base_pointer), struct_static_bool_offset) = true;
            Underlying.AddByteOffset(ref Underlying.AsRef<byte>(struct_non_gc_statics_base_pointer), struct_static_byte_offset) = 128;

            AreEqual(Tester_Struct.static_bool, true);
            AreEqual(Tester_Struct.static_byte, 128);
        }

        [Test]
        public void AllocateTest()
        {
            if (VersionDifferences.UseInternalMethod)
            {
                VersionDifferences.UseInternalMethod = false;

                AllocateTest();

                VersionDifferences.UseInternalMethod = true;
            }


            IsInstanceOf(typeof(Tester), TypeHelper.Allocate(typeof(Tester)));
            IsInstanceOf(typeof(Tester_Struct), TypeHelper.Allocate(typeof(Tester_Struct)));
        }

        [Test]
        public void TypeTest()
        {
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(TypedReference)), false);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(void*)), false);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(int).MakeByRefType()), false);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(TypeHelper)), false);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(int)), true);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(Tester_Struct)), true);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(Tester)), true);
            AreEqual(TypeHelper.CanBeGenericParameter(typeof(IntPtr)), true);

            AreEqual(VersionDifferences.IsByRefLike(typeof(TypedReference)), true);
            AreEqual(VersionDifferences.IsByRefLike(typeof(int)), false);
            AreEqual(VersionDifferences.IsByRefLike(typeof(IntPtr)), false);
            AreEqual(VersionDifferences.IsByRefLike(typeof(void*)), false);
            AreEqual(VersionDifferences.IsByRefLike(typeof(int).MakeByRefType()), false);

            AreEqual(TypeHelper.GetDefaultValue(typeof(IntPtr)), default(IntPtr));
            AreEqual(TypeHelper.GetDefaultValue(typeof(string)), default(string));
            AreEqual(TypeHelper.GetDefaultValue(typeof(string)), default(string));
            AreEqual(TypeHelper.GetDefaultValue(typeof(int)), default(int));
            AreEqual(TypeHelper.GetDefaultValue(typeof(Tester_Struct)), default(Tester_Struct));

            AreEqual(TypeHelper.IsEmptyValue(default(IntPtr)), true);
            AreEqual(TypeHelper.IsEmptyValue(default(Tester_Struct)), true);
            AreEqual(TypeHelper.IsEmptyValue(default(string)), true);
            AreEqual(TypeHelper.IsEmptyValue(""), false);
            AreEqual(TypeHelper.IsEmptyValue(Guid.NewGuid()), false);

            AreEqual(TypeHelper.IsEmptyValue((object)default(IntPtr)), true);
            AreEqual(TypeHelper.IsEmptyValue((object)default(Tester_Struct)), true);
            AreEqual(TypeHelper.IsEmptyValue((object)default(string)), true);
            AreEqual(TypeHelper.IsEmptyValue((object)""), false);
            AreEqual(TypeHelper.IsEmptyValue((object)Guid.NewGuid()), false);

            AreEqual(TypeHelper.IsAutoProperty(typeof(Tester).GetProperty(nameof(Tester.auto_property)), out var fieldInfo), true);
            IsNotNull(fieldInfo);
            AreEqual(TypeHelper.IsAutoProperty(typeof(Tester).GetProperty(nameof(Tester.non_auto_property)), out var _), false);

            AreEqual(TypeHelper.IsAutoProperty(typeof(Tester_Struct).GetProperty(nameof(Tester_Struct.auto_property)), out fieldInfo), true);
            IsNotNull(fieldInfo);
            AreEqual(TypeHelper.IsAutoProperty(typeof(Tester_Struct).GetProperty(nameof(Tester_Struct.non_auto_property)), out var _), false);
        }

        [Test]
        public void EnumTest()
        {
            AreEqual(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                EnumHelper.AsEnum<BindingFlags>(EnumHelper.AsUInt64(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)));

            AreEqual(
                TypeCode.Int32,
                EnumHelper.AsEnum<TypeCode>(EnumHelper.AsUInt64(TypeCode.Int32)));

            AreEqual(nameof(TypeCode.Int32), EnumHelper.GetEnumName(TypeCode.Int32));
            IsNull(EnumHelper.GetEnumName((TypeCode)999));
            AreEqual(nameof(BindingFlags.Public), EnumHelper.GetEnumName(BindingFlags.Public));
            IsNull(EnumHelper.GetEnumName(BindingFlags.Public | BindingFlags.Instance));

            var chars = stackalloc char[100];

            AreEqual(
                (BindingFlags)0x800000,
                EnumHelper.FormatEnumFlags(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | (BindingFlags)0x800000, chars, 100, out var charsWritten));

            AreEqual(
                $"{nameof(BindingFlags.Instance)}{EnumHelper.EnumSeperator}{nameof(BindingFlags.Static)}{EnumHelper.EnumSeperator}{nameof(BindingFlags.Public)}",
                StringHelper.ToString(chars, charsWritten));

            AreEqual(
                true,
                EnumHelper.TryParseEnum<BindingFlags>(new Ps<char>(chars, charsWritten), out var value));

            AreEqual(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public,
                value);
        }

        [Test]
        public void SizeOfTest()
        {
            AreEqual(TypeHelper.SizeOf(typeof(Tester)), Underlying.SizeOf<Tester>());

            AreEqual(TypeHelper.SizeOf(typeof(Tester_Struct)), Underlying.SizeOf<Tester_Struct>());
        }

        [Test]
        public void GetBaseSizeTest()
        {
            int ptr_size = IntPtr.Size;

            var tester_size = 
                ((sizeof(int) + sizeof(long) + Underlying.SizeOf<string>()) + (ptr_size - 1) & (~(ptr_size - 1))) // Instance bytes
                + ptr_size // mathodTable ptr
                + ptr_size // sync block
                ;

            var tester_struct_size =
                ((sizeof(int) + sizeof(long) + Underlying.SizeOf<string>()) + (ptr_size - 1) & (~(ptr_size - 1))) // Instance bytes
                + ptr_size // mathodTable ptr
                + ptr_size // sync block
                ;

            AreEqual(TypeHelper.GetBaseSize(typeof(Tester)), tester_size);
            AreEqual(TypeHelper.GetBaseSize(typeof(Tester_Struct)), tester_struct_size);
        }

        public class Tester
        {
            public static int static_int;
            public static string static_string;
            public static object static_object;

            private int private_int32;
            public long public_int64;
            public string public_string;

            public int auto_property { get; set; }
            public int non_auto_property { get { return default; } set { } }
        }

        public struct Tester_Struct
        {
            public static byte static_byte;
            public static bool static_bool;

            private int private_int32;
            public long public_int64;
            public string public_string;

            public int auto_property { get; set; }
            public int non_auto_property { get { return default; } set { } }
        }
    }
}
