using Swifter.Tools;
using System;
using System.Reflection.Emit;

namespace Swifter.RW
{
    sealed class NonPublicFastObjectRW<T> : FastObjectRW<T>
    {
        public static readonly Action<object> Initialize_Func;

        public static readonly Func<object, string, int> GetOrdinal_String_Func;
        public static readonly Func<object, Ps<char>, int> GetOrdinal_UTF16_Func;
        public static readonly Func<object, Ps<Utf8Byte>, int> GetOrdinal_UTF8_Func;

        public static readonly Action<object, IDataWriter<string>, RWStopToken> OnReadAll_String_Func;
        public static readonly Action<object, IDataWriter<Ps<char>>, RWStopToken> OnReadAll_UTF16_Func;
        public static readonly Action<object, IDataWriter<Ps<Utf8Byte>>, RWStopToken> OnReadAll_UTF8_Func;
        public static readonly Action<object, IDataWriter<int>, RWStopToken> OnReadAll_INT32_Func;

        public static readonly Action<object, IDataReader<string>, RWStopToken> OnWriteAll_String_Func;
        public static readonly Action<object, IDataReader<Ps<char>>, RWStopToken> OnWriteAll_UTF16_Func;
        public static readonly Action<object, IDataReader<Ps<Utf8Byte>>, RWStopToken> OnWriteAll_UTF8_Func;
        public static readonly Action<object, IDataReader<int>, RWStopToken> OnWriteAll_INT32_Func;

        public static readonly Action<object, string, IValueWriter> OnReadValue_String_Func;
        public static readonly Action<object, Ps<char>, IValueWriter> OnReadValue_UTF16_Func;
        public static readonly Action<object, Ps<Utf8Byte>, IValueWriter> OnReadValue_UTF8_Func;
        public static readonly Action<object, int, IValueWriter> OnReadValue_Int32_Func;

        public static readonly Action<object, string, IValueReader> OnWriteValue_String_Func;
        public static readonly Action<object, Ps<char>, IValueReader> OnWriteValue_UTF16_Func;
        public static readonly Action<object, Ps<Utf8Byte>, IValueReader> OnWriteValue_UTF8_Func;
        public static readonly Action<object, int, IValueReader> OnWriteValue_Int32_Func;

        public static readonly LSValueAction LoadValue_Int32_Object_Func;
        public static readonly LSValueAction StoreValue_Int32_Object_Func;
        public static readonly Func<object, int, IValueRW> GetValueRW_Int32_Func;

        public delegate void LSValueAction(object obj, int ordinal, ref object value);

