
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;

namespace Swifter.Reflection
{
    sealed class XDelegateAttributedFieldRW<T> : XAttributedFieldRW, IObjectField
    {
        internal readonly Func<IValueReader, T> read;
        internal readonly Action<IValueWriter, T> write;

        public XDelegateAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, XBindingFlags flags, object firstArg, MethodInfo read, MethodInfo write)
            : base(fieldRW, attribute, flags)
        {
            if (read.IsStatic)
            {
                this.read = MethodHelper.CreateDelegate<Func<IValueReader, T>>(read, false);
            }
            else
            {
                var _read = MethodHelper.CreateDelegate<Func<object, IValueReader, T>>(read, false);

                this.read = valueReader => _read(firstArg, valueReader);
            }

            if (write.IsStatic)
            {
                this.write = MethodHelper.CreateDelegate<Action<IValueWriter, T>>(write, false);
            }
            else
            {
                var _write = MethodHelper.CreateDelegate<Action<object, IValueWriter, T>>(write, false);

                this.write = (valueWriter, value) => _write(firstArg, valueWriter, value);
            }
        }

        public override void OnReadValue(object obj, IValueWriter valueWriter)
        {
            write(valueWriter, ReadValue<T>(obj));
        }

        public override void OnWriteValue(object obj, IValueReader valueReader)
        {
            WriteValue(obj, read(valueReader));
        }

        Type IObjectField.AfterType => typeof(T);
    }
}