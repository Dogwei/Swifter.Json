namespace Swifter.Tools
{
    internal abstract class BaseGroup<T>
    {
        public readonly IDifferenceComparer<T> Comparer;

        public readonly int SortToken;

        public BaseGroup(IDifferenceComparer<T> comparer, int sortToken)
        {
            Comparer = comparer;
            SortToken = sortToken;
        }

        public abstract int GetDepth();
    }
}