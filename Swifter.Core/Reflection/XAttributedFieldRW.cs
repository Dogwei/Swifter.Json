using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Linq;
using System.Reflection;
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


        protected XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute)
        {
            this.fieldRW = fieldRW;
            this.attribute = attribute;

            canRead = (attribute.Access & RWFieldAccess.ReadOnly) != 0 && fieldRW.CanRead;
            canWrite = (attribute.Access & RWFieldAccess.WriteOnly) != 0 && fieldRW.CanWrite;

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


        public static implicit operator XAttributedFieldRW((IXFieldRW fieldRW, RWFieldAttribute attribute) infos)
        {
            infos.attribute.GetBestMatchInterfaceMethod(infos.fieldRW.BeforeType, out var firstArg, out var read, out var write);

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
                                        infos.fieldRW,
                                        infos.attribute,
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
                    typeof(XAttributedFieldRW<>).MakeGenericType(valueType),
                    new object[] {
                            infos.fieldRW,
                            infos.attribute,
                            firstArg,
                            read,
                            write
                    });
            }

            return new XAttributedFieldRW(infos.fieldRW, infos.attribute);
        }
    }

    sealed class XAttributedFieldRW<T> : XAttributedFieldRW, IObjectField
    {
        internal readonly Func<IValueReader, T> read;
        internal readonly Action<IValueWriter, T> write;

        public XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, object firstArg, MethodInfo read, MethodInfo write)
            : base(fieldRW, attribute)
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

    sealed class XInterfaceAttributedFieldRW<T> : XAttributedFieldRW, IObjectField
    {
        internal readonly IValueInterface<T> valueInterface;

        public XInterfaceAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, IValueInterface<T> valueInterface) : base(fieldRW, attribute)
        {
            this.valueInterface = valueInterface;
        }

        public override void OnReadValue(object obj, IValueWriter valueWriter)
        {
            valueInterface.WriteValue(valueWriter, ReadValue<T>(obj));
        }

        public override void OnWriteValue(object obj, IValueReader valueReader)
        {
            WriteValue(obj, valueInterface.ReadValue(valueReader));
        }

        Type IObjectField.AfterType => typeof(T);
    }
}