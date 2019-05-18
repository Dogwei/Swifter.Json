using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    unsafe class BaseJsonSerializer : IDisposable, ISingleThreadOptimize
    {
        public readonly HGlobalCache<char> hGlobal;

        public TextWriter textWriter;
        public int offset;
        public JsonFormatter jsonFormatter;

        public int depth;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public BaseJsonSerializer()
        {
            offset = 0;

            hGlobal = HGlobalCache<char>.OccupancyInstance();

            Expand(255);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (hGlobal.Count - offset < expandMinSize)
            {
                InternalExpand(expandMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalExpand(int expandMinSize)
        {
            if (hGlobal.Count == HGlobalCache<char>.MaxSize && textWriter != null && offset != 0)
            {
                VersionDifferences.WriteChars(textWriter, hGlobal.GetPointer(), offset);

                offset = 0;

                Expand(expandMinSize);
            }
            else
            {
                hGlobal.Expand(expandMinSize);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            hGlobal.GetPointer()[offset] = c;

            ++offset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(string value)
        {
            int length = value.Length;

            Expand(length + 2);

            for (int i = 0; i < length; ++i)
            {
                Append(value[i]);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalWriteLongString(string value)
        {
            Expand(2);

            Append('"');

            for (int i = 0; i < value.Length;)
            {
                int count = value.Length - i;

                Expand(count + 2);

                for (int end = count + offset; offset < end; ++i)
                {
                    var item = value[i];

                    switch (item)
                    {
                        case '\\':
                            Append('\\');
                            Append('\\');
                            break;
                        case '"':
                            Append('\\');
                            Append('"');
                            break;
                        case '\n':
                            Append('\\');
                            Append('n');
                            break;
                        case '\r':
                            Append('\\');
                            Append('r');
                            break;
                        case '\t':
                            Append('\\');
                            Append('t');
                            break;
                        default:
                            Append(item);
                            break;
                    }
                }
            }

            Expand(2);

            Append('"');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteString(string value)
        {
            int length = value.Length;

            if (length > 300)
            {
                InternalWriteLongString(value);

                return;
            }

            Expand(length * 2 + 2);

            Append('"');

            for (int i = 0; i < length; ++i)
            {
                var item = value[i];

                switch (item)
                {
                    case '\\':
                        Append('\\');
                        Append('\\');
                        break;
                    case '"':
                        Append('\\');
                        Append('"');
                        break;
                    case '\n':
                        Append('\\');
                        Append('n');
                        break;
                    case '\r':
                        Append('\\');
                        Append('r');
                        break;
                    case '\t':
                        Append('\\');
                        Append('t');
                        break;
                    default:
                        Append(item);
                        break;
                }
            }

            Append('"');
        }
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteChar(char value)
        {
            Expand(5);

            Append('"');

            switch (value)
            {
                case '\\':
                    Append('\\');
                    Append('\\');
                    break;
                case '"':
                    Append('\\');
                    Append('"');
                    break;
                case '\n':
                    Append('\\');
                    Append('n');
                    break;
                case '\r':
                    Append('\\');
                    Append('r');
                    break;
                case '\t':
                    Append('\\');
                    Append('t');
                    break;
                default:
                    Append(value);
                    break;
            }

            Append('"');
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalWriteDouble(double value)
        {
            // NaN, PositiveInfinity, NegativeInfinity, Or Other...
            InternalWriteString(value.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalWriteSingle(float value)
        {
            // NaN, PositiveInfinity, NegativeInfinity, Or Other...
            InternalWriteString(value.ToString());
        }

        public int StringLength => offset - 1;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Dispose()
        {
            hGlobal.Dispose();
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public int Count => 0;

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            ((IDataWriter<string>)this)[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            ((IDataWriter<int>)this)[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public long Id => jsonFormatter?.id ?? 0;
    }
}