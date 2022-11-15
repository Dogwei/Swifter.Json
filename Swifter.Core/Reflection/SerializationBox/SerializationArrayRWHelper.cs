using Swifter.RW;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class SerializationArrayRWHelper : IGenericInvoker
    {
        public static IArrayReader CreateReader(Array array)
        {
            var genericInvoker = new SerializationArrayRWHelper(array, true);

            TypeHelper.InvokeType(array.GetType().GetElementType()!, genericInvoker);

            return Unsafe.As<IArrayReader>(genericInvoker.result)!;
        }

        public static IArrayWriter CreateWriter(Array array)
        {
            var genericInvoker = new SerializationArrayRWHelper(array, false);

            TypeHelper.InvokeType(array.GetType().GetElementType()!, genericInvoker);

            return Unsafe.As<IArrayWriter>(genericInvoker.result)!;
        }

        readonly Array array;
        readonly bool isCreateReader;

        object? result;

        public SerializationArrayRWHelper(Array array, bool isCreateReader)
        {
            this.array = array;
            this.isCreateReader = isCreateReader;
        }

        public void Invoke<TElement>()
        {
            if (isCreateReader)
            {
                result = new SerializationArrayReader<TElement>(array);
            }
            else
            {
                result = new SerializationArrayWriter<TElement>(array);
            }
        }
    }
}