using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class FastObjectInterface<T> : IValueInterface<T>
    {
        [ThreadStatic]
        static InternalInstance<FastObjectRW<T>> thread_cache;

        static readonly bool CheckChildrenInstance = !TypeInfo<T>.IsFinal;
        static readonly long Int64TypeHandle = TypeInfo<T>.Int64TypeHandle;

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is ISingleThreadOptimize)
            {
                var current_cache = thread_cache;

                if (current_cache == null)
                {
                    InitializeInstance(ref current_cache);
                }

                if (current_cache.IsUsed)
                {
                    goto Default;
                }

                current_cache.IsUsed = true;

                current_cache.Instance.content = default;

                ReadValue(valueReader, current_cache.Instance);

                current_cache.IsUsed = false;

                return current_cache.Instance.content;
            }

        Default:

            var fastObjectRW = FastObjectRW<T>.Create();

            ReadValue(valueReader, fastObjectRW);

            return fastObjectRW.content;
        }
        
        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (CheckChildrenInstance && Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            if (valueWriter is ISingleThreadOptimize)
            {
                var current_cache = thread_cache;

                if (current_cache == null)
                {
                    InitializeInstance(ref current_cache);
                }

                if (current_cache.IsUsed)
                {
                    goto Default;
                }

                current_cache.IsUsed = true;

                current_cache.Instance.Initialize(value);

                valueWriter.WriteObject(current_cache.Instance);

                current_cache.IsUsed = false;

                return;
            }

        Default:

            var fastObjectRW = FastObjectRW<T>.Create();

            fastObjectRW.Initialize(value);

            valueWriter.WriteObject(fastObjectRW);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadValue(IValueReader valueReader, FastObjectRW<T> fastObjectRW)
        {
            if (valueReader is IId64Filler<char> id64Filler)
            {
                id64Filler.FillValue(fastObjectRW);
            }
            else
            {
                valueReader.ReadObject(fastObjectRW);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InitializeInstance(ref InternalInstance<FastObjectRW<T>> current_cache)
        {
            current_cache = new InternalInstance<FastObjectRW<T>>
            {
                Instance = FastObjectRW<T>.Create()
            };

            thread_cache = current_cache;
        }
    }
}