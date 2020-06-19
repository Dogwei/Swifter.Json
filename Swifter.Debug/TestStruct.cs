using System;

namespace Swifter.Debug
{
    public struct TestStruct
    {
        public int public_struct_field_int;
        public string public_struct_field_string;

        private int private_struct_field_int;
        private string private_struct_field_string;

        public int public_struct_property_int { get; set; }
        public string public_struct_property_string { get; set; }

        private int private_struct_property_int { get; set; }
        private string private_struct_property_string { get; set; }

        public event EventHandler<EventArgs> public_struct_event { add { } remove { } }
        private event EventHandler<EventArgs> private_struct_event { add { } remove { } }

        public void public_struct_action()
        {

        }

        public void public_struct_action_int(int num)
        {

        }

        public void public_struct_action_int_string(int num, string str)
        {

        }

        public int public_struct_func()
        {
            return default;
        }

        public int public_struct_func_int(int num)
        {
            return num;
        }

        public string public_struct_func_int_string(int num, string str)
        {
            return str;
        }

        private void private_struct_action()
        {

        }

        private void private_struct_action_int(int num)
        {

        }

        private void private_struct_action_int_string(int num, string str)
        {

        }

        private int private_struct_func()
        {
            return default;
        }

        private int private_struct_func_int(int num)
        {
            return num;
        }

        private string private_struct_func_int_string(int num, string str)
        {
            return str;
        }
    }
}
