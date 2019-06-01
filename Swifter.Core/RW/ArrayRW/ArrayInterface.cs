using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class ArrayInterface<T> : IValueInterface<T> where T : class
    {
        [ThreadStatic]
        public static InternalInstance<ArrayRW<T>> thread_cache;

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is ISingleThreadOptimize)
            {
                var current_cache = thread_cache;

                if (current_cache == null)
                {
                    current_cache = new InternalInstance<ArrayRW<T>>
                    {
                        Instance = ArrayRW<T>.Create()
                    };

                    thread_cache = current_cache;
                }

                if (current_cache.IsUsed)
                {
                    goto Default;
                }

                current_cache.IsUsed = true;

                current_cache.Instance.count = -1;

                valueReader.ReadArray(current_cache.Instance);

                current_cache.IsUsed = false;

                if (current_cache.Instance.count == -1)
                {
                    return default;
                }

                var backup = current_cache.Instance.content;

                var result = current_cache.Instance.Content;

                if (ReferenceEquals(backup, result))
                {
                    backup = TypeHelper.Clone(result);
                }

                current_cache.Instance.content = backup;

                return result;
            }

        Default:

            var writer = ArrayRW<T>.Create();

            valueReader.ReadArray(writer);

            return writer.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var arrayWriter = ArrayRW<T>.Create();

            arrayWriter.Initialize(value);

            valueWriter.WriteArray(arrayWriter);
        }
    }
}