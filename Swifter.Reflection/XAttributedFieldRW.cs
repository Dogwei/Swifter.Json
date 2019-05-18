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
                var type = firstArg.GetType();
                var rwType = typeof(XInterfaceAttributedFieldRW<>).MakeGenericType(read.ReturnType);
                var interfaceType = rwType.GetField(
                    nameof(XInterfaceAttributedFieldRW<object>.valueInterface),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FieldType;

                if (interfaceType.IsAssignableFrom(type))
                {
                    var map = type.GetInterfaceMap(interfaceType);

                    if ((map.TargetMethods.Contains(read) || map.InterfaceMethods.Contains(read)) &&
                        (map.TargetMethods.Contains(write) || map.InterfaceMethods.Contains(write))
                        )
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

                return (XAttributedFieldRW)Activator.CreateInstance(
                    typeof(XAttributedFieldRW<>).MakeGenericType(read.ReturnType),
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
        internal readonly object firstArg;
        internal readonly Func<object, IValueReader, T> read;
        internal readonly Action<object, IValueWriter, T> write;

        public XAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, object firstArg, MethodInfo read, MethodInfo write)
            : base(fieldRW, attribute)
        {
            this.firstArg = firstArg;

            this.read = MethodHelper.CreateDelegate<Func<object, IValueReader, T>>(read, SignatureLevels.Cast);
            this.write = MethodHelper.CreateDelegate<Action<object, IValueWriter, T>>(write, SignatureLevels.Cast);
        }

        public override void OnReadValue(object obj, IValueWriter valueWriter)
        {
            write(firstArg, valueWriter, ReadValue<T>(obj));
        }

        public override void OnWriteValue(object obj, IValueReader valueReader)
        {
            WriteValue(obj, read(firstArg, valueReader));
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