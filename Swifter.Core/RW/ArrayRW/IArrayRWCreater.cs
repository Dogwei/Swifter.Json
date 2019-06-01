namespace Swifter.RW
{
    internal interface IArrayRWCreater<TArray> where TArray : class
    {
        ArrayRW<TArray> Create();
    }
}