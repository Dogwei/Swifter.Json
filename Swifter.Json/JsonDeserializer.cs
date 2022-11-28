using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Swifter.Json.JsonCode;
using static Swifter.Json.JsonFormatter;
#if !NO_OPTIONS
using static Swifter.Json.JsonFormatterOptions;
#endif

namespace Swifter.Json
{
    /// <summary>
    /// Json 反序列化 (读取) 器。
    /// </summary>
    public sealed unsafe partial class JsonDeserializer :
        IFormatterReader,
        IFastArrayValueReader
    {
        /// <summary>
        /// Json 格式化器。
        /// </summary>
        public readonly JsonFormatter? JsonFormatter;

        /// <summary>
        /// 分段读取器
        /// </summary>
        readonly JsonSegmentedContent? content;

        /// <summary>
        /// 最大深度限制。
        /// </summary>
        public readonly int MaxDepth;

#if !NO_OPTIONS
        /// <summary>
        /// 当前配置项。
        /// </summary>
        public readonly JsonFormatterOptions Options;

        readonly object? mode;
#endif

        char* begin;
        char* end;

        char* current;

        int depth;

        /// <summary>
        /// 获取已读取的字符数。
        /// </summary>
        public int Offset
            => (int)(current - begin);

        /// <summary>
        /// 获取剩余字符数。
        /// </summary>
        int Rest
            => (int)(end - current);

#if !NO_OPTIONS
        ReferenceMode ReferenceModeInstance
            => Unsafe.As<ReferenceMode>(mode!);
#endif

        #region -- 构造函数 --

        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="content">分段读取器</param>
        internal JsonDeserializer(JsonSegmentedContent content)
            : this(content.hGCache.First, content.hGCache.Count)
        {
            this.content = content;
        }

        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="chars">Json 字符串</param>
        /// <param name="length">Json 字符串长度</param>
        internal JsonDeserializer(char* chars, int length)
        {
            begin = chars;
            end = chars + length;
            current = chars;

            MaxDepth = DefaultMaxDepth;

#if !NO_OPTIONS
            Options = JsonFormatterOptions.Default;
#endif
        }

#if !NO_OPTIONS
        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="content">分段内容</param>
        /// <param name="options">指定配置</param>
        internal JsonDeserializer(JsonSegmentedContent content, JsonFormatterOptions options)
            : this(content.hGCache.First, content.hGCache.Count, options)
        {
            this.content = content;
        }

        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="chars">Json 字符串</param>
        /// <param name="length">Json 字符串长度</param>
        /// <param name="options">指定配置</param>
        internal JsonDeserializer(char* chars, int length, JsonFormatterOptions options)
            : this(chars, length)
        {
            Options = options;

            if (On(MultiReferencingReference))
            {
                mode = new ReferenceMode();
            }
        }
#endif

        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="jsonFormatter">指定格式化器</param>
        /// <param name="content">分段内容</param>
        internal JsonDeserializer(JsonFormatter jsonFormatter, JsonSegmentedContent content)
            : this(jsonFormatter, content.hGCache.First, content.hGCache.Count)
        {
            this.content = content;
        }

        /// <summary>
        /// 初始化 Json 反序列化 (读取) 器。
        /// </summary>
        /// <param name="jsonFormatter">指定格式化器</param>
        /// <param name="chars">Json 字符串</param>
        /// <param name="length">Json 字符串长度</param>
        internal JsonDeserializer(JsonFormatter jsonFormatter, char* chars, int length)
            : this(chars, length
#if !NO_OPTIONS
                  , jsonFormatter.Options
#endif
                  )
        {
            JsonFormatter = jsonFormatter;
            MaxDepth = jsonFormatter.MaxDepth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="jsonFormatter"></param>
        /// <returns></returns>
        public static JsonDeserializer Create(
            Ps<char> chars,
            JsonFormatter? jsonFormatter = null
            )
        {
            if (jsonFormatter != null)
            {
                return new JsonDeserializer(jsonFormatter, chars.Pointer, chars.Length);
            }

            return new JsonDeserializer(chars.Pointer, chars.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hGCache"></param>
        /// <param name="textReader"></param>
        /// <param name="jsonFormatter"></param>
        /// <returns></returns>
        public static JsonDeserializer Create(
            HGlobalCache<char> hGCache,
            TextReader textReader,
            JsonFormatter? jsonFormatter = null
            )
        {
            if (jsonFormatter != null)
            {
                return new JsonDeserializer(jsonFormatter, JsonSegmentedContent.CreateAndInitialize(textReader, hGCache));
            }

            return new JsonDeserializer(JsonSegmentedContent.CreateAndInitialize(textReader, hGCache));
        }

#if !NO_OPTIONS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonDeserializer Create(
            Ps<char> chars,
            JsonFormatterOptions options
            )
        {
            return new JsonDeserializer(chars.Pointer, chars.Length, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hGCache"></param>
        /// <param name="textReader"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonDeserializer Create(
            HGlobalCache<char> hGCache,
            TextReader textReader,
            JsonFormatterOptions options
            )
        {
            return new JsonDeserializer(JsonSegmentedContent.CreateAndInitialize(textReader, hGCache), options);
        }

#endif

        #endregion

        #region -- 数值 --

        /// <summary>
        /// 读取一个 <see cref="Boolean"/> 值。
        /// </summary>
        public bool ReadBoolean()
        {
            var curr = *current;

            switch (curr)
            {
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return true;
                    }
                    break;
#if !NO_OPTIONS
                case FixString:
                case FixChars:
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }

                    break;
#endif
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return false;
                    }
                    break;
            }

            if (curr >= FixNumberMin && curr <= FixNumberMax && IsValueEnding(1))
            {
                ++current;

                return curr != FixNumberMin;
            }

            return Convert.ToBoolean(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="Byte"/> 值。
        /// </summary>
        public byte ReadByte()
            => checked((byte)ReadUInt64());

        /// <summary>
        /// 读取一个 <see cref="Char"/> 值。
        /// </summary>
        public char ReadChar()
        {
            if (Rest >= 3 && current[0] is FixString or FixChars && current[0] == current[2] && current[1] is not FixEscape)
            {
                var result = current[1];

                current += 3;

                return result;
            }

#if !NO_OPTIONS
            if (On(EmptyStringAsDefault) && current[0] is FixString or FixChars && TryReadEmptyString())
            {
                return default;
            }
#endif

            return char.Parse(ReadString() ?? throw new NullReferenceException());
        }

        /// <summary>
        /// 读取一个 <see cref="DateTime"/> 值。
        /// </summary>
        public DateTime ReadDateTime()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<DateTime>();
            }

            return Convert.ToDateTime(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="Decimal"/> 值。
        /// </summary>
        public decimal ReadDecimal()
        {
            var (_, length, value) = NumberHelper.ParseDecimal(current, Rest);

            if (length != 0 && IsValueEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadDecimal();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        decimal NoInliningReadDecimal()
        {
        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<decimal>();
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    break;
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (numberInfo.IsNumber)
            {
                if (IsValueEnding(numberInfo.End) && numberInfo.IsDecimal)
                {
                    current += numberInfo.End;

                    return numberInfo.ToDecimal();
                }

                return decimal.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToDecimal(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="Double"/> 值。
        /// </summary>
        public double ReadDouble()
        {
#if !NO_OPTIONS
            if (On(UseSystemFloatingPointsMethods))
            {
                var currentBackup = current;

                switch (*currentBackup)
                {
                    case FixPositive:
                    case FixNegative:
                    case var digit when digit >= FixNumberMin && digit <= FixNumberMax:
#if NativeSpan
                        if (double.TryParse(InternalReadText(), out var value))
                        {
                            return value;
                        }

#else

                        if (double.TryParse(InternalReadText().ToStringEx(), out var value))
                        {
                            return value;
                        }

#endif
                        break;
                }

                current = currentBackup;

            }
            else
#endif
            {
                var (_, length, value) = NumberHelper.DecimalParseDouble(current, Rest);

                if (length != 0 && IsValueEnding(length))
                {
                    current += length;

                    return value;
                }
            }

            return NoInliningReadDouble();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        double NoInliningReadDouble()
        {
        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<double>();
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    break;
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (numberInfo.IsNumber)
            {
                if (IsValueEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToDouble(radix);
                }

                return double.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToDouble(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="Int16"/> 值。
        /// </summary>
        public short ReadInt16()
            => checked((short)ReadInt64());

        /// <summary>
        /// 读取一个 <see cref="Int32"/> 值。
        /// </summary>
        public int ReadInt32()
            => checked((int)ReadInt64());

        /// <summary>
        /// 读取一个 <see cref="Int64"/> 值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var (_, length, value) = NumberHelper.DecimalParseInt64(current, Rest);

            if (length != 0 && IsValueEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        long NoInliningReadInt64()
        {

        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<long>();
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    break;

                case FixPositive:
                case FixNegative:
                case var curr when curr >= FixNumberMin && curr <= FixNumberMax:

                    if (TryReadSegment(-1))
                    {
                        var (_, length, value) = NumberHelper.DecimalParseInt64(current, Rest);

                        if (length != 0 && IsValueEnding(length))
                        {
                            current += length;

                            return value;
                        }
                    }

                    break;
            }

            return InternalNoInliningReadInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        long InternalNoInliningReadInt64()
        {
            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (numberInfo.IsNumber)
            {
                if (IsValueEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToInt64(radix);
                }

                // TODO: Span
                return long.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="SByte"/> 值。
        /// </summary>
        public sbyte ReadSByte()
            => checked((sbyte)ReadInt64());

        /// <summary>
        /// 读取一个 <see cref="Single"/> 值。
        /// </summary>
        public float ReadSingle()
        {
            var @double = ReadDouble();
            var @float = (float)@double;

            if (!float.IsInfinity(@float))
            {
                return @float;
            }

            if (double.IsInfinity(@double))
            {
                return @float;
            }

            throw new OverflowException();
        }

        /// <summary>
        /// 读取一个 <see cref="UInt16"/> 值。
        /// </summary>
        public ushort ReadUInt16()
            => checked((ushort)ReadUInt64());

        /// <summary>
        /// 读取一个 <see cref="UInt32"/> 值。
        /// </summary>
        public uint ReadUInt32()
            => checked((uint)ReadUInt64());

        /// <summary>
        /// 读取一个 <see cref="UInt64"/> 值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            var (_, length, value) = NumberHelper.DecimalParseUInt64(current, Rest);

            if (length != 0 && IsValueEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadUInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        ulong NoInliningReadUInt64()
        {
        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<ulong>();
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    break;
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (numberInfo.IsNumber)
            {
                if (IsValueEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToUInt64(radix);
                }

                return ulong.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToUInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="Guid"/> 值。
        /// </summary>
        public Guid ReadGuid()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<Guid>();
            }

            return XConvert.Convert<Guid>(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="DateTimeOffset"/> 值。
        /// </summary>
        public DateTimeOffset ReadDateTimeOffset()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<DateTimeOffset>();
            }

            return XConvert.Convert<DateTimeOffset>(DirectRead());
        }

        /// <summary>
        /// 读取一个 <see cref="TimeSpan"/> 值。
        /// </summary>
        public TimeSpan ReadTimeSpan()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif

                    return InternalReadParse<TimeSpan>();
            }

            return XConvert.Convert<TimeSpan>(DirectRead());
        }

        #endregion

        #region -- 引用 --

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool IsReference()
        {
            var swap = current;

            ++current;

            var res = SkipReferenceKey();

            current = swap;

            return res;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool SkipReferenceKey()
        {
            SkipWhiteSpace();

            var res = false;

        Loop:

            if (current < end)
            {
                switch (current[0])
                {
                    case DollarChar:
                        res = Verify(RefKey);
                        current += RefKey.Length;
                        break;
                    case FixString:
                        res = Verify(RefKeyString);
                        current += RefKeyString.Length;
                        break;
                    case FixChars:
                        res = Verify(RefKeyChars);
                        current += RefKeyChars.Length;
                        break;
                    case FixComment:
                        if (SkipComment())
                        {
                            goto Loop;
                        }
                        break;
                }
            }

            SkipWhiteSpace();

            if (res)
            {

            Key:

                if (current < end)
                {
                    if (*current is KeyEnding)
                    {
                        ++current;

                        SkipWhiteSpace();
                    }
                    else if (*current is FixComment && SkipComment())
                    {
                        goto Key;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 读取一个引用。
        /// </summary>
        /// <exception cref="NotSupportedException">不是一个引用值或引用值格式错误</exception>
        public RWPath ReadReference()
        {
            const string FormatErrorMessage = "Reference value does not end correctly.";
            const string NotReferenceMessage = "Not a reference value.";

            if (!(current < end && *current == FixObject))
            {
                throw new NotSupportedException(FormatErrorMessage);
            }

            ++current;

            if (!SkipReferenceKey())
            {
                throw new NotSupportedException(NotReferenceMessage);
            }

            try
            {

            Loop:

                switch (*current)
                {
                    case FixString:
                    case FixChars:

                        var result = InternalReadParse<RWPath>();

                        if (result is not null)
                        {
                            return result;
                        }

                        throw new NullReferenceException();
                    case FixComment:

                        if (SkipComment())
                        {
                            if (current >= end)
                            {
                                ThrowException();
                            }

                            goto Loop;
                        }

                        goto default;
                    default:
                        var str = ReadString();

                        if (str is not null)
                        {
                            result = SlowParse<RWPath>(str);

                            if (result is not null)
                            {
                                return result;
                            }
                        }

                        throw new NullReferenceException();
                }
            }
            finally
            {
                SkipWhiteSpace();

                if (!(current < end && *current == ObjectEnding))
                {
                    throw new NotSupportedException(FormatErrorMessage);
                }

                ++current;
            }
        }

        #endregion

        #region -- 结构 --

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        public T?[]? ReadArray<T>()
        {
            SkipWhiteSpace();

        Loop:

            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    goto default;
#if !NO_OPTIONS
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return null;
                    }

                    goto default;
#endif
                case FixArray:
                    break;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        goto Loop;
                    }

                    goto default;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto default;

                default:

                    return XConvert.Convert<T[]>(DirectRead());
            }

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.EnterObject(new ArrayRW<T>());
            }
#endif

            var result = ReadArray<T>(JsonArrayAppendingInfo<T[]>.AppendingInfo.MostClosestMeanCommonlyUsedLength, out var length);

            JsonArrayAppendingInfo<T[]>.AppendingInfo.AddUsedLength(length);

            if (result.Length != length)
            {
                Array.Resize(ref result, length);
            }

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.CurrentObject.Content = result;

                ReferenceModeInstance.LeavaObject();
            }
#endif

            return result;
        }

        /// <summary>
        /// 读取一个列表。
        /// </summary>
        public List<T?>? ReadList<T>()
        {
            SkipWhiteSpace();

        Loop:

            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    goto default;
#if !NO_OPTIONS
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return null;
                    }

                    goto default;
#endif
                case FixArray:
                    break;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        goto Loop;
                    }

                    goto default;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto default;

                default:

                    return XConvert.Convert<List<T?>?>(DirectRead());
            }

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.EnterObject(new ArrayRW<T>());
            }
#endif

            var array = ReadArray<T>(JsonArrayAppendingInfo<List<T>>.AppendingInfo.MostClosestMeanCommonlyUsedLength, out var length);

            JsonArrayAppendingInfo<List<T>>.AppendingInfo.AddUsedLength(length);

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.CurrentObject.Content = array;

                ReferenceModeInstance.LeavaObject();
            }
#endif

            return ArrayHelper.CreateList(array, length);
        }

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        /// <param name="defaultCapacity">默认容量</param>
        /// <param name="length">返回数组长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T?[] ReadArray<T>(int defaultCapacity, out int length)
        {
            var array = new T?[defaultCapacity];
            var offset = 0;

            FastObjectRW<T>? fastObjectRW = null;

            if (ValueInterface<T>.IsFastObjectInterface)
            {
                fastObjectRW = FastObjectRW<T>.Create();
            }

            if (depth < MaxDepth)
            {
                ++current;
                ++depth;

#if !NO_OPTIONS
                if (On(MultiReferencingReference))
                {
                    // TODO: 缺少首次赋值
                    ReferenceModeInstance.CurrentObject.Content = array;
                }
#endif

            Loop:

                SkipWhiteSpace();

                if (current < end)
                {
                    if (*current == ArrayEnding)
                    {
                        goto Return;
                    }

                    if (offset >= array.Length)
                    {
                        Array.Resize(ref array, offset * 2 + 1);

#if !NO_OPTIONS
                        if (On(MultiReferencingReference))
                        {
                            // TODO: 缺少首次赋值
                            ReferenceModeInstance.CurrentObject.Content = array;
                        }
#endif
                    }

#if !NO_OPTIONS
                    if (On(MultiReferencingReference))
                    {
                        ReferenceModeInstance.SetCurrentKey(offset);
                    }
#endif
                    if (fastObjectRW is not null)
                    {
                        ReadObject(fastObjectRW);

                        array[offset] = fastObjectRW.content;

#if !NO_OPTIONS
                        if (On(MultiReferencingReference))
                        {
                            fastObjectRW = FastObjectRW<T>.Create();
                        }
                        else
#endif
                        {
                            fastObjectRW.content = default;
                        }
                    }
                    else if (ValueInterface<T>.IsDefaultBehavior)
                    {
                        array[offset] = ReadValue<T>();
                    }
                    else
                    {
                        array[offset] = ValueInterface<T>.ReadValue(this);
                    }

                    ++offset;

                Next:

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        switch (*current)
                        {
                            case ArrayEnding:
                                goto Return;
                            case ValueEnding:
                                ++current;

                                goto Loop;
                            case FixComment:
                                if (SkipComment())
                                {
                                    goto Next;
                                }

                                break;
                        }
                    }
                    else if (TryReadSegment(0))
                    {
                        goto Next;
                    }
                }
                else if (TryReadSegment(0))
                {
                    goto Loop;
                }

                throw MakeException();

            Return:

                --depth;
                ++current;
            }
            else
            {
                ThrowOutOfDepthException();

                SkipValue();
            }

            length = offset;

            return array;
        }

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        public void ReadArray(IDataWriter<int> dataWriter)
        {
            SkipWhiteSpace();

        Loop:

            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return;
                    }
                    goto default;
                case FixObject:
#if !NO_OPTIONS
                    if (On(MultiReferencingReference))
                    {
                        if (IsReference())
                        {
                            goto default;
                        }
                    }
#endif
                    ReadObject(dataWriter.As<string>());
                    return;
                case FixArray:
                    break;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        goto Loop;
                    }

                    goto default;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto default;

                default:

                    if (dataWriter.ContentType is Type contentType)
                    {
                        dataWriter.Content = XConvert.Convert(DirectRead(), contentType);
                    }
                    else
                    {
                        dataWriter.Content = DirectRead();
                    }

                    return;
            }

            if (dataWriter is IFastArrayRW fastArrayRW && dataWriter.ValueType is not null/* 如果是未知的值的类型则不支持快速读取 */)
            {
#if !NO_OPTIONS
                if (On(MultiReferencingReference))
                {
                    ReferenceModeInstance.EnterObject(dataWriter);
                }
#endif

                fastArrayRW.ReadFrom(this);

#if !NO_OPTIONS
                if (On(MultiReferencingReference))
                {
                    ReferenceModeInstance.LeavaObject();
                }
#endif
                return;
            }

            dataWriter.Initialize();

            InternalReadArray(dataWriter);

        }

        void InternalReadArray(IDataWriter<int> dataWriter)
        {
#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.EnterObject(dataWriter);
            }
#endif

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            ++current;
            ++depth;

            var offset = 0;

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                if (*current == ArrayEnding)
                {
                    goto Return;
                }

#if !NO_OPTIONS
                if (On(MultiReferencingReference))
                {
                    ReferenceModeInstance.SetCurrentKey(offset);
                }
#endif

                dataWriter.OnWriteValue(offset, this);

                ++offset;

            Next:

                SkipWhiteSpace();

                if (current < end)
                {
                    switch (*current)
                    {
                        case ArrayEnding:
                            goto Return;
                        case ValueEnding:
                            ++current;

                            goto Loop;
                        case FixComment:
                            if (SkipComment())
                            {
                                goto Next;
                            }

                            break;
                    }
                }
                else if (TryReadSegment(0))
                {
                    goto Next;
                }
            }
            else if (TryReadSegment(0))
            {
                goto Loop;
            }

            throw MakeException();

        Return:

            --depth;
            ++current;

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.LeavaObject();
            }
#endif
        }

        /// <summary>
        /// 读取一个对象。
        /// </summary>
        public void ReadObject(IDataWriter<string> dataWriter)
        {
            SkipWhiteSpace();

        Loop:

            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return;
                    }
                    goto default;
#if !NO_OPTIONS
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {

                        return;
                    }

                    goto default;
#endif
                case FixArray:
                    ReadArray(dataWriter.As<int>());
                    return;
                case FixObject:
#if !NO_OPTIONS
                    if (On(MultiReferencingReference))
                    {
                        if (IsReference())
                        {
                            goto default;
                        }
                    }
#endif
                    break;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        goto Loop;
                    }

                    goto default;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto default;

                default:

                    if (dataWriter.ContentType is Type contentType)
                    {
                        dataWriter.Content = XConvert.Convert(DirectRead(), dataWriter.ContentType);
                    }
                    else
                    {
                        dataWriter.Content = DirectRead();
                    }

                    return;
            }

