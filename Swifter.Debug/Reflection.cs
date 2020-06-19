#if BenchmarkDotNet

using System;
using System.Reflection;
using Swifter.Tools;
using Swifter.Reflection;
using BenchmarkDotNet.Attributes;

namespace Swifter.Debug
{
    [Config(typeof(MyConfig))]
    public class Reflection
    {
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        public const XBindingFlags xBindingFlags = XBindingFlags.Public | XBindingFlags.NonPublic | XBindingFlags.Instance | XBindingFlags.Static;

        public static readonly object obj_TestClass = new TestClass();
        public static readonly object obj_TestStruct = new TestStruct();

        public static readonly EventHandler<EventArgs> test_event_handler = new EventHandler<EventArgs>((sender, args) => { });

        public static readonly MethodInfo system_public_static_action = typeof(TestClass).GetMethod("public_static_action", bindingFlags);
        public static readonly XMethodInfo swifter_public_static_action = XMethodInfo.Create(system_public_static_action, xBindingFlags);
        public static readonly object[] public_static_action_args = new object[] { };

        public static readonly PropertyInfo system_public_class_property_int = typeof(TestClass).GetProperty("public_class_property_int", bindingFlags);
        public static readonly XPropertyInfo swifter_public_class_property_int = XPropertyInfo.Create(system_public_class_property_int, xBindingFlags);
        
    }
}

#endif