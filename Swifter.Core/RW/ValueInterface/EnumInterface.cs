using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class EnumInterface<T> : IValueInterface<T> where T : Enum
    {
        static readonly Type EnumType = typeof(T);
        static readonly TypeCode TypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(EnumType));
        
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is string st)
            {
                return (T)Enum.Parse(EnumType, st);
            }

            return (T)Enum.ToObject(EnumType, value);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            var name = Enum.GetName(EnumType, value);

            if (name != null)
            {
                valueWriter.WriteString(name);
            }
            else
            {
                switch (TypeCode)
                {
                    case TypeCode.SByte:
                        valueWriter.WriteSByte(Unsafe.As<T, sbyte>(ref value));
                        break;
                    case TypeCode.Byte:
                        valueWriter.WriteByte(Unsafe.As<T, byte>(ref value));
                        break;
                    case TypeCode.Int16:
                        valueWriter.WriteInt16(Unsafe.As<T, short>(ref value));
                        break;
                    case TypeCode.UInt16:
                        valueWriter.WriteUInt16(Unsafe.As<T, ushort>(ref value));
                        break;
                    case TypeCode.UInt32:
                        valueWriter.WriteUInt32(Unsafe.As<T, uint>(ref value));
                        break;
                    case TypeCode.Int64:
                        valueWriter.WriteInt64(Unsafe.As<T, long>(ref value));
                        break;
                    case TypeCode.UInt64:
                        valueWriter.WriteUInt64(Unsafe.As<T, ulong>(ref value));
                        break;
                    default:
                        valueWriter.WriteInt32(Unsafe.As<T, int>(ref value));
                        break;
                }
            }
        }
    }
}