            dataWriter.Initialize();

            InternalReadObject(dataWriter);
        }

        void InternalReadObject(IDataWriter<string> dataWriter)
        {
            var isFastDataWriter = dataWriter is IDataWriter<Ps<char>>;

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.EnterObject(dataWriter);
            }
#endif

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            ++current;
            ++depth;

#if !NO_OPTIONS
            var count_non_zero = dataWriter.Count > 0;
#endif

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                Ps<char> name;

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case FixString:
                    case FixChars:
                        name = InternalReadString();
                        break;
                    case FixComment:

                        if (SkipComment())
                        {
                            goto Loop;
                        }

                        goto default;
                    default:
                        name = InternalReadText();
                        break;
                }

            Key:

                SkipWhiteSpace();

                if (current < end)
                {
                    if (*current is KeyEnding)
                    {
                        ++current;

                    Value:

                        SkipWhiteSpace();

                        if (current < end)
                        {
#if !NO_OPTIONS
                            switch (*current)
                            {
                                case FixNull:
                                case Fixnull:
                                    if (count_non_zero && On(IgnoreNull) && Verify(nullString))
                                    {
                                        current += nullString.Length;

                                        goto Next;
                                    }

                                    break;
                                case FixNumberMin:
                                    if (count_non_zero && On(IgnoreZero) && IsValueEnding(1))
                                    {
                                        current += 1/*length of 0*/;

                                        goto Next;
                                    }

                                    break;
                            }
#endif

                            if (isFastDataWriter)
                            {
#if !NO_OPTIONS
                                if (On(MultiReferencingReference))
                                {
                                    ReferenceModeInstance.SetCurrentKey(name);
                                }
#endif

                                Unsafe.As<IDataWriter<Ps<char>>>(dataWriter).OnWriteValue(name, this);
                            }
                            else
                            {
                                var str_name = name.ToStringEx();

#if !NO_OPTIONS
                                if (On(MultiReferencingReference))
                                {
                                    ReferenceModeInstance.SetCurrentKey(str_name);
                                }
#endif

                                dataWriter.OnWriteValue(str_name, this);
                            }

                        Next:

                            SkipWhiteSpace();

                            if (current < end)
                            {
                                switch (*current)
                                {
                                    case ObjectEnding:
                                        goto Return;
                                    case ValueEnding:
                                        ++current;

                                        goto Loop;
                                    case FixComment:
                                        if (SkipComment())
                                        {
                                            goto Next;
                                        }

                                        break;
                                }
                            }
                            else if (TryReadSegment(0))
                            {
                                goto Next;
                            }
                        }
                        else if (TryReadSegmentWithKeepName())
                        {
                            goto Value;
                        }
                    }
                    else if (*current is FixComment && SkipComment())
                    {
                        goto Key;
                    }
                }
                else if (TryReadSegmentWithKeepName())
                {
                    goto Key;
                }

                [MethodImpl(VersionDifferences.AggressiveInlining)]
                bool TryReadSegmentWithKeepName()
                {
                    if (content is null)
                    {
                        return false;
                    }

                    return InternalTryReadSegmentWithKeepName();

                    [MethodImpl(MethodImplOptions.NoInlining)]
                    bool InternalTryReadSegmentWithKeepName()
                    {
                        if (name.Pointer >= begin && name.Pointer < end)
                        {
                            var offset = (int)(end - name.Pointer);

                            if (TryReadSegment(offset))
                            {
                                name = new Ps<char>(begin, name.Length);

                                current += offset;

                                return true;
                            }

                            return false;
                        }

                        return TryReadSegment(0);
                    }
                }
            }
            else if (TryReadSegment(0))
            {
                goto Loop;
            }

            throw MakeException();

        Return:

            --depth;
            ++current;

