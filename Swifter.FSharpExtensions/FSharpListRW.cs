

using Microsoft.FSharp.Collections;
using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.FSharpExtensions
{
    public sealed class FSharpListRW<T> : IDataRW<int>
    {
        public FSharpList<T> content;

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => Enumerable.Range(0, Count);

        public int Count => content.Length;

        public Type ContentType => typeof(FSharpList<T>);

        public object Content
        {
            get => content;
            set => content = (FSharpList<T>)value;
        }

        public void Initialize()
        {
            content = FSharpList<T>.Empty;
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var node = content;
            var length = content.Length;

            for (int i = 0; i < length; i++)
            {
                ValueInterface.WriteValue(dataWriter[i], node.Head);

                node = node.Tail;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = content.Length;

            content = FSharpList<T>.Empty;

            for (int i = 0; i < length; i++)
            {
                content = ListModule.Append(content, new FSharpList<T>(ValueInterface.ReadValue<T>(dataReader[i]), FSharpList<T>.Empty));
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            content = ListModule.Append(content, new FSharpList<T>(ValueInterface.ReadValue<T>(valueReader), FSharpList<T>.Empty));
        }
    }
}