using Microsoft.FSharp.Reflection;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swifter.FSharpExtensions
{
    public sealed class FSharpUnionRW<T> : IDataRW<int>
    {
        public const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static readonly Dictionary<string, (UnionCaseInfo CaceInfo, PropertyInfo[] Fields)> UnionCaceMap;

        static FSharpUnionRW()
        {
            UnionCaceMap = new Dictionary<string, (UnionCaseInfo CaceInfo, PropertyInfo[] Fields)>();

            foreach (var item in FSharpType.GetUnionCases(typeof(T), AllBindingFlags))
            {
                UnionCaceMap.Add(item.Name, (item, item.GetFields()));
            }
        }

        object content;

        public IEnumerable<int> Keys => null;

        public int Count => -1;

        public Type ContentType => typeof(object);

        public object Content
        {
            get => GetContent();
            set
            {
                if (value is T val)
                {
                    content = val;
                }
                else if (value is string str)
                {
                    content = new MakeInfo { Name = str };
                }
                else
                {
                    content = XConvert<T>.FromObject(value);
                }
            }
        }

        public T GetContent()
        {
            if (content is MakeInfo make)
            {
                content = make.Make();
            }

            return (T)content;
        }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var (caseInfo, values) = GetUnionFields();

            if (key == 0)
            {
                valueWriter.WriteString(caseInfo.Name);
            }
            else
            {
                ValueInterface.WriteValue(valueWriter, values[key - 1]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var (caseInfo, values) = GetUnionFields();

            dataWriter[0].WriteString(caseInfo.Name);

            int index = 1;

            foreach (var value in values)
            {
                ValueInterface.WriteValue(dataWriter[index], value);
            }
        }

        public (UnionCaseInfo CaceInfo, object[] Values) GetUnionFields()
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            else if (content is MakeInfo make)
            {
                if (make.Name is null)
                {
                    throw new ArgumentNullException(nameof(make.Name));
                }

                return (UnionCaceMap[make.Name].CaceInfo, make.Values);
            }
            else
            {
                return FSharpValue.GetUnionFields(content, typeof(T), AllBindingFlags).ToValueTuple();
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is MakeInfo make)
            {
                if (key == 0)
                {
                    make.Name = valueReader.ReadString();
                }
                else
                {
                    var index = key - 1;

                    make.Values[index] = ValueInterface.GetInterface(make.Fields[index].PropertyType).Read(valueReader);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            if (content is MakeInfo make)
            {
                make.Name = dataReader[0].ReadString();

                for (int i = 0; i < make.Values.Length; i++)
                {
                    make.Values[i] = ValueInterface.GetInterface(make.Fields[i].PropertyType).Read(dataReader[i + 1]);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Initialize()
        {
            content = new MakeInfo();
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public sealed class MakeInfo
        {
            public string Name
            {
                get => CaceInfo?.Name;

                set
                {
                    if (UnionCaceMap.TryGetValue(value, out var info))
                    {
                        CaceInfo = info.CaceInfo;
                        Fields = info.Fields;

                        Values = new object[info.Fields.Length];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException(value);
                    }
                }
            }

            public UnionCaseInfo CaceInfo { get; private set; }

            public PropertyInfo[] Fields { get; private set; }

            public object[] Values { get; private set; }

            public object Make()
            {
                return FSharpValue.MakeUnion(CaceInfo, Values, AllBindingFlags);
            }
        }
    }
}