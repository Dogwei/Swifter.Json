using NUnit.Framework;
using Swifter.Reflection;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using static NUnit.Framework.Assert;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Text;

namespace Swifter.Test
{
    public unsafe class RWTest
    {
        [Test]
        public void CreateTest()
        {
            AreEqual(RWHelper.CreateRW(new Dictionary<string, object>()).GetType().Name, "DictionaryRW`3");
            AreEqual(RWHelper.CreateRW(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>())).GetType().Name, "DictionaryRW`3");
            AreEqual(RWHelper.CreateRW(new ConcurrentDictionary<string, object>()).GetType().Name, "DictionaryRW`3");
            AreEqual(RWHelper.CreateRW(new SortedDictionary<string, object>()).GetType().Name, "DictionaryRW`3");
            AreEqual(RWHelper.CreateRW(new SortedList<string, object>()).GetType().Name, "DictionaryRW`3");

            AreEqual(RWHelper.CreateRW(new List<object>()).GetType().Name, "ListRW`2");
            AreEqual(RWHelper.CreateRW(new ReadOnlyCollection<object>(new List<object>())).GetType().Name, "ListRW`2");
            AreEqual(RWHelper.CreateRW(new Collection<object>()).GetType().Name, "ListRW`2");

            AreEqual(RWHelper.CreateRW(new LinkedList<object>()).GetType().Name, "CollectionRW`2");
            AreEqual(RWHelper.CreateRW(new HashSet<object>()).GetType().Name, "CollectionRW`2");
            AreEqual(RWHelper.CreateRW(new SortedSet<object>()).GetType().Name, "CollectionRW`2");

            AreEqual(RWHelper.CreateRW(new Hashtable()).GetType().Name, "DictionaryRW`1");
            AreEqual(RWHelper.CreateRW(new SortedList()).GetType().Name, "DictionaryRW`1");
            AreEqual(RWHelper.CreateRW(new OrderedDictionary()).GetType().Name, "DictionaryRW`1");
            AreEqual(RWHelper.CreateRW(new ListDictionary()).GetType().Name, "DictionaryRW`1");

            AreEqual(RWHelper.CreateRW(new ArrayList()).GetType().Name, "ListRW`1");
            AreEqual(RWHelper.CreateRW(new StringCollection()).GetType().Name, "ListRW`1");


            AreEqual(RWHelper.CreateRW(new DataSet()).GetType().Name, "DataSetRW`1");
            AreEqual(RWHelper.CreateRW(new DataTable()).GetType().Name, "DataTableRW`1");
            AreEqual(RWHelper.CreateRW(new DataTable().NewRow()).GetType().Name, "DataRowRW`1");

            AreEqual(GetCSharpName(RWHelper.CreateRW(new int[0]).GetType()), "Swifter.RW.ArrayRW<int>");
            AreEqual(GetCSharpName(RWHelper.CreateRW(new int[0, 0]).GetType()), "Swifter.RW.MultiDimArray<int[,],int>.FirstRW");
            AreEqual(GetCSharpName(RWHelper.CreateRW(new int[0, 0, 0]).GetType()), "Swifter.RW.MultiDimArray<int[,,],int>.FirstRW");


            AreEqual(RWHelper.CreateReader(new List<object>().GetEnumerator().AsEnumerable()).GetType().Name, "EnumeratorReader`2");
            AreEqual(RWHelper.CreateReader(new List<object>().GetEnumerator()).GetType().Name, "EnumeratorReader`2");
            AreEqual(RWHelper.CreateReader(new ArrayList().GetEnumerator().AsEnumerable()).GetType().Name, "EnumeratorReader`1");
            AreEqual(RWHelper.CreateReader(new ArrayList().GetEnumerator()).GetType().Name, "EnumeratorReader`1");

            AreEqual(RWHelper.CreateReader(new DataTable().CreateDataReader()).GetType().Name, "DbDataReaderReader");