        static NonPublicFastObjectRW()
        {
            Initialize_Func = DynamicAssembly.BuildDynamicMethod<Action<object>>((dm, ilGen) => StaticFastObjectRW<T>.ImplInitialize(ilGen), typeof(T).Module, true);

            GetOrdinal_String_Func = DynamicAssembly.BuildDynamicMethod<Func<object, string, int>>((dm, ilGen) => StaticFastObjectRW<T>.ImplGetOrdinal<string>(ilGen), typeof(T).Module, true);
            GetOrdinal_UTF16_Func = DynamicAssembly.BuildDynamicMethod<Func<object, Ps<char>, int>>((dm, ilGen) => StaticFastObjectRW<T>.ImplGetOrdinal<Ps<char>>(ilGen), typeof(T).Module, true);
            GetOrdinal_UTF8_Func = DynamicAssembly.BuildDynamicMethod<Func<object, Ps<Utf8Byte>, int>>((dm, ilGen) => StaticFastObjectRW<T>.ImplGetOrdinal<Ps<Utf8Byte>>(ilGen), typeof(T).Module, true);

            OnReadAll_String_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataWriter<string>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadAll<string>(ilGen), typeof(T).Module, true);
            OnReadAll_UTF16_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataWriter<Ps<char>>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadAll<Ps<char>>(ilGen), typeof(T).Module, true);
            OnReadAll_UTF8_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataWriter<Ps<Utf8Byte>>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadAll<Ps<Utf8Byte>>(ilGen), typeof(T).Module, true);
            OnReadAll_INT32_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataWriter<int>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadAll<int>(ilGen), typeof(T).Module, true);

            OnWriteAll_String_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataReader<string>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteAll<string>(ilGen), typeof(T).Module, true);
            OnWriteAll_UTF16_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataReader<Ps<char>>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteAll<Ps<char>>(ilGen), typeof(T).Module, true);
            OnWriteAll_UTF8_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataReader<Ps<Utf8Byte>>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteAll<Ps<Utf8Byte>>(ilGen), typeof(T).Module, true);
            OnWriteAll_INT32_Func = DynamicAssembly.BuildDynamicMethod<Action<object, IDataReader<int>, RWStopToken>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteAll<int>(ilGen), typeof(T).Module, true);

            OnReadValue_String_Func = DynamicAssembly.BuildDynamicMethod<Action<object, string, IValueWriter>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadValue<string>(ilGen), typeof(T).Module, true);
            OnReadValue_UTF16_Func = DynamicAssembly.BuildDynamicMethod<Action<object, Ps<char>, IValueWriter>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadValue<Ps<char>>(ilGen), typeof(T).Module, true);
            OnReadValue_UTF8_Func = DynamicAssembly.BuildDynamicMethod<Action<object, Ps<Utf8Byte>, IValueWriter>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadValue<Ps<Utf8Byte>>(ilGen), typeof(T).Module, true);
            OnReadValue_Int32_Func = DynamicAssembly.BuildDynamicMethod<Action<object, int, IValueWriter>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnReadValue<int>(ilGen), typeof(T).Module, true);

            OnWriteValue_String_Func = DynamicAssembly.BuildDynamicMethod<Action<object, string, IValueReader>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteValue<string>(ilGen), typeof(T).Module, true);
            OnWriteValue_UTF16_Func = DynamicAssembly.BuildDynamicMethod<Action<object, Ps<char>, IValueReader>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteValue<Ps<char>>(ilGen), typeof(T).Module, true);
            OnWriteValue_UTF8_Func = DynamicAssembly.BuildDynamicMethod<Action<object, Ps<Utf8Byte>, IValueReader>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteValue<Ps<Utf8Byte>>(ilGen), typeof(T).Module, true);
            OnWriteValue_Int32_Func = DynamicAssembly.BuildDynamicMethod<Action<object, int, IValueReader>>((dm, ilGen) => StaticFastObjectRW<T>.ImplOnWriteValue<int>(ilGen), typeof(T).Module, true);

            LoadValue_Int32_Object_Func = DynamicAssembly.BuildDynamicMethod<LSValueAction>((dm, ilGen) => StaticFastObjectRW<T>.ImplLoadValue(ilGen), typeof(T).Module, true);
            StoreValue_Int32_Object_Func = DynamicAssembly.BuildDynamicMethod<LSValueAction>((dm, ilGen) => StaticFastObjectRW<T>.ImplStoreValue(ilGen), typeof(T).Module, true);
            GetValueRW_Int32_Func = DynamicAssembly.BuildDynamicMethod<Func<object, int, IValueRW>>((dm, ilGen) => StaticFastObjectRW<T>.ImplGetValueRW(ilGen), typeof(T).Module, true);
        }

        public override int GetOrdinal(string name) => GetOrdinal_String_Func(this, name);

        public override int GetOrdinal(Ps<char> name) => GetOrdinal_UTF16_Func(this, name);

        public override int GetOrdinal(Ps<Utf8Byte> name) => GetOrdinal_UTF8_Func(this, name);

        public override void Initialize() => Initialize_Func(this);

        public override void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default) => OnReadAll_String_Func(this, dataWriter, stopToken);

        public override void OnReadAll(IDataWriter<Ps<char>> dataWriter, RWStopToken stopToken = default) => OnReadAll_UTF16_Func(this, dataWriter, stopToken);

        public override void OnReadAll(IDataWriter<Ps<Utf8Byte>> dataWriter, RWStopToken stopToken = default) => OnReadAll_UTF8_Func(this, dataWriter, stopToken);

        public override void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default) => OnReadAll_INT32_Func(this, dataWriter, stopToken);

        public override void OnReadValue(string key, IValueWriter valueWriter) => OnReadValue_String_Func(this, key, valueWriter);

        public override void OnReadValue(Ps<char> key, IValueWriter valueWriter) => OnReadValue_UTF16_Func(this, key, valueWriter);

        public override void OnReadValue(Ps<Utf8Byte> key, IValueWriter valueWriter) => OnReadValue_UTF8_Func(this, key, valueWriter);

        public override void OnReadValue(int key, IValueWriter valueWriter) => OnReadValue_Int32_Func(this, key, valueWriter);

        public override void OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken = default) => OnWriteAll_String_Func(this, dataReader, stopToken);

        public override void OnWriteAll(IDataReader<Ps<char>> dataReader, RWStopToken stopToken = default) => OnWriteAll_UTF16_Func(this, dataReader, stopToken);

        public override void OnWriteAll(IDataReader<Ps<Utf8Byte>> dataReader, RWStopToken stopToken = default) => OnWriteAll_UTF8_Func(this, dataReader, stopToken);

        public override void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default) => OnWriteAll_INT32_Func(this, dataReader, stopToken);

        public override void OnWriteValue(string key, IValueReader valueReader) => OnWriteValue_String_Func(this, key, valueReader);

        public override void OnWriteValue(Ps<char> key, IValueReader valueReader) => OnWriteValue_UTF16_Func(this, key, valueReader);

        public override void OnWriteValue(Ps<Utf8Byte> key, IValueReader valueReader) => OnWriteValue_UTF8_Func(this, key, valueReader);

        public override void OnWriteValue(int key, IValueReader valueReader) => OnWriteValue_Int32_Func(this, key, valueReader);

        protected internal override IValueRW GetValueRW(int ordinal) => GetValueRW_Int32_Func(this, ordinal);

        protected internal override void LoadValue(int ordinal, ref object value) => LoadValue_Int32_Object_Func(this, ordinal, ref value);

        protected internal override void StoreValue(int ordinal, ref object value) => StoreValue_Int32_Object_Func(this, ordinal, ref value);
    }
}