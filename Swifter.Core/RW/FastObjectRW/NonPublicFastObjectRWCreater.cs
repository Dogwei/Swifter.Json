using System;

namespace Swifter.RW
{
    internal sealed class NonPublicFastObjectRWCreater<T> : IFastObjectRWCreater<T>
    {
        public readonly FastObjectRW<T> firstInstance;

        public NonPublicFastObjectRWCreater(Type rwType) => firstInstance = (FastObjectRW<T>)Activator.CreateInstance(rwType);

        public FastObjectRW<T> Create() => firstInstance.Clone();
    }
}