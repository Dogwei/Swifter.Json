using NUnit.Framework;
using Swifter.Reflection;
using System;
using System.Threading;

namespace Swifter.Test
{
    public class ReflectionTest
    {
        [Test]
        public void TypeTest()
        {
            var xTypeInfo = XTypeInfo.Create<Tester>(XBindingFlags.Default | XBindingFlags.Static | XBindingFlags.NonPublic);

            var obj = new Tester();

            obj.public_event_action += null;

            xTypeInfo.GetEvent("public_event_func").AddEventHandler(obj, null);
            xTypeInfo.GetEvent("public_event_func").AddEventHandler(obj, null);

            Assert.AreEqual(2, obj.public_event_func_count);

            xTypeInfo.GetEvent("public_event_func").RemoveEventHandler(obj, null);

            Assert.AreEqual(1, obj.public_event_func_count);

            xTypeInfo.GetField("private_field_string").SetValue(obj, "Fuck");

            Assert.AreEqual("Fuck", xTypeInfo.GetField("private_field_string").GetValue(obj));


            xTypeInfo.GetField("public_field_int").SetValue(obj, 999);

            Assert.AreEqual(999, xTypeInfo.GetField("public_field_int").GetValue(obj));



            xTypeInfo.GetProperty("public_property_int").SetValue(obj, 123);

            Assert.AreEqual(123, xTypeInfo.GetProperty("public_property_int").GetValue(obj));


            xTypeInfo.GetProperty("private_property_string").SetValue(obj, "Dogwei");

            Assert.AreEqual("Dogwei", xTypeInfo.GetProperty("private_property_string").GetValue(obj));

            static void test()
            {

            }

            xTypeInfo.GetEvent("public_event_action").AddEventHandler(obj, (Action)test);

            xTypeInfo.GetEvent("public_event_action").RemoveEventHandler(obj, (Action)test);


            xTypeInfo.GetField("public_static_field_int").SetValue(456);

            Assert.AreEqual(456, xTypeInfo.GetField("public_static_field_int").GetValue());


            xTypeInfo.GetField("public_static_field_string").SetValue("JB");

            Assert.AreEqual("JB", xTypeInfo.GetField("public_static_field_string").GetValue());


            xTypeInfo.GetProperty("public_static_property_int").SetValue(789);

            Assert.AreEqual(789, xTypeInfo.GetProperty("public_static_property_int").GetValue());


            xTypeInfo.GetProperty("public_static_property_string").SetValue("JBP");

            Assert.AreEqual("JBP", xTypeInfo.GetProperty("public_static_property_string").GetValue());



            Assert.AreEqual(0, xTypeInfo.GetField("public_thread_static_field_int").GetValue());

            xTypeInfo.GetField("public_thread_static_field_int").SetValue(456);

            Assert.AreEqual(456, xTypeInfo.GetField("public_thread_static_field_int").GetValue());


            Assert.AreEqual(null, xTypeInfo.GetField("public_thread_static_field_string").GetValue());

            xTypeInfo.GetField("public_thread_static_field_string").SetValue("JB");

            Assert.AreEqual("JB", xTypeInfo.GetField("public_thread_static_field_string").GetValue());

            new Thread(() =>
            {
                Assert.AreEqual(0, xTypeInfo.GetField("public_thread_static_field_int").GetValue());

                xTypeInfo.GetField("public_thread_static_field_int").SetValue(456);

                Assert.AreEqual(456, xTypeInfo.GetField("public_thread_static_field_int").GetValue());


                Assert.AreEqual(null, xTypeInfo.GetField("public_thread_static_field_string").GetValue());

                xTypeInfo.GetField("public_thread_static_field_string").SetValue("JB");

                Assert.AreEqual("JB", xTypeInfo.GetField("public_thread_static_field_string").GetValue());
            }).Start();

            Assert.AreEqual(9999, xTypeInfo.GetField("public_const_int").GetValue());

            Assert.Catch<Exception>(() => xTypeInfo.GetField("public_const_int").SetValue(9999));

            Assert.AreEqual("indexer_getter_123", Assert.Catch<Exception>(() => xTypeInfo.GetIndexer(new object[] { 123 }).GetValue(obj, new object[] { 123 })).Message);
            Assert.AreEqual("indexer_setter_123_fuck", Assert.Catch<Exception>(() => xTypeInfo.GetIndexer(new Type[] { typeof(int) }).SetValue(obj, new object[] { 123 }, "fuck")).Message);

        }

        public class Tester
        {
            public const int public_const_int = 9999;

            public int public_field_int;

            private string private_field_string;

            public int public_property_int { get; set; }

            public string private_property_string { get => private_field_string; set => private_field_string = value; }

            public event Action public_event_action;

            public byte public_event_func_count;

            public event Func<int> public_event_func
            {
                add
                {
                    Assert.IsNotNull(this);

                    ++public_event_func_count;
                }
                remove
                {
                    Assert.IsNotNull(this);

                    --public_event_func_count;
                }
            }


            public static int public_static_field_int;
            public static string public_static_field_string;
            public static int public_static_property_int { get; set; }
            public static string public_static_property_string { get => public_static_field_string; set => public_static_field_string = value; }

            [ThreadStatic]
            public static int public_thread_static_field_int;
            [ThreadStatic]
            public static string public_thread_static_field_string;

            public string this[int index]
            {
                get
                {
                    Assert.IsNotNull(this);

                    throw new Exception($"indexer_getter_{index}");
                }
                set
                {
                    Assert.IsNotNull(this);

                    throw new Exception($"indexer_setter_{index}_{value}");
                }
            }
        }
    }
}
