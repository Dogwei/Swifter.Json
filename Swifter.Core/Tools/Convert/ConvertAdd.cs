using System;

namespace Swifter.Tools
{
    internal static class ConvertAdd
    {
        public static string ToString(Guid value) => NumberHelper.ToString(value);

        public static Guid ToGuid(string value) => NumberHelper.TryParse(value, out Guid guid) ? guid : new Guid(value);

        public static Guid ToGuid(object value)
        {
            if (value is Guid guid)
            {
                return guid;
            }

            if (value is string str)
            {
                return ToGuid(str);
            }

            return (Guid)Convert.ChangeType(value, typeof(Guid));
        }
    }
}