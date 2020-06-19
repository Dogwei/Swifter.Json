using Swifter.RW;

namespace Swifter.Test.WPF.Tests
{
    public class DecimalArray : BaseTest<decimal[]>
    {
        public override decimal[] GetObject()
        {
            return new RandomValueReader(1812).ReadArray<decimal>();
        }
    }

}