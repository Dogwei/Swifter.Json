using Swifter.RW;
using Swifter.Test.WPF.Models;
using System.Collections.Generic;

namespace Swifter.Test.WPF.Tests
{
    public class Devices : BaseTest<List<Device>>
    {
        public override string TestName => "List<Model>";

        public override List<Device> GetObject()
        {
            return new RandomValueReader(1218).ReadList<Device>();
        }
    }

}