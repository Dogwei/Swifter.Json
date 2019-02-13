using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonDefaultSerializer : BaseJsonSerializer, IValueWriter, IValueWriter<Guid>, IValueWriter<DateTimeOffset>, IDataWriter<string>, IDataWriter<int>
    {
        public readonly int maxDepth;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDefaultSerializer(int maxDepth)
        {
            this.maxDepth = maxDepth;
        }

        public IValueWriter this[int key] => this;

        public IValueWriter this[string key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                InternalWriteString(key);

                WriteKeyAfter();

                return this;
            }
        }
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool AddDepth()
        {
            ++depth;

            return depth <= maxDepth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }
       
        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueAfter()
        {
            Append(',');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyAfter()
        {
            Append(':');
        }
        
        public void DirectWrite(object value)
        {
            if (value == null)
            {
                WriteNull();

                return;
            }

            if (value is IConvertible)
            {
                switch (((IConvertible)value).GetTypeCode())
                {
                    case TypeCode.DBNull:
                        WriteNull();
                        return;
                    case TypeCode.Boolean:
                        WriteBoolean((bool)value);
                        return;
                    case TypeCode.Char:
                        WriteChar((char)value);
                        return;
                    case TypeCode.SByte:
                        WriteSByte((sbyte)value);
                        return;
                    case TypeCode.Byte:
                        WriteByte((byte)value);
                        return;
                    case TypeCode.Int16:
                        WriteInt16((short)value);
                        return;
                    case TypeCode.UInt16:
                        WriteUInt16((ushort)value);
                        return;
                    case TypeCode.Int32:
                        WriteInt32((int)value);
                        return;
                    case TypeCode.UInt32:
                        WriteUInt32((uint)value);
                        return;
                    case TypeCode.Int64:
                        WriteInt64((long)value);
                        return;
                    case TypeCode.UInt64:
                        WriteUInt64((ulong)value);
                        return;
                    case TypeCode.Single:
                        WriteSingle((float)value);
                        return;
                    case TypeCode.Double:
                        WriteDouble((double)value);
                        return;
                    case TypeCode.Decimal:
                        WriteDecimal((decimal)value);
                        return;
                    case TypeCode.DateTime:
                        WriteDateTime((DateTime)value);
                        return;
                    case TypeCode.String:
                        WriteString((string)value);
                        return;
                }
            }

            if (value is Guid)
            {
                WriteValue((Guid)value);

                return;
            }

            WriteString(value.ToString());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            Expand(6);

            // null
            Append('n');
            Append('u');
            Append('l');
            Append('l');

            WriteValueAfter();
        }
        
        public void WriteArray(IDataReader<int> dataReader)
        {
            Expand(2);

            Append('[');

            int tOffset = offset;

            if (AddDepth())
            {
                dataReader.OnReadAll(this);
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            Append(']');

            WriteValueAfter();
        }
        
        public void WriteBoolean(bool value)
        {
            Expand(6);

            if (value)
            {
                // true
                Append('t');
                Append('r');
                Append('u');
                Append('e');
            }
            else
            {
                // false
                Append('f');
                Append('a');
                Append('l');
                Append('s');
                Append('e');
            }

            WriteValueAfter();
        }
        
        public void WriteByte(byte value)
        {
            Expand(4);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteChar(char value)
        {
            Expand(4);

            Append('"');
            Append(value);
            Append('"');

            WriteValueAfter();
        }
        
        public void WriteDateTime(DateTime value)
        {
            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
        
        public void WriteDecimal(decimal value)
        {
            Expand(33);

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteDouble(double value)
        {
            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }
        
        public void WriteInt16(short value)
        {
            Expand(8);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }
        
        public void WriteInt32(int value)
        {
            Expand(12);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteInt64(long value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteObject(IDataReader<string> dataReader)
        {
            Expand(2);

            Append('{');

            int tOffset = offset;

            if (AddDepth())
            {
                dataReader.OnReadAll(this);
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }
            
            Append('}');
            
            WriteValueAfter();
        }
        
        public void WriteSByte(sbyte value)
        {
            Expand(5);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteSingle(float value)
        {
            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteString(value);

                WriteValueAfter();
            }
        }
        
        public void WriteUInt16(ushort value)
        {
            Expand(7);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteUInt32(uint value)
        {
            Expand(11);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteUInt64(ulong value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteValue(Guid value)
        {
            Expand(40);

            Append('"');

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }

        public void WriteValue(DateTimeOffset value)
        {
            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
    }
}