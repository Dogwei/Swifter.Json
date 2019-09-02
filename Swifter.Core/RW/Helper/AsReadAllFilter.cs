
using Swifter.Tools;

namespace Swifter.RW
{
    sealed class AsReadAllFilter<TIn, TOut> : IValueFilter<TIn>
    {
        public readonly IValueFilter<TOut> valueFilter;

        public AsReadAllFilter(IValueFilter<TOut> valueFilter)
        {
            this.valueFilter = valueFilter;
        }

        public bool Filter(ValueFilterInfo<TIn> valueInfo)
        {
            return valueFilter.Filter(new ValueFilterInfo<TOut>(
                XConvert<TOut>.Convert(valueInfo.Key),
                valueInfo.Type,
                valueInfo.ValueCopyer
                ));
        }
    }
}