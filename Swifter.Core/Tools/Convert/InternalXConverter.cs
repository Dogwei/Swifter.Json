using System;
using System.Reflection;

namespace Swifter.Tools
{
    sealed class InternalXConverter
    {
        public MethodBase? Method;
        public XConvertMode Mode;
        public Func<object, object?> Convert;

        public InternalXConverter(MethodBase? method, XConvertMode mode, Func<object, object?> convert)
        {
            Method = method;
            Mode = mode;
            Convert = convert;
        }
    }
}