            AreEqual(
                RWHelper.CreateItemRW(RWHelper.CreateRW(new { Data = new Dictionary<string, object>() }).As<string>(), "Data").GetType().Name, 
                "DictionaryRW`3");

            AreEqual(
                RWHelper.CreateItemReader(RWHelper.CreateRW(new { Data = new DataTable().CreateDataReader() }).As<string>(), "Data").GetType().Name,
                "DbDataReaderReader");

            Catch<NotSupportedException>(() => RWHelper.CreateRW("err"));

            var asTester = (IAsDataRW)RWHelper.CreateRW(new int[0]).As<string>();

            Catch<SuccessException>(() => asTester.InvokeTIn(new AsTestInvoker<int>()));
            Catch<SuccessException>(() => asTester.InvokeTOut(new AsTestInvoker<string>()));
        }

        public static string GetCSharpName(Type type)
        {
            if (typeof(byte) == type) return "byte";
            if (typeof(bool) == type) return "bool";
            if (typeof(sbyte) == type) return "sbyte";
            if (typeof(short) == type) return "short";
            if (typeof(ushort) == type) return "ushort";
            if (typeof(char) == type) return "char";
            if (typeof(int) == type) return "int";
            if (typeof(uint) == type) return "uint";
            if (typeof(float) == type) return "float";
            if (typeof(long) == type) return "long";
            if (typeof(ulong) == type) return "ulong";
            if (typeof(double) == type) return "double";
            if (typeof(string) == type) return "string";
            if (typeof(object) == type) return "object";
            if (typeof(decimal) == type) return "decimal";
            if (typeof(void) == type) return "void";

            if (type.IsGenericTypeParameter)
            {
                return type.Name;
            }


            var sb = new StringBuilder();

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var rank = type.GetArrayRank();

                sb.Append(GetCSharpName(elementType));

                sb.Append("[");

                for (int i = 1; i < rank; i++)
                {
                    sb.Append(",");
                }

                sb.Append("]");
            }
            else if (type.IsPointer)
            {
                var elementType = type.GetElementType();

                sb.Append(GetCSharpName(elementType));

                sb.Append("*");
            }
            else if (type.IsByRef)
            {
                var elementType = type.GetElementType();

                sb.Append(GetCSharpName(elementType));

                sb.Append("&");
            }
            else
            {
                if (type.IsNested)
                {
                    var declaringType = type.DeclaringType;

                    if (declaringType.IsGenericTypeDefinition && !type.IsGenericTypeDefinition)
                    {
                        var genericArguments = type.GetGenericArguments();

                        genericArguments = genericArguments.AsSpan().Slice(0, declaringType.GetGenericArguments().Length).ToArray();

                        declaringType = declaringType.MakeGenericType(genericArguments);
                    }

                    sb.Append(GetCSharpName(declaringType));

                    sb.Append(".");
                }
                else
                {
                    sb.Append(type.Namespace);

                    if (!string.IsNullOrEmpty(type.Namespace))
                    {
                        sb.Append(".");
                    }
                }

                if (type.IsGenericType)
                {
                    var genericArguments = type.GetGenericArguments();

                    if (type.IsNested && type.DeclaringType.IsGenericTypeDefinition)
                    {
                        genericArguments = genericArguments.AsSpan().Slice(type.DeclaringType.GetGenericArguments().Length).ToArray();
                    }

                    var name = type.IsGenericTypeDefinition ? type.Name : type.GetGenericTypeDefinition().Name;

                    if (name.LastIndexOf('`') >= 0)
                    {
                        name = name.Substring(0, name.LastIndexOf('`'));
                    }


                    sb.Append(name);

                    if (genericArguments.Length > 0)
                    {
                        if (type.IsGenericTypeDefinition)
                        {
                            sb.Append("<");

                            for (int i = 1; i < genericArguments.Length; i++)
                            {
                                sb.Append(",");
                            }

                            sb.Append(">");
                        }
                        else
                        {
                            sb.Append("<");

                            for (int i = 0; i < genericArguments.Length; i++)
                            {
                                sb.Append(GetCSharpName(genericArguments[i]));
                                sb.Append(",");
                            }

                            --sb.Length;

                            sb.Append(">");
                        }
                    }

                }
                else
                {
                    sb.Append(type.Name);
                }
            }


