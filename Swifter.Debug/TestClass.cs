using System;

namespace Swifter.Debug
{
    public class TestClass
    {
        public int public_class_field_int;
        public string public_class_field_string;

        private int private_class_field_int;
        private string private_class_field_string;

        public int public_class_property_int { get; set; }
        public string public_class_property_string { get; set; }

        private int private_class_property_int { get; set; }
        private string private_class_property_string { get; set; }

        public static int public_static_field_int;
        public static string public_static_field_string;

        private static int private_static_field_int;
        private static string private_static_field_string;

        [ThreadStatic]
        public static int public_thread_static_field_int;
        [ThreadStatic]
        public static string public_thread_static_field_string;

        public static int public_static_property_int { get; set; }
        public static string public_static_property_string { get; set; }

        private static int private_static_property_int { get; set; }
        private static string private_static_property_string { get; set; }

        public event EventHandler<EventArgs> public_class_event { add { } remove { } }
        private event EventHandler<EventArgs> private_class_event { add { } remove { } }

        public static event EventHandler<EventArgs> public_static_event { add { } remove { } }
        private static event EventHandler<EventArgs> private_static_event { add { } remove { } }

        public static void public_static_action()
        {

        }

        public static void public_static_action_int(int num)
        {

        }

        public static void public_static_action_int_string(int num, string str)
        {

        }

        public static int public_static_func()
        {
            return default;
        }

        public static int public_static_func_int(int num)
        {
            return num;
        }

        public static string public_static_func_int_string(int num, string str)
        {
            return str;
        }

        private static void private_static_action()
        {

        }

        private static void private_static_action_int(int num)
        {

        }

        private static void private_static_action_int_string(int num, string str)
        {

        }

        private static int private_static_func()
        {
            return default;
        }

        private static int private_static_func_int(int num)
        {
            return num;
        }

        private static string private_static_func_int_string(int num, string str)
        {
            return str;
        }

        public void public_class_action()
        {

        }

        public  void public_class_action_int(int num)
        {

        }

        public void public_class_action_int_string(int num, string str)
        {

        }

        public int public_class_func()
        {
            return default;
        }

        public int public_class_func_int(int num)
        {
            return num;
        }

        public string public_class_func_int_string(int num, string str)
        {
            return str;
        }

        private void private_class_action()
        {

        }

        private void private_class_action_int(int num)
        {

        }

        private void private_class_action_int_string(int num, string str)
        {

        }

        private int private_class_func()
        {
            return default;
        }

        private int private_class_func_int(int num)
        {
            return num;
        }

        private string private_class_func_int_string(int num, string str)
        {
            return str;
        }
    }
}
