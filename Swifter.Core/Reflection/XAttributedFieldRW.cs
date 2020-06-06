
using Swifter.RW;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    class XAttributedFieldRW : IXFieldRW
    {
        internal readonly IXFieldRW fieldRW;
        internal readonly RWFieldAttribute attribute;
        internal readonly bool canRead;
        internal readonly bool canWrite;
        internal readonly string name;
        internal readonly bool skipDefaultValue;
        internal readonly bool cannotGetException;
        internal readonly bool cannotSetException;


        protected XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, XBindingFlags flags)
        {
            this.fieldRW = fieldRW;
            this.attribute = attribute;

            canRead = (attribute.Access & RWFieldAccess.ReadOnly) != 0 && fieldRW.CanRead;
            canWrite = (attribute.Access & RWFieldAccess.WriteOnly) != 0 && fieldRW.CanWrite;

            skipDefaultValue = attribute.SkipDefaultValue != RWBoolean.None ? attribute.SkipDefaultValue == RWBoolean.Yes : (flags & XBindingFlags.RWSkipDefaultValue) != 0;
            cannotGetException = attribute.CannotGetException != RWBoolean.None ? attribute.CannotGetException == RWBoolean.Yes : (flags & XBindingFlags.RWCannotGetException) != 0;
            cannotSetException = attribute.CannotSetException != RWBoolean.None ? attribute.CannotSetException == RWBoolean.Yes : (flags & XBindingFlags.RWCannotSetException) != 0;

            name = attribute.Name ?? fieldRW.Name;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void Assert(bool err, string name)
        {
            if (!err)
            {
                Throw();
            }

            void Throw()
            {
                throw new MemberAccessException($"Attributed Property '{Name}' Don't access '{name}' method.");
            }
        }

        public bool CanRead => canRead;

        public bool CanWrite => canWrite;

        public string Name => name;

        public int Order => attribute.Order;

        public Type BeforeType => fieldRW.BeforeType;

        public Type AfterType => fieldRW.AfterType;

        public bool IsPublic => fieldRW.IsPublic;

        public bool IsStatic => fieldRW.IsStatic;

        public object Original => fieldRW.Original;

        public bool SkipDefaultValue => skipDefaultValue;

        public bool CannotGetException => cannotGetException;

        public bool CannotSetException => cannotSetException;

        public virtual void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(canRead, "read");

            fieldRW.OnReadValue(obj, valueWriter);
        }

        public virtual void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(canWrite, "write");

            fieldRW.OnWriteValue(obj, valueReader);
        }

        public T ReadValue<T>(object obj)
        {
            Assert(canRead, "read");

            return fieldRW.ReadValue<T>(obj);
        }

        public void WriteValue<T>(object obj, T value)
        {
            Assert(canWrite, "write");

            fieldRW.WriteValue(obj, value);
        }


        public static XAttributedFieldRW Create(IXFieldRW fieldRW, RWFieldAttribute attribute, XBindingFlags flags)
        {
            attribute.GetBestMatchInterfaceMethod(fieldRW.BeforeType, out var firstArg, out var read, out var write);

            if (read != null && write != null)
            {
                var valueType = write.GetParameters()[1].ParameterType;

                if (valueType != read.ReturnType)
                {
                    throw new NotSupportedException("The type of the value of the Read method is inconsistent with the value of the Write method.");
                }

                var type = firstArg.GetType();

                foreach (var item in type.GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IValueInterface<>))
                    {
                        var arguments = item.GetGenericArguments();

                        if (arguments.Length == 1 && arguments[0] == valueType)
                        {
                            var rwType = typeof(XInterfaceAttributedFieldRW<>).MakeGenericType(valueType);

                            var map = type.GetInterfaceMap(item);

                            if ((map.TargetMethods.Contains(read) || map.InterfaceMethods.Contains(read)) &&
                                (map.TargetMethods.Contains(write) || map.InterfaceMethods.Contains(write)))
                            {
                                return (XAttributedFieldRW)Activator.CreateInstance(
                                    rwType,
                                    new object[] {
                                        fieldRW,
                                        attribute,
                                        flags,
                                        firstArg
                                    });
                            }
                        }
                    }
                }

                if (valueType.IsByRef || valueType.IsPointer)
                {
                    valueType = typeof(IntPtr);
                }

                return (XAttributedFieldRW)Activator.CreateInstance(
                    typeof(XDelegateAttributedFieldRW<>).MakeGenericType(valueType),
                    new object[] {
                            fieldRW,
                            attribute,
                            flags,
                            firstArg,
                            read,
                            write
                    });
            }

            return new XAttributedFieldRW(fieldRW, attribute, flags);
        }
    }
}