using Swifter.RW;
using Swifter.Test.WPF.Models;

namespace Swifter.Test.WPF.Tests
{
    public class DeviceModel : BaseTest<Device>
    {
        public override string TestName => "Model";

        public override Device GetObject()
        {
            return new RandomValueReader(1218).FastReadObject<Device>();
        }
    }

}