#if !NO_OPTIONS
            if (On(MultiReferencingReference))
            {
                ReferenceModeInstance.LeavaObject();
            }
#endif
        }

        /// <summary>
        /// 确切读取一个属性名。此方法的返回值不要跨线程使用！
        /// </summary>
        public Ps<char> InternalReadPropertyName()
        {
            Ps<char> name;

        Loop:

            switch (*current)
            {
                case ObjectEnding:
                    goto Exception;
                case FixString:
                case FixChars:
                    name = InternalReadString();
                    break;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            goto Exception;
                        }

                        goto Loop;
                    }

                    goto default;
                default:
                    name = InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (current < end && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();

                return name;
            }

        Exception:

            throw MakeException();
        }

        /// <summary>
        /// 确切读取一个属性名。
        /// </summary>
        public string ReadPropertyName()
        {
            return InternalReadPropertyName().ToStringEx();
        }

        /// <summary>
        /// 尝试读取对象开头。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadBeginObject()
        {
            if (current < end && *current == FixObject)
            {
                ++current;

                SkipWhiteSpace();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试读取对象结尾。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEndObject()
        {
            SkipWhiteSpace();

            if (current < end)
            {
                switch (*current)
                {
                    case ObjectEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (current < end && *current == ObjectEnding)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试读取数组开头。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadBeginArray()
        {
            if (current < end && *current == FixArray)
            {
                ++current;

                SkipWhiteSpace();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试读取数组结尾。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEndArray()
        {
            SkipWhiteSpace();

            if (current < end)
            {
                switch (*current)
                {
                    case ArrayEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (current < end && *current == ArrayEnding)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        #endregion

        #region -- 字符串 --

        /// <summary>
        /// 确切读取一个元文本。此方法的返回值不要跨线程使用！
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Ps<char> InternalReadText()
        {
            var chars = current;

            int length;

        Loop:

            for (; chars < end; ++chars)
            {
                switch (*chars)
                {
                    case KeyEnding:
                    case ValueEnding:
                    case ObjectEnding:
                    case ArrayEnding:

                        length = (int)(chars - current);

                        goto Return;

                    case FixComment:

                        length = (int)(chars - current);

                        if (IsValueEnding(length))
                        {
                            goto Return;
                        }

                        break;
                }
            }

            length = (int)(chars - current);

            if (TryReadSegment(length))
            {
                chars = current + length;

                goto Loop;
            }

        Return:

            chars = current;

            current += length;

            return
#if !NO_OPTIONS
                On(DeflateDeserialize)
                ? new Ps<char>(chars, length)
                :
#endif
                StringHelper.Trim(chars, length);
        }

        /// <summary>
        /// 确切读取一个字符串。此方法的返回值不要跨线程使用！
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Ps<char> InternalReadString()
        {
            var fixStr = *current;

            ++current;

            var offset = StringHelper.IndexOfAny(current, Rest, fixStr, FixEscape);

            if (offset >= 0 && current[offset] != FixEscape)
            {
                var result = new Ps<char>(current, offset);

                current = current + offset + 1;

                return result;
            }

            return NoInliningInternalReadString(fixStr, offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        Ps<char> NoInliningInternalReadString(char fixStr, int offset)
        {
            var hGChars = CharsPool.Current();

            goto Enter;

        Loop:

            offset = StringHelper.IndexOfAny(current, Rest, fixStr, FixEscape);

        Enter:

            if (offset is -1)
            {
                if (content is not null)
                {
                    hGChars.ReadFrom(ref *current, (int)(end - current));

                    if (TryReadSegment(0))
                    {
                        goto Loop;
                    }
                }
            }
            else
            {
                hGChars.ReadFrom(ref *current, offset);

                current += offset;

                if (*current is not FixEscape)
                {
                    ++current;

                    return hGChars;
                }

                ++current;

                if (end - current <= 5)
                {
                    TryReadSegment(-1);
                }

                hGChars.Append(*current switch
                {
                    EscapedWhiteChar2 => WhiteChar2,
                    EscapedWhiteChar3 => WhiteChar3,
                    EscapedWhiteChar4 => WhiteChar4,
                    EscapedWhiteChar5 => WhiteChar5,
                    EscapedWhiteChar6 => WhiteChar6,
                    FixUnicodeEscape or FixunicodeEscape => (char)(
                        (GetDigital(*++current) << 12) |
                        (GetDigital(*++current) << 8) |
                        (GetDigital(*++current) << 4) |
                        (GetDigital(*++current))
                    ),
                    var val => val
                });

                ++current;

                if (current < end)
                {
                    goto Loop;
                }
            }

            throw MakeException();
        }

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        public string? ReadString()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return InternalReadString().ToStringEx();
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
            }

            return NoInliningReadString();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        string? NoInliningReadString()
        {
        Loop:

            switch (*current)
            {
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return TrueString;
                    }
                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return FalseString;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
                case FixObject:
                case FixArray:
                    return Convert.ToString(DirectRead());
                case FixPositive:
                case FixNegative:
                    goto Number;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        return ReadString();
                    }

                    goto default;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto default;

                default:
                    if (*current >= FixNumberMin && *current <= FixNumberMax)
                    {
                        goto Number;
                    }
                    break;
            }

        Text:

            return InternalReadText().ToStringEx();

        Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (IsValueEnding(numberInfo.End) && numberInfo.IsNumber)
            {
                current += numberInfo.End;

                return numberInfo.ToString();
            }

            goto Text;
        }

#if !NO_OPTIONS
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool TryReadEmptyString()
        {
            if (On(EmptyStringAsNull | EmptyStringAsDefault))
            {
                if (current + 1 < end && current[0] == current[1])
                {
                    current += 2;

                    return true;
                }
            }

            return false;
        }
#endif

        void SkipString()
        {
            var fixStr = *current;

            ++current;

        Loop:

            var offset = StringHelper.IndexOf(current, Rest, fixStr);

            if (offset is -1)
            {
                if (TryReadSegment(0))
                {
                    goto Loop;
                }

                ThrowException();
            }

            current += offset + 1;

            if (current[-2] is FixEscape)
            {
                goto Loop;
            }
        }

        #endregion

        #region -- 公共值 --

        /// <summary>
        /// 获取当前值的 Token。
        /// </summary>
        public JsonToken GetToken()
        {
            if (current < end)
            {
                switch (*current)
                {
                    case FixString:
                    case FixChars:
                        return JsonToken.String;
                    case FixArray:
                        return JsonToken.Array;
                    case FixObject:

#if !NO_OPTIONS
                        if (On(MultiReferencingReference))
                        {
                            if (IsReference())
                            {
                                return JsonToken.Reference;
                            }
                        }
#endif

                        return JsonToken.Object;
                    case Fixtrue:
                    case FixTrue:
                    case Fixfalse:
                    case FixFalse:
                        return JsonToken.Boolean;
                    case Fixnull:
                    case FixNull:
                    case Fixundefined:
                    case FixUndefined:
                        return JsonToken.Null;
                    case FixComment:

                        if (GetCommentType() != JsonCommentType.Non)
                        {
                            return JsonToken.Comment;
                        }

                        return JsonToken.Other;
                    case FixPositive:
                    case FixNegative:
                    case var dight when dight >= FixNumberMin && dight <= FixNumberMax:
                        return JsonToken.Number;
                    default:
                        return JsonToken.Other;
                }
            }

            return JsonToken.End;
        }


        /// <summary>
        /// 直接读取一个值。
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public object? DirectRead()
        {
        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
                    return InternalReadString().ToStringEx();
                case FixArray:
                    return ((JsonFormatter?.ArrayValueInterface) ?? DefaultArrayValueInterface).Read(this);
                case FixObject:

#if !NO_OPTIONS
                    if (On(MultiReferencingReference))
                    {
                        if (IsReference())
                        {
                            return ReferenceModeInstance.GetValue(ReadReference());
                        }
                    }
#endif

                    return ((JsonFormatter?.ObjectValueInterface) ?? DefaultObjectValueInterface).Read(this);
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return true;
                    }
                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return false;
                    }
                    break;
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
                case FixPositive:
                case FixNegative:
                    goto Number;
                case ValueEnding:

                    if (TryReadIsTuple())
                    {
                        goto Loop;
                    }

                    goto Text;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    goto Text;

                case var curr when curr >= FixNumberMin && curr <= FixNumberMax:
                    goto Number;
            }

        Text:

            return InternalReadText().ToStringEx();

        Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, Rest);

            if (IsValueEnding(numberInfo.End) && numberInfo.IsNumber && numberInfo.IsCommonRadix(out var radix))
            {
                current += numberInfo.End;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 3)
                    {
                        return numberInfo.ToDouble(radix);
                    }
                }
                else if (numberInfo.HaveFractional)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                    {
                        return numberInfo.ToDouble(radix);
                    }

                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28 && numberInfo.IsDecimal)
                    {
                        return numberInfo.ToDecimal();
                    }
                }
                else if (numberInfo.IntegerCount <= 18)
                {
                    var value = numberInfo.ToInt64(radix);

                    if (value <= int.MaxValue && value >= int.MinValue)
                    {
                        return (int)value;
                    }

                    return value;
                }
                else if (numberInfo.IntegerCount <= 28 && numberInfo.IsDecimal)
                {
                    return numberInfo.ToDecimal();
                }

                return numberInfo.ToString();
            }

            goto Text;
        }

        /// <summary>
        /// 读取一个可空值。
        /// </summary>
        public T? ReadNullable<T>() where T : struct
        {
        Loop:

            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
#if !NO_OPTIONS
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return default;
                    }

                    break;
#endif
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            ThrowException();
                        }

                        goto Loop;
                    }

                    break;
            }

            return ReadValue<T>();
        }

        /// <summary>
        /// 读取一个枚举值。
        /// </summary>
        public T ReadEnum<T>() where T : struct, Enum
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
#if !NO_OPTIONS
                    if (On(EmptyStringAsDefault) && TryReadEmptyString())
                    {
                        return default;
                    }
