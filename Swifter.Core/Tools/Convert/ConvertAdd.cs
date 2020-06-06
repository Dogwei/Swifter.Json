using System;

namespace Swifter.Tools
{
    internal static class ConvertAdd
    {
        public static string ToString(Guid value)
        {
            return value.ToString();
        }

        public static Guid ToGuid(string value)
        {
            return new Guid(value);
        }

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

            throw new InvalidCastException(nameof(value));
        }
    }
}