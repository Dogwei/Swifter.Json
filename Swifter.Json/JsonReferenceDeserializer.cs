using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonReferenceDeserializer : BaseJsonDeserializer
    {
        public const string RefKey = "\"$ref\"";
        public const string RefName = "$ref";

        public readonly LinkedList<ReferenceInfo> references;
        public TargetPathInfo referenceTarget;

        public bool updateBase;

        public IDataWriter writer;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonReferenceDeserializer(char* chars, int length)
            : base(chars, length)
        {
            references = new LinkedList<ReferenceInfo>();
        }

        public override void ReadObject(IDataWriter<string> dataWriter)
        {
            if (!IsObject)
            {
                NoObjectOrArray(dataWriter);

                return;
            }

            var haveRef = false;

            ++current;

            dataWriter.Initialize();

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                string name;

                switch (*current)
                {
                    case '}':
                        goto Return;
                    case '"':
                    case '\'':
                        name = InternalReadString();
                        break;
                    default:
                        name = InternalReadText();
                        break;
                }

                SkipWhiteSpace();

                if (current < end && *current == ':')
                {
                    ++current;

                    SkipWhiteSpace();

                    if (TryReadRefReferenceTarget())
                    {
                        dataWriter.OnWriteValue(name, RWHelper.DefaultValueReader);

                        references.AddLast(new ReferenceInfo(referenceTarget, new SourcePathInfo(name, null, dataWriter)));

                        if (dataWriter is IDataReader reader && reader.ReferenceToken == null)
                        {
                            updateBase = true;
                        }

                        haveRef = true;
                    }
                    else
                    {
                        dataWriter.OnWriteValue(name, this);

                        if (updateBase)
                        {
                            var last = references.Last.Value;

                            last.source = new SourcePathInfo(name, last.source, dataWriter);

                            updateBase = false;

                            haveRef = true;
                        }
                    }

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        switch (*current)
                        {
                            case '}':
                                goto Return;
                            case ',':
                                ++current;

                                goto Loop;
                        }
                    }
                }
            }

            throw GetException();

        Return:

            ++current;

            if (haveRef && dataWriter is IDataReader dataReader && dataReader.ReferenceToken == null)
            {
                writer = dataWriter;

                updateBase = true;
            }
        }

        public override void ReadArray(IDataWriter<int> dataWriter)
        {
            if (!IsArray)
            {
                NoObjectOrArray(dataWriter);

                return;
            }

            var haveRef = false;
            var i = 0;

            ++current;

            dataWriter.Initialize();

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                if (*current == ']')
                {
                    goto Return;
                }

                if (TryReadRefReferenceTarget())
                {
                    dataWriter.OnWriteValue(i, RWHelper.DefaultValueReader);

                    references.AddLast(new ReferenceInfo(referenceTarget, new SourcePathInfo(i, null, dataWriter)));

                    if (dataWriter is IDataReader reader && reader.ReferenceToken == null)
                    {
                        updateBase = true;
                    }

                    haveRef = true;
                }
                else
                {
                    dataWriter.OnWriteValue(i, this);

                    if (updateBase)
                    {
                        var last = references.Last.Value;

                        last.source = new SourcePathInfo(i, last.source, dataWriter);

                        updateBase = false;

                        haveRef = true;
                    }
                }

                SkipWhiteSpace();

                if (current < end)
                {
                    switch (*current)
                    {
                        case ']':
                            goto Return;
                        case ',':
                            ++current;

                            goto Loop;
                    }
                }
            }

            throw GetException();

        Return:

            ++current;

            if (haveRef && dataWriter is IDataReader dataReader && dataReader.ReferenceToken == null)
            {
                writer = dataWriter;

                updateBase = true;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool TryReadRefReferenceTarget()
        {
            if (!IsObject)
            {
                return false;
            }

            var temp = current;

            ++current;

            SkipWhiteSpace();

            if (Length > 5 && (current[1] == '$' || current[0] == '$') && (EqualsByLower(RefKey) || EqualsByLower(RefName)))
            {
                if (*current == '$')
                {
                    current += RefName.Length;
                }
                else
                {
                    current += RefKey.Length;
                }

                SkipWhiteSpace();

                if (current < end && *current == ':')
                {
                    ++current;

                    SkipWhiteSpace();

                    referenceTarget = ReadRefReferenceTarget();

                    SkipWhiteSpace();

                    if (current < end && *current == '}')
                    {
                        ++current;

                        return true;
                    }
                }
            }

            current = temp;

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private TargetPathInfo ReadRefReferenceTarget()
        {
            var refString = ReadString();

            var refs = refString.Split('/');

            var target = new TargetPathInfo("#", null);

            var index = 0;

            switch (refs[0])
            {
                case "#":
                case "":
                    ++index;
                    break;
            }

            for (; index < refs.Length; ++index)
            {
                if (NumberHelper.Decimal.TryParse(refs[index], out long i) && i >= 0 && i <= int.MaxValue)
                {
                    target = new TargetPathInfo((int)i, target);
                }
                else
                {
                    target = new TargetPathInfo(refs[index], target);
                }
            }

            return target;
        }
    }
}