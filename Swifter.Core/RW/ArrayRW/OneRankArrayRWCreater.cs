namespace Swifter.RW
{
    internal sealed class OneRankArrayRWCreater<T> : IArrayRWCreater<T[]>
    {
        public ArrayRW<T[]> Create()
        {
            return new OneRankArrayRW<T>();
        }
    }
}