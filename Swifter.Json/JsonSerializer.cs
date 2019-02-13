using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonSerializer : BaseJsonSerializer, IValueWriter, IValueWriter<Guid>, IValueWriter<DateTimeOffset>, IDataWriter<string>, IDataWriter<int>, IValueFilter<string>, IValueFilter<int>
    {
        public readonly JsonFormatterOptions options;
        public readonly int maxDepth;
        
        public string indentedChars;
        public string lineBreak;
        public string middleChars;

        /// <summary>
        /// True: In Array, False: In Object.
        /// </summary>
        public bool isInArray;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(JsonFormatterOptions options, int maxDepth)
        {
            this.options = options;

            this.maxDepth = maxDepth;
        }

        public IValueWriter this[int key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return this;
            }
        }

        public IValueWriter this[string key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                WriteKeyBefore();

                InternalWriteString(key);

                WriteKeyAfter();
                
                return this;
            }
        }
        
        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool AddDepth()
        {
            ++depth;

            if (depth <= maxDepth)
            {
                return true;
            }

            if ((options & JsonFormatterOptions.OutOfDepthException) != 0)
            {
                throw new JsonOutOfDepthException();
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if ((options & JsonFormatterOptions.IgnoreNull) != 0
                && basicType == TypeCode.Empty)
            {
                return false;
            }

            if ((options & JsonFormatterOptions.IgnoreZero) != 0)
            {
                switch (basicType)
                {
                    case TypeCode.SByte:
                        return valueCopyer.ReadSByte() != 0;
                    case TypeCode.Int16:
                        return valueCopyer.ReadInt16() != 0;
                    case TypeCode.Int32:
                        return valueCopyer.ReadInt32() != 0;
                    case TypeCode.Int64:
                        return valueCopyer.ReadInt64() != 0;
                    case TypeCode.Byte:
                        return valueCopyer.ReadByte() != 0;
                    case TypeCode.UInt16:
                        return valueCopyer.ReadUInt16() != 0;
                    case TypeCode.UInt32:
                        return valueCopyer.ReadUInt32() != 0;
                    case TypeCode.UInt64:
                        return valueCopyer.ReadUInt64() != 0;
                    case TypeCode.Single:
                        return valueCopyer.ReadSingle() != 0;
                    case TypeCode.Double:
                        return valueCopyer.ReadDouble() != 0;
                    case TypeCode.Decimal:
                        return valueCopyer.ReadDecimal() != 0;
                }
            }

            if ((options & JsonFormatterOptions.IgnoreEmptyString) != 0
                && basicType == TypeCode.String
                && valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }
        
        public bool Filter(ValueFilterInfo<string> valueInfo)
        {
            return Filter(valueInfo.ValueCopyer);
        }
        
        public bool Filter(ValueFilterInfo<int> valueInfo)
        {
            return Filter(valueInfo.ValueCopyer);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueBefore()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                if (isInArray)
                {
                    Append(lineBreak);

                    for (int i = depth; i > 0; --i)
                    {
                        Append(indentedChars);
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueAfter()
        {
            Append(',');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyBefore()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                if (!isInArray)
                {
                    Append(lineBreak);

                    for (int i = depth; i > 0; --i)
                    {
                        Append(indentedChars);
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyAfter()
        {
            Append(':');

            WriteMiddleChars();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteMiddleChars()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                Append(middleChars);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructBefore()
        {
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructAfter()
        {
            if ((options & JsonFormatterOptions.Indented) != 0)
            {
                Append(lineBreak);

                for (int i = depth; i > 0; --i)
                {
                    Append(indentedChars);
                }
            }
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
            WriteValueBefore();

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
            WriteValueBefore();

            Expand(2);

            Append('[');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = true;

            int tOffset = offset;

            if (AddDepth())
            {
                if (options >= JsonFormatterOptions.IgnoreNull && (options & JsonFormatterOptions.ArrayOnFilter) != 0)
                {
                    dataReader.OnReadAll(this, this);
                }
                else
                {
                    dataReader.OnReadAll(this);
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            WriteStructAfter();

            Append(']');

            this.isInArray = isInArray;

            WriteValueAfter();
        }
        
        public void WriteBoolean(bool value)
        {
            WriteValueBefore();

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
            WriteValueBefore();

            Expand(4);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteChar(char value)
        {
            WriteValueBefore();

            Expand(4);

            Append('"');
            Append(value);
            Append('"');

            WriteValueAfter();
        }
        
        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
        
        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Expand(33);

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteDouble(double value)
        {
            WriteValueBefore();


            Expand(19);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }

        public void WriteInt16(short value)
        {
            WriteValueBefore();


            Expand(8);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);


            WriteValueAfter();
        }
        
        public void WriteInt32(int value)
        {
            WriteValueBefore();


            Expand(12);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteInt64(long value)
        {
            WriteValueBefore();


            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteObject(IDataReader<string> dataReader)
        {
            WriteValueBefore();

            Expand(2);

            Append('{');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = false;

            int tOffset = offset;

            if (AddDepth())
            {
                if (options >= JsonFormatterOptions.IgnoreNull)
                {
                    dataReader.OnReadAll(this, this);
                }
                else
                {
                    dataReader.OnReadAll(this);
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            WriteStructAfter();

            Append('}');

            this.isInArray = isInArray;

            WriteValueAfter();
        }
        
        public void WriteSByte(sbyte value)
        {
            WriteValueBefore();
            
            Expand(5);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteSingle(float value)
        {
            WriteValueBefore();
            
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
                WriteValueBefore();
                
                InternalWriteString(value);

                WriteValueAfter();
            }
        }
        
        public void WriteUInt16(ushort value)
        {
            WriteValueBefore();


            Expand(7);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteUInt32(uint value)
        {
            WriteValueBefore();


            Expand(11);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteUInt64(ulong value)
        {
            WriteValueBefore();


            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGlobal.chars + offset);

            WriteValueAfter();
        }
        
        public void WriteValue(Guid value)
        {
            WriteValueBefore();


            Expand(40);

            Append('"');

            offset += NumberHelper.ToString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }

        public void WriteValue(DateTimeOffset value)
        {
            WriteValueBefore();
            
            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGlobal.chars + offset);

            Append('"');

            WriteValueAfter();
        }
    }
}