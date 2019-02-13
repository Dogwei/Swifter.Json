using Swifter.Readers;
using Swifter.RW;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    sealed class ReferenceInfo
    {
        public readonly TargetPathInfo target;

        public SourcePathInfo source;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ReferenceInfo(TargetPathInfo target, SourcePathInfo source)
        {
            this.target = target;
            this.source = source;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object GetTarget(object obj)
        {
            return InternalGetTarget(obj, target);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetSource(object value)
        {
            InternalSetSource(source, value);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void InternalSetSource(SourcePathInfo source, object value)
        {
            if (source.next != null)
            {
                InternalSetSource(source.next, value);

                value = RWHelper.GetContent<object>(source.next.writer);
            }

            if (source.name != null)
            {
                source.writer.As<string>()[source.name].DirectWrite(value);
            }
            else
            {
                source.writer.As<int>()[source.index].DirectWrite(value);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static object InternalGetTarget(object obj, TargetPathInfo target)
        {
            if (target == null || target.IsRoot)
            {
                return obj;
            }

            var dataReader = RWHelper.CreateReader(obj);

            dataReader = InternalGetTarget(dataReader, target.Parent);

            if (target.Name != null)
            {
                return dataReader.As<string>()[target.Name].DirectRead();
            }
            else
            {
                return dataReader.As<int>()[target.Index].DirectRead();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static IDataReader InternalGetTarget(IDataReader dataReader, TargetPathInfo target)
        {
            if (target == null || target.IsRoot)
            {
                return dataReader;
            }

            dataReader = InternalGetTarget(dataReader, target.Parent);

            if (target.Name != null)
            {
                return RWHelper.CreateItemReader(dataReader.As<string>(), target.Name);
            }
            else
            {
                return RWHelper.CreateItemReader(dataReader.As<int>(), target.Index);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ProcessReference(object obj, IEnumerable<ReferenceInfo> references)
        {
            foreach (var item in references)
            {
                item.SetSource(item.GetTarget(obj));
            }
        }
    }
}