            return sb.ToString();
        }

        class AsTestInvoker<TExpect> : IGenericInvoker
        {
            public void Invoke<TKey>()
            {
                AreEqual(typeof(TKey), typeof(TExpect));

                throw new SuccessException("invoked");
            }
        }

        [Test]
        public void FastObjectTest()
        {
            var rw = FastObjectRW<ObjectTester>.Create();

            AreEqual(rw.content, null);
            
            AreEqual(rw.Count, 2);

            AreEqual(rw.ContentType, typeof(ObjectTester));

            rw.Initialize();

            AreNotEqual(rw.content, null);

            AreEqual(rw.content.Id, 0);
            AreEqual(rw.content.Name, null);

            rw["@id"].WriteInt32(123);
            rw["@name"].WriteString("Dogwei");

            AreEqual(rw.content.Id, 123);
            AreEqual(rw.content.Name, "Dogwei");

            AreEqual(rw["@id"].ReadInt32(), 123);
            AreEqual(rw["@name"].ReadString(), "Dogwei");

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic &&
                dic.Count == 2 &&
                (int)dic["@id"] == 123 &&
                (string)dic["@name"] == "Dogwei"
                );

            rw[rw.GetOrdinal("@id")].WriteInt32(456);
            rw[rw.GetOrdinal("@name")].WriteString("Eway");

            AreEqual(rw[rw.GetOrdinal("@id")].ReadInt32(), 456);
            AreEqual(rw[rw.GetOrdinal("@name")].ReadString(), "Eway");
        }

        [Test]
        public void XObjectTest()
        {
            var rw = XObjectRW.Create<ObjectTester>();

            AreEqual(rw.Content, null);

            AreEqual(rw.Count, 2);

            AreEqual(rw.ContentType, typeof(ObjectTester));

            rw.Initialize();

            AreNotEqual(rw.Content, null);

            AreEqual(((ObjectTester)rw.Content).Id, 0);
            AreEqual(((ObjectTester)rw.Content).Name, null);

            rw["@id"].WriteInt32(123);
            rw["@name"].WriteString("Dogwei");

            AreEqual(((ObjectTester)rw.Content).Id, 123);
            AreEqual(((ObjectTester)rw.Content).Name, "Dogwei");

            AreEqual(rw["@id"].ReadInt32(), 123);
            AreEqual(rw["@name"].ReadString(), "Dogwei");

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic &&
                dic.Count == 2 &&
                (int)dic["@id"] == 123 &&
                (string)dic["@name"] == "Dogwei"
                );
        }

        class ObjectTester
        {
            [RWField("@id")]
            public int Id { get; set; }

            [RWField("@name")]
            public string Name { get; set; }
        }

        [Test]
        public void FastSkipDefaultValueTest()
        {
            var rw = FastObjectRW<SkipDefaultValueTester>.Create();



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 123 },
                { "Name", null }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic1 &&
                dic1.Count == 1 && 
                (int)dic1["Id"] == 123
                );



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 0 },
                { "Name", "Dogwei" }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic2 &&
                dic2.Count == 1 &&
                (string)dic2["Name"] == "Dogwei"
                );



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 0 },
                { "Name", null }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic3 &&
                dic3.Count == 0
                );
        }

        [Test]
        public void XSkipDefaultValueTest()
        {
            var rw = XObjectRW.Create<SkipDefaultValueTester>();



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 123 },
                { "Name", null }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic1 &&
                dic1.Count == 1 &&
                (int)dic1["Id"] == 123
                );



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 0 },
                { "Name", "Dogwei" }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic2 &&
                dic2.Count == 1 &&
                (string)dic2["Name"] == "Dogwei"
                );



            rw.Initialize();

            rw.OnWriteAll(RWHelper.CreateReader(new Dictionary<string, object> {
                { "Id", 0 },
                { "Name", null }
            }).As<string>());

            IsTrue(
                ValueInterface<Dictionary<string, object>>.ReadValue(ValueCopyer.ValueOf(rw)) is Dictionary<string, object> dic3 &&
                dic3.Count == 0
                );
        }

        [RWObject(SkipDefaultValue = RWBoolean.Yes)]
        class SkipDefaultValueTester
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Test]
        public void FastIgnoreCaseTest()
        {
            var chars = stackalloc char[100];
            var bytes = stackalloc byte[100];

            var rw = FastObjectRW<IgnoreCaseTester>.Create();

            AreEqual(FastObjectRW<IgnoreCaseTester>.CurrentOptions & FastObjectRWOptions.IgnoreCase, FastObjectRWOptions.IgnoreCase);

            rw.Initialize();


            rw["name"].WriteString("low");

            AreEqual(rw["Name"].ReadString(), "low");

            rw["NAME"].WriteString("upp");

            AreEqual(rw["Name"].ReadString(), "upp");

            Catch<MissingMemberException>(() => rw["EMAN"].WriteString("err"));
            Catch<MissingMemberException>(() => rw["eman"].ReadString());
            Catch<MemberAccessException>(() => rw["READONLY"].WriteString("err"));
            rw["readonly"].ReadString();
            rw["WRITEONLY"].WriteString("err");
            Catch<MemberAccessException>(() => rw["writeonly"].ReadString());

            rw[ToUtf16s("name")].WriteString("low");

            AreEqual(rw[ToUtf16s("Name")].ReadString(), "low");

            rw[ToUtf16s("NAME")].WriteString("upp");

            AreEqual(rw[ToUtf16s("Name")].ReadString(), "upp");

            Catch<MissingMemberException>(() => rw[ToUtf16s("EMAN")].WriteString("err"));
            Catch<MissingMemberException>(() => rw[ToUtf16s("eman")].ReadString());
            Catch<MemberAccessException>(() => rw[ToUtf8s("READONLY")].WriteString("err"));
            rw[ToUtf8s("readonly")].ReadString();
            rw[ToUtf8s("WRITEONLY")].WriteString("err");
            Catch<MemberAccessException>(() => rw[ToUtf8s("writeonly")].ReadString());


            rw[ToUtf8s("name")].WriteString("low");

            AreEqual(rw[ToUtf8s("Name")].ReadString(), "low");

            rw[ToUtf8s("NAME")].WriteString("upp");

            AreEqual(rw[ToUtf8s("Name")].ReadString(), "upp");

            Catch<MissingMemberException>(() => rw[ToUtf8s("EMAN")].WriteString("err"));
            Catch<MissingMemberException>(() => rw[ToUtf8s("eman")].ReadString());
            Catch<MemberAccessException>(() => rw[ToUtf8s("READONLY")].WriteString("err"));
            rw[ToUtf8s("readonly")].ReadString();
            rw[ToUtf8s("WRITEONLY")].WriteString("err");
            Catch<MemberAccessException>(() => rw[ToUtf8s("writeonly")].ReadString());



            var nicrw = FastObjectRW<NoIgnoreCaseTester>.Create();

            AreEqual(FastObjectRW<NoIgnoreCaseTester>.CurrentOptions & FastObjectRWOptions.IgnoreCase, default(FastObjectRWOptions));

            nicrw.Initialize();

            nicrw["Name"].WriteString("ok");
            Catch<MissingMemberException>(() => nicrw["name"].WriteString("err"));
            Catch<MissingMemberException>(() => nicrw["NAME"].WriteString("err"));

            nicrw[ToUtf16s("Name")].WriteString("ok");
            Catch<MissingMemberException>(() => nicrw[ToUtf16s("name")].WriteString("err"));
            Catch<MissingMemberException>(() => nicrw[ToUtf16s("NAME")].WriteString("err"));

            nicrw[ToUtf8s("Name")].WriteString("ok");
            Catch<MissingMemberException>(() => nicrw[ToUtf8s("name")].WriteString("err"));
            Catch<MissingMemberException>(() => nicrw[ToUtf8s("NAME")].WriteString("err"));

            Ps<char> ToUtf16s(string str)
            {
                Underlying.CopyBlock(
                    ref Underlying.As<char, byte>(ref *chars),
                ref Underlying.As<char, byte>(ref StringHelper.GetRawStringData(str)),
                (uint)(str.Length * sizeof(char))
                    );

                return new Ps<char>(chars, str.Length);
            }

            Ps<Utf8Byte> ToUtf8s(string str)
            {
                var length = StringHelper.GetUtf8Bytes(ref StringHelper.GetRawStringData(str), str.Length, bytes);

                return new Ps<Utf8Byte>((Utf8Byte*)bytes, length);
            }
        }

        [Test]
        public void XIgnoreCaseTest()
        {
            var rw = XObjectRW.Create<IgnoreCaseTester>();

            AreEqual(rw.xTypeInfo.Flags & XBindingFlags.RWIgnoreCase, XBindingFlags.RWIgnoreCase);

            rw.Initialize();


            rw["name"].WriteString("low");

            AreEqual(rw["Name"].ReadString(), "low");

            rw["NAME"].WriteString("upp");

            AreEqual(rw["Name"].ReadString(), "upp");

            Catch<MissingMemberException>(() => rw["EMAN"].WriteString("err"));
            Catch<MissingMemberException>(() => rw["eman"].ReadString());
            Catch<MemberAccessException>(() => rw["READONLY"].WriteString("err"));
            rw["readonly"].ReadString();
            rw["WRITEONLY"].WriteString("err");
            Catch<MemberAccessException>(() => rw["writeonly"].ReadString());

            var nicrw = XObjectRW.Create<NoIgnoreCaseTester>();

            AreEqual(nicrw.xTypeInfo.Flags & XBindingFlags.RWIgnoreCase, default(XBindingFlags));

            nicrw.Initialize();

            nicrw["Name"].WriteString("ok");
            Catch<MissingMemberException>(() => nicrw["name"].WriteString("err"));
            Catch<MissingMemberException>(() => nicrw["NAME"].WriteString("err"));
        }

        [RWObject(IgnoreCace = RWBoolean.Yes, NotFoundException = RWBoolean.Yes, CannotGetException = RWBoolean.Yes, CannotSetException = RWBoolean.Yes)]
        struct IgnoreCaseTester
        {
            public int ID { get; set; }

            public string NAMA { get; set; }

            public string NAMB { get; set; }

            public string NAMC { get; set; }

            public string NAMD { get; set; }

            public string NAME { get; set; }

            public string NAMF { get; set; }

            public string NAMG { get; set; }

            public string NAMH { get; set; }

            public string NAMI { get; set; }

            public string NAMJ { get; set; }

            public string NAMK { get; set; }

            public string NAMM { get; set; }

            public string NAMN { get; set; }

            public string NAME1 { get; set; }

            public string NAME2 { get; set; }

            public string NAME3 { get; set; }

            public string NAME4 { get; set; }

            public string NAME5 { get; set; }

            public string NAME6 { get; set; }

            public string NAME7 { get; set; }

            public string NAME8 { get; set; }

            public string NAME9 { get; set; }

            public string ReadOnly
            {
                get
                {
                    return null;
                }
            }

            public string WriteOnly
            {
                set
                {

                }
            }
        }

        [RWObject(IgnoreCace = RWBoolean.No, NotFoundException = RWBoolean.Yes)]
        struct NoIgnoreCaseTester
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
