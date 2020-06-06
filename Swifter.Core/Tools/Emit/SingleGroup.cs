namespace Swifter.Tools
{
    internal class SingleGroup<T> : BaseGroup<T>
    {
        public SingleGroup(CaseInfo<T> value, IDifferenceComparer<T> comparer) : base(comparer, 1)
        {
            Value = value;
        }

        public CaseInfo<T> Value { get; private set; }

        public override int GetDepth() => 0;
    }
}