#endif


                    var str = InternalReadString();

                    if (EnumHelper.TryParseEnum(str, out T value))
                    {
                        return value;
                    }

                    return (T)Enum.Parse(typeof(T), str.ToStringEx());
                case FixNegative:

                    var psd = NumberHelper.Decimal.ParseInt64(current, Rest);

                    if (psd.length != 0 && IsValueEnding(psd.length))
                    {
                        current += psd.length;

                        return EnumHelper.AsEnum<T>((ulong)psd.value);
                    }

                    break;
                case var curr when curr >= FixNumberMin && curr <= FixNumberMax:

                    var upsd = NumberHelper.Decimal.ParseUInt64(current, Rest);

                    if (upsd.length != 0 && IsValueEnding(upsd.length))
                    {
                        current += upsd.length;

                        return EnumHelper.AsEnum<T>(upsd.value);
                    }

                    break;
            }

            return XConvert.Convert<T>(DirectRead());
        }

        /// <summary>
        /// 读取一个已知类型的值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T? ReadValue<T>()
        {
            if (typeof(T) == typeof(int)) return As(ReadInt32());
            if (typeof(T) == typeof(string)) return As(ReadString());
            if (typeof(T) == typeof(double)) return As(ReadDouble());
            if (typeof(T) == typeof(bool)) return As(ReadBoolean());
            if (typeof(T) == typeof(byte)) return As(ReadByte());
            if (typeof(T) == typeof(sbyte)) return As(ReadSByte());
            if (typeof(T) == typeof(short)) return As(ReadInt16());
            if (typeof(T) == typeof(ushort)) return As(ReadUInt16());
            if (typeof(T) == typeof(uint)) return As(ReadUInt32());
            if (typeof(T) == typeof(long)) return As(ReadInt64());
            if (typeof(T) == typeof(ulong)) return As(ReadUInt64());
            if (typeof(T) == typeof(float)) return As(ReadSingle());
            if (typeof(T) == typeof(char)) return As(ReadChar());
            if (typeof(T) == typeof(decimal)) return As(ReadDecimal());
            if (typeof(T) == typeof(DateTime)) return As(ReadDateTime());
            if (typeof(T) == typeof(Guid)) return As(ReadGuid());
            if (typeof(T) == typeof(DateTimeOffset)) return As(ReadDateTimeOffset());
            if (typeof(T) == typeof(TimeSpan)) return As(ReadTimeSpan());
            if (typeof(T) == typeof(object)) return As(DirectRead());

            return ValueInterface<T>.ReadValue(this);

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static T As<TInput>(TInput input)
                => Unsafe.As<TInput, T>(ref input);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T? InternalReadParse<T>()
        {
            return FastParse<T>(InternalReadString());
        }

        /// <summary>
        /// 跳过一个值。
        /// </summary>
        public void SkipValue()
        {
            var depth = 0;

        Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
                    SkipString();
                    break;
                case Fixtrue:
                case FixTrue:
                    if (!Verify(trueString)) goto default;

                    current += trueString.Length;
                    break;
                case Fixfalse:
                case FixFalse:
                    if (!Verify(falseString)) goto default;

                    current += falseString.Length;
                    break;
                case Fixnull:
                case FixNull:
                    if (!Verify(nullString)) goto default;

                    current += nullString.Length;
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (!Verify(undefinedString)) goto default;

                    current += undefinedString.Length;
                    break;
                case FixArray:
                case FixObject:

                    ++depth;
                    ++current;

                    break;
                case ArrayEnding:
                case ObjectEnding:

                    --depth;
                    ++current;

                    SkipWhiteSpace();
                    break;
                case FixComment:

                    if (SkipComment())
                    {
                        if (current >= end)
                        {
                            break;
                        }

                        goto Loop;
                    }

                    goto default;
                case FixPositive:
                case FixNegative:
                case var digit when digit >= FixNumberMin && digit <= FixNumberMax:
                    ReadDouble();
                    break;
                default:
                    InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (depth is 0)
            {
                return;
            }

            if (current < end)
            {
                switch (*current)
                {
                    case KeyEnding:
                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        break;
                }
            }

            if (current < end)
            {
                goto Loop;
            }

            ThrowException();
        }


        #endregion

        #region -- 辅助函数 --

#if !NO_OPTIONS
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool On(JsonFormatterOptions options)
            => (Options & options) != 0;
#endif

        /// <summary>
        /// 跳过空白字符。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipWhiteSpace()
        {
#if !NO_OPTIONS
            if (On(DeflateDeserialize))
            {
                return;
            }
#endif

            if (current < end && *current <= 0x20)
            {
                NoInliningSkipWhiteSpace();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void NoInliningSkipWhiteSpace()
        {
        Loop:

            switch (*current)
            {
                case WhiteChar1:
                case WhiteChar2:
                case WhiteChar3:
                case WhiteChar4:
                case WhiteChar5:
                case WhiteChar6:
                    ++current;
                    if (current < end)
                    {
                        goto Loop;
                    }
                    break;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool IsValueEnding(int offset)
        {
            return current[offset] is ObjectEnding or ArrayEnding or KeyEnding or ValueEnding 
                || NoInliningIsValueEnding(offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool NoInliningIsValueEnding(int offset)
        {
        Loop:

            if (current + offset == end)
            {
                return content is null || content.IsFinalBlock;
            }

            switch (current[offset])
            {
                case ObjectEnding or ArrayEnding or KeyEnding or ValueEnding:
                    return true;

                case WhiteChar1 or WhiteChar2 or WhiteChar3 or WhiteChar4 or WhiteChar5 or WhiteChar6:

#if !NO_OPTIONS
                    if (On(DeflateDeserialize))
                    {
                        return false;
                    }

                    if (On(StandardDeserialize))
                    {
                        return true;
                    }
#endif

                    ++offset;

                    goto Loop;
                case FixComment:

                    ++offset;

                    if (current + offset == end)
                    {
                        return false;
                    }

                    return current[offset] is FixComment or AsteriskChar;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool Verify(string lowerstr)
        {
#if !NO_OPTIONS
            if (On(DeflateDeserialize))
            {
                return true;
            }
#endif

            if (!IsValueEnding(lowerstr.Length))
            {
                return false;
            }

            for (int i = 0; i < lowerstr.Length; i++)
            {
                if (current[i] != lowerstr[i] && StringHelper.ToLower(current[i]) != lowerstr[i])
                {
                    return false;
                }
            }

            return true;
        }

        bool IsFinalBlock => content is null || content.IsFinalBlock;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool TryReadSegment(int retain)
        {
            if (content is null)
            {
                return false;
            }

            return InternalTryReadSegment();

            [MethodImpl(MethodImplOptions.NoInlining)]
            bool InternalTryReadSegment()
            {
                if (retain is -1)
                {
                    retain = (int)(end - current);
                }

                var readCount = content.ReadSegment(retain);

                begin = content.hGCache.First;
                end = begin + content.hGCache.Count;
                current = begin;

                return readCount > 0;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        bool TryReadIsTuple()
        {
            var temp = current++;

            SkipWhiteSpace();

            if (current < end && *current != ValueEnding)
            {
                return true;
            }

            current = temp;

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        Exception MakeException()
        {
            return new JsonDeserializeException(Offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ThrowException()
        {
            throw MakeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ThrowOutOfDepthException()
        {
#if !NO_OPTIONS
            if (On(OutOfDepthException))
#endif
            {
                throw new JsonOutOfDepthException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void DeserializeTo(IDataWriter dataWriter)
        {
            switch (GetToken())
            {
                case JsonToken.Object:

                    InternalReadObject(dataWriter.As<string>());

                    break;
                case JsonToken.Array:

                    InternalReadArray(dataWriter.As<int>());

                    break;
                default:

                    if (dataWriter.ContentType is Type contentType)
                    {
                        dataWriter.Content = XConvert.Convert(DirectRead(), contentType);
                    }
                    else
                    {
                        dataWriter.Content = DirectRead();
                    }

                    break;
            }
        }



        [MethodImpl(VersionDifferences.AggressiveInlining)]
        T? FastParse<T>(Ps<char> str)
        {
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTimeHelper.TryParseISODateTime(str.Pointer, str.Length, out DateTime date_time)) return As(date_time);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                if (DateTimeHelper.TryParseISODateTime(str.Pointer, str.Length, out DateTimeOffset date_time_offset)) As(date_time_offset);
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                if (DateTimeHelper.TryParseISODateTime(str.Pointer, str.Length, out DateTime date_time)) return As(new TimeSpan(date_time.Ticks));
            }
            else if (typeof(T) == typeof(Guid))
            {
                var (_, length, value) = GuidHelper.ParseGuid(str.Pointer, str.Length);

                if (length == str.Length) As(value);
            }
            else if (typeof(T) == typeof(long))
            {
                var (_, length, value) = NumberHelper.DecimalParseInt64(str.Pointer, str.Length);

                if (length == str.Length) As(value);
            }
            else if (typeof(T) == typeof(ulong))
            {
                var (_, length, value) = NumberHelper.DecimalParseUInt64(str.Pointer, str.Length);

                if (length == str.Length) As(value);
            }
            else if (typeof(T) == typeof(double))
            {
#if !NO_OPTIONS
                if (On(JsonFormatterOptions.UseSystemFloatingPointsMethods))
                {
#if NativeSpan
                    if (double.TryParse(str, out var value))
                    {
                        return As(value);
                    }
#endif
                }
                else
#endif
                {
                    var (_, length, value) = NumberHelper.DecimalParseDouble(str.Pointer, str.Length);

                    if (length == str.Length) As(value);
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                var (_, length, value) = NumberHelper.ParseDecimal(str.Pointer, str.Length);

                if (length == str.Length) As(value);
            }
            else if (typeof(T) == typeof(RWPath))
            {
                return As(ParseReference(str.Pointer, str.Length));
            }

            if (typeof(T) == typeof(long) ||
                typeof(T) == typeof(ulong) ||
                typeof(T) == typeof(double) ||
                typeof(T) == typeof(decimal))
            {
                var numberInfo = NumberHelper.GetNumberInfo(str.Pointer, str.Length);

                if (numberInfo.End == str.Length && numberInfo.IsNumber && numberInfo.IsCommonRadix(out var radix))
                {
                    if (typeof(T) == typeof(long)) return As(numberInfo.ToInt64(radix));
                    if (typeof(T) == typeof(ulong)) return As(numberInfo.ToUInt64(radix));
                    if (typeof(T) == typeof(double)) return As(numberInfo.ToDouble(radix));

                    if (typeof(T) == typeof(decimal) && numberInfo.IsDecimal) return As(numberInfo.ToDecimal());
                }
            }

            return SlowParse<T>(str.ToStringEx());

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static T As<TInput>(TInput input)
                => Unsafe.As<TInput, T>(ref input);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe T? SlowParse<T>(string str)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return As(DateTime.Parse(str));
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                return As(DateTimeOffset.Parse(str));
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                return As(TimeSpan.Parse(str));
            }
            else if (typeof(T) == typeof(Guid))
            {
                return As(new Guid(str));
            }
            else if (typeof(T) == typeof(long))
            {
                return As(long.Parse(str, NumberStyle));
            }
            else if (typeof(T) == typeof(ulong))
            {
                return As(ulong.Parse(str, NumberStyle));
            }
            else if (typeof(T) == typeof(double))
            {
                return As(double.Parse(str, NumberStyle));
            }
            else if (typeof(T) == typeof(decimal))
            {
                return As(decimal.Parse(str, NumberStyle));
            }
            else if (typeof(T) == typeof(RWPath))
            {
                fixed (char* chars = str)
                {
                    return (T)(object)ParseReference(chars, str.Length);
                }
            }

            return default;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static T As<TInput>(TInput input)
                => Unsafe.As<TInput, T>(ref input);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static unsafe RWPath ParseReference(char* chars, int length)
        {
            var reference = new RWPath();

            // i: Index;
            // j: Item start;
            // k: Item length;
            for (int i = 0, j = 0, k = 0; ; i++)
            {
                if (i < length)
                {
                    switch (chars[i])
                    {
                        case RefSeparater:
                            break;
                        case RefEscape:

                            if (i + 2 < length)
                            {
                                GetDigital(chars[++i]);
                                GetDigital(chars[++i]);

                                continue;
                            }

                            throw new FormatException();
                        default:
                            ++k;
                            continue;
                    }
                }

                if (j < length)
                {
                    var item = i - j == k ? StringHelper.ToString(chars + j, k) : GetItem(j, i, k);

                    j = i + 1;
                    k = 0;

                    switch (item)
                    {
                        case ReferenceRootPathName:
                        case ReferenceRootPathName2:
                            if (reference.Nodes.IsEmpty)
                            {
                                continue;
                            }
                            break;
                    }

                    if (item.Length != 0 && item[0] >= FixNumberMin && item[0] <= FixNumberMax && int.TryParse(item, out var result))
                    {
                        reference.AddPathNode(result);
                    }
                    else
                    {
                        reference.AddPathNode(item);
                    }

                    continue;
                }

                break;
            }

            return reference;


            string GetItem(int start, int end, int count)
            {
                var bytesCount = (end - start - count) / 3;

                var bytes = stackalloc byte[bytesCount];

                for (int i = start, j = 0; i < end; i++)
                {
                    if (chars[i] == RefEscape)
                    {
                        bytes[j++] = (byte)((GetDigital(chars[++i]) << 4) | GetDigital(chars[++i]));
                    }
                }

                var charsCount = Encoding.UTF8.GetCharCount(bytes, bytesCount);

                var str = StringHelper.MakeString(charsCount + count);

                fixed (char* pStr = str)
                {
                    for (int i = start, j = 0, k = 0; i < end; i++)
                    {
                        if (chars[i] == RefEscape)
                        {
                            var l = 1;

                            for (i += 3; i < end && chars[i] == RefEscape; i += 3, ++l) ;

                            --i;

                            j += Encoding.UTF8.GetChars(bytes + k, l, pStr + j, charsCount);

                            k += l;
                        }
                        else
                        {
                            pStr[j] = chars[i];

                            ++j;
                        }
                    }
                }

                return str;
            }
        }

        #endregion

        #region -- 注释 --

        /// <summary>
        /// 读取一个注释。
        /// </summary>
        /// <param name="includeSymbols">是否包含注释符</param>
        /// <returns>返回注释</returns>
        public string ReadComment(bool includeSymbols = false)
        {
            return InternalReadComment(includeSymbols).ToStringEx();
        }

        /// <summary>
        /// 获取当前注释类型。
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public JsonCommentType GetCommentType()
        {
            if (current < end && *current is FixComment)
            {
                if (current + 1 < end || TryReadSegment(1))
                {
                    switch (current[1])
                    {
                        case FixComment:
                            return JsonCommentType.SingleLine;
                        case AsteriskChar:
                            return JsonCommentType.MultiLine;
                    }
                }
            }

            return JsonCommentType.Non;
        }

        /// <summary>
        /// 读取一个注释。此方法的返回值不要跨线程使用！
        /// </summary>
        /// <param name="includeSymbols">是否包含注释符</param>
        /// <returns>返回注释</returns>
        public Ps<char> InternalReadComment(bool includeSymbols = false)
        {
            var commentType = GetCommentType();

            if (commentType is JsonCommentType.Non)
            {
                return default;
            }

            var endSymbol = commentType is JsonCommentType.SingleLine ? LineFeedChar : FixComment;

            var offset = commentType is JsonCommentType.SingleLine ? SingleLineCommentPrefix.Length : MultiLineCommentPrefix.Length;

        Loop:

            var index = StringHelper.IndexOf(current + offset, Rest, endSymbol);

            if (index is -1)
            {
                index += Rest;

                if (TryReadSegment(-1))
                {
                    goto Loop;
                }

                var chars = current;
                var length = Rest;

                current += length;

                if (includeSymbols)
                {
                    return new Ps<char>(chars, length);
                }

                if (commentType is JsonCommentType.SingleLine)
                {
                    chars += SingleLineCommentPrefix.Length;
                    length -= SingleLineCommentPrefix.Length;

                    return StringHelper.Trim(chars, length);
                }
                else
                {
                    chars += MultiLineCommentPrefix.Length;
                    length -= MultiLineCommentPrefix.Length;

                    return StringHelper.TrimStart(chars, length);
                }
            }
            else if (commentType is JsonCommentType.MultiLine && !(index > 0 && current[offset + index - 1] is AsteriskChar))
            {
                offset += index + 1;

                goto Loop;
            }
            else
            {
                var chars = current;
                var length = offset + index + 1;

                current += length;

                if (includeSymbols)
                {
                    return new Ps<char>(chars, length);
                }

                if (commentType is JsonCommentType.SingleLine)
                {
                    chars += SingleLineCommentPrefix.Length;
                    length -= SingleLineCommentPrefix.Length;

                    return StringHelper.Trim(chars, length);
                }
                else
                {
                    chars += MultiLineCommentPrefix.Length;
                    length -= MultiLineCommentPrefix.Length;
                    length -= MultiLineCommentSuffix.Length;

                    return StringHelper.Trim(chars, length);
                }
            }
        }

        /// <summary>
        /// 跳过一个注释。
        /// </summary>
        /// <returns>返回是否已跳过一个注释</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool SkipComment()
        {
            if (InternalReadComment(true).Length != 0)
            {
                SkipWhiteSpace();

                return true;
            }

            return false;
        }

        #endregion

        #region -- 接口实现 --

        Type? IValueReader.ValueType
        {
            get
            {
                switch (GetToken())
                {
                    case JsonToken.Object:
                        return ((JsonFormatter?.ObjectValueInterface) ?? DefaultObjectValueInterface).Type;
                    case JsonToken.Array:
                        return ((JsonFormatter?.ArrayValueInterface) ?? DefaultArrayValueInterface).Type;
                    case JsonToken.Boolean:
                        return typeof(bool);
                    case JsonToken.Number:
                        return typeof(double);
                    case JsonToken.String:
                        return typeof(string);
                    default:
                        return null;
                }
            }
        }

        void IValueReader.Pop() => SkipValue();

        TargetableValueInterfaceMap ITargetableValueRW.ValueInterfaceMap => TargetableValueInterfaceMap.FromSource(JsonFormatter);

        #endregion

        #region -- 辅助类 --

#if !NO_OPTIONS

        class ReferenceMode
        {
            SinglyLinkedList<BindItem> Binds;
            SinglyLinkedList<ObjectItem> Objects;

            public IDataWriter CurrentObject
            {
                get
                {
                    var first = Objects.First;

                    if (first is null)
                    {
                        throw new InvalidOperationException();
                    }

                    return first.DataWriter;
                }
            }

            public void EnterObject(IDataWriter dataWriter)
            {
                Objects.AddFirst(new ObjectItem(dataWriter));
            }

            public void LeavaObject()
            {
                Objects.RemoveFirst(out _);

                if (Objects.IsEmpty)
                {
                    Process();
                }
            }

            private void Process()
            {
                const string NotWritableMsg = "The destination is not writable.";
                const string NotReadableMsg = "The source is not readable.";
                const string UnableCreateMsg = "Unable to create data reader!.";

                while (Binds.RemoveFirst(out var item))
                {
                    if (item.SourcePath.Nodes.IsEmpty)
                    {
                        var valueWriter = item.DestinationPath.GetFirstValueWriter(item.Destination);

                        if (valueWriter is null)
                        {
                            throw new NotSupportedException(NotWritableMsg);
                        }

                        valueWriter.DirectWrite(item.Source.Content);
                    }
                    else
                    {
                        var dataReader = item.Source as IDataReader;

                        if (dataReader is null)
                        {
                            var content = item.Source.Content;

                            if (content is null)
                            {
                                throw new NullReferenceException(nameof(item.Source.Content));
                            }

                            dataReader = RWHelper.CreateReader(content);
                        }

                        if (dataReader is null)
                        {
                            throw new NotSupportedException(UnableCreateMsg);
                        }

                        var valueWriter = item.DestinationPath.GetFirstValueWriter(item.Destination);
                        var valueReader = item.SourcePath.GetFirstValueReader(dataReader);

                        if (valueWriter is null)
                        {
                            throw new NotSupportedException(NotWritableMsg);
                        }

                        if (valueReader is null)
                        {
                            throw new NotSupportedException(NotReadableMsg);
                        }

                        valueWriter.DirectWrite(valueReader.DirectRead());
                    }
                }
            }

            public void SetCurrentKey<TKey>(TKey key) where TKey : notnull
            {
                var first = Objects.First;

                if (first is null)
                {
                    throw new InvalidOperationException();
                }

                first.CurrentKey = new RWPathConstantNode<TKey>(key);
            }

            public object? GetValue(RWPath reference)
            {
                var first = Objects.First;
                var last = Objects.Last;

                if (first is null || last is null || first.CurrentKey is null)
                {
                    throw new InvalidOperationException();
                }

                var destinationPath = new RWPath();

                destinationPath.Nodes.AddLast(first.CurrentKey);

                Binds.AddLast(new BindItem(
                    last.DataWriter,
                    reference,
                    first.DataWriter,
                    destinationPath
                    ));

                return null;
            }

            sealed class BindItem
            {
                public readonly IDataWriter Source;
                public readonly RWPath SourcePath;
                public readonly IDataWriter Destination;
                public readonly RWPath DestinationPath;

                public BindItem(IDataWriter source, RWPath sourcePath, IDataWriter destination, RWPath destinationPath)
                {
                    Source = source;
                    SourcePath = sourcePath;
                    Destination = destination;
                    DestinationPath = destinationPath;
                }
            }

            sealed class ObjectItem
            {
                public readonly IDataWriter DataWriter;
                public RWPathNode? CurrentKey;

                public ObjectItem(IDataWriter dataWriter)
                {
                    DataWriter = dataWriter;
                }
            }
        }

#endif

        #endregion
    }

#if Async

    partial class JsonDeserializer
    {
        #region -- 构造函数 --

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hGCache"></param>
        /// <param name="textReader"></param>
        /// <param name="jsonFormatter"></param>
        /// <returns></returns>
        public static async ValueTask<JsonDeserializer> CreateAsync(
            HGlobalCache<char> hGCache,
            TextReader textReader,
            JsonFormatter? jsonFormatter = null
            )
        {
            if (jsonFormatter != null)
            {
                return new JsonDeserializer(jsonFormatter, await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGCache));
            }

            return new JsonDeserializer(JsonSegmentedContent.CreateAndInitialize(textReader, hGCache));
        }

#if !NO_OPTIONS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hGCache"></param>
        /// <param name="textReader"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async ValueTask<JsonDeserializer> CreateAsync(
            HGlobalCache<char> hGCache,
            TextReader textReader,
            JsonFormatterOptions options
            )
        {
            return new JsonDeserializer(await JsonSegmentedContent.CreateAndInitializeAsync(textReader, hGCache), options);
        }

#endif

        #endregion
    }

#endif
}