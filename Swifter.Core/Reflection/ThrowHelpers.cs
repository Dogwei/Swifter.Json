using System;
using System.Reflection;

namespace Swifter.Reflection
{
    static class ThrowHelpers
    {
        public static void ThrowMissingMethodException(string memberType, Type type, MemberInfo member, string methodType)
        {
            throw new MissingMethodException($"{memberType} '{type.Name}.{member.Name}' No define '{methodType}' method.");
        }

        public static void ThrowTargetException(string parameterName, Type targetType)
        {
            throw new TargetException("Object does not match target type.");
        }

        public static void ThrowArgumentNullException(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }


        public static void ThrowInvalidOperationException(string memberType, string memberAttribute)
        {
            throw new InvalidOperationException($"Is not {memberAttribute} {memberType}.");
        }
    }
}