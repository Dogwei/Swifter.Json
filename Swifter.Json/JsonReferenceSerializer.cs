using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Json
{
    internal sealed unsafe class JsonReferenceSerializer : BaseJsonSerializer, IValueWriter, IValueWriter<Guid>, IValueWriter<DateTimeOffset>, IDataWriter<string>, IDataWriter<int>, IValueFilter<string>, IValueFilter<int>
    {
        public readonly ReferenceCache<TargetPathInfo> references;
        public readonly JsonFormatterOptions options;

        public string indentedChars;
        public string lineBreak;
        public string middleChars;

        public TargetPathInfo reference;

        /// <summary>
        /// True: In Array, False: In Object.
        /// </summary>
        public bool isInArray;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonReferenceSerializer(JsonFormatterOptions options)
        {
            if ((options & JsonFormatterOptions.PriorCheckReferences) != 0 &&
                (options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.MultiReferencingReference)) == 0)
            {
                options ^= JsonFormatterOptions.PriorCheckReferences;
            }

            this.options = options;

            references = new ReferenceCache<TargetPathInfo>();
            
            reference = new TargetPathInfo("#", null);
        }

        public IValueWriter this[int key]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                if (options < JsonFormatterOptions.IgnoreNull)
                {
                    reference = new TargetPathInfo(key, reference.Parent);
                }

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

                if (options < JsonFormatterOptions.IgnoreNull)
                {
                    reference = new TargetPathInfo(key, reference.Parent);
                }

                return this;
            }
        }

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AddDepth()
        {
            ++depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool ContainsOrSaveReference(IDataReader dataReader)
        {
            var token = dataReader.ReferenceToken;

            if (token == null)
            {
                return false;
            }

            if (references.TryGetValue(token, out var reference))
            {
                if ((options & JsonFormatterOptions.LoopReferencingException) != 0)
                {
                    if (reference.IsFinish)
                    {
                        return false;
                    }

                    throw new JsonLoopReferencingException(reference, this.reference);
                }
                else if ((options & JsonFormatterOptions.PriorCheckReferences) != 0 && reference.Equals(this.reference))
                {
                    return false;
                }
                else if ((options & JsonFormatterOptions.LoopReferencingNull) != 0 && reference.IsFinish)
                {
                    reference.IsFinish = false;

                    return false;
                }

                return true;
            }

            references.Add(token, this.reference);

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool GetOrSaveReference(IDataReader dataReader, out TargetPathInfo reference)
        {
            var token = dataReader.ReferenceToken;

            if (token == null)
            {
                reference = null;

                return false;
            }

            if (references.TryGetValue(token, out reference))
            {
                if ((options & JsonFormatterOptions.PriorCheckReferences) != 0)
                {
                    if (reference.Equals(this.reference))
                    {
                        return false;
                    }
                }

                return true;
            }

            references.Add(token, this.reference);

            reference = null;

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if ((options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingException | JsonFormatterOptions.LoopReferencingNull)) != 0
                && basicType == TypeCode.Object
                && valueCopyer.Value is IDataReader reader
                && ContainsOrSaveReference(reader))
            {
                valueCopyer.DirectWrite(null);

                basicType = TypeCode.Empty;
            }

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
            reference = new TargetPathInfo(valueInfo.Key, reference.Parent);

            return Filter(valueInfo.ValueCopyer);
        }

        public bool Filter(ValueFilterInfo<int> valueInfo)
        {
            reference = new TargetPathInfo(valueInfo.Key, reference.Parent);

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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool CheckObject(IDataReader dataReader)
        {
            if ((options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingException | JsonFormatterOptions.LoopReferencingNull)) != 0)
            {
                if (options >= JsonFormatterOptions.IgnoreNull && !reference.IsRoot)
                {
                    /* In Filter Executed. */
                    return true;
                }

                if (ContainsOrSaveReference(dataReader))
                {
                    WriteNull();

                    return false;
                }
            }
            else
            {
                if (GetOrSaveReference(dataReader, out var reference))
                {
                    WriteReference(reference);

                    return false;
                }
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetLength(int index)
        {
            if (index <= 9)
            {
                return 1;
            }

            if (index <= 99)
            {
                return 2;
            }

            index /= 1000;

            int len = 3;

            while (index != 0)
            {
                index /= 10;

                ++len;
            }

            return len;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void WriteReferenceItem(TargetPathInfo reference)
        {
            var temp = reference;

            var length = 0;

        GetLengthLoop:

            if (temp.Name != null)
            {
                var name = temp.Name;

                for (int i = name.Length - 1; i >= 0; --i)
                {
                    switch (name[i])
                    {
                        case '\\':
                        case '/':
                        case '"':
                            length += 3;
                            break;
                        default:
                            ++length;
                            break;
                    }
                }
            }
            else
            {
                length += GetLength(temp.Index);
            }

            if (temp.Parent != null)
            {
                temp = temp.Parent;

                ++length;

                goto GetLengthLoop;
            }

            Expand(length + 2);

            this.offset += length;

            var offset = this.offset;

            var chars = hGlobal.chars;

        AppendLoop:

            if (reference.Name != null)
            {
                var name = reference.Name;

                for (int i = name.Length - 1; i >= 0; --i)
                {
                    var c = name[i];

                    switch (c)
                    {
                        case '\\':
                            // %5C
                            chars[--offset] = 'C';
                            chars[--offset] = '5';
                            chars[--offset] = '%';
                            break;
                        case '/':
                            // %2F
                            chars[--offset] = 'F';
                            chars[--offset] = '2';
                            chars[--offset] = '%';
                            break;
                        case '"':
                            // %22
                            chars[--offset] = '2';
                            chars[--offset] = '2';
                            chars[--offset] = '%';
                            break;
                        default:
                            chars[--offset] = c;
                            break;
                    }
                }
            }
            else
            {
                var index = reference.Index;

                do
                {
                    chars[--offset] = (char)(index - (index /= 10) * 10 + '0');

                } while (index > 0);
            }

            if (reference.Parent != null)
            {
                reference = reference.Parent;

                chars[--offset] = '/';

                goto AppendLoop;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteReference(TargetPathInfo reference)
        {
            WriteValueBefore();

            Expand(2);

            Append('{');

            WriteMiddleChars();

            Expand(8);

            Append('"');
            Append('$');
            Append('r');
            Append('e');
            Append('f');
            Append('"');

            WriteKeyAfter();

            Expand(2);

            Append('"');

            WriteReferenceItem(reference);

            Expand(3);

            Append('"');

            WriteMiddleChars();

            Append('}');

            WriteValueAfter();
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
            if (!CheckObject(dataReader))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append('[');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = true;

            int tOffset = offset;

            AddDepth();

            if ((options & JsonFormatterOptions.PriorCheckReferences) != 0 && dataReader.Count != 0)
            {
                dataReader.OnReadAll(new PriorCheckReferenceWriter(references, reference));
            }

            reference = new TargetPathInfo(null, reference);

            if (options >= JsonFormatterOptions.IgnoreNull && (options & JsonFormatterOptions.ArrayOnFilter) != 0)
            {
                dataReader.OnReadAll(this, this);
            }
            else
            {
                dataReader.OnReadAll(this);
            }

            reference = reference.Parent;

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

            reference.IsFinish = true;
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
            if (!CheckObject(dataReader))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append('{');

            WriteStructBefore();

            var isInArray = this.isInArray;

            this.isInArray = false;

            int tOffset = offset;

            AddDepth();

            if ((options & JsonFormatterOptions.PriorCheckReferences) != 0 && dataReader.Count != 0)
            {
                dataReader.OnReadAll(new PriorCheckReferenceWriter(references, reference));
            }

            reference = new TargetPathInfo(null, reference);

            if (options >= JsonFormatterOptions.IgnoreNull)
            {
                dataReader.OnReadAll(this, this);
            }
            else
            {
                dataReader.OnReadAll(this);
            }

            reference = reference.Parent;
            
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

            reference.IsFinish = true;
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