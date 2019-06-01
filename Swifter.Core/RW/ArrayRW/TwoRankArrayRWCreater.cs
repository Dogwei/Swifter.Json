namespace Swifter.RW
{
    internal sealed class TwoRankArrayRWCreater<T> : IArrayRWCreater<T[,]>
    {
        public ArrayRW<T[,]> Create()
        {
            return new TwoRankArrayRW<T>();
        }
    }
}