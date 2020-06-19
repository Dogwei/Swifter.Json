using Swifter.RW;

namespace Swifter.Test.WPF.Tests
{
    public class ThreeDimArray : BaseTest<int[,,]>
    {
        public override int[,,] GetObject()
        {
            return ValueInterface<int[,,]>.ReadValue(new RandomValueReader(1218));
        }
    }

}